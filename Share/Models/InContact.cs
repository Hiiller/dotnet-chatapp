namespace Shared.Models;

public class InContact
{
    public Guid _oppo_id { get; set; }
    public string _oppo_name { get; set; }
    
    public InContact()
    {
        _oppo_id = Guid.Empty;
        _oppo_name = string.Empty;
    }
    
}