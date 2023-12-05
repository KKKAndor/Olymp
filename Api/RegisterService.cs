using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Groups.Requests;
using GitLabApiClient.Models.Users.Requests;

namespace Api;

public class RegisterService
{
    public async Task<string[]> Registrate(IFormFile file)
    {
        var readStream = file.OpenReadStream();
        readStream.Read()
    }
}