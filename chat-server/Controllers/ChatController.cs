using chat_server.data;
using chat_server.DTOs;
using chat_server.Hubs;
using chat_server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chat_server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatController(AppDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<User> users = await context.Users.OrderBy(p => p.UserName).ToListAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetChats(Guid toUserId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Message> messages = await context.Messages
                                        .Where(p => (p.User.Id == userId.ToString() && p.SenderId == toUserId.ToString())
                                        || (p.SenderId == userId.ToString() && p.User.Id == toUserId.ToString()))
                                        .OrderBy(p => p.SendAt)
                                        .ToListAsync(cancellationToken);
            return Ok(messages);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.ToUserId.ToString(), cancellationToken);

            if (user is null)
            {
                return NotFound("User not found.");
            }

            Message message = new Message()
            {
                SenderId = request.UserId.ToString(),
                Content = request.Content,
                SendAt = DateTime.Now,
                User = user
            };

            await context.Messages.AddAsync(message, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            string connectionId = ChatHub.Users.First(p => p.Value.ToString() == message.User.Id).Key;
            await hubContext.Clients.Client(connectionId).SendAsync("Messages", message);

            return Ok(message);
        }
    }
}

