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
        // Get groups that the user is a member of
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GroupResponse>>> GetGroupsByUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            // Get groups through GroupMember table
            var groups = await _db.Set<GroupMember>()
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Select(gm => new GroupResponse
                {
                    Id = gm.Group.Id,
                    Name = gm.Group.Name,
                    GroupCode = gm.Group.GroupCode,
                    CreatorId = gm.Group.CreatorId,
                    CreatedAt = gm.Group.CreatedAt,
                    MemberCount = _db.Set<GroupMember>().Count(m => m.GroupId == gm.Group.Id)
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
            
            if (request.CreatorId == Guid.Empty)
            {
                return BadRequest("Creator ID is required.");
            }

            var group = new Group(request.Name, request.CreatorId);
            _db.Set<Group>().Add(group);
            
            // Add creator as admin member
            var creatorMember = new GroupMember(group.Id, request.CreatorId, GroupMemberRole.Creator);
            _db.Set<GroupMember>().Add(creatorMember);
            
            await _db.SaveChangesAsync();

            return Ok(new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                CreatorId = group.CreatorId,
                CreatedAt = group.CreatedAt,
                MemberCount = 1
            });
        }
        
        // GET: /api/groups/search?code={groupCode}
        [HttpGet("search")]
        public async Task<ActionResult<GroupResponse>> SearchGroupByCode([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Group code is required.");
            }
            
            var group = await _db.Set<Group>()
                .FirstOrDefaultAsync(g => g.GroupCode == code.ToUpper());
            
            if (group == null)
            {
                return NotFound("Group not found.");
            }
            
            var memberCount = await _db.Set<GroupMember>()
                .Where(gm => gm.GroupId == group.Id)
                .CountAsync();
            
            return Ok(new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                CreatorId = group.CreatorId,
                CreatedAt = group.CreatedAt,
                MemberCount = memberCount
            });
        }

        public class GroupResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string GroupCode { get; set; } = string.Empty;
            public Guid CreatorId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int MemberCount { get; set; }
        }

        public class CreateGroupRequest
        {
            public string Name { get; set; } = string.Empty;
            public Guid CreatorId { get; set; }
        }
    }
}