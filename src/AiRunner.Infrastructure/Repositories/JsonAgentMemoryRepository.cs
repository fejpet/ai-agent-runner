using System.Text.Json;
using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using AiRunner.Infrastructure.Dtos;
using Microsoft.Extensions.Logging;

namespace AiRunner.Infrastructure.Repositories;

public class JsonAgentMemoryRepository : IAgentMemoryRepository
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly ILogger<JsonAgentMemoryRepository> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonAgentMemoryRepository(
        IConfigurationRepository configurationRepository,
        ILogger<JsonAgentMemoryRepository> logger)
    {
        _configurationRepository = configurationRepository;
        _logger = logger;
    }

    public async Task InitializeAsync(string agentName, CancellationToken cancellationToken = default)
    {
        var memoryFile = await GetMemoryFilePathAsync(agentName, cancellationToken);
        if (!File.Exists(memoryFile))
        {
            await File.WriteAllTextAsync(memoryFile, "[]", cancellationToken);
            _logger.LogInformation("Initialized memory file for agent '{AgentName}' at {Path}", agentName, memoryFile);
        }
    }

    public async Task SaveAsync(string agentName, AgentMemoryEntry entry, CancellationToken cancellationToken = default)
    {
        var memoryFile = await GetMemoryFilePathAsync(agentName, cancellationToken);
        var entries = await ReadEntriesAsync(memoryFile, cancellationToken);

        entries.Add(new AgentMemoryEntryDto
        {
            Id = entry.Id,
            Content = entry.Content,
            Tags = [.. entry.Tags],
            CreatedAt = entry.CreatedAt
        });

        await WriteEntriesAsync(memoryFile, entries, cancellationToken);
        _logger.LogInformation("Saved memory entry {EntryId} for agent '{AgentName}'", entry.Id, agentName);
    }

    public async Task<IReadOnlyList<AgentMemoryEntry>> SearchAsync(
        string agentName,
        string query,
        CancellationToken cancellationToken = default)
    {
        var memoryFile = await GetMemoryFilePathAsync(agentName, cancellationToken);
        var entries = await ReadEntriesAsync(memoryFile, cancellationToken);

        var lowerQuery = query.ToLowerInvariant();

        return entries
            .Where(e =>
                e.Content.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
                e.Tags.Any(t => t.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase)))
            .Select(MapToDomain)
            .ToList();
    }

    public async Task<IReadOnlyList<AgentMemoryEntry>> GetAllAsync(
        string agentName,
        CancellationToken cancellationToken = default)
    {
        var memoryFile = await GetMemoryFilePathAsync(agentName, cancellationToken);
        var entries = await ReadEntriesAsync(memoryFile, cancellationToken);
        return entries.Select(MapToDomain).ToList();
    }

    private async Task<string> GetMemoryFilePathAsync(string agentName, CancellationToken cancellationToken)
    {
        var config = await _configurationRepository.GetConfigurationAsync(cancellationToken);
        var agentFolder = Path.Combine(config.InstancesFolder, $"{agentName}-agent");
        return Path.Combine(agentFolder, "memory.json");
    }

    private async Task<List<AgentMemoryEntryDto>> ReadEntriesAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return [];

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<List<AgentMemoryEntryDto>>(json, JsonOptions) ?? [];
    }

    private async Task WriteEntriesAsync(string filePath, List<AgentMemoryEntryDto> entries, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(entries, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    private static AgentMemoryEntry MapToDomain(AgentMemoryEntryDto dto) =>
        new()
        {
            Id = dto.Id,
            Content = dto.Content,
            Tags = dto.Tags,
            CreatedAt = dto.CreatedAt
        };
}
