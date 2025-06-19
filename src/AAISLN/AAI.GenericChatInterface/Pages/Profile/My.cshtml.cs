using AAI.Core;
using AAI.Interfaces;
using AAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAI.GenericChatInterface.Pages.Profile;

public class MyPageModel(ILogger<MyPageModel> logger, ISettingsService settingsService) : PageModel
{
    public async Task OnGetAsync()
    {
        logger.LogInformation("MyPageModel OnGet called at {Time}", DateTime.UtcNow);
        var id = User.Identity?.Name;
        MySettings = await settingsService.GetAsync(id ?? string.Empty);
        logger.LogInformation("MySettings loaded for user {UserId} at {Time}", id, DateTime.UtcNow);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            logger.LogInformation("MyPageModel OnPost called at {Time}", DateTime.UtcNow);
            await settingsService.UpdateAsync(MySettings);
            logger.LogInformation("Settings saved for user {UserId} at {Time}",
                User.Identity?.Name,
                DateTime.UtcNow);
            TempData["Message"] = "Settings saved successfully.";
        }
        catch (Exception e)
        {
            logger.LogError("Error saving settings for user {UserId} at {Time}: {Message}", User.Identity?.Name,
                DateTime.UtcNow, e.Message);
            TempData["Message"] = "Error saving settings: " + e.Message;
        }

        return RedirectToPage();
    }

    [BindProperty] public AppSettings MySettings { get; set; } = new();
    [BindProperty] public List<BotMode> BotModes { get; set; } = BotMode.GetAll();
}