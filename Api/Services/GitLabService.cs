using System.Security.Cryptography;
using System.Text;
using Api.Models;
using Api.Options;
using GitLabApiClient;
using GitLabApiClient.Models.Users.Requests;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Api.Services;

public class GitLabService : IGitLabService
{
    private readonly GitLabOptions _gitLabOptions;
    private readonly MailOptions _mailOptions;

    public GitLabService(
        IOptions<GitLabOptions> gitLabOptions,
        IOptions<MailOptions> mailOptions)
    {
        _gitLabOptions = gitLabOptions?.Value ?? throw new ArgumentException(nameof(GitLabOptions));
        if (string.IsNullOrEmpty(_gitLabOptions.Url) || string.IsNullOrEmpty(_gitLabOptions.Token))
        {
            throw new ArgumentException(nameof(GitLabOptions));
        }

        _mailOptions = mailOptions?.Value ?? throw new ArgumentException(nameof(MailOptions));
        if (string.IsNullOrEmpty(_mailOptions.Address) || string.IsNullOrEmpty(_mailOptions.Password))
        {
            throw new ArgumentException(nameof(MailOptions));
        }
    }

    public async Task<List<User>> RegisterUsers(List<User> users)
    {
        var gitLabClient = new GitLabClient(_gitLabOptions.Url, _gitLabOptions.Token);

        var failedUsers = new List<User>();

        foreach (var user in users)
        {
            try
            {
                var password = GenerateRandomPassword(10);

                var existedUser = await gitLabClient.Users.GetAsync(user.Email);

                if (existedUser == null)
                {
                    failedUsers.Add(user);
                    continue;
                }

                var createUserRequest = new CreateUserRequest(user.Name, user.Email, user.Email)
                {
                    Password = password
                };

                var createdUser = await gitLabClient.Users.CreateAsync(createUserRequest);

                // TODO А надо ли?
                // var addGroupMemberRequest = new AddGroupMemberRequest(AccessLevel.Developer, createdUser.Id);
                //
                // await gitLabClient.Groups.AddMemberAsync(_gitLabOptions.GroupId, addGroupMemberRequest);
            }
            catch (Exception ex)
            {
                failedUsers.Add(user);
            }
        }

        return failedUsers;
    }

    private async Task SendMail(string email, string name, string password)
    {
        using var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Dex", _mailOptions.Address));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = "Приглашение";
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            // TODO Здесь по идее какая-то html'а 
            Text = name
        };

        using var client = new SmtpClient();

        await client.ConnectAsync("smtp.yandex.ru", 25, false);
        await client.AuthenticateAsync(_mailOptions.Address, _mailOptions.Password);
        await client.SendAsync(emailMessage);

        await client.DisconnectAsync(true);
    }

    private static string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[length];
        rng.GetBytes(randomBytes);

        var password = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var randomNumber = randomBytes[i] % validChars.Length;
            password.Append(validChars[randomNumber]);
        }

        return password.ToString();
    }
}