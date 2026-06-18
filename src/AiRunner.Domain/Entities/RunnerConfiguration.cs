namespace AiRunner.Domain.Entities;

public class RunnerConfiguration
{
    public string RootFolder { get; init; } = string.Empty;
    public string TerminalMultiplexer { get; init; } = string.Empty;
    public string Argument { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public IReadOnlyList<Agent> Agents { get; init; } = [];

    public string InstancesFolder => Path.Combine(RootFolder, "instances");
    public string TemplatesFolder => Path.Combine(RootFolder, "templates");
}
