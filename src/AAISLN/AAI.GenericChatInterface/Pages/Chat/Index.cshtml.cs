using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Chat;

public class IndexPageModel(ILogger<IndexPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("Chat Index page called at {Time}.", DateTime.UtcNow);
}