namespace AAI.Models;

public class AppSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool NotificationsEnabled { get; set; } = false;
}