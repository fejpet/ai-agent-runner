using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class InitializeInfrastructureUseCase
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<InitializeInfrastructureUseCase> _logger;

    public InitializeInfrastructureUseCase(
        IConfigurationRepository configurationRepository,
        IFileSystemService fileSystemService,
        ILogger<InitializeInfrastructureUseCase> logger)
    {
        _configurationRepository = configurationRepository;
        _fileSystemService = fileSystemService;
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
        }
    }
}
