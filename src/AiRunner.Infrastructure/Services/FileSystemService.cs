using AiRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiRunner.Infrastructure.Services;

public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    public void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Created directory: {Path}", path);
        }
        else
        {
            _logger.LogDebug("Directory already exists: {Path}", path);
        }
    }

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}
