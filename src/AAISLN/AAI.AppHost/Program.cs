using Projects;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<AAI_GenericChatInterface>("generic-chat-interface")
    .WithEndpoint(5009, scheme: "https");
builder.Build().Run();