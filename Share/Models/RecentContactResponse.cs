namespace Shared.Models;

public class RecentContactResponse
{
    public Guid UserId { get; set; }
    public Dictionary<Guid,string> NewMsgs { get; set; }
}