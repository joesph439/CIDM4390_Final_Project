using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Services;

public class EngineHourService
{
    private readonly ApplicationDbContext _context;
    private readonly SimulationClock _simulationClock;
    public EngineHourService(ApplicationDbContext context, SimulationClock simulationClock)
    {
        _context = context;
        _simulationClock = simulationClock;
    }

    private DateTime GetCurrentSimulationTime()
    {
        return _simulationClock.UtcNowSeconds;
    }

    public async Task<bool> RecordEngineHoursAsync( int equipmentID, DateTime startTime, DateTime endTime)
    {
        // Validate timestamps

        if (startTime.Kind != DateTimeKind.Utc || endTime.Kind != DateTimeKind.Utc)
        {
            return false;
        }

        if (endTime <= startTime)
        {
            return false;
        }

        // Calculate elapsed engine seconds

        long elapsedSeconds = (long)(endTime - startTime).TotalSeconds;

        if (elapsedSeconds <= 0)
        {
            return false;
        }

        // Preserve fractional seconds for the next interval

        endTime = startTime.AddSeconds(elapsedSeconds);

        // Begin database transaction

        await using var transaction = _context.Database.CurrentTransaction == null? await _context.Database.BeginTransactionAsync() : null;

        // Retrieve equipment

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == equipmentID);

        if (equipment == null)
        {
            return false;
        }

        // Ensure usage begins at the last saved checkpoint

        if (!equipment.LastTelemetryAt.HasValue || equipment.LastTelemetryAt.Value != startTime)
        {
            return false;
        }

        // Prevent recording future usage

        if (endTime > GetCurrentSimulationTime())
        {
            return false;
        }

        // Prevent archived equipment from accumulating hours

        if (equipment.IsSold || equipment.IsOutOfService)
        {
            return false;
        }

        // Only Running and Idle accumulate engine hours

        if (equipment.EngineStatus != "Running" && equipment.EngineStatus != "Idle")
        {
            return false;
        }

        // Prevent duplicate or overlapping usage records

        bool overlappingTelemetry = await _context.EquipmentTelemetry.AnyAsync(et =>
                et.EquipmentID == equipmentID &&
                et.StartTime < endTime &&
                et.EndTime > startTime);

        if (overlappingTelemetry)
        {
            return false;
        }

        // Retrieve project and crew assignments
        // that overlap the usage interval

        var assignments = await _context.ProjectEquipment.Where(pe =>
                pe.EquipmentID == equipmentID &&
                pe.AssignedDate < endTime &&
                (pe.RemovedDate == null || pe.RemovedDate > startTime))
            .ToListAsync();

        // Reject conflicting or changing assignments

        if (assignments.Count > 1)
        {
            return false;
        }

        var assignment = assignments.SingleOrDefault();

        if (assignment != null && (assignment.AssignedDate > startTime || (assignment.RemovedDate.HasValue && assignment.RemovedDate.Value < endTime)))
        {
            return false;
        }

        // Retrieve operator sessions
        // that overlap the usage interval

        var sessions = await _context.EquipmentSession
            .Where(es =>
                es.EquipmentID == equipmentID &&
                es.StartTime < endTime &&
                (es.EndTime == null ||
                 es.EndTime > startTime))
            .ToListAsync();

        // Reject conflicting or changing operator sessions

        if (sessions.Count > 1)
        {
            return false;
        }

        var session = sessions.SingleOrDefault();

        if (session != null &&
            (session.StartTime > startTime ||
             (session.EndTime.HasValue &&
              session.EndTime.Value < endTime)))
        {
            return false;
        }

        // Convert elapsed seconds into engine hours

        double accumulatedHours =
            elapsedSeconds / 3600.0;

        // Update lifetime engine hours

        equipment.TotalEngineHours += accumulatedHours;
        equipment.LastTelemetryAt = endTime;

        // Create historical telemetry record

        var telemetry = new EquipmentTelemetry
        {
            EquipmentID = equipment.EquipmentID,

            ProjEquipID = assignment?.ProjEquipID,

            SessionID = session?.SessionID,

            StartTime = startTime,

            EndTime = endTime,

            EngineStatus = equipment.EngineStatus,

            EngineOnSeconds = elapsedSeconds,

            TotalEngineHours = equipment.TotalEngineHours
        };

        _context.EquipmentTelemetry.Add(telemetry);

        // Save equipment and telemetry together

        await _context.SaveChangesAsync();

        if (transaction != null)
        {
            await transaction.CommitAsync();
        }

        return true;
    }
}
