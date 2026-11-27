using HeavyEquipmentMonitoring.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HeavyEquipmentMonitoring.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Equipment> Equipment {get; set;}
    public DbSet<EquipmentSession> EquipmentSession {get; set;}
    public DbSet<EquipmentTelemetry> EquipmentTelemetry {get; set;}
    public DbSet<Inspection> Inspection {get; set;}
    public DbSet<DailyInspection> DailyInspection {get; set;}
    public DbSet<Maintenance> Maintenance {get; set;}
    public DbSet<Operator> Operator {get; set;}
    public DbSet<Project> Project {get; set;}
    public DbSet<ProjectEquipment> ProjectEquipment {get; set;}
    public DbSet<ProjectUser> ProjectUser {get; set;}
    public DbSet<Alert> Alert {get; set;}
    public DbSet<AlertRecipient> AlertRecipient {get; set;}
    public DbSet<SimulationSettings> SimulationSettings {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Primary Keys
        modelBuilder.Entity<Equipment>().HasKey(e => e.EquipmentID);

        modelBuilder.Entity<EquipmentSession>().HasKey(s => s.SessionID);

        modelBuilder.Entity<Inspection>().HasKey(i => i.InspectionID);

        modelBuilder.Entity<DailyInspection>().HasKey(d => d.DailyID);

        modelBuilder.Entity<Maintenance>().HasKey(m => m.MaintenanceID);

        modelBuilder.Entity<Operator>().HasKey(o => o.OperatorID);

        modelBuilder.Entity<Project>().HasKey(p => p.ProjectID);

        modelBuilder.Entity<ProjectEquipment>().HasKey(pe => pe.ProjEquipID);

        modelBuilder.Entity<ProjectUser>().HasKey(pu => pu.ProjectUserID);

        modelBuilder.Entity<Alert>().HasKey(a => a.AlertID);

        modelBuilder.Entity<AlertRecipient>().HasKey(ar => ar.RecipientID);

        modelBuilder.Entity<EquipmentTelemetry>().HasKey(et => et.TelemetryID);

        modelBuilder.Entity<SimulationSettings>().HasKey(s => s.SimulationSettingsID);

        // Table Relationships

        // Equipment -> EquipmentSession
        modelBuilder.Entity<EquipmentSession>().HasOne<Equipment>().WithMany().HasForeignKey(s => s.EquipmentID);

        // Equipment -> Inspection
        modelBuilder.Entity<Inspection>().HasOne<Equipment>().WithMany().HasForeignKey(i => i.EquipmentID);

        // Equipment -> DailyInspection
        modelBuilder.Entity<DailyInspection>().HasOne<Equipment>().WithMany().HasForeignKey(d => d.EquipmentID);

        // Equipment -> Maintenance
        modelBuilder.Entity<Maintenance>().HasOne<Equipment>().WithMany().HasForeignKey(m => m.EquipmentID);

        // Equipment -> Alert
        modelBuilder.Entity<Alert>().HasOne<Equipment>().WithMany().HasForeignKey(a => a.EquipmentID);

        // Equipment -> ProjectEquipment
        modelBuilder.Entity<ProjectEquipment>().HasOne<Equipment>().WithMany().HasForeignKey(pe => pe.EquipmentID);

         // Operator -> EquipmentSession
        modelBuilder.Entity<EquipmentSession>().HasOne<Operator>().WithMany().HasForeignKey(s => s.OperatorID);

        // ApplicationUser -> Operator
        modelBuilder.Entity<Operator>().HasOne<ApplicationUser>().WithOne().HasForeignKey<Operator>(o => o.UserID);

        // ApplicationUser -> Inspection
        modelBuilder.Entity<Inspection>().HasOne<ApplicationUser>().WithMany().HasForeignKey(i => i.UserID);

        // ApplicationUser -> ProjectUser
        modelBuilder.Entity<ProjectUser>().HasOne<ApplicationUser>().WithMany().HasForeignKey(pu => pu.UserID);

        // ApplicationUser -> AlertRecipient
        modelBuilder.Entity<AlertRecipient>().HasOne<ApplicationUser>().WithMany().HasForeignKey(ar => ar.UserID);

        // Project -> ProjectEquipment
        modelBuilder.Entity<ProjectEquipment>().HasOne<Project>().WithMany().HasForeignKey(pe => pe.ProjectID);

        // Project -> ProjectUser
        modelBuilder.Entity<ProjectUser>().HasOne<Project>().WithMany().HasForeignKey(pu => pu.ProjectID);

        // Inspection -> DailyInspection
        modelBuilder.Entity<DailyInspection>().HasOne<Inspection>().WithMany().HasForeignKey(d => d.InspectionID);

        // Alert -> AlertRecipient
        modelBuilder.Entity<AlertRecipient>().HasOne<Alert>().WithMany().HasForeignKey(ar => ar.AlertID);

        modelBuilder.Entity<EquipmentTelemetry>().HasOne<Equipment>().WithMany().HasForeignKey(et => et.EquipmentID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentTelemetry>().HasOne<ProjectEquipment>().WithMany().HasForeignKey(et => et.ProjEquipID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentTelemetry>().HasOne<EquipmentSession>().WithMany().HasForeignKey(et => et.SessionID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EquipmentTelemetry>().HasIndex(et => new{et.EquipmentID, et.StartTime});

        modelBuilder.Entity<EquipmentTelemetry>().HasIndex(et => new{et.ProjEquipID, et.StartTime});

        modelBuilder.Entity<EquipmentTelemetry>().HasIndex(et => new{et.SessionID, et.StartTime});

        modelBuilder.Entity<SimulationSettings>().ToTable("SimulationSettings", table =>
        {
            table.HasCheckConstraint("CK_SimulationSettings_SingleRecord", "SimulationSettingsID = 1");

            table.HasCheckConstraint("CK_SimulationSettings_Speed", "SimulationSpeed IN (1, 10, 60)");

            table.HasCheckConstraint("CK_SimulationSettings_Mode", "OperatingMode IN ('Manual', 'Automatic', 'Hybrid')");
        });


        modelBuilder.Entity<SimulationSettings>().HasData(new SimulationSettings
        {
            SimulationSettingsID = 1,

            OperatingMode = "Manual",

            SimulationSpeed = 1,

            WorkStartTime = new TimeSpan(7, 0, 0),

            WorkEndTime = new TimeSpan(17, 0, 0),

            WorkMonday = true,

            WorkTuesday = true,

            WorkWednesday = true,

            WorkThursday = true,

            WorkFriday = true,

            WorkSaturday = true,

            WorkSunday = false,

            MinimumRunningMinutes = 15,

            MaximumRunningMinutes = 90,

            MinimumIdleMinutes = 2,

            MaximumIdleMinutes = 15,

            MinimumOffMinutes = 5,

            MaximumOffMinutes = 30,

            SimulationTimeUtc = null,

            UpdatedAtUtc = null
        });
    }
}
