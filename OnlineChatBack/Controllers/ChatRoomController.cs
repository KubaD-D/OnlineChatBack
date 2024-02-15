using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChatBack.Dtos;
using OnlineChatBack.Models;

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
        public async Task<ActionResult<List<ChatRoom>>> GetChatRooms() 
        {
            var username = HttpContext.User.Identity?.Name;

            if (username == null)
            {
                return BadRequest();
            }

            var chatRooms = await _applicationDbContext.ChatRooms.Where(cr => cr.Usernames.Contains(username)).ToListAsync();

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
        public async Task<ActionResult<ChatRoom>> PostChatRoom(string title)
        {
            var username = HttpContext.User.Identity?.Name;

            if(username == null)
            {
                return BadRequest();
            }

            var chatRoom = new ChatRoom(username, title);

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

            if(!chatRoom.Usernames.Contains(sender))
            {
                return Unauthorized();
            }

            var newMessage = new Message
            {
                Sender = sender,
                TimeSent = DateTime.Now,
                Content = message.Content
            };

            chatRoom.Messages.Add(newMessage);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { newMessage });
        }

        [HttpGet("{id}/get-users")]
        public async Task<ActionResult<List<string>>> GetUsersFromChatRoom(Guid id)
        {
            var username = HttpContext.User.Identity?.Name;

            if(username == null)
            {
                return BadRequest();
            }

            var chatRoom = await _applicationDbContext.ChatRooms.FirstOrDefaultAsync(cr => cr.Id == id);

            if(chatRoom == null)
            {
                return NotFound();
            }

            if(!chatRoom.Usernames.Contains(username))
            {
                return Unauthorized();
            }

            var users = chatRoom.Usernames;

            return Ok(users);
        }

        [HttpPost("{id}/add-user")]
        public async Task<IActionResult> AddUser(Guid id, UsernameDto usernameRequest)
        {
            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(id);
            var requestingUser = HttpContext.User.Identity?.Name;

            if(chatRoom == null || requestingUser == null)
            {
                return BadRequest();
            }

            if(requestingUser != chatRoom.Owner)
            {
                return Unauthorized();
            }

            if(chatRoom.Usernames.Contains(usernameRequest.Username))
            {
                return Conflict();
            }

            chatRoom.Usernames.Add(usernameRequest.Username);
            await _applicationDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/remove-user")]
        public async Task<IActionResult> RemoveUser(Guid id, UsernameDto usernameRequest)
        {
            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(id);
            var requestingUser = HttpContext.User.Identity?.Name;

            if (chatRoom == null || requestingUser == null)
            {
                return BadRequest();
            }

            if (requestingUser != chatRoom.Owner)
            {
                return Unauthorized();
            }

            if(usernameRequest.Username == chatRoom.Owner)
            {
                return Forbid();
            }

            if(!chatRoom.Usernames.Remove(usernameRequest.Username))
            {
                return NotFound();
            }

            await _applicationDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatRoom(Guid id)
        {
            var chatRoom = await _applicationDbContext.ChatRooms.FindAsync(id);
            var requestingUser = HttpContext.User.Identity?.Name;

            if (chatRoom == null || requestingUser == null)
            {
                return BadRequest();
            }

            if (requestingUser != chatRoom.Owner)
            {
                return Unauthorized();
            }

            _applicationDbContext.ChatRooms.Remove(chatRoom);
            await _applicationDbContext.SaveChangesAsync();

            return Ok();
        }


    }
}
