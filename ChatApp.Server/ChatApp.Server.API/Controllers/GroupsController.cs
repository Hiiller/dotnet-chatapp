using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ChatApp.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GroupsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /api/groups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupResponse>>> GetAll()
        {
            var groups = await _db.Set<Group>().Select(g => new GroupResponse
            {
                Id = g.Id,
                Name = g.Name
            }).ToListAsync();

            return Ok(groups);
        }

        // GET: /api/groups/user/{userId}
        // Get groups that the user has participated in (sent messages to)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GroupResponse>>> GetGroupsByUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            // Find distinct groups where the user has sent messages
            var groupIds = await _db.Set<Message>()
                .Where(m => m.SenderId == userId && m.GroupId != null)
                .Select(m => m.GroupId!.Value)
                .Distinct()
                .ToListAsync();

            // Get group details
            var groups = await _db.Set<Group>()
                .Where(g => groupIds.Contains(g.Id))
                .Select(g => new GroupResponse
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(groups);
        }

        // POST: /api/groups
        [HttpPost]
        public async Task<ActionResult<GroupResponse>> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Group name is required.");
            }

            var group = new Group(request.Name);
            _db.Set<Group>().Add(group);
            await _db.SaveChangesAsync();

            return Ok(new GroupResponse
            {
                Id = group.Id,
                Name = group.Name
            });
        }

        public class GroupResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class CreateGroupRequest
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}