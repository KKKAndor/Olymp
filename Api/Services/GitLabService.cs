using System.Security.Cryptography;
using System.Text;
using Api.Models;
using Api.Options;
using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
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
        _mailOptions = mailOptions?.Value ?? throw new ArgumentException(nameof(MailOptions));
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

                var login = user.Email.Split('@')[0];

                var existedUser = await gitLabClient.Users.GetAsync(login);

                if (existedUser != null)
                {
                    failedUsers.Add(user);
                    continue;
                }

                var nameToRegister = TransliterateToLatin(user.Name);

                var createUserRequest = new CreateUserRequest(nameToRegister, login, user.Email)
                {
                    Password = password
                };

                var createdUser = await gitLabClient.Users.CreateAsync(createUserRequest);

                var addGroupMemberRequest = new AddGroupMemberRequest(AccessLevel.Reporter, createdUser.Id);

                await gitLabClient.Groups.AddMemberAsync(user.Group, addGroupMemberRequest);

                await SendMail(user.Email, login, user.Name, password);
            }
            catch (Exception)
            {
                failedUsers.Add(user);
            }
        }

        return failedUsers;
    }

    private async Task SendMail(string email, string login, string name, string password)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Dex", _mailOptions.Address));
        emailMessage.To.Add(new MailboxAddress(name, email));
        emailMessage.Subject = "Ivitation";
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            // TODO Здесь по идее какая-то html'а 
            Text = $"Hello there {name}, here is your shit {login} - {password}"
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(_mailOptions.SmtpType, _mailOptions.SmtpPort, false);
        await smtpClient.AuthenticateAsync(_mailOptions.Address, _mailOptions.Password);
        await smtpClient.SendAsync(emailMessage);
        await smtpClient.DisconnectAsync(true);
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

    static string TransliterateToLatin(string text)
    {
        var cyrillicToLatin = new Dictionary<char, string>
        {
            {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"}, {'Е', "E"}, {'Ё', "YO"},
            {'Ж', "ZH"}, {'З', "Z"}, {'И', "I"}, {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"},
            {'Н', "N"}, {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"}, {'У', "U"},
            {'Ф', "F"}, {'Х', "KH"}, {'Ц', "TS"}, {'Ч', "CH"}, {'Ш', "SH"}, {'Щ', "SCH"}, {'Ъ', ""},
            {'Ы', "Y"}, {'Ь', ""}, {'Э', "E"}, {'Ю', "YU"}, {'Я', "YA"},
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"}, {'ё', "yo"},
            {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"},
            {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"}, {'у', "u"},
            {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"}, {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""},
            {'ы', "y"}, {'ь', ""}, {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        var latinText = new StringBuilder();

        foreach (var symbol in text)
        {
            if (cyrillicToLatin.TryGetValue(symbol, out var value))
            {
                latinText.Append(value);
            }
            else
            {
                latinText.Append(symbol);
            }
        }

        return latinText.ToString();
    }
}