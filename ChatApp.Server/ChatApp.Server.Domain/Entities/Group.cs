using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChatApp.Server.Domain.Entities
{
    public class Group
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        // 导航属性
        public ICollection<UserGroup> UserGroups { get; private set; }
        public ICollection<Message> Messages { get; private set; }

        public IEnumerable<User> Members => UserGroups.Select(ug => ug.User);

        public Group(string name)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UserGroups = new List<UserGroup>();
            Messages = new List<Message>();
        }

        public void UpdateName(string newName)
        {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
        }
    }
}
