using Api.Models;

namespace Api.Services;

public interface IFileReaderService
{
    Task<List<User>> ReadUsers(IFormFile file);
}