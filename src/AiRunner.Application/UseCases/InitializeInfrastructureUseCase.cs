using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class InitializeInfrastructureUseCase
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IFileSystemService _fileSystemService;
    private readonly IAgentMemoryRepository _agentMemoryRepository;
    private readonly ITemplateService _templateService;
    private readonly ILogger<InitializeInfrastructureUseCase> _logger;

    public InitializeInfrastructureUseCase(
        IConfigurationRepository configurationRepository,
        IFileSystemService fileSystemService,
        IAgentMemoryRepository agentMemoryRepository,
        ITemplateService templateService,
        ILogger<InitializeInfrastructureUseCase> logger)
    {
        _configurationRepository = configurationRepository;
        _fileSystemService = fileSystemService;
        _agentMemoryRepository = agentMemoryRepository;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetConfigurationAsync(cancellationToken);

        _logger.LogInformation("Initializing infrastructure under root folder: {RootFolder}", config.RootFolder);

        _fileSystemService.EnsureDirectoryExists(config.RootFolder);
        _fileSystemService.EnsureDirectoryExists(config.InstancesFolder);
        _fileSystemService.EnsureDirectoryExists(config.TemplatesFolder);

        foreach (var agent in config.Agents)
        {
            var agentFolder = Path.Combine(config.InstancesFolder, agent.AgentFolderName);
            _fileSystemService.EnsureDirectoryExists(agentFolder);
            _logger.LogInformation("Agent folder ready: {AgentFolder}", agentFolder);

            await _agentMemoryRepository.InitializeAsync(agent.Name, cancellationToken);

            InitializeAgentMd(agentFolder, agent.Name, agent.Role, config.TemplatesFolder);
            InitializeAgentOverrideMd(agentFolder);
        }
    }

    private void InitializeAgentMd(string agentFolder, string agentName, string agentRole, string templatesFolder)
    {
        var agentMdPath = Path.Combine(agentFolder, "AGENTS.md");
        if (_fileSystemService.FileExists(agentMdPath))
        {
            _logger.LogDebug("AGENTS.md already exists for agent '{AgentName}' — skipping initialization.", agentName);
            return;
        }

        var templateFileName = $"{agentRole}-template.md";
        var templatePath = Path.Combine(templatesFolder, templateFileName);

        if (!_fileSystemService.FileExists(templatePath))
        {
            _logger.LogWarning(
                "Template file '{TemplateFile}' not found for agent '{AgentName}' — AGENTS.md will not be created.",
                templateFileName,
                agentName);
            return;
        }

        var templateContent = _fileSystemService.ReadAllText(templatePath);

        var agentValues = new Dictionary<string, string>
        {
            ["agent-name"] = agentName,
            ["BOT_NAME"] = agentName,
            ["role"] = agentRole
        };

        var resolvedContent = _templateService.Resolve(templateContent, agentValues);

        _fileSystemService.WriteAllText(agentMdPath, resolvedContent);
        _logger.LogInformation("AGENTS.md initialized for agent '{AgentName}'.", agentName);
    }

    private void InitializeAgentOverrideMd(string agentFolder)
    {
        var overridePath = Path.Combine(agentFolder, "AGENTS.override.md");
        if (_fileSystemService.FileExists(overridePath))
        {
            _logger.LogDebug("AGENTS.override.md already exists — skipping.");
            return;
        }

        _fileSystemService.WriteAllText(overridePath, string.Empty);
        _logger.LogInformation("AGENTS.override.md created in '{AgentFolder}'.", agentFolder);
    }
}
