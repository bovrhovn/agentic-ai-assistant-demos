namespace AAI.GenericChatInterface.ViewModels;

public class ChatHistoryViewModel
{
    public required string ThreadName { get; set; }
    public int MessageCount { get; set; } = 0;
    public int UserMessageCount { get; set; } = 0;
    public int BotMessageCount { get; set; } = 0;
}