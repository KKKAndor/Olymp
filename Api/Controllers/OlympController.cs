using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]/[action]")]
public class OlympController : Controller
{
    private readonly IFileReaderService _fileReaderService;
    private readonly IGitLabService _gitLabService;

    public OlympController(IFileReaderService fileReaderService, IGitLabService gitLabService)
    {
        _fileReaderService = fileReaderService;
        _gitLabService = gitLabService;
    }

    [HttpPost]
    public async Task<IActionResult> ReadFileAndRegister(IFormFile file)
    {
        var users = await _fileReaderService.ReadUsers(file);
        var failedUsersResponses = await _gitLabService.RegisterUsers(users);
        if (failedUsersResponses.Any())
        {
            return BadRequest(failedUsersResponses);
        }

        return Ok();
    }

    [HttpDelete]
    public async Task DeleteUsers(IFormFile file)
    {
        var users = await _fileReaderService.ReadUsers(file);
        await _gitLabService.DeleteUsers(users);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdatePasswordAndResend([FromBody] ResendModel model)
    {
        var failedUserResponse = await _gitLabService.UpdateUserPasswordAndResend(model);
        if (failedUserResponse != null)
        {
            return BadRequest(failedUserResponse);
        }

        return Ok();
    }
}