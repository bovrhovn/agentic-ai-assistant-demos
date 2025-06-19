using AAI.Interfaces;
using AAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Chat;

public class IndexPageModel(ILogger<IndexPageModel> logger, ISettingsService settingsService) : PageModel
{
    public async Task OnGetAsync()
    {
        logger.LogInformation("Chat Index page called at {Time}.", DateTime.UtcNow);
        var email = User.Identity?.Name!;
        UserSettings = await settingsService.GetAsync(email);
        logger.LogInformation("Settings loaded at {Time} with bot mode {BotMode}.", DateTime.UtcNow,
            UserSettings.BotMode.Name);
    }

    [BindProperty] public required AppSettings UserSettings { get; set; }
}