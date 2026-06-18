using AiRunner.Application.UseCases;
using AiRunner.Domain.Entities;
using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AiRunner.Tests;

public class InitializeInfrastructureUseCaseTests
{
    private readonly Mock<IConfigurationRepository> _configRepoMock = new();
    private readonly Mock<IFileSystemService> _fileSystemMock = new();
    private readonly Mock<IAgentMemoryRepository> _agentMemoryMock = new();
    private readonly Mock<ITemplateService> _templateServiceMock = new();
    private readonly InitializeInfrastructureUseCase _sut;

    private const string RootFolder = "/fake/root";
    private const string InstancesFolder = "/fake/root/instances";
    private const string TemplatesFolder = "/fake/root/templates";

    public InitializeInfrastructureUseCaseTests()
    {
        _templateServiceMock
            .Setup(t => t.Resolve(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns((string template, IReadOnlyDictionary<string, string>? _) => template);

        _sut = new InitializeInfrastructureUseCase(
            _configRepoMock.Object,
            _fileSystemMock.Object,
            _agentMemoryMock.Object,
            _templateServiceMock.Object,
            NullLogger<InitializeInfrastructureUseCase>.Instance);
    }

    private RunnerConfiguration BuildConfig(IReadOnlyList<Agent> agents) =>
        new()
        {
            RootFolder = RootFolder,
            OwnerName = "TestOwner",
            Commands = [],
            Agents = agents
        };

    // ── AGENT.md initialization ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CreatesAgentMd_WhenTemplateExistsAndAgentMdDoesNot()
    {
        var agent = new Agent { Name = "boss", Role = "boss" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");
        var templatePath = Path.Combine(TemplatesFolder, "boss-template.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(false);
        _fileSystemMock.Setup(f => f.FileExists(templatePath)).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllText(templatePath)).Returns("# Agent: {{BOT_NAME}}");

        _templateServiceMock
            .Setup(t => t.Resolve("# Agent: {{BOT_NAME}}", It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns("# Agent: boss");

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.WriteAllText(agentMdPath, "# Agent: boss"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsAgentMd_WhenAgentMdAlreadyExists()
    {
        var agent = new Agent { Name = "boss", Role = "boss" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(true);

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _fileSystemMock.Verify(f => f.WriteAllText(agentMdPath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsAgentMd_WhenTemplateFileDoesNotExist()
    {
        var agent = new Agent { Name = "boss", Role = "boss" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");
        var templatePath = Path.Combine(TemplatesFolder, "boss-template.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(false);
        _fileSystemMock.Setup(f => f.FileExists(templatePath)).Returns(false);

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.WriteAllText(agentMdPath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesTemplateWithAgentValues()
    {
        var agent = new Agent { Name = "worker", Role = "dev" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "worker-agent", "AGENT.md");
        var templatePath = Path.Combine(TemplatesFolder, "dev-template.md");
        const string templateContent = "Role: {{role}}, Bot: {{BOT_NAME}}";

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(false);
        _fileSystemMock.Setup(f => f.FileExists(templatePath)).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllText(templatePath)).Returns(templateContent);

        IReadOnlyDictionary<string, string>? capturedValues = null;
        _templateServiceMock
            .Setup(t => t.Resolve(templateContent, It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Callback((string _, IReadOnlyDictionary<string, string>? values) => capturedValues = values)
            .Returns("resolved");

        await _sut.ExecuteAsync();

        Assert.NotNull(capturedValues);
        Assert.Equal("worker", capturedValues["agent-name"]);
        Assert.Equal("worker", capturedValues["BOT_NAME"]);
        Assert.Equal("dev", capturedValues["role"]);
    }

    [Fact]
    public async Task ExecuteAsync_UsesRoleBasedTemplateFileName()
    {
        var agent = new Agent { Name = "boss", Role = "manager" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");

        // Wrong template name should NOT be used
        var wrongTemplatePath = Path.Combine(TemplatesFolder, "boss-template.md");
        var correctTemplatePath = Path.Combine(TemplatesFolder, "manager-template.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(false);
        _fileSystemMock.Setup(f => f.FileExists(wrongTemplatePath)).Returns(false);
        _fileSystemMock.Setup(f => f.FileExists(correctTemplatePath)).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllText(correctTemplatePath)).Returns("manager template");

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.ReadAllText(wrongTemplatePath), Times.Never);
        _fileSystemMock.Verify(f => f.ReadAllText(correctTemplatePath), Times.Once);
    }

    // ── AGENT.override.md initialization ──────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CreatesOverrideMd_WhenItDoesNotExist()
    {
        var agent = new Agent { Name = "boss", Role = "boss" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");
        var overridePath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.override.md");
        var templatePath = Path.Combine(TemplatesFolder, "boss-template.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(true); // skip AGENT.md
        _fileSystemMock.Setup(f => f.FileExists(overridePath)).Returns(false);

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.WriteAllText(overridePath, string.Empty), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsOverrideMd_WhenItAlreadyExists()
    {
        var agent = new Agent { Name = "boss", Role = "boss" };
        var config = BuildConfig([agent]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var agentMdPath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.md");
        var overridePath = Path.Combine(InstancesFolder, "boss-agent", "AGENT.override.md");

        _fileSystemMock.Setup(f => f.FileExists(agentMdPath)).Returns(true); // skip AGENT.md
        _fileSystemMock.Setup(f => f.FileExists(overridePath)).Returns(true);

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.WriteAllText(overridePath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesOverrideMd_ForEachAgent()
    {
        var agents = new List<Agent>
        {
            new() { Name = "boss", Role = "boss" },
            new() { Name = "worker", Role = "dev" }
        };
        var config = BuildConfig(agents);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var bossOverride = Path.Combine(InstancesFolder, "boss-agent", "AGENT.override.md");
        var workerOverride = Path.Combine(InstancesFolder, "worker-agent", "AGENT.override.md");

        // All FileExists calls return false (files don't exist yet)
        _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);
        // No templates found — that's fine, override creation still happens
        _fileSystemMock.Setup(f => f.FileExists(It.Is<string>(p => p.EndsWith("-template.md")))).Returns(false);

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.WriteAllText(bossOverride, string.Empty), Times.Once);
        _fileSystemMock.Verify(f => f.WriteAllText(workerOverride, string.Empty), Times.Once);
    }

    // ── Directory initialization (regression) ─────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CreatesRequiredDirectories()
    {
        var config = BuildConfig([new Agent { Name = "boss", Role = "boss" }]);

        _configRepoMock
            .Setup(r => r.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true); // skip file init

        await _sut.ExecuteAsync();

        _fileSystemMock.Verify(f => f.EnsureDirectoryExists(RootFolder), Times.Once);
        _fileSystemMock.Verify(f => f.EnsureDirectoryExists(InstancesFolder), Times.Once);
        _fileSystemMock.Verify(f => f.EnsureDirectoryExists(TemplatesFolder), Times.Once);
        _fileSystemMock.Verify(
            f => f.EnsureDirectoryExists(Path.Combine(InstancesFolder, "boss-agent")),
            Times.Once);
    }
}
