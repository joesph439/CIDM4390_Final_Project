using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class AssignCrewModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AssignCrewModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Project Project {get; set;} = new();
    public HeavyEquipmentMonitoring.Models.Equipment Equipment {get; set;} = new();
    public ProjectEquipment Assignment {get; set;} = new();
    public List<string> CrewTypes {get;} = new()
    {
        "DIRT",
        "ASPHALT",
        "CONCRETE",
        "UTILITIES"
    };

    [BindProperty]
    public CrewInput Input { get; set; } = new();
    public class CrewInput
    {
        [Required(ErrorMessage = "Select a new crew.")]
        public string CrewType { get; set; } = string.Empty;
    }

    // Load current assignment information

    private async Task<bool> LoadAssignmentAsync(int id)
    {
        var assignment = await _context.ProjectEquipment.FirstOrDefaultAsync(pe => pe.ProjEquipID == id && pe.RemovedDate == null);

        if (assignment == null)
        {
            return false;
        }

        var project = await _context.Project.FirstOrDefaultAsync(p => p.ProjectID == assignment.ProjectID);

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == assignment.EquipmentID);

        if (project == null || equipment == null)
        {
            return false;
        }

        Assignment = assignment;

        Project = project;

        Equipment = equipment;

        return true;
    }

    // Check equipment transfer eligibility

    private async Task<bool> CanTransferAsync()
    {
        if (Project.ProjectStatus != "Active")
        {
            return false;
        }

        if (Equipment.IsSold || Equipment.IsOutOfService || Equipment.EngineStatus != "Off")
        {
            return false;
        }

        bool activeSession = await _context.EquipmentSession.AnyAsync(es => es.EquipmentID == Equipment.EquipmentID && es.EndTime == null);

        return !activeSession;
    }

    // Load crew transfer page

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAssignmentAsync(id))
        {
            return NotFound();
        }

        if (Project.ProjectStatus != "Active")
        {
            return RedirectToPage("./Details", new { id = Project.ProjectID });
        }

        return Page();
    }

    // Process crew transfer

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAssignmentAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate selected crew

        if (!CrewTypes.Contains(Input.CrewType))
        {
            ModelState.AddModelError(
                "Input.CrewType",
                "Select a valid crew type.");

            return Page();
        }

        // Prevent transfer to current crew

        if (Assignment.CrewType == Input.CrewType)
        {
            ModelState.AddModelError(
                "Input.CrewType",
                "Equipment is already assigned to this crew.");

            return Page();
        }

        // Verify transfer eligibility

        if (!await CanTransferAsync())
        {
            ModelState.AddModelError(string.Empty,
                "Equipment is not currently eligible for a crew transfer.");

            return Page();
        }

        // Use the same timestamp for both records

        var transferTime = DateTime.UtcNow;

        // Close existing crew assignment

        Assignment.RemovedDate = transferTime;

        // Create new crew assignment

        var newAssignment = new ProjectEquipment
        {
            ProjectID = Assignment.ProjectID,

            EquipmentID = Assignment.EquipmentID,

            CrewType = Input.CrewType,

            AssignedDate = transferTime,

            RemovedDate = null
        };

        _context.ProjectEquipment.Add(newAssignment);

        // Save both changes together

        await _context.SaveChangesAsync();

        return RedirectToPage("./Details",
            new { id = Project.ProjectID });
    }
}
