namespace LeaveManagerApp.Web.Domain.Models
{
	public class LeaveRequestResult
	{
		public LeaveRequest? LeaveRequest { get; set; }
		public List<string> Errors { get; set; } = new();
	}
}
