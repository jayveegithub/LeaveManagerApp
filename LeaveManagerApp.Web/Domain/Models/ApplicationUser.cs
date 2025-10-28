using Microsoft.AspNetCore.Identity;

namespace LeaveManagerApp.Web.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}