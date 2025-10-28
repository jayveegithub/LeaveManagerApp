using LeaveManagerApp.Web.Controllers.DTO;
using LeaveManagerApp.Web.Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagerApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
	[Authorize]
	public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
		public LeaveRequestsController(ILeaveService leaveService)
        {
			_leaveService = leaveService;
		}

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeaveRequestCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _leaveService.CreateLeaveRequest(dto);
			if (result.Errors.Any())
			{
				return Conflict(new { result.Errors });
			}

			return CreatedAtAction(nameof(GetById), new { id = result?.LeaveRequest?.Id }, result?.LeaveRequest);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _leaveService.GetById(id);
			if (r == null) return NotFound();
            return Ok(r);
        }

		[HttpGet("applicant/{applicantId}")]
		public async Task<IActionResult> GetByApplicantId(string  applicantId)
		{
			var r = await _leaveService.GetByApplicantId(applicantId);

			return Ok(r);
		}

		[HttpGet("leavetypes")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] bool includeInactive = false)
        {
            var types = await _leaveService.GetLeaveTypes(includeInactive);
			return Ok(types);
        }
    }
}