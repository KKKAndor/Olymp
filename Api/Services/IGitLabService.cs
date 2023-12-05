using Api.Models;

namespace Api.Services;

public interface IGitLabService
{
    Task<List<User>> RegisterUsers(List<User> users);
}