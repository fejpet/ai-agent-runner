using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class StartAgentsUseCase
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IProcessService _processService;
    private readonly ILogger<StartAgentsUseCase> _logger;

    public StartAgentsUseCase(
        IConfigurationRepository configurationRepository,
        IProcessService processService,
        ILogger<StartAgentsUseCase> logger)
    {
        _configurationRepository = configurationRepository;
        _processService = processService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetConfigurationAsync(cancellationToken);

        _logger.LogInformation("Starting {Count} agent(s)...", config.Agents.Count);

        foreach (var agent in config.Agents)
        {
            var agentFolder = Path.Combine(config.InstancesFolder, agent.AgentFolderName);
            _logger.LogInformation(
                "Starting agent '{AgentName}' in directory '{AgentFolder}'",
                agent.Name,
                agentFolder);

            await _processService.StartAgentProcessAsync(
                agentFolder,
                config.TerminalMultiplexer,
                config.Cli,
                cancellationToken);
        }
    }
}
