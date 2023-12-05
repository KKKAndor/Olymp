using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]/[action]")]
public class RegisterController : Controller
{
    private readonly IFileReaderService _fileReaderService;
    private readonly IGitLabService _gitLabService;

    public RegisterController(IFileReaderService fileReaderService, IGitLabService gitLabService)
    {
        _fileReaderService = fileReaderService;
        _gitLabService = gitLabService;
    }

    [HttpPost]
    public async Task<IActionResult> Register(IFormFile file)
    {
        var users = await _fileReaderService.ReadUsers(file);
        var failedUsers = await _gitLabService.RegisterUsers(users);
        if (failedUsers.Any())
        {
            return Conflict();
        }

        return Ok();
    }
}