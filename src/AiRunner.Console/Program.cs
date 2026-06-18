using AiRunner.Application.UseCases;
using AiRunner.Console;
using AiRunner.Domain.Interfaces;
using AiRunner.Infrastructure.Repositories;
using AiRunner.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConfigurationRepository, JsonConfigurationRepository>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IFileSystemService, FileSystemService>();
builder.Services.AddSingleton<IProcessService, ProcessService>();
builder.Services.AddTransient<InitializeInfrastructureUseCase>();
builder.Services.AddTransient<StartAgentsUseCase>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
