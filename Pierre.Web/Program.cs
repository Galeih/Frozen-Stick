using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pierre.Web.Application.Interfaces;
using Pierre.Web.Application.Services;
using Pierre.Web.Configuration;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Infrastructure.Data;
using Pierre.Web.Infrastructure.Data.Repositories;
using Pierre.Web.Infrastructure.Email;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine("logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);
    });

    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection(EmailSettings.SectionName));

    builder.Services.Configure<AdminSeedSettings>(
        builder.Configuration.GetSection(AdminSeedSettings.SectionName));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.SlidingExpiration = true;
    });

    builder.Services.AddScoped<IContentRepository, ContentRepository>();
    builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
    builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
    builder.Services.AddScoped<IClientRepository, ClientRepository>();
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    builder.Services.AddScoped<ContentService>();
    builder.Services.AddScoped<ClientService>();
    builder.Services.AddScoped<AppointmentService>();
    builder.Services.AddScoped<DatabaseSeeder>();
    builder.Services.AddScoped<DataSeeder>();

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/Admin");
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();

            if (app.Environment.IsDevelopment())
            {
                var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await dataSeeder.SeedDevelopmentDataAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database initialization skipped (design-time or no connection)");
        }
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
