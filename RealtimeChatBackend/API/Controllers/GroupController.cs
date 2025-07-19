using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Bu controller'a api/group üzerinden erişilecek
    [Authorize] // Bu controller'daki tüm endpoint'ler için kimlik doğrulaması zorunlu
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        // JWT token'dan kullanıcının ID'sini almak için yardımcı metot
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
        /// Yeni bir grup oluşturur.
        /// </summary>
        [HttpPost("create")] 
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var creatorId = GetCurrentUserId(); 
            var group = await _groupService.CreateGroupAsync(request.Name, request.Description, request.IsPublic, creatorId);

            if (group == null)
            {
                return BadRequest("Grup oluşturulamadı.");
            }
            return Ok(group);
        }

      
        /// Kullanıcının üye olduğu tüm grupları listeler.
       
        [HttpGet("my-groups")] // URL: api/group/my-groups
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = GetCurrentUserId();
            var groups = await _groupService.GetUserGroupsAsync(userId);
            return Ok(groups);
        }

       
        /// Belirli bir gruba katılır.
       
        [HttpPost("{groupId}/join")] // URL: api/group/{groupId}/join
        public async Task<IActionResult> JoinGroup(Guid groupId)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.JoinGroupAsync(groupId, userId);

            if (!result)
            {
                return BadRequest("Gruba katılamadı. Grup bulunamadı veya zaten üyesiniz.");
            }
            return Ok("Gruba başarıyla katıldınız.");
        }

       
        /// Belirli bir gruptan ayrılır.
       
        [HttpPost("{groupId}/leave")] 
        public async Task<IActionResult> LeaveGroup(Guid groupId)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.LeaveGroupAsync(groupId, userId);

            if (!result)
            {
                return BadRequest("Gruptan ayrılamadı. Grup bulunamadı, üye değilsiniz veya grubun yaratıcısısınız.");
            }
            return Ok("Gruptan başarıyla ayrıldınız.");
        }

        /// <summary>
        /// Herkese açık gruplar arasında arama yapar.
        /// </summary>
        [HttpGet("search-public")] // URL: api/group/search-public?searchTerm=test&pageNumber=1&pageSize=10
        public async Task<IActionResult> SearchPublicGroups([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            
            var groups = await _groupService.SearchPublicGroupsAsync(searchTerm, pageNumber, pageSize);
            return Ok(groups);
        }
    }

    // İstek (Request) modeli
    public class CreateGroupRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
    }
}