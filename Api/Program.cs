using Api.Options;
using Api;
using Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(nameof(MailOptions)));
builder.Services.Configure<GitLabOptions>(builder.Configuration.GetSection(nameof(GitLabOptions)));
builder.Services.AddTransient<IFileReaderService, FileReaderService>();
builder.Services.AddTransient<IGitLabService, GitLabService>();

builder.Services.AddSwaggerGen();

builder.Services.AddOptions();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Run();