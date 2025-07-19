using Application.Interfaces;
using Core.Entities;
using Core.Models; // MessageDto için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json; // JsonSerializer için

namespace Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGroupService _groupService;
        private readonly ICacheService _cacheService; 

        public MessageService(ApplicationDbContext context, IGroupService groupService, ICacheService cacheService) 
        {
            _context = context;
            _groupService = groupService;
            _cacheService = cacheService; 
        }

       
        private async Task<MessageDto> MapToMessageDto(Message message)
        {
            
            var senderUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == message.SenderId);
            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderUsername = senderUser?.Username ?? "Bilinmeyen Kullanıcı",
                GroupId = message.GroupId,
                Content = message.Content,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                Timestamp = message.Timestamp,
                EditedAt = message.EditedAt,
                IsDeleted = message.IsDeleted
            };
        }

        public async Task<MessageDto?> SendMessageAsync(Guid senderId, Guid groupId, string content, string? fileUrl = null, string? fileName = null)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return null;
            }

            if (!group.MemberIds.Contains(senderId))
            {
                return null;
            }

            var newMessage = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                GroupId = groupId,
                Content = content,
                FileUrl = fileUrl,
                FileName = fileName,
                Timestamp = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            
            await _cacheService.RemoveCacheValueAsync($"groupMessages:{groupId}");

            return await MapToMessageDto(newMessage);
        }

        public async Task<IEnumerable<MessageDto>> GetGroupMessagesAsync(Guid groupId, int pageNumber, int pageSize)
        {
            var cacheKey = $"groupMessages:{groupId}:page:{pageNumber}:size:{pageSize}";
            var cachedMessagesJson = await _cacheService.GetCacheValueAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedMessagesJson))
            {
                
                return JsonSerializer.Deserialize<List<MessageDto>>(cachedMessagesJson) ?? Enumerable.Empty<MessageDto>();
            }

            var messages = await _context.Messages
                                         .Where(m => m.GroupId == groupId && !m.IsDeleted)
                                         .OrderByDescending(m => m.Timestamp)
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Take(pageSize)
                                         .Include(m => m.Sender) 
                                         .ToListAsync();

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender?.Username ?? "Bilinmeyen Kullanıcı",
                GroupId = m.GroupId,
                Content = m.Content,
                FileUrl = m.FileUrl,
                FileName = m.FileName,
                Timestamp = m.Timestamp,
                EditedAt = m.EditedAt,
                IsDeleted = m.IsDeleted
            }).ToList();

           
            await _cacheService.SetCacheValueAsync(cacheKey, JsonSerializer.Serialize(messageDtos), TimeSpan.FromMinutes(5));

            return messageDtos.OrderBy(m => m.Timestamp);
        }

        public async Task<MessageDto?> UpdateMessageAsync(Guid messageId, Guid userId, string newContent)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

            if (message == null || message.SenderId != userId)
            {
                return null;
            }

            message.Content = newContent;
            message.EditedAt = DateTime.UtcNow;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();

           
            await _cacheService.RemoveCacheValueAsync($"groupMessages:{message.GroupId}");

            return await MapToMessageDto(message);
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

            if (message == null || message.SenderId != userId)
            {
                return false;
            }

            _context.Messages.Remove(message); 
            await _context.SaveChangesAsync();

           
            await _cacheService.RemoveCacheValueAsync($"groupMessages:{message.GroupId}");

            return true;
        }

        public async Task<MessageDto?> GetMessageByIdAsync(Guid messageId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);
            if (message == null) return null;
            return await MapToMessageDto(message);
        }

        public async Task<IEnumerable<MessageDto>> SearchMessagesInGroupAsync(Guid groupId, string? searchTerm, int pageNumber, int  pageSize)
        {
            var cacheKey = $"groupMessagesSearch:{groupId}:{searchTerm}:page:{pageNumber}:size:{pageSize}";
            var cachedMessagesJson = await _cacheService.GetCacheValueAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedMessagesJson))
            {
            
                return JsonSerializer.Deserialize<List<MessageDto>>(cachedMessagesJson) ?? Enumerable.Empty<MessageDto>();
            }

            var query = _context.Messages
                                .Where(m => m.GroupId == groupId && !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.Content != null && m.Content.Contains(searchTerm));
            }

            var pagedMessages = await query
                .OrderByDescending(m => m.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.Sender) 
                .ToListAsync();

            var messageDtos = pagedMessages.Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender?.Username ?? "Bilinmeyen Kullanıcı",
                GroupId = m.GroupId,
                Content = m.Content,
                FileUrl = m.FileUrl,
                FileName = m.FileName,
                Timestamp = m.Timestamp,
                EditedAt = m.EditedAt,
                IsDeleted = m.IsDeleted
            }).ToList();

          
            await _cacheService.SetCacheValueAsync(cacheKey, JsonSerializer.Serialize(messageDtos), TimeSpan.FromMinutes(5));

            return messageDtos.OrderBy(m => m.Timestamp);
        }
    }
}
