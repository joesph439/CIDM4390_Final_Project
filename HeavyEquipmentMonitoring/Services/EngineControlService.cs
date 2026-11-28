using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;

namespace HeavyEquipmentMonitoring.Services;

public class EngineControlService
{
    private readonly ApplicationDbContext _context;
    private readonly EngineHourService _engineHourService;
    private readonly SimulationClock _simulationClock;
    public EngineControlService(ApplicationDbContext context, EngineHourService engineHourService, SimulationClock simulationClock)
    {
        _context = context;
        _engineHourService = engineHourService;
        _simulationClock = simulationClock;
    }

    // Start or resume equipment

    public async Task<bool> StartEngineAsync(int equipmentID)
    {
        return await ChangeEngineStatusAsync(equipmentID, "Running");
    }

    // Set equipment to idle

    public async Task<bool> IdleEngineAsync(int equipmentID)
    {
        return await ChangeEngineStatusAsync(equipmentID, "Idle");
    }

    // Stop equipment

    public async Task<bool> StopEngineAsync(int equipmentID)
    {
        return await ChangeEngineStatusAsync(equipmentID, "Off");
    }

    // Central engine status management

    private async Task<bool> ChangeEngineStatusAsync(int equipmentID, string newStatus)
    {
        // Validate requested status

        if (newStatus != "Running" && newStatus != "Idle" && newStatus != "Off")
        {
            return false;
        }

        // Begin database transaction

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Retrieve equipment

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == equipmentID);

        if (equipment == null)
        {
            return false;
        }

        // Prevent archived equipment from operating

        if (equipment.IsSold || equipment.IsOutOfService)
        {
            return false;
        }

        // Prevent operation during maintenance

        if (equipment.EngineStatus == "Maintenance")
        {
            return false;
        }

        // Validate existing engine status

        string previousStatus = equipment.EngineStatus;

        if (previousStatus != "Running" &&
            previousStatus != "Idle" &&
            previousStatus != "Off")
        {
            return false;
        }

        // Prevent unnecessary status changes

        if (previousStatus == newStatus)
        {
            return true;
        }

        // Determine previous and requested engine state

        bool wasEngineOn = previousStatus == "Running" || previousStatus == "Idle";

        bool willEngineBeOn = newStatus == "Running" || newStatus == "Idle";

        // Use whole-second UTC timestamps

        DateTime now = GetCurrentSimulationTime();

        // Record pending engine hours before changing status

        if (wasEngineOn)
        {
            if (!equipment.LastTelemetryAt.HasValue)
            {
                return false;
            }

            DateTime lastTelemetryAt = DateTime.SpecifyKind(equipment.LastTelemetryAt.Value, DateTimeKind.Utc);

            if (now < lastTelemetryAt)
            {
                return false;
            }

            if (now > lastTelemetryAt)
            {
                bool recorded = await _engineHourService.RecordEngineHoursAsync(equipmentID, lastTelemetryAt, now);

                if (!recorded)
                {
                    return false;
                }
            }
        }

        // Starting a new engine operating cycle

        if (!wasEngineOn && willEngineBeOn)
        {
            equipment.EngineStartedAt = now;
        }

        // Ending the current engine operating cycle

        if (wasEngineOn && !willEngineBeOn)
        {
            equipment.EngineStartedAt = null;
        }

        // Update engine status

        equipment.EngineStatus = newStatus;

        equipment.EngineStatusChangedAt = now;

        equipment.LastTelemetryAt = now;

        // Save equipment changes

        await _context.SaveChangesAsync();

        // Commit engine usage and status together

        await transaction.CommitAsync();

        return true;
    }
    private DateTime GetCurrentSimulationTime()
    {
        return _simulationClock.UtcNowSeconds;
    }

}
