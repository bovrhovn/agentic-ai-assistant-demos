using AAI.Core;
using AAI.GenericChatInterface.Options;
using AAI.GenericChatInterface.ViewModels;
using AAI.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AAI.GenericChatInterface.Services;

public class ChatHttpService(
    ILogger<ChatHttpService> logger,
    HttpClient client,
    IOptions<GeneralOptions> generalOptions)
{
    public async Task<List<ChatHistoryViewModel>> GetHistoryAsync(string userId)
    {
        logger.LogInformation("Called get history endpoint at {DateCalled}", DateTime.UtcNow);
        var url = $"{generalOptions.Value.ApiBaseUrl}{GeneralRoutes.ChatRoute}/{DataRoutes.GetHistoryRoute}/{userId}";
        var response = await client.GetAsync(url);
        var list = new List<ChatHistoryViewModel>();
        if (response.IsSuccessStatusCode)
        {
            var history = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Retrieved chat history for user {UserId} with data {Data}", userId, history);
            // Process the history as needed
            var chats = JsonConvert.DeserializeObject<List<Chat>>(history);
            list = (chats ?? []).GroupBy(current => current.ThreadName)
                .Select(group =>
                    new ChatHistoryViewModel
                    {
                        ThreadName = group.Key,
                        MessageCount = group.Count(),
                        BotMessageCount = group.Count(msg =>
                            msg.ChatType is ChatModelType.System or ChatModelType.Assistant),
                        UserMessageCount = group.Count(msg =>
                            msg.ChatType is ChatModelType.User)
                    }
                ).ToList();
        }

        logger.LogInformation("Found {Results} for user {UserId} at {DateCalled}", list.Count, userId, DateTime.UtcNow);
        return list;
    }
}