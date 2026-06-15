namespace AiRunner.Domain.Entities;

public class Agent
{
    public string Name { get; init; } = string.Empty;
    public string? ReportTo { get; init; }
    public string Role { get; init; } = string.Empty;

    public string AgentFolderName => $"{Name}-agent";
}
