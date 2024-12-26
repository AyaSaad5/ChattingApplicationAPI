using Microsoft.AspNetCore.Identity;

namespace ChattingApplication.Entities
{
    public class AppRole : IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}
