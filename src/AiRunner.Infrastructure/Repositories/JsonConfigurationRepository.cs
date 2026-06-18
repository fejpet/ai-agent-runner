using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using AiRunner.Infrastructure.Dtos;
using Microsoft.Extensions.Configuration;

namespace AiRunner.Infrastructure.Repositories;

public class JsonConfigurationRepository : IConfigurationRepository
{
    private readonly IConfiguration _configuration;

    public JsonConfigurationRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<RunnerConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var dto = _configuration.GetSection("runner").Get<RunnerConfigurationDto>()
            ?? new RunnerConfigurationDto();

        var configuration = new RunnerConfiguration
        {
            RootFolder = dto.RootFolder,
            OwnerName = dto.OwnerName,
            Commands = dto.Commands
                .Select(c => new RunnerCommand
                {
                    Name = c.Name,
                    Command = c.Command
                })
                .ToList(),
            Agents = dto.Agents
                .Select(a => new Agent
                {
                    Name = a.Name,
                    ReportTo = a.ReportTo,
                    Role = a.Role
                })
                .ToList()
        };

        return Task.FromResult(configuration);
    }
}
