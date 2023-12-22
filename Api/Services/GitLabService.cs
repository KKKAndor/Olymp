using Api.Models;
using Api.Options;
using GitLabApiClient;
using GitLabApiClient.Internal.Paths;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
using GitLabApiClient.Models.Users.Requests;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Exceptions;

namespace Api.Services;

public class GitLabService : IGitLabService
{
    private readonly GitLabOptions _gitLabOptions;
    // private readonly IEmailSender _emailSender;

    public GitLabService(IOptions<GitLabOptions> gitLabOptions /*, IEmailSender emailSender*/)
    {
        _gitLabOptions = gitLabOptions?.Value ?? throw new ArgumentException(nameof(GitLabOptions));
        // _emailSender = emailSender ?? throw new ArgumentException(nameof(MailOptions));
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
                await Task.Delay(3000);
                if (existedUser != null)
                {
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email,
                            "Finding in gitlab",
                            "Already existed at gitlab"));
                    continue;
                }

                //var password = Helper.GenerateRandomPassword(10);

                // Creating user
                int userId;
                try
                {
                    //var nameToRegister = Helper.TransliterateToLatin(user.Name);
                    var createUserRequest = new CreateUserRequest(user.Name, login, user.Email)
                    {
                        // Password = password,
                        ResetPassword = true
                    };
                    var createdUser = await gitLabClient.Users.CreateAsync(createUserRequest);
                    await Task.Delay(3000);
                    userId = createdUser.Id;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(
                        $"User {user.Name} with email {user.Email} and login {login} created with Id {userId}");
                    Console.ForegroundColor = default;
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"ERROR: User {user.Name} with email {user.Email} and login {login} was not created. Message: "
                        + exception.Message);
                    Console.ForegroundColor = default;
                    failedUsers.Add(
                        new UserFailureResponse(
                            user.Email,
                            "Creating user",
                            exception.Message));
                    continue;
                }

                // Adding user to group
                var accessLevel = AccessLevel.Reporter;
                var addGroupMemberRequest = new AddGroupMemberRequest(accessLevel, userId);
                foreach (var group in user.Group.Split(','))
                {
                    try
                    {
                        await gitLabClient.Groups.AddMemberAsync(group, addGroupMemberRequest);
                        await Task.Delay(3000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(
                            $"User {user.Name} with email {user.Email} and login {login} added to group {group} as {accessLevel}");
                        Console.ForegroundColor = default;
                    }

                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"ERROR: User {user.Name} with email {user.Email} and login {login} was not added to group {group}. Message: "
                            + exception.Message);
                        Console.ForegroundColor = default;
                        failedUsers.Add(
                            new UserFailureResponse(
                                user.Email,
                                "Adding user to group",
                                exception.Message));
                    }
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

    public async Task DeleteUsers(List<User> users)
    {
        var gitLabClient = new GitLabClient(_gitLabOptions.Url, _gitLabOptions.Token);

        foreach (var user in users)
        {
            var currentUserId = 0;
            try
            {
                var login = user.Email.Split('@')[0];
                var existedUser = await gitLabClient.Users.GetAsync(login);
                currentUserId = existedUser.Id;
                await Task.Delay(1000);
                await gitLabClient.Users.DeleteAsync(currentUserId);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"User {currentUserId} deleted");
                Console.ForegroundColor = default;
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"There is error while deleting user {currentUserId}"
                    + exception.Message);
                Console.ForegroundColor = default;
            }
            await Task.Delay(4000);
        }
    }

    public async Task<string> UpdateUserPasswordAndResend(ResendModel model)
    {
        var gitLabClient = new GitLabClient(_gitLabOptions.Url, _gitLabOptions.Token);
        var email = model.Email;

        var login = email.Split('@')[0];

        var existedUser = await gitLabClient.Users.GetAsync(login);

        if (existedUser == null)
        {
            throw new OpenApiException();
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
            throw new OpenApiException();
        }

        // // ReSenging email to user
        // try
        // {
        //     await _emailSender.SendMail(email, login, model.Name, password);
        // }
        // catch (Exception exception)
        // {
        //     return new UserFailureResponse(
        //         email,
        //         "Sending email",
        //         exception.Message);
        // }

        return password;
    }
}