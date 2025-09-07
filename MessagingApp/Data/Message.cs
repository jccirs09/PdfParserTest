using System;

namespace MessagingApp.Data
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
    }
}
