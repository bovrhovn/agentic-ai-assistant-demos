using AAI.Core;
using AAI.Interfaces;
using AAI.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AAI.Data.Services;

public class CosmosDbStorageSettingsService : ISettingsService
{
    private readonly Container container;

    public CosmosDbStorageSettingsService(string database, string containerName,
        string serviceUrl)
    {
        // Initialize Cosmos DB client and database connection here
        var defaultAzureCredential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = false,
                ExcludeVisualStudioCredential = true
            });
        CosmosClient cosmosClient = new(accountEndpoint: serviceUrl,
            tokenCredential: defaultAzureCredential);
        var settingDatabase = cosmosClient.GetDatabase(database);
        cosmosClient.CreateDatabaseIfNotExistsAsync(database)
            .GetAwaiter().GetResult();
        container = settingDatabase.GetContainer(containerName);
        container.Database.CreateContainerIfNotExistsAsync(containerName, "/email")
            .GetAwaiter().GetResult();
    }

    public async Task<bool> UpdateAsync(AppSettings settings)
    {
        var profileSettings = await GetProfileSettingsAsync(settings.Id);
        profileSettings = profileSettings with { notificationsEnabled = settings.NotificationsEnabled };
        var response =
            await container.UpsertItemAsync(item: profileSettings, partitionKey: new PartitionKey(settings.Id));
        if (response.StatusCode != System.Net.HttpStatusCode.OK &&
            response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new Exception($"Failed to update settings: {response.StatusCode}");
        }

        return true;
    }

    private async Task<ProfileSettings> GetProfileSettingsAsync(string settingsId)
    {
        var id = settingsId.GetUniqueValue();
        try
        {
            var result =
                await container.ReadItemAsync<ProfileSettings>(id: id,
                    partitionKey: new PartitionKey(settingsId));
            return result.Resource;
        }
        catch (Exception e)
        {
            if (e is CosmosException { StatusCode: System.Net.HttpStatusCode.NotFound })
                return new ProfileSettings(id, settingsId, false); // Item not found

            throw new Exception($"Failed to retrieve settings: {e.Message}");
        }
    }

    public async Task<AppSettings> GetAsync(string settingsId)
    {
        var profileSettings = await GetProfileSettingsAsync(settingsId);
        return new AppSettings
        {
            Id = profileSettings.email,
            NotificationsEnabled = profileSettings.notificationsEnabled
        };
    }

    private record ProfileSettings(string id, string email, bool notificationsEnabled);
}