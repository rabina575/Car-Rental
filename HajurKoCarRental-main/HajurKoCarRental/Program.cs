using HajurKoCarRental.Context;
using HajurKoCarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

var builder = WebApplication.CreateBuilder(args);

//Register ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "users",
    pattern: "users/{Index}",
     defaults: new { controller = "Users", action = "Index" });

app.MapControllerRoute(
    name: "requests",
    pattern: "requests/{Index}",
     defaults: new { controller = "CarRequest", action = "Index" });

app.MapControllerRoute(
    name: "bills",
    pattern: "bills/{Index}",
     defaults: new { controller = "RepairBill", action = "Index" });

app.MapControllerRoute(
    name: "notifications",
    pattern: "notifications/{Index}",
     defaults: new { controller = "Notifications", action = "Index" });


app.MapControllerRoute(
    name: "offers",
    pattern: "offers/{Index}",
     defaults: new { controller = "Offers", action = "Index" });

app.MapRazorPages();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppDbInitialize.SeedUsersAndRolesAsync(app).Wait();
app.Run();
