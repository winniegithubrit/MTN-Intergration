using BankSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using BankSystem.Options;

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
        // Register MoMoDisbursementOptions from configuration
        builder.Services.Configure<MoMoDisbursementOptions>(builder.Configuration.GetSection("MoMoDisbursementOptions"));

        // Register the MTNMomoService  and MTNDisbursementService with an HttpClient
        builder.Services.AddHttpClient<MTNMomoService>();
        builder.Services.AddHttpClient<MTNDisbursementService>();

        // Add JSON options configuration
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keeps the original property names
                options.JsonSerializerOptions.WriteIndented = true; // For pretty print, optional
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
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
