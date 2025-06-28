using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Authentication.Azure;

namespace AAI.GraphConnector.Services;

public class GraphSchemaManager
{
    private readonly HttpClient graphHttpClient;
    private readonly GraphServiceClient graphClient;

    public GraphSchemaManager(string tenantId, string clientId, string clientSecret)
    {
        graphHttpClient = GraphClientFactory.Create();
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var authProvider = new AzureIdentityAuthenticationProvider(
            credential, scopes: ["https://graph.microsoft.com/.default"]);
        graphClient = new GraphServiceClient(graphHttpClient, authProvider);
    }

    public async Task<Schema?> GetSchemaAsync(string? connectionId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        Schema? schema = null;
        try
        {
            schema = await graphClient.External
                .Connections[connectionId]
                .Schema
                .GetAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting schema for connection {connectionId}: {e.Message}");
        }

        return schema;
    }

    public async Task<bool> RegisterSchemaAsync(string? connectionId, Schema schema)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        var requestInfo = graphClient.External
            .Connections[connectionId]
            .Schema
            .ToGetRequestInformation();

        requestInfo.SetContentFromParsable(graphClient.RequestAdapter, "application/json", schema);

        // Convert the SDK request to an HttpRequestMessage
        var requestMessage = await graphClient.RequestAdapter
            .ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        _ = requestMessage ?? throw new Exception("Could not create native HTTP request");
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("Prefer", "respond-async");

        // Send the request
        try
        {
            var responseMessage = await graphHttpClient.SendAsync(requestMessage) ??
                                  throw new Exception("No response returned from API");

            if (responseMessage.IsSuccessStatusCode)
            {
                // The operation ID is contained in the Location header returned
                // in the response
                var operationId = responseMessage.Headers.Location?.Segments.Last() ??
                                  throw new Exception("Could not get operation ID from Location header");
                await WaitForOperationToCompleteAsync(connectionId!, operationId);
            }
            else
            {
                throw new ServiceException("Registering schema failed",
                    responseMessage.Headers, (int)responseMessage.StatusCode);
            }
        }
        catch (ServiceException serviceException)
        {
            Console.WriteLine(
                $"Error registering schema: {serviceException.ResponseStatusCode} {serviceException.Message}");
            return false;
        }
        catch (ODataError odataError)
        {
            Console.WriteLine(
                $"Error registering schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
            return false;
        }

        return true;
    }

    public Schema GetDefaultSchema()
    {
        var schema = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties =
            [
                new Property
                {
                    Name = "machineId",
                    Type = PropertyType.String,
                    IsQueryable = true,
                    IsSearchable = false,
                    IsRetrievable = true,
                    IsRefinable = true
                },
                new Property
                {
                    Name = "name",
                    Type = PropertyType.String,
                    IsQueryable = true,
                    IsSearchable = true,
                    IsRetrievable = true,
                    IsRefinable = false,
                    Labels = [Label.Title]
                },
                new Property
                {
                    Name = "activated",
                    Type = PropertyType.DateTime,
                    IsQueryable = false,
                    IsSearchable = false,
                    IsRetrievable = true,
                    IsRefinable = false,
                    Labels = [Label.CreatedDateTime]
                },
                new Property
                {
                    Name = "restartCount",
                    Type = PropertyType.Int64,
                    IsQueryable = false,
                    IsRefinable = false,
                    IsSearchable = false,
                    IsRetrievable = true
                },
                new Property
                {
                    Name = "message",
                    Type = PropertyType.String,
                    IsQueryable = true,
                    IsSearchable = false,
                    IsRetrievable = true,
                    IsRefinable = true
                }
            ]
        };
        return schema;
    }

    private async Task WaitForOperationToCompleteAsync(string connectionId, string operationId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");

        do
        {
            var operation = await graphClient.External
                .Connections[connectionId]
                .Operations[operationId]
                .GetAsync();

            switch (operation?.Status)
            {
                case ConnectionOperationStatus.Completed:
                    return;
                case ConnectionOperationStatus.Failed:
                    Console.WriteLine($"Schema operation failed: {operation.Error?.Code} {operation.Error?.Message}");
                    throw new ServiceException(
                        $"Schema operation failed: {operation.Error?.Code} {operation.Error?.Message}");
                default:
                    // Wait 5 seconds and check again
                    await Task.Delay(5000);
                    break;
            }
        } while (true);
    }
}