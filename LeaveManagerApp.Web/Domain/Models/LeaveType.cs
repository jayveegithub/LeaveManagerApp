using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagerApp.Web.Domain.Models
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxDurationDays { get; set; } = 30;
		[NotMapped]
		public int MinAdvanceDays { get; set; } = 2;
		public bool IsActive { get; set; } = true;
    }
}