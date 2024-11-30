using System.ComponentModel.DataAnnotations;
using ChatApp.Server.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    [Required]
    public string Username { get; private set; }

    [Required]
    public string DisplayName { get; private set; }

    [Required]
    public string Password { get; private set; }

    // 导航属性
    public ICollection<Message> SentMessages { get; private set; }
    public ICollection<Message> ReceivedMessages { get; private set; }

    // 无参构造函数（EF Core 使用）
    private User()
    {
        SentMessages = new List<Message>();
        ReceivedMessages = new List<Message>();
    }

    public User(string username, string password) : this()
    {
        Id = Guid.NewGuid();
        Username = username ?? throw new ArgumentNullException(nameof(username));
        DisplayName = username; // 默认显示名为用户名，可以根据需求调整
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public void UpdateDisplayName(string newDisplayName) => DisplayName = newDisplayName ?? throw new ArgumentNullException(nameof(newDisplayName));

    public void UpdatePassword(string newPassword) => Password = newPassword ?? throw new ArgumentNullException(nameof(newPassword));
}