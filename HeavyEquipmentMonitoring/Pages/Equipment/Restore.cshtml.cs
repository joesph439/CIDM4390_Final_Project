using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class RestoreModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public RestoreModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public HeavyEquipmentMonitoring.Models.Equipment Equipment{get; set;} = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var equipment = await _context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (equipment == null || equipment.IsSold || !equipment.IsOutOfService)
        {
            return NotFound();
        }

        Equipment = equipment;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (equipment == null || equipment.IsSold || !equipment.IsOutOfService)
        {
            return NotFound();
        }

        Equipment = equipment;

        // Check for an active project assignment
        bool isAssigned = await _context.ProjectEquipment.AnyAsync(pe => pe.EquipmentID == id && pe.RemovedDate == null);

        if (isAssigned)
        {
            ModelState.AddModelError(string.Empty, "Equipment cannot be restored with an active project assignment.");

            return Page();
        }

        // Check for an active operating session
        bool hasActiveSession = await _context.EquipmentSession.AnyAsync(es => es.EquipmentID == id && es.EndTime == null);

        if (hasActiveSession)
        {
            ModelState.AddModelError(string.Empty, "Equipment cannot be restored with an active operating session.");

            return Page();
        }

        // Restore equipment to active inventory
        equipment.IsOutOfService = false;
        equipment.OutOfServiceDate = null;
        equipment.ArchiveReason = null;
        equipment.EngineStatus = "Off";

        await _context.SaveChangesAsync();

        return RedirectToPage("./Standby");
    }
}
