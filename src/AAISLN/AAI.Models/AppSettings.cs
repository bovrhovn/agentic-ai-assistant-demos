﻿using AAI.Core;

namespace AAI.Models;

public class AppSettings
{
    public string AppSettingsId { get; set; } = Guid.NewGuid().ToString();
    public bool NotificationsEnabled { get; set; } = false;
    public BotMode BotMode { get; set; } = BotMode.Default;
}