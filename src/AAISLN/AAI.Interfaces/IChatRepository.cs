using AAI.Models;

namespace AAI.Interfaces;

public interface IChatRepository
{
    Task<List<Chat>> GetForUserAsync(string userId);
    Task<List<Chat>> GetForThreadAsync(string threadName);
    Task<bool> SaveAsync(Chat chat);
}