using System;
using System.Text;
using LeaveManagerApp.Web.Domain.Contracts;
using LeaveManagerApp.Web.Domain.Models;
using LeaveManagerApp.Web.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LeaveManagerApp.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Db context (update connection string in appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT configuration (reads from appsettings: Jwt:Key, Jwt:Issuer, Jwt:Audience)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "S3cur3RandomK3y2025!@#$%LeaveManager987654321";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeaveManagerAppIssuer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LeaveManagerAppAudience";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = true;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = true,
		ValidIssuer = jwtIssuer,
		ValidateAudience = true,
		ValidAudience = jwtAudience,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.FromMinutes(5)
	};
});

// CORS for Angular dev server (optional)
builder.Services.AddCors(o => o.AddPolicy("AllowAngularDev", policy =>
{
	policy.WithOrigins("https://localhost:7077", "https://localhost:49860", "http://localhost:49860")
		  .AllowAnyHeader()
		  .AllowAnyMethod()
		  .AllowCredentials();
}));

// register IHttpClientFactory
builder.Services.AddHttpClient();

// Application services
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

var app = builder.Build();

// Apply migrations and seed (runs at startup)
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	await SeedData.EnsureSeedData(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAngularDev");

app.UseAuthentication(); // <-- ensure __UseAuthentication__ is between __UseRouting__ and __UseAuthorization__

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapRazorPages();
app.MapControllers();

app.Run();