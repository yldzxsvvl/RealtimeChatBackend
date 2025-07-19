using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGroupService
    {
       
        Task<Group?> CreateGroupAsync(string name, string? description, bool isPublic, Guid creatorId);

        // Bir kullanıcının üye olduğu tüm grupları listeler.
        Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId);

        // Belirli bir grubu ID'sine göre getirir.
        Task<Group?> GetGroupByIdAsync(Guid groupId);

        // Mevcut bir gruba katılır.
        Task<bool> JoinGroupAsync(Guid groupId, Guid userId);

        // Mevcut bir gruptan ayrılır.
        Task<bool> LeaveGroupAsync(Guid groupId, Guid userId);

        // Herkese açık gruplar arasında arama yapar (sayfalama ve arama terimi ile).
        Task<IEnumerable<Group>> SearchPublicGroupsAsync(string? searchTerm, int pageNumber, int pageSize);
    }
}
