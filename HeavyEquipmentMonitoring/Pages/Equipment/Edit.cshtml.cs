using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public HeavyEquipmentMonitoring.Models.Equipment Equipment { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (equipment == null)
        {
            return NotFound();
        }

        Equipment = equipment;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        // Find the existing equipment record
        var existingEquipment = await _context.Equipment.FirstOrDefaultAsync(e => e.EquipmentID == id);

        if (existingEquipment == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check for duplicate equipment numbers
        bool equipmentExists = await _context.Equipment.AnyAsync(e => e.EquipmentNumber == Equipment.EquipmentNumber && e.EquipmentID != id);

        if (equipmentExists)
        {
            ModelState.AddModelError(
                "Equipment.EquipmentNumber",
                "This equipment number already exists.");

            return Page();
        }

        // Validate engine hours
        if (Equipment.TotalEngineHours < 0)
        {
            ModelState.AddModelError(
                "Equipment.TotalEngineHours",
                "Engine hours cannot be negative.");

            return Page();
        }

        // Update equipment information
        existingEquipment.EquipmentNumber = Equipment.EquipmentNumber;
        existingEquipment.EquipmentType = Equipment.EquipmentType;
        existingEquipment.EquipmentModel = Equipment.EquipmentModel;
        existingEquipment.TotalEngineHours = Equipment.TotalEngineHours;

        // Save changes to SQLite
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
