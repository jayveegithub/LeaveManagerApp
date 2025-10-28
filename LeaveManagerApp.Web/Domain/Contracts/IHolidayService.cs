namespace LeaveManagerApp.Web.Domain.Contracts
{
	public interface IHolidayService
    {
        Task<List<DateTime>> GetHolidaysAsync();
    }
}