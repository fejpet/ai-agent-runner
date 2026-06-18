namespace AiRunner.Domain.Entities;

public class AgentMemoryEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Content { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
