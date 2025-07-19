using Application.Interfaces; 
using Core.Entities; 
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks; 
using Infrastructure.Data; 
using Microsoft.EntityFrameworkCore; 

namespace Infrastructure.Services 
{ 
    public class GroupService : IGroupService 
    { 
        private readonly ApplicationDbContext _context; 

        public GroupService(ApplicationDbContext context) 
        { 
            _context = context; 
        } 

        public List<Group> GetGroups() 
        { 
            return _context.Groups.ToList(); 
        } 

        public async Task<Group?> CreateGroupAsync(string name, string? description, bool isPublic, Guid creatorId) 
        { 
            var creator = await _context.Users.FirstOrDefaultAsync(u => u.Id == creatorId); 
            if (creator == null) 
            { 
                return null; 
            } 

            var newGroup = new Group 
            { 
                Id = Guid.NewGuid(), 
                Name = name, 
                Description = description, 
                CreatedAt = DateTime.UtcNow, 
                IsPublic = isPublic, 
                CreatorId = creatorId, 
                MemberIds = new List<Guid> { creatorId } 
            }; 

            _context.Groups.Add(newGroup); 

            creator.JoinedGroupIds.Add(newGroup.Id); 
            _context.Users.Update(creator); 

            await _context.SaveChangesAsync(); 

            return newGroup; 
        } 

        public async Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId) 
        { 
             
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId); 
            if (user == null) 
            { 
                return Enumerable.Empty<Group>(); 
            } 

         
            var userGroups = await _context.Groups 
                                            .Where(g => user.JoinedGroupIds.Contains(g.Id)) 
                                            .ToListAsync(); 
            return userGroups; 
        } 

        public async Task<Group?> GetGroupByIdAsync(Guid groupId) 
        { 
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId); 
            return group; 
        } 

        public async Task<bool> JoinGroupAsync(Guid groupId, Guid userId) 
        { 
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId); 
            if (group == null || !group.IsPublic) 
            { 
                return false; 
            } 

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId); 
            if (user == null) 
            { 
                return false; 
            } 

            if (!group.MemberIds.Contains(userId)) 
            { 
                group.MemberIds.Add(userId); 
                user.JoinedGroupIds.Add(groupId); 

                _context.Groups.Update(group); 
                _context.Users.Update(user); 
                await _context.SaveChangesAsync(); 
                return true; 
            } 
            return false; 
        } 

        public async Task<bool> LeaveGroupAsync(Guid groupId, Guid userId) 
        { 
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId); 
            if (group == null) 
            { 
                return false; 
            } 

            if (group.CreatorId == userId) 
            { 
                return false; 
            } 

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId); 
            if (user == null) 
            { 
                return false; 
            } 

            if (group.MemberIds.Contains(userId)) 
            { 
                group.MemberIds.Remove(userId); 
                user.JoinedGroupIds.Remove(groupId); 

                _context.Groups.Update(group); 
                _context.Users.Update(user); 
                await _context.SaveChangesAsync(); 
                return true; 
            } 
            return false; 
        } 

        public async Task<IEnumerable<Group>> SearchPublicGroupsAsync(string? searchTerm, int pageNumber, int pageSize) 
        { 
            var query = _context.Groups.AsQueryable().Where(g => g.IsPublic); 

            if (!string.IsNullOrWhiteSpace(searchTerm)) 
            { 
                query = query.Where(g => g.Name.Contains(searchTerm) || 
                                        (g.Description != null && g.Description.Contains(searchTerm))); 
            } 

            var pagedGroups = await query 
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize) 
                .ToListAsync(); 

            return pagedGroups; 
        } 
    } 
}