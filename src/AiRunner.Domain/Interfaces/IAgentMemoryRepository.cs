using AiRunner.Domain.Entities;

namespace AiRunner.Domain.Interfaces;

public interface IAgentMemoryRepository
{
    Task SaveAsync(string agentName, AgentMemoryEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentMemoryEntry>> SearchAsync(string agentName, string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentMemoryEntry>> GetAllAsync(string agentName, CancellationToken cancellationToken = default);
    Task InitializeAsync(string agentName, CancellationToken cancellationToken = default);
}
