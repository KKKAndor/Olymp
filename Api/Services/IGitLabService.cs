using Api.Models;

namespace Api.Services;

public interface IGitLabService
{
    Task<List<UserFailureResponse>> RegisterUsers(List<User> users);

    Task<string> UpdateUserPasswordAndResend(ResendModel model);

    Task DeleteUsers(List<User> ids);
}