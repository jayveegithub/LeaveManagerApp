using LeaveManagerApp.Web.Controllers.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

//namespace LeaveManagerApp.Web.Pages.Account
namespace LeaveManagerApp.Web.Domain.Models
{
	public class LoginModel : PageModel
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public LoginModel(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public string? ErrorMessage { get; set; }

		public class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
				return Page();

			var client = _httpClientFactory.CreateClient();
			var loginDto = new LoginDto
			{
				Email = Input.Email,
				Password = Input.Password
			};

			var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
				// Store the token in local storage or cookie
				// Redirect to home page or dashboard
				return RedirectToPage("/Index");
			}

			ErrorMessage = "Invalid login attempt.";
			return Page();
		}
	}
}