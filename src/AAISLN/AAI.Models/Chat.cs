namespace AAI.Models;

public class Chat
{
    public string ChatId { get; set; } = Guid.NewGuid().ToString();
    public required string UserId { get; set; }
    public DateTime DatePosted { get; set; } = DateTime.Now;
    public Chat ParentChat { get; set; } = new()
    {
        UserId = string.Empty, 
        ChatId = string.Empty, 
        Text = string.Empty,
        ThreadName = string.Empty
    };
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