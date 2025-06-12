using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Info;

[AllowAnonymous]
public class IndexPageModel(ILogger<IndexPageModel> logger) : PageModel
{
    public void OnGet() =>
        logger.LogInformation("Info information page accessed at {DateLoaded}.", DateTime.Now);
}