using Microsoft.Extensions.Configuration;

namespace AiRunner.Infrastructure.Dtos;

internal class RunnerCommandDto
{
    [ConfigurationKeyName("name")]
    public string Name { get; set; } = string.Empty;

    [ConfigurationKeyName("command")]
    public string Command { get; set; } = string.Empty;
}
