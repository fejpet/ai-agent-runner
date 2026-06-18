using AiRunner.Application.UseCases;
using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AiRunner.Tests;

public class SaveAgentMemoryUseCaseTests
{
    private readonly Mock<IAgentMemoryRepository> _memoryRepoMock = new();
    private readonly SaveAgentMemoryUseCase _sut;

    public SaveAgentMemoryUseCaseTests()
    {
        _sut = new SaveAgentMemoryUseCase(_memoryRepoMock.Object, NullLogger<SaveAgentMemoryUseCase>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_CallsSaveWithCorrectContent()
    {
        await _sut.ExecuteAsync("boss", "Remember the deployment steps");

        _memoryRepoMock.Verify(r => r.SaveAsync(
            "boss",
            It.Is<AgentMemoryEntry>(e => e.Content == "Remember the deployment steps"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsSaveWithTags_WhenProvided()
    {
        await _sut.ExecuteAsync("boss", "Important task", ["urgent", "infra"]);

        _memoryRepoMock.Verify(r => r.SaveAsync(
            "boss",
            It.Is<AgentMemoryEntry>(e =>
                e.Content == "Important task" &&
                e.Tags.Contains("urgent") &&
                e.Tags.Contains("infra")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsSaveWithEmptyTags_WhenNotProvided()
    {
        await _sut.ExecuteAsync("boss", "Simple memory");

        _memoryRepoMock.Verify(r => r.SaveAsync(
            "boss",
            It.Is<AgentMemoryEntry>(e => e.Tags.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class SearchAgentMemoryUseCaseTests
{
    private readonly Mock<IAgentMemoryRepository> _memoryRepoMock = new();
    private readonly SearchAgentMemoryUseCase _sut;

    public SearchAgentMemoryUseCaseTests()
    {
        _sut = new SearchAgentMemoryUseCase(_memoryRepoMock.Object, NullLogger<SearchAgentMemoryUseCase>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSearchResults()
    {
        var expected = new List<AgentMemoryEntry>
        {
            new() { Content = "Deploy to production" }
        };

        _memoryRepoMock
            .Setup(r => r.SearchAsync("boss", "production", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var results = await _sut.ExecuteAsync("boss", "production");

        Assert.Single(results);
        Assert.Equal("Deploy to production", results[0].Content);
    }

    [Fact]
    public async Task ExecuteAsync_CallsSearchWithCorrectParameters()
    {
        _memoryRepoMock
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.ExecuteAsync("boss", "deployment");

        _memoryRepoMock.Verify(r => r.SearchAsync("boss", "deployment", It.IsAny<CancellationToken>()), Times.Once);
    }
}
