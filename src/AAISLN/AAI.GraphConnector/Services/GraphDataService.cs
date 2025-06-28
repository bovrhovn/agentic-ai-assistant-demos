using AAI.GraphConnector.Data;
using Azure.Identity;
using Bogus;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Authentication.Azure;

namespace AAI.GraphConnector.Services;

public class GraphDataService
{
    private readonly string tenantId;
    private readonly GraphServiceClient graphClient;

    public GraphDataService(string tenantId, string clientId, string clientSecret)
    {
        this.tenantId = tenantId;
        var httpClient = GraphClientFactory.Create();
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var authProvider = new AzureIdentityAuthenticationProvider(credential,
            scopes: ["https://graph.microsoft.com/.default"]);
        graphClient = new GraphServiceClient(httpClient, authProvider);
    }

    public async Task<bool> AddOrUpdateItemAsync(string connectionId, CompanyMachineInfo item)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is null");
        var newItem = new ExternalItem
        {
            Id = item.MachineId,
            Content = new ExternalItemContent
            {
                Type = ExternalItemContentType.Text,
                Value = item.Title
            },
            Acl =
            [
                new Acl
                {
                    AccessType = AccessType.Grant,
                    Type = AclType.Everyone,
                    Value = tenantId
                }
            ],
            Properties = item.AsBasicExternalItemProperties()
        };

        try
        {
            await graphClient.External
                .Connections[connectionId]
                .Items[newItem.Id]
                .PutAsync(newItem);
        }
        catch (ODataError error)
        {
            Console.WriteLine(error.Message);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteItemAsync(string? connectionId, string? itemId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is null");
        _ = itemId ?? throw new ArgumentException("itemId is null");

        try
        {
            await graphClient.External
                .Connections[connectionId]
                .Items[itemId]
                .DeleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> PopulateWithRandomDataAsync(string connectionId, int numberOfItems = 100)
    {
        var faker = new Faker<CompanyMachineInfo>()
            .RuleFor(mc => mc.MachineId, (f, _) => f.Random.Long(1, 300000).ToString())
            .RuleFor(mc => mc.Title, (f, _) => f.Commerce.ProductName())
            .RuleFor(mc => mc.Activated, (f, _) => f.Date.Past(f.Random.Int(1, 28)))
            .RuleFor(mc => mc.RestartCount, (f, _) => f.Random.Long(0,99000))
            .RuleFor(mc => mc.Message, (f, _) => f.Lorem.Sentence(5, 10))
            .Generate(numberOfItems);
        foreach (var item in faker)
        {
            var hasBeenAdded = await AddOrUpdateItemAsync(connectionId, item);
            if (hasBeenAdded)
                Console.WriteLine("Item added successfully: " + item.MachineId);
            else
                Console.WriteLine($"Failed to add item {item.MachineId}");
        }
        return true;
    }
}