namespace AAI.Models;

public class AppSettings
{
    public string AppSettingsId { get; set; } = Guid.NewGuid().ToString();
    public bool NotificationsEnabled { get; set; } = false;
}