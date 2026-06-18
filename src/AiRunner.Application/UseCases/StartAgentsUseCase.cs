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

        var startCommandTemplate = config.Commands.FirstOrDefault(c => c.Name == "start")?.Command
            ?? throw new InvalidOperationException("No 'start' command found in runner configuration.");

        var hasSessionCommandTemplate = config.Commands.FirstOrDefault(c => c.Name == "has-session")?.Command;

        foreach (var agent in config.Agents)
        {
            var agentFolder = Path.Combine(config.InstancesFolder, agent.AgentFolderName);
            var templateValues = new Dictionary<string, string> { ["agent-name"] = agent.Name };

            if (hasSessionCommandTemplate is not null)
            {
                var resolvedHasSessionCommand = _templateService.Resolve(hasSessionCommandTemplate, templateValues);

                var exitCode = await _processService.RunCommandAndGetExitCodeAsync(
                    agentFolder,
                    resolvedHasSessionCommand,
                    cancellationToken);

                if (exitCode == 0)
                {
                    _logger.LogInformation(
                        "Agent '{AgentName}' session already exists — skipping start.",
                        agent.Name);
                    continue;
                }
            }

            var resolvedStartCommand = _templateService.Resolve(startCommandTemplate, templateValues);

            _logger.LogInformation(
                "Starting agent '{AgentName}' in directory '{AgentFolder}'",
                agent.Name,
                agentFolder);

            await _processService.StartAgentProcessAsync(
                agentFolder,
                resolvedStartCommand,
                cancellationToken);
        }
    }
}
