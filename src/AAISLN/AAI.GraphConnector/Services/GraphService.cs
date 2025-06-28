using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Kiota.Authentication.Azure;

namespace AAI.GraphConnector.Services;

public class GraphService 
{
    private readonly GraphServiceClient graphClient;

    public GraphService(string tenantId, string clientId,string clientSecret)
    {
        var credential = new ClientSecretCredential(
            tenantId,clientId,clientSecret);
        var client = GraphClientFactory.Create();
        var authProvider = new AzureIdentityAuthenticationProvider(
            credential, scopes: ["https://graph.microsoft.com/.default"]);
        graphClient = new GraphServiceClient(client, authProvider);
    }

    public async Task<ExternalConnection?> CreateConnectionAsync(string id, string name, string? description)
    {
        _ = graphClient ?? throw new MemberAccessException("GraphClient is null, check data initialization");
        var connection = new ExternalConnection
        {
            Id = id,
            Name = name,
            Description = description
        };

        var createdConnection = await graphClient.External.Connections.PostAsync(connection);
        return createdConnection;
    }

    public async Task<bool> DeleteConnectionAsync(string connectionId)
    {
        _ = graphClient ?? throw new MemberAccessException("GraphClient is null, check data initialization");
        ArgumentException.ThrowIfNullOrEmpty(connectionId);
        await graphClient.External.Connections[connectionId].DeleteAsync();
        return true;
    }

    public async Task<bool> SaveConnectionAsync(ExternalConnection connection)
    {
        _ = graphClient ?? throw new MemberAccessException("GraphClient is null, check data initialization");
        ArgumentException.ThrowIfNullOrEmpty(connection.Id);

        var savedConnection = await graphClient.External.Connections[connection.Id].GetAsync();

        if (savedConnection == null)
        {
            Console.WriteLine("Connection not found");
            return false;
        }

        savedConnection.Name = connection.Name;
        savedConnection.Description = connection.Description;
        await graphClient.External.Connections[connection.Id].PatchAsync(savedConnection);
        return true;
    }

    public async Task<ExternalConnection?> GetConnectionAsync(string connectionId)
    {
        _ = graphClient ?? throw new MemberAccessException("GraphClient is null, check data initialization");
        ArgumentException.ThrowIfNullOrEmpty(connectionId);
        var connection = await graphClient.External.Connections[connectionId].GetAsync();
        return connection;
    }

    public async Task<List<ExternalConnection>> GetConnectionListAsync()
    {
        _ = graphClient ?? throw new MemberAccessException("GraphClient is null, check data initialization");
        var collectionsFromGraph = await graphClient.External.Connections.GetAsync();
        var connections = collectionsFromGraph?.Value ?? [];
        return connections;
    }
}