using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using BankSystem.Models;
using BankSystem.Services;
using BankSystem.Options;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql;



public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddConsole();

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Register MoMoApiOptions from configuration
        builder.Services.Configure<MoMoApiOptions>(builder.Configuration.GetSection("MoMoApiOptions"));

        // Register the MtnMomoService with an HttpClient
        builder.Services.AddHttpClient<MtnMomoService>();

        // Register ApplicationDbContext with MySQL configuration
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySQL(connectionString));

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

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel((context, options) =>
                {
                    var certPath = context.Configuration["Kestrel:Certificates:Default:Path"];
                    var certPassword = context.Configuration["Kestrel:Certificates:Default:Password"];

                    if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
                    {
                        throw new InvalidOperationException("Certificate path or password is not configured properly.");
                    }

                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
                    });
                })
                .UseUrls("https://localhost:5002", "http://localhost:5001"); 
            });
}
