using AiRunner.Application.UseCases;
using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AiRunner.Tests;

public class StartAgentsUseCaseTests
{
    private readonly Mock<IConfigurationRepository> _configRepoMock = new();
    private readonly Mock<IProcessService> _processServiceMock = new();
    private readonly Mock<ITemplateService> _templateServiceMock = new();
    private readonly StartAgentsUseCase _sut;

    public StartAgentsUseCaseTests()
    {
        _templateServiceMock
            .Setup(t => t.Resolve(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns((string template, IReadOnlyDictionary<string, string>? _) => template);

        _sut = new StartAgentsUseCase(
            _configRepoMock.Object,
            _processServiceMock.Object,
            _templateServiceMock.Object,
            NullLogger<StartAgentsUseCase>.Instance);
    }

    private RunnerConfiguration BuildConfig(
        IReadOnlyList<RunnerCommand> commands,
        IReadOnlyList<Agent> agents)
    {
        return new RunnerConfiguration
        {
            RootFolder = "/fake/root",
            OwnerName = "TestOwner",
            Commands = commands,
            Agents = agents
        };
    }

    [Fact]
    public async Task ExecuteAsync_StartsAgent_WhenNoHasSessionCommandConfigured()
    {
        var config = BuildConfig(
            commands: [new RunnerCommand { Name = "start", Command = "tmux new -d -s boss-agent" }],
            agents: [new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        await _sut.ExecuteAsync();

        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _processServiceMock.Verify(
            p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsStart_WhenHasSessionReturnsExitCodeZero()
    {
        var config = BuildConfig(
            commands:
            [
                new RunnerCommand { Name = "start", Command = "tmux new -d -s boss-agent" },
                new RunnerCommand { Name = "has-session", Command = "tmux has-session -t boss-agent" }
            ],
            agents: [new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // session exists

        await _sut.ExecuteAsync();

        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_StartsAgent_WhenHasSessionReturnsNonZeroExitCode()
    {
        var config = BuildConfig(
            commands:
            [
                new RunnerCommand { Name = "start", Command = "tmux new -d -s boss-agent" },
                new RunnerCommand { Name = "has-session", Command = "tmux has-session -t boss-agent" }
            ],
            agents: [new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // session does not exist

        await _sut.ExecuteAsync();

        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ChecksHasSession_BeforeStarting()
    {
        var config = BuildConfig(
            commands:
            [
                new RunnerCommand { Name = "start", Command = "start-cmd" },
                new RunnerCommand { Name = "has-session", Command = "has-session-cmd" }
            ],
            agents: [new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var callOrder = new List<string>();

        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), "has-session-cmd", It.IsAny<CancellationToken>()))
            .Callback((string _, string cmd, CancellationToken __) => callOrder.Add("has-session"))
            .ReturnsAsync(1);

        _processServiceMock
            .Setup(p => p.StartAgentProcessAsync(It.IsAny<string>(), "start-cmd", It.IsAny<CancellationToken>()))
            .Callback((string _, string cmd, CancellationToken __) => callOrder.Add("start"))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync();

        Assert.Equal(["has-session", "start"], callOrder);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleAgents_StartsOnlyThoseWithoutExistingSession()
    {
        var config = BuildConfig(
            commands:
            [
                new RunnerCommand { Name = "start", Command = "start {{agent-name}}" },
                new RunnerCommand { Name = "has-session", Command = "has-session {{agent-name}}" }
            ],
            agents:
            [
                new Agent { Name = "boss", Role = "boss" },
                new Agent { Name = "worker", Role = "worker" }
            ]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Simulate template resolution returning the template unchanged (keys pass through)
        _templateServiceMock
            .Setup(t => t.Resolve("has-session {{agent-name}}", It.Is<IReadOnlyDictionary<string, string>>(d => d["agent-name"] == "boss")))
            .Returns("has-session boss");

        _templateServiceMock
            .Setup(t => t.Resolve("has-session {{agent-name}}", It.Is<IReadOnlyDictionary<string, string>>(d => d["agent-name"] == "worker")))
            .Returns("has-session worker");

        _templateServiceMock
            .Setup(t => t.Resolve("start {{agent-name}}", It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns((string _, IReadOnlyDictionary<string, string> d) => $"start {d["agent-name"]}");

        // boss session already exists (exit code 0), worker does not (exit code 1)
        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), "has-session boss", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _processServiceMock
            .Setup(p => p.RunCommandAndGetExitCodeAsync(It.IsAny<string>(), "has-session worker", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sut.ExecuteAsync();

        // Only worker should be started
        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), "start worker", It.IsAny<CancellationToken>()),
            Times.Once);

        _processServiceMock.Verify(
            p => p.StartAgentProcessAsync(It.IsAny<string>(), "start boss", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenNoStartCommandConfigured()
    {
        var config = BuildConfig(
            commands: [],
            agents: [new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync());
    }
}
