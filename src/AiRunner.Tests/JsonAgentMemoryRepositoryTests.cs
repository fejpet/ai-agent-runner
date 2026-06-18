using System.Text.Json;
using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using AiRunner.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AiRunner.Tests;

public class JsonAgentMemoryRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _agentFolder;
    private readonly Mock<IConfigurationRepository> _configRepoMock;
    private readonly JsonAgentMemoryRepository _sut;

    public JsonAgentMemoryRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        var instancesFolder = Path.Combine(_tempDir, "instances");
        _agentFolder = Path.Combine(instancesFolder, "boss-agent");
        Directory.CreateDirectory(_agentFolder);

        var config = new RunnerConfiguration
        {
            RootFolder = _tempDir,
            OwnerName = "TestOwner",
            Commands = [],
            Agents = []
        };

        _configRepoMock = new Mock<IConfigurationRepository>();
        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _sut = new JsonAgentMemoryRepository(_configRepoMock.Object, NullLogger<JsonAgentMemoryRepository>.Instance);
    }

    [Fact]
    public async Task InitializeAsync_CreatesMemoryFile_WhenNotExists()
    {
        await _sut.InitializeAsync("boss");

        var memoryFile = Path.Combine(_agentFolder, "memory.json");
        Assert.True(File.Exists(memoryFile));

        var content = await File.ReadAllTextAsync(memoryFile);
        Assert.Equal("[]", content);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotOverwrite_WhenFileExists()
    {
        var memoryFile = Path.Combine(_agentFolder, "memory.json");
        var existingContent = "[{\"existing\":true}]";
        await File.WriteAllTextAsync(memoryFile, existingContent);

        await _sut.InitializeAsync("boss");

        var content = await File.ReadAllTextAsync(memoryFile);
        Assert.Equal(existingContent, content);
    }

    [Fact]
    public async Task SaveAsync_PersistsEntry_ToMemoryFile()
    {
        await _sut.InitializeAsync("boss");

        var entry = new AgentMemoryEntry
        {
            Content = "Remember this important thing",
            Tags = ["important", "task"]
        };

        await _sut.SaveAsync("boss", entry);

        var entries = await _sut.GetAllAsync("boss");
        Assert.Single(entries);
        Assert.Equal(entry.Content, entries[0].Content);
        Assert.Equal(entry.Id, entries[0].Id);
    }

    [Fact]
    public async Task SaveAsync_AppendsEntries_WithMultipleSaves()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "First memory" });
        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Second memory" });

        var entries = await _sut.GetAllAsync("boss");
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingEntries_ByContent()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Deploy to production server" });
        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Review pull request" });

        var results = await _sut.SearchAsync("boss", "production");

        Assert.Single(results);
        Assert.Contains("production", results[0].Content);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingEntries_ByTag()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Some task", Tags = ["deployment", "critical"] });
        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Other task", Tags = ["review"] });

        var results = await _sut.SearchAsync("boss", "deployment");

        Assert.Single(results);
        Assert.Equal("Some task", results[0].Content);
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "UPPERCASE memory content" });

        var results = await _sut.SearchAsync("boss", "uppercase");

        Assert.Single(results);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenNoMatch()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Some content" });

        var results = await _sut.SearchAsync("boss", "nonexistent");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntries()
    {
        await _sut.InitializeAsync("boss");

        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Memory 1" });
        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Memory 2" });
        await _sut.SaveAsync("boss", new AgentMemoryEntry { Content = "Memory 3" });

        var results = await _sut.GetAllAsync("boss");

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoEntries()
    {
        await _sut.InitializeAsync("boss");

        var results = await _sut.GetAllAsync("boss");

        Assert.Empty(results);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
