namespace Shared.Models;

public class Friend
{
    public Guid friendId { get; set; }
    
    public string? friendName { get; set; }
    
    public Friend()
    {
        friendId = Guid.Empty;
        friendName = string.Empty;
    }
}