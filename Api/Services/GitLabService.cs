using System.Security.Cryptography;
using System.Text;
using Api.Models;
using Api.Options;
using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
using GitLabApiClient.Models.Users.Requests;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class GitLabService : IGitLabService
{
    private readonly GitLabOptions _gitLabOptions;
    private readonly IEmailSender _emailSender;

    public GitLabService(IOptions<GitLabOptions> gitLabOptions, IEmailSender emailSender)
    {
        _gitLabOptions = gitLabOptions?.Value ?? throw new ArgumentException(nameof(GitLabOptions));
        _emailSender = emailSender ?? throw new ArgumentException(nameof(MailOptions));
    }

    public async Task<List<UserFailureResponse>> RegisterUsers(List<User> users)
    {
        var gitLabClient = new GitLabClient(_gitLabOptions.Url, _gitLabOptions.Token);
        var failedUsers = new List<UserFailureResponse>();

        foreach (var user in users)
        {
            try
            {
                var login = user.Email.Split('@')[0];

                var existedUser = await gitLabClient.Users.GetAsync(login);

                if (existedUser != null)
                {
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email, 
                            "Finding in gitlab",
                            "Already existed at gitlab"));
                    continue;
                }

                var password = Helper.GenerateRandomPassword(10);

                // Creating user
                int userId;
                try
                {
                    var nameToRegister = Helper.TransliterateToLatin(user.Name);
                    var createUserRequest = new CreateUserRequest(nameToRegister, login, user.Email)
                    {
                        Password = password
                    };
                    var createdUser = await gitLabClient.Users.CreateAsync(createUserRequest);
                    userId = createdUser.Id;
                }
                catch (Exception exception)
                {
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email, 
                            "Creating user",
                            exception.Message));
                    continue;
                }
                
                // Adding user to group
                try
                {
                    var addGroupMemberRequest = new AddGroupMemberRequest(AccessLevel.Reporter, userId);
                    await gitLabClient.Groups.AddMemberAsync(user.Group, addGroupMemberRequest);
                }
                catch (Exception exception)
                {
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email, 
                            "Adding user to group",
                            exception.Message));
                    continue;
                }

                // Senging email to user
                try
                {
                    await _emailSender.SendMail(user.Email, login, user.Name, password);
                }
                catch (Exception exception)
                {
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email, 
                            "Sending email",
                            exception.Message));
                }
            }
            catch (Exception)
            {
                failedUsers.Add(new UserFailureResponse(
                    user.Email, 
                    "Unexpected shit",
                    "Unexpected shit"));
            }
        }

        return failedUsers;
    }
}