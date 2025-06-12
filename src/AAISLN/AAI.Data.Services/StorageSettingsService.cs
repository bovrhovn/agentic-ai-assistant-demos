using System.Diagnostics;
using System.Text;
using AAI.Interfaces;
using AAI.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Newtonsoft.Json;

namespace AAI.Data.Services;

public class StorageSettingsService : ISettingsService
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly string profileContainerName;

    public StorageSettingsService(string profileContainerName, string storageUrl)
    {
        this.profileContainerName = profileContainerName;
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

    public async Task<bool> UpdateAsync(AppSettings settings)
    {
        var currentSettings = await GetAsync(settings.Id);
        var containerClient = blobServiceClient.GetBlobContainerClient(profileContainerName);
        try
        {
            var blobClient = containerClient.GetBlobClient(settings.Id);
            var data = JsonConvert.SerializeObject(currentSettings);
            var bytes = Encoding.UTF8.GetBytes(data);
            using var ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            await blobClient.UploadAsync(ms, true);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }

        return true;
    }

    public async Task<AppSettings> GetAsync(string settingsId)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(profileContainerName);
        var settings = new AppSettings { Id = settingsId };
        if (!await containerClient.ExistsAsync())
            await blobServiceClient.CreateBlobContainerAsync(profileContainerName);
        var blobClient = containerClient.GetBlobClient(settingsId);
        if (!await blobClient.ExistsAsync()) return settings;
        var downloadedContent = await blobClient.DownloadContentAsync();
        if (!downloadedContent.HasValue) return settings;
        var downloadedProfile = Encoding.UTF8.GetString(downloadedContent.Value.Content);
        if (string.IsNullOrEmpty(downloadedProfile)) return settings;
        settings = JsonConvert.DeserializeObject<AppSettings>(downloadedProfile);
        return settings ?? new AppSettings
        {
            Id = settingsId
        };
    }
}