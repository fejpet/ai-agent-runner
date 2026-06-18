namespace AiRunner.Infrastructure.Dtos;

internal class AgentMemoryEntryDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}
