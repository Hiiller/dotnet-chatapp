using System;
using System.Collections.Generic;
using System.Linq;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var groups = await _db.Set<Group>()
                .Select(g => new
                {
                    Group = g,
                    MemberCount = _db.Set<GroupMember>().Count(m => m.GroupId == g.Id)
                })
                .ToListAsync();

            var responses = groups.Select(g => ToResponse(g.Group, g.MemberCount));
            return Ok(responses);
        }

        // GET: /api/groups/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GroupResponse>>> GetGroupsByUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var memberships = await _db.Set<GroupMember>()
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .ToListAsync();

            var groupIds = memberships.Select(gm => gm.GroupId).Distinct().ToList();
            var memberCounts = await _db.Set<GroupMember>()
                .Where(m => groupIds.Contains(m.GroupId))
                .GroupBy(m => m.GroupId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            var responses = memberships
                .Select(gm =>
                {
                    var count = memberCounts.TryGetValue(gm.GroupId, out var value) ? value : 0;
                    return ToResponse(gm.Group, count, gm);
                })
                .ToList();

            return Ok(responses);
        }

        // GET: /api/groups/{groupId}/details?userId={userId}
        [HttpGet("{groupId}/details")]
        public async Task<ActionResult<GroupDetailResponse>> GetGroupDetails(Guid groupId, [FromQuery] Guid userId)
        {
            var group = await _db.Set<Group>()
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound("Group not found.");
            }

            GroupMember? membership = null;
            if (userId != Guid.Empty)
            {
                membership = await _db.Set<GroupMember>()
                    .Include(gm => gm.User)
                    .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            }

            var members = await _db.Set<GroupMember>()
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .OrderByDescending(gm => gm.Role)
                .ThenBy(gm => gm.JoinedAt)
                .ToListAsync();

            var memberResponses = members.Select(ToMemberResponse).ToList();
            var hasPendingRequest = userId != Guid.Empty && membership == null &&
                await _db.Set<GroupRequest>()
                    .AnyAsync(gr => gr.GroupId == groupId &&
                                    gr.RequesterId == userId &&
                                    gr.Status == GroupRequestStatus.Pending);

            var baseResponse = ToResponse(group, memberResponses.Count, membership, hasPendingRequest);
            var detailResponse = new GroupDetailResponse(baseResponse)
            {
                CreatorName = group.Creator?.DisplayName ?? group.Creator?.Username ?? string.Empty,
                Description = string.Empty,
                CurrentUserRole = membership?.Role.ToString(),
                CanManageMembers = IsManager(membership),
                Members = memberResponses
            };

            if (detailResponse.CanManageMembers)
            {
                var pendingRequests = await _db.Set<GroupRequest>()
                    .Where(gr => gr.GroupId == groupId && gr.Status == GroupRequestStatus.Pending)
                    .Include(gr => gr.Requester)
                    .OrderBy(gr => gr.CreatedAt)
                    .ToListAsync();

                detailResponse.PendingRequests = pendingRequests
                    .Select(ToRequestResponse)
                    .ToList();
            }

            return Ok(detailResponse);
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

            var creator = await _db.Set<User>().FindAsync(request.CreatorId);
            if (creator == null)
            {
                return BadRequest("Creator not found.");
            }

            var group = new Group(request.Name, request.CreatorId);
            _db.Set<Group>().Add(group);

            var creatorMember = new GroupMember(group.Id, request.CreatorId, GroupMemberRole.Creator);
            _db.Set<GroupMember>().Add(creatorMember);

            await _db.SaveChangesAsync();

            return Ok(ToResponse(group, 1, creatorMember));
        }

        // POST: /api/groups/{groupId}/requests
        [HttpPost("{groupId}/requests")]
        public async Task<ActionResult<GroupRequestResponse>> RequestToJoin(Guid groupId, [FromBody] GroupJoinRequest request)
        {
            if (request.RequesterId == Guid.Empty)
            {
                return BadRequest("Requester ID is required.");
            }

            var group = await _db.Set<Group>().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var requester = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == request.RequesterId);
            if (requester == null)
            {
                return NotFound("Requester not found.");
            }

            var existingMember = await _db.Set<GroupMember>()
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == request.RequesterId);
            if (existingMember)
            {
                return Conflict("You are already a member of this group.");
            }

            var existingRequest = await _db.Set<GroupRequest>()
                .FirstOrDefaultAsync(gr => gr.GroupId == groupId &&
                                           gr.RequesterId == request.RequesterId &&
                                           gr.Status == GroupRequestStatus.Pending);
            if (existingRequest != null)
            {
                return Conflict("A pending request already exists.");
            }

            var joinRequest = new GroupRequest(request.RequesterId, groupId);
            _db.Set<GroupRequest>().Add(joinRequest);
            await _db.SaveChangesAsync();

            var loadedRequest = await _db.Set<GroupRequest>()
                .Include(gr => gr.Requester)
                .FirstAsync(gr => gr.Id == joinRequest.Id);

            return Ok(ToRequestResponse(loadedRequest));
        }

        // GET: /api/groups/{groupId}/requests?requesterId={userId}
        [HttpGet("{groupId}/requests")]
        public async Task<ActionResult<IEnumerable<GroupRequestResponse>>> GetPendingRequests(Guid groupId, [FromQuery] Guid requesterId)
        {
            if (requesterId == Guid.Empty)
            {
                return BadRequest("Requester ID is required.");
            }

            var membership = await _db.Set<GroupMember>()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == requesterId);

            if (!IsManager(membership))
            {
                return Forbid();
            }

            var requests = await _db.Set<GroupRequest>()
                .Where(gr => gr.GroupId == groupId && gr.Status == GroupRequestStatus.Pending)
                .Include(gr => gr.Requester)
                .OrderBy(gr => gr.CreatedAt)
                .ToListAsync();

            return Ok(requests.Select(ToRequestResponse));
        }

        // POST: /api/groups/{groupId}/requests/{requestId}/respond
        [HttpPost("{groupId}/requests/{requestId}/respond")]
        public async Task<IActionResult> RespondToRequest(Guid groupId, Guid requestId, [FromBody] RespondGroupRequest dto)
        {
            if (dto.ApproverId == Guid.Empty)
            {
                return BadRequest("Approver ID is required.");
            }

            var request = await _db.Set<GroupRequest>()
                .Include(gr => gr.Requester)
                .FirstOrDefaultAsync(gr => gr.Id == requestId && gr.GroupId == groupId);

            if (request == null)
            {
                return NotFound("Request not found.");
            }

            if (request.Status != GroupRequestStatus.Pending)
            {
                return BadRequest("Request has already been processed.");
            }

            var membership = await _db.Set<GroupMember>()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == dto.ApproverId);

            if (!IsManager(membership))
            {
                return Forbid();
            }

            if (dto.Accept)
            {
                request.Accept();

                var alreadyMember = await _db.Set<GroupMember>()
                    .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == request.RequesterId);
                if (!alreadyMember)
                {
                    var newMember = new GroupMember(groupId, request.RequesterId, GroupMemberRole.Member);
                    _db.Set<GroupMember>().Add(newMember);
                }
            }
            else
            {
                request.Reject();
            }

            await _db.SaveChangesAsync();
            return Ok(ToRequestResponse(request));
        }

        // DELETE: /api/groups/{groupId}/members/{memberId}?requesterId={userId}
        [HttpDelete("{groupId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid groupId, Guid memberId, [FromQuery] Guid requesterId)
        {
            if (requesterId == Guid.Empty)
            {
                return BadRequest("Requester ID is required.");
            }

            var requesterMembership = await _db.Set<GroupMember>()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == requesterId);

            if (requesterMembership == null)
            {
                return BadRequest("Requester is not a member of this group.");
            }

            var isSelfRequest = memberId == requesterId;
            if (!isSelfRequest && !IsManager(requesterMembership))
            {
                return Forbid();
            }

            var targetMembership = await _db.Set<GroupMember>()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (targetMembership == null)
            {
                return NotFound("Member not found.");
            }

            if (targetMembership.Role == GroupMemberRole.Creator)
            {
                return BadRequest("The group creator cannot be removed.");
            }

            if (!isSelfRequest &&
                targetMembership.Role == GroupMemberRole.Admin &&
                requesterMembership?.Role != GroupMemberRole.Creator)
            {
                return Forbid();
            }

            if (isSelfRequest && targetMembership.Role == GroupMemberRole.Creator)
            {
                return BadRequest("The group creator cannot leave the group.");
            }

            _db.Set<GroupMember>().Remove(targetMembership);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: /api/groups/search?code={groupCode}&userId={userId}
        [HttpGet("search")]
        public async Task<ActionResult<GroupResponse>> SearchGroupByCode([FromQuery] string code, [FromQuery] Guid? userId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Group code is required.");
            }

            var normalized = code.ToUpperInvariant();
            var group = await _db.Set<Group>()
                .FirstOrDefaultAsync(g => g.GroupCode == normalized);

            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var memberCount = await _db.Set<GroupMember>()
                .CountAsync(gm => gm.GroupId == group.Id);

            GroupMember? membership = null;
            var pending = false;
            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                membership = await _db.Set<GroupMember>()
                    .FirstOrDefaultAsync(gm => gm.GroupId == group.Id && gm.UserId == userId.Value);

                if (membership == null)
                {
                    pending = await _db.Set<GroupRequest>()
                        .AnyAsync(gr => gr.GroupId == group.Id &&
                                        gr.RequesterId == userId.Value &&
                                        gr.Status == GroupRequestStatus.Pending);
                }
            }

            return Ok(ToResponse(group, memberCount, membership, pending));
        }

        private static GroupResponse ToResponse(Group group, int memberCount, GroupMember? membership = null, bool hasPendingRequest = false)
        {
            return new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                CreatorId = group.CreatorId,
                CreatedAt = group.CreatedAt,
                MemberCount = memberCount,
                MemberRole = membership?.Role.ToString(),
                IsMember = membership != null,
                HasPendingRequest = hasPendingRequest
            };
        }

        private static GroupMemberResponse ToMemberResponse(GroupMember member)
        {
            return new GroupMemberResponse
            {
                UserId = member.UserId,
                Username = member.User?.Username ?? string.Empty,
                DisplayName = member.User?.DisplayName ?? string.Empty,
                PersonalCode = member.User?.PersonalCode ?? string.Empty,
                Role = member.Role.ToString(),
                JoinedAt = member.JoinedAt
            };
        }

        private static GroupRequestResponse ToRequestResponse(GroupRequest request)
        {
            return new GroupRequestResponse
            {
                Id = request.Id,
                RequesterId = request.RequesterId,
                RequesterUsername = request.Requester?.Username ?? string.Empty,
                RequesterDisplayName = request.Requester?.DisplayName ?? string.Empty,
                RequesterPersonalCode = request.Requester?.PersonalCode ?? string.Empty,
                CreatedAt = request.CreatedAt,
                Status = request.Status.ToString()
            };
        }

        private static bool IsManager(GroupMember? membership) =>
            membership != null && (membership.Role == GroupMemberRole.Admin || membership.Role == GroupMemberRole.Creator);

        public class GroupResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string GroupCode { get; set; } = string.Empty;
            public Guid CreatorId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int MemberCount { get; set; }
            public string? MemberRole { get; set; }
            public bool IsMember { get; set; }
            public bool HasPendingRequest { get; set; }
        }

        public class GroupDetailResponse : GroupResponse
        {
            public GroupDetailResponse()
            {
            }

            public GroupDetailResponse(GroupResponse source)
            {
                Id = source.Id;
                Name = source.Name;
                GroupCode = source.GroupCode;
                CreatorId = source.CreatorId;
                CreatedAt = source.CreatedAt;
                MemberCount = source.MemberCount;
                MemberRole = source.MemberRole;
                IsMember = source.IsMember;
                HasPendingRequest = source.HasPendingRequest;
            }

            public string CreatorName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? CurrentUserRole { get; set; }
            public bool CanManageMembers { get; set; }
            public List<GroupMemberResponse> Members { get; set; } = new();
            public List<GroupRequestResponse> PendingRequests { get; set; } = new();
        }

        public class GroupMemberResponse
        {
            public Guid UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string PersonalCode { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public DateTime JoinedAt { get; set; }
        }

        public class GroupRequestResponse
        {
            public Guid Id { get; set; }
            public Guid RequesterId { get; set; }
            public string RequesterUsername { get; set; } = string.Empty;
            public string RequesterDisplayName { get; set; } = string.Empty;
            public string RequesterPersonalCode { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; } = GroupRequestStatus.Pending.ToString();
        }

        public class GroupJoinRequest
        {
            public Guid RequesterId { get; set; }
        }

        public class RespondGroupRequest
        {
            public Guid ApproverId { get; set; }
            public bool Accept { get; set; }
        }

        public class CreateGroupRequest
        {
            public string Name { get; set; } = string.Empty;
            public Guid CreatorId { get; set; }
        }
    }
}
