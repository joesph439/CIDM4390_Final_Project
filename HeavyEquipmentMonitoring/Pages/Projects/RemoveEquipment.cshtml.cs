using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class RemoveEquipmentModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public RemoveEquipmentModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Project Project {get; set;} = new();

    public HeavyEquipmentMonitoring.Models.Equipment Equipment { get; set; } = new();
    public ProjectEquipment Assignment {get; set;} = new();
    public bool CanRemove {get; set;}

    public string RemovalBlockReason {get; set;} = string.Empty;

    // Load current equipment assignment

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

    // Check equipment removal eligibility

    private async Task CheckRemovalEligibilityAsync()
    {
        CanRemove = false;

        RemovalBlockReason = string.Empty;

        // Project must be active

        if (Project.ProjectStatus != "Active")
        {
            RemovalBlockReason = "Equipment can only be removed from an active project.";

            return;
        }

        // Equipment must be available

        if (Equipment.IsSold || Equipment.IsOutOfService)
        {
            RemovalBlockReason = "Archived equipment cannot be returned to standby.";

            return;
        }

        // Engine must be off

        if (Equipment.EngineStatus != "Off")
        {
            RemovalBlockReason = "The equipment engine must be Off before removal.";

            return;
        }

        // Check active operator sessions

        bool activeSession = await _context.EquipmentSession.AnyAsync(es => es.EquipmentID == Equipment.EquipmentID && es.EndTime == null);

        if (activeSession)
        {
            RemovalBlockReason = "Equipment has an active operator session. " + "End the session before removing equipment.";

            return;
        }

        // Check for conflicting assignments

        bool conflictingAssignment =
            await _context.ProjectEquipment.AnyAsync(pe =>
                pe.EquipmentID == Equipment.EquipmentID &&
                pe.ProjEquipID != Assignment.ProjEquipID &&
                pe.RemovedDate == null);

        if (conflictingAssignment)
        {
            RemovalBlockReason = "Equipment has conflicting active assignments.";

            return;
        }

        CanRemove = true;
    }

    // Load removal confirmation page

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAssignmentAsync(id))
        {
            return NotFound();
        }

        await CheckRemovalEligibilityAsync();

        return Page();
    }

    // Process equipment removal

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAssignmentAsync(id))
        {
            return NotFound();
        }

        // Recheck eligibility before saving

        await CheckRemovalEligibilityAsync();

        if (!CanRemove)
        {
            ModelState.AddModelError(string.Empty, RemovalBlockReason);

            return Page();
        }

        // Close current assignment

        Assignment.RemovedDate = DateTime.UtcNow;

        // Preserve equipment and assignment history

        await _context.SaveChangesAsync();

        return RedirectToPage("./Details",
            new { id = Project.ProjectID });
    }
}
