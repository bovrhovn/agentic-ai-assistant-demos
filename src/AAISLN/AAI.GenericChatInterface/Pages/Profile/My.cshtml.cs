using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Profile;

public class MyPageModel(ILogger<MyPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("MyPageModel OnGet called at {Time}", DateTime.UtcNow);
}