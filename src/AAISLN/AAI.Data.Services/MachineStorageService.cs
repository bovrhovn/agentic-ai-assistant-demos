using System.Text;
using AAI.Interfaces;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace AAI.Data.Services;

public class MachineStorageService : IStorageService
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly string containerName;

    public MachineStorageService(string containerName, string storageUrl)
    {
        this.containerName = containerName;
        var defaultAzureCredential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = false,
                ExcludeEnvironmentCredential = false,
                ExcludeManagedIdentityCredential = true,
                ExcludeVisualStudioCredential = true
            });
        blobServiceClient = new BlobServiceClient(new Uri(storageUrl),
            defaultAzureCredential);
    }

    public async Task<string> GetFileContentAsync(string fileName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!await containerClient.ExistsAsync())
            ArgumentException.ThrowIfNullOrEmpty("Container does not exist.");
        var blobClient = containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync())
            ArgumentException.ThrowIfNullOrEmpty("Container does not exist.");
        var downloadedContent = await blobClient.DownloadContentAsync();
        var machineLogInfo = Encoding.UTF8.GetString(downloadedContent.Value.Content);
        if (string.IsNullOrEmpty(machineLogInfo))
            ArgumentException.ThrowIfNullOrEmpty("File content is empty.");
        return machineLogInfo;
    }

    public async Task<Stream> GetFileContentRawAsync(string fileName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (!await containerClient.ExistsAsync())
            ArgumentException.ThrowIfNullOrEmpty("Container does not exist.");
        var blobClient = containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync())
            ArgumentException.ThrowIfNullOrEmpty("Container does not exist.");
        var downloadedContent = await blobClient.DownloadContentAsync();
        var machineLogInfo = downloadedContent.Value.Content.ToStream();
        if (machineLogInfo == null || machineLogInfo.Length == 0)
            ArgumentException.ThrowIfNullOrEmpty("File content is empty.");

        machineLogInfo!.Position = 0; // Reset stream position to the beginning
        return machineLogInfo;
    }
}