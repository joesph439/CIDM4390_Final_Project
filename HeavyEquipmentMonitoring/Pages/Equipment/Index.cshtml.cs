using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<HeavyEquipmentMonitoring.Models.Equipment>
    EquipmentList {get; set;} = new();
    public List<Project> ProjectList {get; set;} = new();
    public Dictionary<int, string> EquipmentProjects{get; set;} = new();
    public Dictionary<int, string> EquipmentCrews{get; set;} = new();

    // Search and filter properties

    [BindProperty(SupportsGet = true)]
    public string? SearchString {get; set;}

    [BindProperty(SupportsGet = true)]
    public string? AssignmentFilter {get; set;}

    [BindProperty(SupportsGet = true)]
    public int? ProjectFilter {get; set;}

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter {get; set;}

    // Equipment counts

    public int TotalFleet {get; set;}
    public int MatchingEquipment {get; set;}
    public int AssignedEquipment {get; set;}
    public int StandbyEquipment {get; set;}

    // Equipment analytics chart data
    public List<string> TopEquipmentLabels {get; set;} = new();
    public List<double> TopEquipmentHours {get; set;} = new();
    public List<string> EquipmentTypeLabels {get; set;} = new();
    public List<int> EquipmentTypeCounts {get; set;} = new();
    public List<string> EngineStatusLabels {get; set;} = new();
    public List<int> EngineStatusCounts {get; set;} = new();

    public async Task OnGetAsync()
    {
        // Retrieve active fleet inventory

        var query = _context.Equipment.AsNoTracking().Where(e => !e.IsSold && !e.IsOutOfService);

        TotalFleet = await query.CountAsync();

        // Retrieve projects for the dropdown filter

        ProjectList = await _context.Project.AsNoTracking().OrderBy(p => p.ProjectName).ToListAsync();

        // Retrieve current equipment assignments

        var activeAssignments = _context.ProjectEquipment.AsNoTracking().Where(pe => pe.RemovedDate == null);

        // Search equipment

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            var search = SearchString.Trim().ToLower();

            query = query.Where(e =>
                e.EquipmentNumber.ToString().Contains(search) ||
                e.EquipmentType.ToLower().Contains(search) ||
                e.EquipmentModel.ToLower().Contains(search) ||
                activeAssignments.Any(pe => pe.EquipmentID == e.EquipmentID &&
                    (
                        pe.CrewType.ToLower().Contains(search) ||
                        _context.Project.Any(p =>
                            p.ProjectID == pe.ProjectID &&
                            p.ProjectName.ToLower().Contains(search))
                    )));
        }

        // Filter assigned or standby equipment

        if (AssignmentFilter == "Assigned")
        {
            query = query.Where(e => activeAssignments.Any(pe =>pe.EquipmentID == e.EquipmentID));
        }
        else if (AssignmentFilter == "Standby")
        {
            query = query.Where(e => !activeAssignments.Any(pe => pe.EquipmentID == e.EquipmentID));
        }

        // Filter by project

        if (ProjectFilter.HasValue && ProjectFilter.Value > 0)
        {
            query = query.Where(e => activeAssignments.Any(pe => pe.EquipmentID == e.EquipmentID && pe.ProjectID == ProjectFilter.Value));
        }

        // Filter by engine status

        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            query = query.Where(e => e.EngineStatus == StatusFilter);
        }

        // Retrieve filtered equipment

        EquipmentList = await query.OrderBy(e => e.EquipmentNumber).ToListAsync();

        // Retrieve current project and crew information

        var equipmentIDs = EquipmentList.Select(e => e.EquipmentID).ToList();

        var assignments = await (
            from pe in activeAssignments
            join p in _context.Project
                on pe.ProjectID equals p.ProjectID
            where equipmentIDs.Contains(pe.EquipmentID)
            select new
            {
                pe.EquipmentID,
                p.ProjectName,
                pe.CrewType,
                pe.AssignedDate,
                pe.ProjEquipID
            })
            .ToListAsync();

        foreach (var group in assignments.GroupBy(a => a.EquipmentID))
        {
            var assignment = group
                .OrderByDescending(a => a.AssignedDate)
                .ThenByDescending(a => a.ProjEquipID)
                .First();

            EquipmentProjects[assignment.EquipmentID] =
                assignment.ProjectName;

            EquipmentCrews[assignment.EquipmentID] =
                assignment.CrewType;
        }

        // Calculate matching equipment counts

        MatchingEquipment = EquipmentList.Count;

        AssignedEquipment = await query.CountAsync(e =>
            activeAssignments.Any(pe =>
                pe.EquipmentID == e.EquipmentID));

        StandbyEquipment = MatchingEquipment - AssignedEquipment;

        // CHART 1: TOP 5 EQUIPMENT BY ENGINE HOURS

        var topEquipment = EquipmentList.OrderByDescending(e => e.TotalEngineHours).Take(5).ToList();

        TopEquipmentLabels = topEquipment.Select(e => $"{e.EquipmentNumber} - {e.EquipmentModel}").ToList();

        TopEquipmentHours = topEquipment.Select(e => Convert.ToDouble(e.TotalEngineHours)).ToList();

        // CHART 2: EQUIPMENT BY TYPE


        var equipmentTypes = EquipmentList.GroupBy(e => e.EquipmentType).Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ToList();

        EquipmentTypeLabels = equipmentTypes.Select(g => g.Type).ToList();

        EquipmentTypeCounts = equipmentTypes.Select(g => g.Count).ToList();

        // CHART 3: ENGINE STATUS DISTRIBUTION

        EngineStatusLabels = new List<string>
            {
                "Running",
                "Idle",
                "Maintenance",
                "Off"
            };

        EngineStatusCounts = EngineStatusLabels.Select(status => EquipmentList.Count(e => e.EngineStatus == status)).ToList();

    }
}

