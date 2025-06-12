using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Profile;

public class LogoutPageModel(ILogger<LogoutPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("User requested logout at {Time}.", DateTime.UtcNow);

    public IActionResult OnPostAsync()
    {
        logger.LogInformation("User requested logout at {Time}.", DateTime.UtcNow);
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/Info/Index"
        }, "OpenIdConnect","Cookies");
    }
}