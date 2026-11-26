using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public HeavyEquipmentMonitoring.Models.Equipment Equipment { get; set; } = new();

    public void OnGet()
    {
        Equipment.EngineStatus = "Off";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Equipment.EngineStatus = "Off";

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check for duplicate equipment numbers
        bool equipmentExists = await _context.Equipment.AnyAsync(e => e.EquipmentNumber == Equipment.EquipmentNumber);

        if (equipmentExists)
        {
            ModelState.AddModelError(
                "Equipment.EquipmentNumber",
                "This equipment number already exists.");

            return Page();
        }

        // Add equipment to the database
        _context.Equipment.Add(Equipment);

        await _context.SaveChangesAsync();

        // Return to Equipment Inventory
        return RedirectToPage("./Index");
    }
}
