using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using Application.Interfaces;
using System.Security.Claims;
using Core.Entities;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Hubs
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IGroupService _groupService;
        private readonly ApplicationDbContext _context;

        public ChatHub(IMessageService messageService, IGroupService groupService, ApplicationDbContext context)
        {
            _messageService = messageService;
            _groupService = groupService;
            _context = context;
        }

        public async Task SendMessageToGroup(Guid groupId, string content)
        {
            var senderIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdClaim))
            {
                await Clients.Caller.SendAsync("ReceiveMessageError", "Kullanıcı kimliği bulunamadı.");
                return;
            }
            var senderId = Guid.Parse(senderIdClaim);

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null || !group.MemberIds.Contains(senderId))
            {
                await Clients.Caller.SendAsync("ReceiveMessageError", "Mesaj göndermek için bu gruba üye olmanız gerekmektedir.");
                return;
            }

            var messageDto = await _messageService.SendMessageAsync(senderId, groupId, content);

            if (messageDto != null)
            {
                await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessageError", "Mesaj gönderilemedi.");
            }
        }

        public async Task JoinGroup(Guid groupId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                await Clients.Caller.SendAsync("JoinGroupError", "Kullanıcı kimliği bulunamadı.");
                return;
            }
            var userId = Guid.Parse(userIdClaim);

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                await Clients.Caller.SendAsync("JoinGroupError", "Grup bulunamadı.");
                return;
            }

            if (!group.MemberIds.Contains(userId))
            {
                var joined = await _groupService.JoinGroupAsync(groupId, userId);
                if (!joined)
                {
                    await Clients.Caller.SendAsync("JoinGroupError", "Gruba katılım başarısız oldu.");
                    return;
                }
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
            await Clients.Caller.SendAsync("JoinedGroup", groupId, "Gruba başarıyla katıldınız.");
        }

        public async Task LeaveGroup(Guid groupId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                await Clients.Caller.SendAsync("LeaveGroupError", "Kullanıcı kimliği bulunamadı.");
                return;
            }
            var userId = Guid.Parse(userIdClaim);

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                await Clients.Caller.SendAsync("LeaveGroupError", "Grup bulunamadı.");
                return;
            }

            var left = await _groupService.LeaveGroupAsync(groupId, userId);
            if (left)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
                await Clients.Caller.SendAsync("LeftGroup", groupId, "Gruptan başarıyla ayrıldınız.");
            }
            else
            {
                await Clients.Caller.SendAsync("LeaveGroupError", "Gruptan ayrılma başarısız oldu. Belki de grubun yaratıcısısınız.");
            }
        }

       
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync(); 

            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    var userId = Guid.Parse(userIdClaim);
                    var userGroups = await _groupService.GetUserGroupsAsync(userId);
                    foreach (var group in userGroups)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Hatanın detaylarını sunucu konsoluna yazdırıyoruz.
                Console.WriteLine($"--- SIGNALR HUB ERROR (OnConnectedAsync) ---");
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"------------------------------------------");
                
                // Hata olduğu için bağlantıyı sonlandırıyoruz.
                Context.Abort();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
