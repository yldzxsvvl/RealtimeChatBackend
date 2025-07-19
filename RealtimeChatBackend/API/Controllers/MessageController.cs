using Application.Interfaces;
using Core.Entities;
using Core.Models; 
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IGroupService _groupService;
        private readonly IFileStorageService _fileStorageService;
        

        public MessageController(IMessageService messageService, IGroupService groupService, IFileStorageService fileStorageService) // Constructor güncellendi
        {
            _messageService = messageService;
            _groupService = groupService;
            _fileStorageService = fileStorageService;
            
        }

        
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID claim not found in token.");
            }
            return Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// Belirli bir gruba yeni bir metin mesajı gönderir.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderId = GetCurrentUserId();

            var group = await _groupService.GetGroupByIdAsync(request.GroupId);
            if (group == null || !group.MemberIds.Contains(senderId))
            {
                return Forbid("Mesaj göndermek için bu gruba üye olmanız gerekmektedir.");
            }

           
            var messageDto = await _messageService.SendMessageAsync(senderId, request.GroupId, request.Content, request.FileUrl, request.FileName);

            if (messageDto == null)
            {
                return BadRequest("Mesaj gönderilemedi. Grup bulunamadı veya yetkiniz yok.");
            }

            return Ok(messageDto);
        }

        /// <summary>
        /// Dosya yükler ve belirli bir gruba mesajla birlikte gönderir.
        /// </summary>
        [HttpPost("upload-and-send")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFileAndSendMessage([FromForm] UploadFileAndSendMessageRequest request)
        {
            var senderId = GetCurrentUserId();

            var group = await _groupService.GetGroupByIdAsync(request.GroupId);
            if (group == null || !group.MemberIds.Contains(senderId))
            {
                return Forbid("Dosya göndermek için bu gruba üye olmanız gerekmektedir.");
            }

            string? fileUrl = null;
            string? fileName = null;

            if (request.File != null && request.File.Length > 0)
            {
                try
                {
                    fileUrl = await _fileStorageService.SaveFileAsync(request.File);
                    fileName = request.File.FileName;
                }
                catch (ArgumentException ex)
                {
                    return BadRequest($"Dosya yükleme hatası: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Dosya yüklenirken beklenmeyen bir hata oluştu: {ex.Message}");
                }
            }

           
            var messageDto = await _messageService.SendMessageAsync(senderId, request.GroupId, request.Content ?? "", fileUrl, fileName);

            if (messageDto == null)
            {
                if (fileUrl != null)
                {
                    _fileStorageService.DeleteFile(fileUrl);
                }
                return BadRequest("Mesaj gönderilemedi. Grup bulunamadı veya yetkiniz yok.");
            }

            return Ok(messageDto);
        }


        /// <summary>
        /// Belirli bir grup için mesajları listeler (sayfalama ile).
        /// </summary>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetMessagesForGroup(Guid groupId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null || !group.MemberIds.Contains(userId))
            {
                return Forbid("Bu grubun mesajlarını görmek için üye olmanız gerekmektedir.");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            
            var messages = await _messageService.GetGroupMessagesAsync(groupId, pageNumber, pageSize);
            return Ok(messages);
        }

        /// <summary>
        /// Belirli bir gruptaki mesajlar arasında arama yapar.
        /// </summary>
        [HttpGet("group/{groupId}/search")]
        public async Task<IActionResult> SearchMessagesInGroup(Guid groupId, string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null || !group.MemberIds.Contains(userId))
            {
                return Forbid("Bu grubun mesajlarını aramak için üye olmanız gerekmektedir.");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // DÜZELTİLDİ: MessageService artık MessageDto listesi döndürüyor
            var messages = await _messageService.SearchMessagesInGroupAsync(groupId, searchTerm, pageNumber, pageSize);
            return Ok(messages);
        }


        /// <summary>
        /// Bir mesajı düzenler. Sadece mesajın göndereni düzenleyebilir.
        /// </summary>
        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageRequest request)
        {
            var editorId = GetCurrentUserId();
            
            var updatedMessage = await _messageService.UpdateMessageAsync(messageId, editorId, request.NewContent);

            if (updatedMessage == null)
            {
                return Forbid("Mesaj düzenlenemedi. Mesaj bulunamadı veya yetkiniz yok.");
            }
            return Ok(updatedMessage);
        }

        /// <summary>
        /// Mesajı siler (soft delete). Sadece mesajın göndereni silebilir.
        /// </summary>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var deleterId = GetCurrentUserId();
            var result = await _messageService.DeleteMessageAsync(messageId, deleterId);

            if (!result)
            {
                return Forbid("Mesaj silinemedi. Mesaj bulunamadı veya yetkiniz yok.");
            }
            return Ok("Mesaj başarıyla silindi.");
        }
    }

    // İstek (Request) modelleri - API'ye gelen isteklerin yapısını tanımlar.
    public class SendMessageRequest
    {
        public Guid GroupId { get; set; }
        public string Content { get; set; } = null!;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
    }

    /// <summary>
    /// Dosya yükleme ve mesaj gönderme isteği için model.
    /// </summary>
    public class UploadFileAndSendMessageRequest
    {
        [FromForm(Name = "groupId")]
        public Guid GroupId { get; set; }

        [FromForm(Name = "content")]
        public string? Content { get; set; }

        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }
    }

    /// <summary>
    /// Mesaj düzenleme isteği için model.
    /// </summary>
    public class EditMessageRequest
    {
        public string NewContent { get; set; } = null!;
    }
}
