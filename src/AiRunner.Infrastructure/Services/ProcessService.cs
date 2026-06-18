using System.Diagnostics;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Infrastructure.Services;

public class ProcessService : IProcessService
{
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(ILogger<ProcessService> logger)
    {
        _logger = logger;
    }

    public Task StartAgentProcessAsync(
        string workingDirectory,
        string command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Launching process: {Command} in {WorkingDirectory}",
            command, workingDirectory);

        var spaceIndex = command.IndexOf(' ');
        var fileName = spaceIndex < 0 ? command : command[..spaceIndex];
        var arguments = spaceIndex < 0 ? string.Empty : command[(spaceIndex + 1)..];

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
            CreateNoWindow = false
        };

        var process = Process.Start(startInfo);

        if (process is null)
        {
            _logger.LogWarning(
                "Failed to start process '{Command}' in '{WorkingDirectory}'",
                command, workingDirectory);
        }
        else
        {
            _logger.LogInformation("Started process with PID {Pid}", process.Id);
        }

        return Task.CompletedTask;
    }

}
