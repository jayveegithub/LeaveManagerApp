using LeaveManagerApp.Web.Controllers.DTO;
using LeaveManagerApp.Web.Data;
using LeaveManagerApp.Web.Domain.Contracts;
using LeaveManagerApp.Web.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagerApp.Web.Domain.Services
{
	public class LeaveService : ILeaveService
	{
		private readonly ApplicationDbContext _db;
		private readonly IHolidayService _holidayService;
		public LeaveService(ApplicationDbContext db, IHolidayService holidayService)
		{
			_db = db;
			_holidayService = holidayService;
		}

		public async Task<LeaveRequestResult> CreateLeaveRequest(LeaveRequestCreateDto dto)
		{
			var errors = new List<string>();

			if (dto.ApplicantId == dto.ManagerId) errors.Add("Applicant and Manager must be different.");

			var applicant = await _db.Users.FindAsync(dto.ApplicantId);
			if (applicant == null || !applicant.IsActive) errors.Add("Applicant not found or inactive.");

			var manager = await _db.Users.FindAsync(dto.ManagerId);
			if (manager == null || !manager.IsActive) errors.Add("Manager not found or inactive.");

			var leaveType = await _db.LeaveTypes.FindAsync(dto.LeaveTypeId);
			if (leaveType == null || !leaveType.IsActive) errors.Add("Leave type invalid or inactive.");

			var today = DateTime.UtcNow.Date;
			var start = dto.StartDate.Date;
			var end = dto.EndDate.Date;
			var ret = dto.ReturnDate.Date;

			if (start < today) errors.Add("Start Date must not be in the past.");
			if (end <= start) errors.Add("End Date must be after Start Date.");
			if (ret <= end) errors.Add("Return Date must be after End Date.");

			var holidays = await _holidayService.GetHolidaysAsync();

			if (!IsWorkingDay(start, holidays)) errors.Add("Start Date cannot be a weekend or holiday.");
			if (!IsWorkingDay(end, holidays)) errors.Add("End Date cannot be a weekend or holiday.");
			if (!IsWorkingDay(ret, holidays)) errors.Add("Return Date cannot be a weekend or holiday.");

			var minNotice = 2; // working days
			var noticeDays = CountWorkingDays(DateTime.UtcNow.Date, start, holidays) - 1;
			if (noticeDays < minNotice) errors.Add($"Leave must be submitted at least {minNotice} working days in advance.");

			var calcDays = CountWorkingDays(start, end, holidays);
			if (calcDays > leaveType.MaxDurationDays) errors.Add($"Leave duration exceeds {leaveType.MaxDurationDays} days for {leaveType.Name}.");

			// overlapping
			var overlap = await _db.LeaveRequests.AnyAsync(r =>
				r.ApplicantId == dto.ApplicantId &&
				r.Status != LeaveStatus.Cancelled &&
				r.StartDate <= end && r.EndDate >= start);

			if (overlap) errors.Add("You have an overlapping leave request.");

			// duplicate (exact identical)
			var duplicate = await _db.LeaveRequests.AnyAsync(r =>
				r.ApplicantId == dto.ApplicantId &&
				r.ManagerId == dto.ManagerId &&
				r.LeaveTypeId == dto.LeaveTypeId &&
				r.StartDate == start &&
				r.EndDate == end &&
				r.ReturnDate == ret &&
				r.GeneralComments == dto.GeneralComments);

			if (duplicate) errors.Add("An identical leave request already exists.");

			//if (errors.Any()) return Conflict(new { errors });

			var entity = new LeaveRequest
			{
				ApplicantId = dto.ApplicantId,
				ManagerId = dto.ManagerId,
				LeaveTypeId = dto.LeaveTypeId,
				StartDate = start,
				EndDate = end,
				ReturnDate = ret,
				NumberOfDays = dto.NumberOfDays ?? calcDays,
				GeneralComments = dto.GeneralComments
			};

			_db.LeaveRequests.Add(entity);
			await _db.SaveChangesAsync();

			return new LeaveRequestResult
			{
				LeaveRequest = entity,
				Errors = errors
			};
		}

		public async Task<List<LeaveRequest>> GetByApplicantId(string applicantId)
		{
			var r = await _db.LeaveRequests
						.Include(x => x.Applicant)
						.Include(x => x.Manager)
						.Include(x => x.LeaveType)
						.Where(x => x.ApplicantId == applicantId)
						.ToListAsync();

						return r;
		}

		public async Task<LeaveRequest?> GetById(int id)
		{
			var r = await _db.LeaveRequests
				.Include(x => x.Applicant)
				.Include(x => x.Manager)
				.Include(x => x.LeaveType)
				.FirstOrDefaultAsync(x => x.Id == id);

			return r;
		}

		public async Task<List<LeaveType>> GetLeaveTypes(bool includeInactive = false)
		{
			var query = _db.LeaveTypes.AsQueryable();

			if (!includeInactive)
			{
				query = query.Where(lt => lt.IsActive);
			}

			var types = await query
				.OrderBy(lt => lt.Name)
				.ToListAsync();

			return types;
		}


		#region Private Methods

		private bool IsWorkingDay(DateTime date, List<DateTime> holidays)
		{
			if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return false;
			return !holidays.Any(h => h.Date == date.Date);
		}

		private int CountWorkingDays(DateTime from, DateTime to, List<DateTime> holidays)
		{
			if (to < from) return 0;
			var count = 0;
			for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
			{
				if (IsWorkingDay(d, holidays)) count++;
			}
			return count;
		}
		#endregion

	}
}
