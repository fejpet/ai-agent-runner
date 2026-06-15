using Microsoft.Extensions.Configuration;

namespace AiRunner.Infrastructure.Dtos;

internal class AgentDto
{
    [ConfigurationKeyName("name")]
    public string Name { get; set; } = string.Empty;

    [ConfigurationKeyName("report-to")]
    public string? ReportTo { get; set; }

    [ConfigurationKeyName("role")]
    public string Role { get; set; } = string.Empty;
}
