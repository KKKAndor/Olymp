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
                        // Password = password,
                        ResetPassword = true
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

                // TODO А когда то надо было
                // Senging email to user
                // try
                // {
                //     await _emailSender.SendMail(user.Email, login, user.Name, password);
                // }
                // catch (Exception exception)
                // {
                //     failedUsers.Add(
                //         new UserFailureResponse(
                //             user.Email,
                //             "Sending email",
                //             exception.Message));
                // }
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

    public async Task<UserFailureResponse> UpdateUserPasswordAndResend(ResendModel model)
    {
        var gitLabClient = new GitLabClient(_gitLabOptions.Url, _gitLabOptions.Token);
        var email = model.Email;

        var login = email.Split('@')[0];

        var existedUser = await gitLabClient.Users.GetAsync(login);

        if (existedUser == null)
        {
            return new UserFailureResponse(
                email,
                "Finding in gitlab",
                "Not existed at gitlab");
        }

        var password = Helper.GenerateRandomPassword(10);

        // Updating user
        try
        {
            var createUserRequest = new UpdateUserRequest
            {
                Password = password
            };
            await gitLabClient.Users.UpdateAsync(existedUser.Id, createUserRequest);
        }
        catch (Exception exception)
        {
            return new UserFailureResponse(
                email,
                "Updating user",
                exception.Message);
        }

        // ReSenging email to user
        try
        {
            await _emailSender.SendMail(email, login, model.Name, password);
        }
        catch (Exception exception)
        {
            return new UserFailureResponse(
                email,
                "Sending email",
                exception.Message);
        }

        return null;
    }
}