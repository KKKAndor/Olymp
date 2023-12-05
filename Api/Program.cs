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

builder.Services.AddOptions();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();