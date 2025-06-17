using AAI.GenericChatInterface.Services;
using AAI.GenericChatInterface.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Profile;

public class ChatHistoryPageModel(ILogger<ChatHistoryPageModel> logger, ChatHttpService chatHttpService) : PageModel
{
    public async Task OnGetAsync()
    {
        var userEmail = User?.Identity?.Name;
        logger.LogInformation("ChatHistory page accessed by user {Email}.", userEmail);
        ChatHistoryList = await chatHttpService.GetHistoryAsync(userEmail);
        logger.LogInformation("Retrieved {Count} chat history items for user {Email}.", 
            ChatHistoryList.Count, userEmail);
    }

    public List<ChatHistoryViewModel> ChatHistoryList { get; set; } = new();
}