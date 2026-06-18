using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class SaveAgentMemoryUseCase
{
    private readonly IAgentMemoryRepository _agentMemoryRepository;
    private readonly ILogger<SaveAgentMemoryUseCase> _logger;

    public SaveAgentMemoryUseCase(
        IAgentMemoryRepository agentMemoryRepository,
        ILogger<SaveAgentMemoryUseCase> logger)
    {
        _agentMemoryRepository = agentMemoryRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        string agentName,
        string content,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new AgentMemoryEntry
        {
            Content = content,
            Tags = tags?.ToList() ?? []
        };

        _logger.LogInformation(
            "Saving memory for agent '{AgentName}': {Content}",
            agentName, content);

        await _agentMemoryRepository.SaveAsync(agentName, entry, cancellationToken);
    }
}
