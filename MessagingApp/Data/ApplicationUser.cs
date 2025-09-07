using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MessagingApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
