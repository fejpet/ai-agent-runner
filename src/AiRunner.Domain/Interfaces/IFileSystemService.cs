namespace AiRunner.Domain.Interfaces;

public interface IFileSystemService
{
    void EnsureDirectoryExists(string path);
    bool DirectoryExists(string path);
}
