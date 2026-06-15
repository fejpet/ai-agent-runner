using AiRunner.Domain.Entities;

namespace AiRunner.Domain.Interfaces;

public interface IConfigurationRepository
{
    Task<RunnerConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
}
