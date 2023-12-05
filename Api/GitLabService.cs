﻿using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
using GitLabApiClient.Models.Users.Requests;

namespace Api;

public class GitLabService
{
    public Task RegisterUsers()
    {
        var emails = new[] {"email@mail.com"};
        var gitLabUrl = "https://git.olympics.dex-it.ru";
        var accessToken = "tokan";
        var groupId = 1;

        var gitLabClient = new GitLabClient(gitLabUrl, accessToken);

        foreach (var email in emails)
        {
            try
            {
                var existedUser = await gitLabClient.Users.GetAsync(email);

                if (existedUser == null)
                {
                    continue;
                }

                var createUserRequest = new CreateUserRequest(email, email, email)
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