using System.Collections.ObjectModel;
using AAI.Interfaces;
using AAI.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace AAI.Data.Services;

internal record ChatModel(
    string id,
    string email,
    string threadName,
    string parentId,
    string text,
    int modelType,
    DateTime createdAt);

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

    public async Task<List<Chat>> GetForUserAsync(string userId)
    {
        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM chats p WHERE p.email = @email ORDER BY p.threadName"
        ).WithParameter("@email", userId);
        using var filteredFeed = container.GetItemQueryIterator<ChatModel>(
            queryDefinition: parameterizedQuery
        );
        // Iterate query result pages
        var list = new List<Chat>();
        while (filteredFeed.HasMoreResults)
        {
            var response = await filteredFeed.ReadNextAsync();
            // Iterate query results
            list.AddRange(response.Select(item => new Chat
            {
                UserId = item.email,
                ThreadName = item.threadName,
                ChatId = item.id,
                ParentChat = new Chat
                {
                    ChatId = item.parentId,
                    ThreadName = item.threadName,
                    UserId = item.email, Text = string.Empty
                },
                Text = item.text,
                ChatType = (ChatModelType)item.modelType,
                DatePosted = item.createdAt
            }));
        }

        return list;
    }

    public async Task<List<Chat>> GetForThreadAsync(string threadName)
    {
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM chats p WHERE p.threadName = @threadName ORDER BY p.createdAt ASC"
        ).WithParameter("@threadName", threadName);
        using var filteredFeed = container.GetItemQueryIterator<ChatModel>(
            queryDefinition: parameterizedQuery
        );
        // Iterate query result pages
        var list = new List<Chat>();
        while (filteredFeed.HasMoreResults)
        {
            var response = await filteredFeed.ReadNextAsync();
            // Iterate query results
            list.AddRange(response.Select(item => new Chat
            {
                UserId = item.email,
                ThreadName = item.threadName,
                ChatId = item.id,
                ParentChat = new Chat
                {
                    ChatId = item.parentId,
                    ThreadName = item.threadName,
                    UserId = item.email, Text = string.Empty
                },
                Text = item.text,
                ChatType = (ChatModelType)item.modelType,
                DatePosted = item.createdAt
            }));
        }

        return list;
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