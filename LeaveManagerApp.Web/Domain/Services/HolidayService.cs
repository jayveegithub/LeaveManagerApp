using LeaveManagerApp.Web.Domain.Contracts;

namespace LeaveManagerApp.Web.Domain.Services
{
    public class HolidayService : IHolidayService
    {
        // Replace with DB or calendar integration
        public Task<List<DateTime>> GetHolidaysAsync()
        {
            var holidays = new List<DateTime>
            {
                new DateTime(2025, 1, 1),   // New Year's Day
				new DateTime(2025, 6, 9),   // King's Birthday
				new DateTime(2025, 10, 6),  // National Day
				new DateTime(2025, 12, 25), // Christmas Day
				new DateTime(2025, 12, 26)  // Boxing Day 
			};
            return Task.FromResult(holidays);
        }
    }
}