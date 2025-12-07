using Microsoft.AspNetCore.Mvc;
using DamslaApi.Dtos.Chat;
using DamslaApi.Services;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Chat(ChatRequestDto request)
        {
            var response = await _chatService.ProcessMessageAsync(request);
            return Ok(response);
        }
    }
}
