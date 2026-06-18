using Microsoft.Extensions.Configuration;

namespace AiRunner.Infrastructure.Dtos;

internal class RunnerConfigurationDto
{
    [ConfigurationKeyName("root-folder")]
    public string RootFolder { get; set; } = string.Empty;

    [ConfigurationKeyName("terminal-multiplexer")]
    public string TerminalMultiplexer { get; set; } = string.Empty;

    [ConfigurationKeyName("argument")]
    public string Argument { get; set; } = string.Empty;

    [ConfigurationKeyName("owner-name")]
    public string OwnerName { get; set; } = string.Empty;

    [ConfigurationKeyName("agents")]
    public List<AgentDto> Agents { get; set; } = [];
}
