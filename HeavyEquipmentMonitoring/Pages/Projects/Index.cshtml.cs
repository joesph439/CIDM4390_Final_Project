
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Project> ProjectList {get; set;} = new();
    public Dictionary<int, int> ProjectEquipmentCounts{get; set;} = new();

    // Search and filter properties

    [BindProperty(SupportsGet = true)]
    public string? SearchString {get; set;}

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter {get; set;}

    // Project summary counts

    public int TotalProjects {get; set;}
    public int UpcomingProjects {get; set;}
    public int ActiveProjects {get; set;}
    public int CompletedProjects {get; set;}
    public int AssignedEquipment {get; set;}
    public int MatchingProjects {get; set;}

    public async Task OnGetAsync()
    {
        // Retrieve all projects

        var query = _context.Project.AsNoTracking();

        // Calculate project summary counts

        TotalProjects = await query.CountAsync();

        UpcomingProjects = await query.CountAsync(p => p.ProjectStatus == "Upcoming");

        ActiveProjects = await query.CountAsync(p => p.ProjectStatus == "Active");

        CompletedProjects = await query.CountAsync(p => p.ProjectStatus == "Completed");

        // Retrieve current equipment assignments

        var activeAssignments = _context.ProjectEquipment.AsNoTracking().Where(pe => pe.RemovedDate == null);

        AssignedEquipment = await activeAssignments.Select(pe => pe.EquipmentID).Distinct().CountAsync();

        // Search by project name

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            var search = SearchString.Trim().ToLower();

            query = query.Where(p => p.ProjectName.ToLower().Contains(search));
        }

        // Filter by project status

        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            query = query.Where(p => p.ProjectStatus == StatusFilter);
        }

        // Retrieve matching projects

        ProjectList = await query
            .OrderByDescending(p => p.StartDate)
            .ThenBy(p => p.ProjectName)
            .ToListAsync();

        MatchingProjects = ProjectList.Count;

        // Count equipment assigned to each project

        ProjectEquipmentCounts = await activeAssignments.GroupBy(pe => pe.ProjectID)
            .Select(g => new
            {
                ProjectID = g.Key,
                EquipmentCount = g
                    .Select(pe => pe.EquipmentID)
                    .Distinct()
                    .Count()
            })
            .ToDictionaryAsync(
                g => g.ProjectID,
                g => g.EquipmentCount);
    }
}
