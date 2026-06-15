using AiRunner.Application.UseCases;

namespace AiRunner.Console;

public class Worker : BackgroundService
{
    private readonly InitializeInfrastructureUseCase _initializeInfrastructure;
    private readonly StartAgentsUseCase _startAgents;
    private readonly ILogger<Worker> _logger;

    public Worker(
        InitializeInfrastructureUseCase initializeInfrastructure,
        StartAgentsUseCase startAgents,
        ILogger<Worker> logger)
    {
        _initializeInfrastructure = initializeInfrastructure;
        _startAgents = startAgents;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ai-runner starting...");

        await _initializeInfrastructure.ExecuteAsync(stoppingToken);
        await _startAgents.ExecuteAsync(stoppingToken);

        _logger.LogInformation("ai-runner: all agents started.");
    }
}
