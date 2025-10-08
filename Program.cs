using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Project02.Binder;
using Project02.Data;
using Project02.Middleware;
using Project02.Models;
using Project02.Security;
using Project02.Services;
using System.Runtime;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "UserScheme";
            options.DefaultSignInScheme = "UserScheme";
            options.DefaultAuthenticateScheme = "UserScheme";
        }).AddCookie("AdminScheme", options =>
        {
            options.LoginPath = "/admin/login";
            options.LogoutPath = "/admin/logout";
            options.AccessDeniedPath = "/admin/login";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
        }).
        AddCookie("UserScheme", options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
        });
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IFileStorage, FileStorage>();
        builder.Services.AddControllersWithViews(options =>
        {
            options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Accounts.Any(a => a.UserName == "admin"))
            {
                var (hash, salt) = PasswordHasher.HashPassword("123456");
                db.Accounts.Add(new Account
                {
                    UserName = "admin",
                    Password_Hash = hash,
                    Password_Salt = salt,
                    Password_Algo = "PBKDF2",
                    Password_Iterations = 100000,
                    Role = "Admin",
                    Status = "Active",
                    Create_At = DateTime.UtcNow
                });
                db.SaveChanges();
            }
        }

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

        app.UseAuthentication();
        app.UseMiddleware<AdminMiddleware>();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}