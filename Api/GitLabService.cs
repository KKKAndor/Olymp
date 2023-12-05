using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
using GitLabApiClient.Models.Users.Requests;

namespace Api;

public class GitLabService
{
    public async Task RegisterUsers(User[] users)
    {
        var gitLabUrl = "https://git.olympics.dex-it.ru";
        var accessToken = "tokan";
        var groupId = 1;

        var gitLabClient = new GitLabClient(gitLabUrl, accessToken);

        foreach (var user in users)
        {
            try
            {
                var existedUser = await gitLabClient.Users.GetAsync(user.Firstname + user.Surname);

                if (existedUser == null)
                {
                    continue;
                }

                var createUserRequest = new CreateUserRequest(user.Firstname + user.Surname, user.Firstname + user.Surname, user.Email)
                {
                    Password = "SomeRandomShit"
                };

                var createdUser = await gitLabClient.Users.CreateAsync(createUserRequest);
                    
                var addGroupMemberRequest = new AddGroupMemberRequest(AccessLevel.Developer, createdUser.Id);
                    
                await gitLabClient.Groups.AddMemberAsync(groupId, addGroupMemberRequest);
            }
            catch (Exception ex)
            {
                // ОбработОчка
            }
        }
    }
}