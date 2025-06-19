namespace AAI.Models;

public class Chat
{
    public required string ChatId { get; set; } 
    public required string UserId { get; set; }
    public DateTime DatePosted { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public required string Text { get; set; }
    public ChatModelType ChatType { get; set; } = ChatModelType.User;
    public required string ThreadName { get; set; }
}

public enum ChatModelType
{
    User = 0,
    Assistant,
    System
}