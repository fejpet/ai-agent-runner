namespace AiRunner.Domain.Interfaces;

public interface IProcessService
{
    Task StartAgentProcessAsync(
        string workingDirectory,
        string command,
        CancellationToken cancellationToken = default);

    Task<int> RunCommandAndGetExitCodeAsync(
        string workingDirectory,
        string command,
        CancellationToken cancellationToken = default);
}
