using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Application.UseCases;

public class SearchAgentMemoryUseCase
{
    private readonly IAgentMemoryRepository _agentMemoryRepository;
    private readonly ILogger<SearchAgentMemoryUseCase> _logger;

    public SearchAgentMemoryUseCase(
        IAgentMemoryRepository agentMemoryRepository,
        ILogger<SearchAgentMemoryUseCase> logger)
    {
        _agentMemoryRepository = agentMemoryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AgentMemoryEntry>> ExecuteAsync(
        string agentName,
        string query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Searching memory for agent '{AgentName}' with query: {Query}",
            agentName, query);

        return await _agentMemoryRepository.SearchAsync(agentName, query, cancellationToken);
    }
}
