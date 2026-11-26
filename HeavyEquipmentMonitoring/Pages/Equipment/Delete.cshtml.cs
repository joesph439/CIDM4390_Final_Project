using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public HeavyEquipmentMonitoring.Models.Equipment Equipment{get; set;} = new();

    [BindProperty]
    [Required(ErrorMessage = "Select an archive status.")]
    public string ArchiveStatus {get; set;} = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Provide a reason for archiving.")]
    [StringLength(500)]
    public string ArchiveReason {get; set;} = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (equipment == null || equipment.IsSold)
        {
            return NotFound();
        }

        Equipment = equipment;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (equipment == null || equipment.IsSold)
        {
            return NotFound();
        }

        Equipment = equipment;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate the selected archive status
        if (ArchiveStatus != "Sold" && ArchiveStatus != "OutOfService")
        {
            ModelState.AddModelError(nameof(ArchiveStatus), "Invalid archive status.");
            return Page();
        }

        // Check for an active project assignment
        bool isAssigned = await _context.ProjectEquipment.AnyAsync(pe => pe.EquipmentID == id && pe.RemovedDate == null);

        if (isAssigned)
        {
            ModelState.AddModelError(string.Empty, "Remove this equipment from its project before archiving.");
            return Page();
        }

        // Check for an active operating session
        bool hasActiveSession = await _context.EquipmentSession.AnyAsync(es => es.EquipmentID == id && es.EndTime == null);

        if (hasActiveSession)
        {
            ModelState.AddModelError(string.Empty, "Equipment has an active operating session.");
            return Page();
        }

        // Prevent archiving operating equipment
        if (equipment.EngineStatus == "Running" || equipment.EngineStatus == "Idle")
        {
            ModelState.AddModelError(string.Empty, "Turn off the equipment before archiving.");
            return Page();
        }

        // Prevent duplicate Out of Service actions
        if (equipment.IsOutOfService && ArchiveStatus == "OutOfService")
        {
            ModelState.AddModelError(string.Empty, "This equipment is already out of service.");
            return Page();
        }

        // Update equipment lifecycle status
        if (ArchiveStatus == "Sold")
        {
            equipment.IsSold = true;
            equipment.SoldDate = DateTime.UtcNow;

            equipment.IsOutOfService = false;
            equipment.OutOfServiceDate = null;
        }
        
        else
        {
            equipment.IsOutOfService = true;
            equipment.OutOfServiceDate = DateTime.UtcNow;
        }

        equipment.ArchiveReason = ArchiveReason.Trim();
        equipment.EngineStatus = "Off";

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
