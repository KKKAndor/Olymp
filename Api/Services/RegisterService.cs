namespace Api.Services;

public class RegisterService
{
    public async Task<string[]> Registrate(IFormFile file)
    {
        var readStream = file.OpenReadStream();
        readStream.Read()
    }
}