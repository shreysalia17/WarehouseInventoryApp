using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WarehouseMvc.Data;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Add EF Core (SQL Server)
var conn = builder.Configuration.GetConnectionString("WarehouseDb");
builder.Services.AddDbContext<WarehouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WarehouseDb")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)//app uses cookies to track user sessions.
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // where to send users who are not logged in
        options.LogoutPath = "/Account/Logout"; // logout URL
    });


var app = builder.Build();

// Create the database at startup 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseContext>();
    db.Database.EnsureCreated();//checks if the database exists 
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();//ckecks if user is authenticated.
app.UseAuthorization();// Authirizes control

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");// if no control then it will re-direct to the home page

app.Run();
