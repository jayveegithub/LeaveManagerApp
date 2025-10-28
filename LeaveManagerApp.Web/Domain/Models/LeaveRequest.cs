using System.ComponentModel.DataAnnotations;

namespace LeaveManagerApp.Web.Domain.Models
{
    public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }

    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string ApplicantId { get; set; } = null!;
        public ApplicationUser Applicant { get; set; } = null!;

        [Required]
        public string ManagerId { get; set; } = null!;
        public ApplicationUser Manager { get; set; } = null!;

        [Required]
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime ReturnDate { get; set; }

        public int NumberOfDays { get; set; }

        [MaxLength(500)]
        public string? GeneralComments { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}