using Microsoft.Extensions.Configuration;

namespace AiRunner.Infrastructure.Dtos;

internal class RunnerConfigurationDto
{
    [ConfigurationKeyName("root-folder")]
    public string RootFolder { get; set; } = string.Empty;

    [ConfigurationKeyName("commands")]
    public List<RunnerCommandDto> Commands { get; set; } = [];

    [ConfigurationKeyName("owner-name")]
    public string OwnerName { get; set; } = string.Empty;

    [ConfigurationKeyName("agents")]
    public List<AgentDto> Agents { get; set; } = [];
}
