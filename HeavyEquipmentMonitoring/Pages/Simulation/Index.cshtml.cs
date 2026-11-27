using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using HeavyEquipmentMonitoring.Services;

namespace HeavyEquipmentMonitoring.Pages.Simulation;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    private readonly EngineControlService _engineControlService;

    public IndexModel(ApplicationDbContext context, EngineControlService engineControlService)
    {
        _context = context;

        _engineControlService = engineControlService;
    }

    // Equipment records

    public List<SimulationEquipmentRow> EquipmentList {get; set;} = new();

    // Recent telemetry records

    public List<TelemetryRow> RecentTelemetry {get; set;} = new();

    // Dashboard summary cards

    public int TotalEquipment {get; set;}
    public int RunningEquipment {get; set;}
    public int IdleEquipment {get; set;}
    public int OffEquipment {get; set;}
    public int MaintenanceEquipment {get; set;}

    // Operation feedback messages

    [TempData]
    public string? SuccessMessage {get; set;}

    [TempData]
    public string? ErrorMessage {get; set;}

    // Equipment display model

    public class SimulationEquipmentRow
    {
        public int EquipmentID {get; set;}

        public string EquipmentNumber {get; set;} = string.Empty;
        public string EquipmentType {get; set;} = string.Empty;
        public string EquipmentModel {get; set;} = string.Empty;
        public string ProjectName {get; set;} = "Standby";
        public string CrewType {get; set;} = "Unassigned";
        public string EngineStatus {get; set;} = string.Empty;
        public double TotalEngineHours {get; set;}
        public DateTime? EngineStartedAt {get; set;}
        public DateTime? EngineStatusChangedAt {get; set;}
        public DateTime? LastTelemetryAt {get; set;}
        public bool HasConflictingAssignments {get; set;}
        public bool CanOperate => !HasConflictingAssignments &&
            (EngineStatus == "Off" ||
             ((EngineStatus == "Running" || EngineStatus == "Idle") &&
              LastTelemetryAt.HasValue));
    }

    // Telemetry display model

    public class TelemetryRow
    {
        public int TelemetryID {get; set;}
        public string EquipmentNumber {get; set;} = string.Empty;
        public string EngineStatus {get; set;} = string.Empty;
        public DateTime StartTime {get; set;}
        public DateTime EndTime {get; set;}
        public long EngineOnSeconds {get; set;}
        public double TotalEngineHours {get; set;}
        public double RecordedEngineHours => EngineOnSeconds / 3600.0;
    }

    // Load the Simulation Control Panel

    public async Task OnGetAsync()
    {
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        // Retrieve all non-archived equipment

        EquipmentList = await _context.Equipment.AsNoTracking().Where(e => !e.IsSold && !e.IsOutOfService)
            .OrderBy(e => e.EquipmentNumber)
            .Select(e => new SimulationEquipmentRow
            {
                EquipmentID = e.EquipmentID,

                EquipmentNumber = e.EquipmentNumber.ToString(),

                EquipmentType = e.EquipmentType,

                EquipmentModel = e.EquipmentModel,

                EngineStatus = e.EngineStatus,

                TotalEngineHours = e.TotalEngineHours,

                EngineStartedAt = e.EngineStartedAt,

                EngineStatusChangedAt = e.EngineStatusChangedAt,

                LastTelemetryAt = e.LastTelemetryAt
            })
            .ToListAsync();

        // Retrieve current project and crew assignments

        var equipmentIDs = EquipmentList.Select(e => e.EquipmentID).ToList();

        var assignments = await(from pe in _context.ProjectEquipment.AsNoTracking()

             join p in _context.Project.AsNoTracking()
                on pe.ProjectID equals p.ProjectID

             where pe.RemovedDate == null && equipmentIDs.Contains(pe.EquipmentID)

             select new
             {
                 pe.EquipmentID,
                 p.ProjectName,
                 pe.CrewType
             })
            .ToListAsync();

        var assignmentLookup = assignments.ToLookup(a => a.EquipmentID);

        // Associate equipment with its current assignment

        foreach (var equipment in EquipmentList)
        {
            var currentAssignments = assignmentLookup[equipment.EquipmentID].ToList();

            if (currentAssignments.Count > 1)
            {
                equipment.HasConflictingAssignments = true;

                equipment.ProjectName = "Assignment Conflict";

                equipment.CrewType = "Unknown";

                continue;
            }

            var assignment = currentAssignments.SingleOrDefault();

            if (assignment != null)
            {
                equipment.ProjectName = assignment.ProjectName;

                equipment.CrewType = assignment.CrewType;
            }
        }

        // Calculate summary card values

        TotalEquipment = EquipmentList.Count;

        RunningEquipment = EquipmentList.Count(e => e.EngineStatus == "Running");

        IdleEquipment = EquipmentList.Count(e => e.EngineStatus == "Idle");

        OffEquipment = EquipmentList.Count(e => e.EngineStatus == "Off");

        MaintenanceEquipment = EquipmentList.Count(e => e.EngineStatus == "Maintenance");

        // Retrieve the 20 most recent telemetry records

        RecentTelemetry = await
            (from et in _context.EquipmentTelemetry.AsNoTracking()

             join e in _context.Equipment.AsNoTracking()
                on et.EquipmentID equals e.EquipmentID

             orderby et.EndTime descending, et.TelemetryID descending

             select new TelemetryRow
             {
                 TelemetryID = et.TelemetryID,

                 EquipmentNumber = e.EquipmentNumber.ToString(),

                 EngineStatus = et.EngineStatus,

                 StartTime = et.StartTime,

                 EndTime = et.EndTime,

                 EngineOnSeconds = et.EngineOnSeconds,

                 TotalEngineHours = et.TotalEngineHours
             })
            .Take(20)
            .ToListAsync();
    }

    // Start or resume equipment

    public async Task<IActionResult> OnPostStartAsync(int equipmentID)
    {
        return await ExecuteCommandAsync(equipmentID, "Start");
    }

    // Set equipment to Idle

    public async Task<IActionResult> OnPostIdleAsync(int equipmentID)
    {
        return await ExecuteCommandAsync(equipmentID, "Idle");
    }

    // Stop equipment

    public async Task<IActionResult> OnPostStopAsync(int equipmentID)
    {
        return await ExecuteCommandAsync(equipmentID, "Stop");
    }

    // Central command execution

    private async Task<IActionResult> ExecuteCommandAsync(int equipmentID, string command)
    {
        var equipment = await _context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.EquipmentID == equipmentID);

        if (equipment == null)
        {
            return NotFound();
        }

        if (equipment.IsSold || equipment.IsOutOfService)
        {
            ErrorMessage = "Archived equipment cannot be operated.";

            return RedirectToPage();
        }

        if (equipment.EngineStatus == "Maintenance")
        {
            ErrorMessage = "Equipment under maintenance cannot be operated.";

            return RedirectToPage();
        }

        int activeAssignments = await _context.ProjectEquipment.CountAsync(pe => pe.EquipmentID == equipmentID && pe.RemovedDate == null);

        if (activeAssignments > 1 && command != "Stop")
        {
            ErrorMessage = "Equipment has conflicting project assignments.";

            return RedirectToPage();
        }

        bool success = command switch
        {
            "Start" => await _engineControlService.StartEngineAsync(equipmentID),

            "Idle" => await _engineControlService.IdleEngineAsync(equipmentID),

            "Stop" => await _engineControlService.StopEngineAsync(equipmentID),

            _ => false
        };

        if (success)
        {
            SuccessMessage = command switch
            {
                "Start" => $"Equipment #{equipment.EquipmentNumber} is Running.",

                "Idle" => $"Equipment #{equipment.EquipmentNumber} is Idle.",

                "Stop" => $"Equipment #{equipment.EquipmentNumber} is Off.",

                _ => "Engine command completed."
            };
        }
        else
        {
            ErrorMessage =
                $"Unable to execute {command} for equipment " +
                $"#{equipment.EquipmentNumber}. " +
                "Check the equipment status and telemetry checkpoint.";
        }

        return RedirectToPage();
    }
}
