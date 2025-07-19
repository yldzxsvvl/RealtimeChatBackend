using Core.Entities; 
using Core.Models; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        
        Task<MessageDto?> SendMessageAsync(Guid senderId, Guid groupId, string content, string? fileUrl = null, string? fileName = null);

       
        Task<IEnumerable<MessageDto>> GetGroupMessagesAsync(Guid groupId, int pageNumber, int pageSize);

       
        Task<MessageDto?> UpdateMessageAsync(Guid messageId, Guid userId, string newContent);

        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);

        Task<MessageDto?> GetMessageByIdAsync(Guid messageId);

       
        Task<IEnumerable<MessageDto>> SearchMessagesInGroupAsync(Guid groupId, string? searchTerm, int pageNumber, int pageSize);
    }
}
