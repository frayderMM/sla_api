namespace DamslaApi.Dtos.Chat
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
