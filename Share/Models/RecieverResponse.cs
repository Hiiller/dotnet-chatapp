namespace Shared.Models;

public class RecentContactResponse
{
    public Guid UserId { get; set; }
    public Dictionary<Guid,string> Contacts { get; set; }
    
    public RecentContactResponse()
    {
        Contacts = new Dictionary<Guid, string>();
    }
}