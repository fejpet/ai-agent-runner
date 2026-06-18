namespace AiRunner.Domain.Interfaces;

public interface IFileSystemService
{
    void EnsureDirectoryExists(string path);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
}
