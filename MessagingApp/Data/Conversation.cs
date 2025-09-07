using System.Collections.Generic;

namespace MessagingApp.Data
{
    public class Conversation
    {
        public int Id { get; set; }
        public ICollection<ApplicationUser> Participants { get; set; } = new List<ApplicationUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
