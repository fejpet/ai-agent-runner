namespace AiRunner.Domain.Interfaces;

public interface IProcessService
{
    Task StartAgentProcessAsync(
        string workingDirectory,
        string terminalMultiplexer,
        string cli,
        CancellationToken cancellationToken = default);
}
