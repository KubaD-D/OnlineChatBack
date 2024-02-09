using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineChatBack.Repositories;

namespace OnlineChatBack.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        private readonly ChatRoomRepository _chatRoomRepository;

        public ChatRoomController(ChatRoomRepository chatRoomRepository)
        {
            _chatRoomRepository = chatRoomRepository;
        }

        [HttpGet]
        public IActionResult GetChatRoom() 
        {
            Console.WriteLine(HttpContext.User.Identity.Name);

            return Ok(_chatRoomRepository.ChatRooms);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetChatRoom(Guid id)
        {
            var chatRoom = _chatRoomRepository.GetChatRoom(id);

            if(chatRoom == null)
            {
                return NotFound();
            }

            return Ok(chatRoom);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult PostChatRoom()
        {
            var chatRoom = _chatRoomRepository.CreateChatRoom();

            if(chatRoom == null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetChatRoom), new {id =  chatRoom.Id}, chatRoom);
        }
    }
}
