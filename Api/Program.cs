using Api.Options;
using Api;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitLabOptions>(builder.Configuration.GetSection(nameof(GitLabOptions)));



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var api = app.MapGroup("api");
var register = api.MapGroup("register");
register.MapGet("/",
        async (IFormFile file, RegisterService registerService) =>
            await registerService.Registrate(file))
    .WithSummary("Регистрация пользователей в гитлабе");

app.Run();