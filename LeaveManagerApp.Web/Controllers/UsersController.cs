using LeaveManagerApp.Web.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagerApp.Web.Controllers.DTO;

namespace LeaveManagerApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/Users?includeInactive=false
        [HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var usersQuery = _userManager.Users.AsQueryable();
            if (!includeInactive) usersQuery = usersQuery.Where(u => u.IsActive);

            var list = await usersQuery
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive
                })
				.OrderBy(u => u.FullName)
				.ToListAsync();

            return Ok(list);
        }

		[HttpGet("managers")]
		[Authorize]
		public async Task<IActionResult> GetAllManagers()
		{
			var usersInRole = await _userManager.GetUsersInRoleAsync("Manager");

			var list = usersInRole
				.Select(u => new UserDto
				{
					Id = u.Id,
					FullName = u.FullName,
					Email = u.Email,
					IsActive = u.IsActive
				})
                .OrderBy(u => u.FullName)
				.ToList();

			return Ok(list);
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var dto = new UserDto { Id = user.Id, FullName = user.FullName, Email = user.Email, IsActive = user.IsActive };
            return Ok(dto);
        }
    }
}