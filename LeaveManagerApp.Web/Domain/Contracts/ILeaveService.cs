using LeaveManagerApp.Web.Controllers.DTO;
using LeaveManagerApp.Web.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagerApp.Web.Domain.Contracts
{
	public interface ILeaveService
	{
		Task<LeaveRequest?> GetById(int id);

		Task<List<LeaveType>> GetLeaveTypes(bool includeInactive = false);

		Task<List<LeaveRequest>> GetByApplicantId(string applicantId);

		Task<LeaveRequestResult> CreateLeaveRequest(LeaveRequestCreateDto dto);
	}
}