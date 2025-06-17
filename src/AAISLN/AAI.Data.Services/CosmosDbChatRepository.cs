using System.Collections.ObjectModel;
using AAI.Interfaces;
using AAI.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AAI.Data.Services;

public class CosmosDbChatRepository : IChatRepository
{
    private readonly Container container;

    public CosmosDbChatRepository(string database, string containerName,
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
        container.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPaths = ["/email", "/threadName"]
            }).GetAwaiter().GetResult();
    }

    private record ChatModel(
        string id,
        string email,
        string threadName,
        string parentId,
        string text,
        int modelType,
        DateTime createdAt);

    public Task<List<Chat>> GetForUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Chat>> GetForThreadAsync(string threadName)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SaveAsync(Chat chat)
    {
        var model = new ChatModel(
            id: chat.ChatId,
            threadName: chat.ThreadName,
            email: chat.UserId,
            parentId: chat.ParentChat.ChatId,
            text: chat.Text,
            modelType: (int)chat.ChatType,
            createdAt: chat.DatePosted);

        var response =
            await container.UpsertItemAsync(item: model, partitionKey: new PartitionKey(chat.UserId));
        if (response.StatusCode != System.Net.HttpStatusCode.OK &&
            response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new Exception($"Failed to save chat: {response.StatusCode}");
        }

        return true;
    }
}