using Api.Options;
using Api;
using Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(nameof(MailOptions)));
builder.Services.Configure<GitLabOptions>(builder.Configuration.GetSection(nameof(GitLabOptions)));
builder.Services.AddTransient<RegisterService>();
builder.Services.AddTransient<GitLabService>();

builder.Services.AddOptions();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var api = app.MapGroup("api");
var register = api.MapGroup("register");
register.MapGet("/",
        async (IFormFile file, RegisterService registerService, GitLabService gitLabService) =>
        {
            
            var users = await registerService.Registrate(file);
            var failedUsers = await gitLabService.RegisterUsers(users);
            if (failedUsers.Any())
            {
                throw new Exception();
            }
        })
    .WithSummary("Регистрация пользователей в гитлабе");

app.Run();