using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;

namespace HeavyEquipmentMonitoring.Services;

public class EngineSimulationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EngineSimulationService> _logger;
    private readonly SimulationClock _simulationClock;
    private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _telemetryInterval = TimeSpan.FromSeconds(60);
    public EngineSimulationService(IServiceScopeFactory scopeFactory, ILogger<EngineSimulationService> logger, SimulationClock simulationClock)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _simulationClock = simulationClock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Engine Simulation Service started.");

        // Recover equipment states before monitoring

        await RecoverEquipmentAsync(stoppingToken);

        using var timer = new PeriodicTimer(_monitoringInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await MonitorEquipmentAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error monitoring equipment.");
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown
        }

        _logger.LogInformation("Engine Simulation Service stopped.");
    }

    private async Task MonitorEquipmentAsync(CancellationToken stoppingToken)
    {
        List<int> equipmentIDs;

        // Retrieve active equipment IDs

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            equipmentIDs = await context.Equipment.AsNoTracking().Where(e =>
                    !e.IsSold &&
                    !e.IsOutOfService &&
                    (e.EngineStatus == "Running" || e.EngineStatus == "Idle"))
                .Select(e => e.EquipmentID)
                .ToListAsync(stoppingToken);
        }

        // Process each machine independently

        foreach (int equipmentID in equipmentIDs)
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var engineHourService = scope.ServiceProvider.GetRequiredService<EngineHourService>();

                var equipment = await context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.EquipmentID == equipmentID, stoppingToken);

                if (equipment == null || equipment.IsSold || equipment.IsOutOfService)
                {
                    continue;
                }

                if (equipment.EngineStatus != "Running" && equipment.EngineStatus != "Idle")
                {
                    continue;
                }

                if (!equipment.LastTelemetryAt.HasValue)
                {
                    _logger.LogWarning("Equipment {EquipmentID} has no telemetry checkpoint.", equipmentID);

                    continue;
                }

                DateTime lastTelemetryAt = DateTime.SpecifyKind(equipment.LastTelemetryAt.Value, DateTimeKind.Utc);

                DateTime now = GetUtcTimestamp();

                TimeSpan elapsed = now - lastTelemetryAt;

                // Only save when enough time has elapsed

                if (elapsed < _telemetryInterval)
                {
                    continue;
                }

                bool recorded = await engineHourService.RecordEngineHoursAsync( equipmentID, lastTelemetryAt, now);

                if (recorded)
                {
                    _logger.LogInformation("Equipment {EquipmentID}: recorded {Seconds} engine seconds.", equipmentID, elapsed.TotalSeconds);
                }
                else
                {
                    _logger.LogWarning("Telemetry recording rejected for equipment {EquipmentID}.", equipmentID);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording equipment {EquipmentID}.", equipmentID);
            }
        }
    }

    private async Task RecoverEquipmentAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

        var equipmentList = await context.Equipment.Where(e => e.EngineStatus == "Running" || e.EngineStatus == "Idle").ToListAsync(stoppingToken);

        DateTime now = GetUtcTimestamp();

        foreach (var equipment in equipmentList)
        {
            // Preserve previously saved lifetime hours.
            // Do not accumulate usage while offline.

            equipment.EngineStatus = "Off";

            equipment.EngineStartedAt = null;

            equipment.EngineStatusChangedAt = now;

            equipment.LastTelemetryAt = now;
        }

        await context.SaveChangesAsync(stoppingToken);

        await transaction.CommitAsync(stoppingToken);

        _logger.LogInformation("Engine recovery completed. {Count} machines reset to Off.", equipmentList.Count);
    }

    private DateTime GetUtcTimestamp()
    {
        return _simulationClock.UtcNowSeconds;
    }
}
