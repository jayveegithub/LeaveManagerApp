namespace LeaveManagerApp.Web.Controllers.DTO
{
	public class LeaveRequestCreateDto
	{
		public string ApplicantId { get; set; } = null!;
		public string ManagerId { get; set; } = null!;
		public int LeaveTypeId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime ReturnDate { get; set; }
		public int? NumberOfDays { get; set; }
		public string? GeneralComments { get; set; }
	}
}
