using Projects;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<AAI_Rest_Services>("rest-api-copilot-services")
    .WithEndpoint(5010, scheme: "https");
builder.AddProject<AAI_GenericChatInterface>("generic-chat-interface")
    .WithEndpoint(5009, scheme: "https");
builder.Build().Run();