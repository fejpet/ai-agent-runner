using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class StartAgentsUseCase
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IProcessService _processService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<StartAgentsUseCase> _logger;

    public StartAgentsUseCase(
        IConfigurationRepository configurationRepository,
        IProcessService processService,
        ITemplateService templateService,
        ILogger<StartAgentsUseCase> logger)
    {
        _configurationRepository = configurationRepository;
        _processService = processService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetConfigurationAsync(cancellationToken);

        _logger.LogInformation("Starting {Count} agent(s)...", config.Agents.Count);

        foreach (var agent in config.Agents)
        {
            var agentFolder = Path.Combine(config.InstancesFolder, agent.AgentFolderName);
            var resolvedArguments = _templateService.Resolve(
                config.Argument,
                new Dictionary<string, string> { ["agent-name"] = agent.Name });

            _logger.LogInformation(
                "Starting agent '{AgentName}' in directory '{AgentFolder}'",
                agent.Name,
                agentFolder);

            await _processService.StartAgentProcessAsync(
                agentFolder,
                config.TerminalMultiplexer,
                resolvedArguments,
                cancellationToken);
        }
    }
}
