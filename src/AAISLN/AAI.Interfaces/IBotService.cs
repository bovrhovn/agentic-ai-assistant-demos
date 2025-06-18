using AAI.Models;

namespace AAI.Interfaces;

public interface IBotService
{
    Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId, string parentId="");
}