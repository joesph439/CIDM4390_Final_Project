using HeavyEquipmentMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SimulationClock>();
builder.Services.AddScoped<EngineHourService>();
builder.Services.AddScoped<EngineControlService>();
builder.Services.AddHostedService<EngineSimulationService>();

var app = builder.Build();

// Initialize the shared simulation clock

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var simulationClock = scope.ServiceProvider.GetRequiredService<SimulationClock>();

    var settings = await context.SimulationSettings.SingleAsync(s => s.SimulationSettingsID == 1);

    DateTime startingTimeUtc = settings.SimulationTimeUtc.HasValue
            ? DateTime.SpecifyKind(settings.SimulationTimeUtc.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

    simulationClock.Initialize(startingTimeUtc, settings.SimulationSpeed);

    app.Logger.LogInformation("Simulation clock initialized at {Time:O} with speed {Speed}x.", simulationClock.UtcNow, simulationClock.Speed);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
