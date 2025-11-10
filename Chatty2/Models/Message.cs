namespace Chatty2.Models
{
    // Models/Message.cs
    public class Message
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsPinned { get; set; } = false;
    }
}
