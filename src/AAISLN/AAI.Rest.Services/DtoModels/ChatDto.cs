namespace AAI.Rest.Services.DtoModels;

public class ChatDto
{
    public required string Email { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public string ThreadName { get; set; } = string.Empty;
    public required string Text { get; set; }
}