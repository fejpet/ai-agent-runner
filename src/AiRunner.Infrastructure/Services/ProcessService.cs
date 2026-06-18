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
        string terminalMultiplexer,
        string argument,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Launching process: {TerminalMultiplexer} {Argument} in {WorkingDirectory}",
            terminalMultiplexer, argument, workingDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = terminalMultiplexer,
            Arguments = argument,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
            CreateNoWindow = false
        };

        var process = Process.Start(startInfo);

        if (process is null)
        {
            _logger.LogWarning(
                "Failed to start process '{TerminalMultiplexer} {Argument}' in '{WorkingDirectory}'",
                terminalMultiplexer, argument, workingDirectory);
        }
        else
        {
            _logger.LogInformation("Started process with PID {Pid}", process.Id);
        }

        return Task.CompletedTask;
    }
}
