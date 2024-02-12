using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChatBack.Dtos;
using OnlineChatBack.Models;
using OnlineChatBack.Repositories;
using System.Net.WebSockets;

namespace OnlineChatBack.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ChatRoomController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatRoom>>> GetChatRoom() 
        {
            var chatRooms = await _applicationDbContext.ChatRooms.ToListAsync();

            return Ok(chatRooms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChatRoom>> GetChatRoom(Guid id)
        {
            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(id);

            if(chatRoom == null)
            {
                return NotFound();
            }

            return Ok(chatRoom);
        }

        [HttpPost]
        public async Task<ActionResult<ChatRoom>> PostChatRoom()
        {
            var username = HttpContext.User.Identity?.Name;

            if(username == null)
            {
                return BadRequest();
            }

            var chatRoom = new ChatRoom(username);

            _applicationDbContext.ChatRooms.Add(chatRoom);
            await _applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChatRoom), new {id =  chatRoom.Id}, chatRoom);
        }

        [HttpGet("{id}/messages")]
        public async Task<ActionResult<List<Message>>> GetMessages(Guid id)
        {
            var chatRoom = await _applicationDbContext.ChatRooms.Include(cr => cr.Messages).FirstOrDefaultAsync(cr => cr.Id == id);

            if( chatRoom == null)
            {
                return NotFound();
            }

            var currentUsername = HttpContext.User.Identity?.Name;

            if(currentUsername == null || !chatRoom.Usernames.Contains(currentUsername))
            {
                return Unauthorized();
            }

            var messages = chatRoom.Messages;

            return Ok(messages);
        }

        [HttpPost("{id}/send-message")]
        public async Task<IActionResult> SendMessage(Guid id, SendMessageDto message)
        {
            var sender = HttpContext.User.Identity?.Name;
            var chatRoom = await _applicationDbContext.ChatRooms.Include(cr => cr.Messages).FirstOrDefaultAsync(cr => cr.Id == id);

            if (sender == null || chatRoom == null)
            {
                return BadRequest();
            }

            var newMessage = new Message
            {
                Sender = sender,
                TimeSent = DateTime.Now,
                Content = message.Content
            };

            chatRoom.Messages.Add(newMessage);
            await _applicationDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
