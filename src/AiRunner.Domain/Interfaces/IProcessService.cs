namespace AiRunner.Domain.Interfaces;

public interface IProcessService
{
    Task StartAgentProcessAsync(
        string workingDirectory,
        string terminalMultiplexer,
        string argument,
        CancellationToken cancellationToken = default);
}
