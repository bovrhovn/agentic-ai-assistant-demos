namespace AAI.Interfaces;

public interface IStorageService
{
    Task<string> GetFileContentAsync(string fileName);
    Task<Stream> GetFileContentRawAsync(string fileName);
}