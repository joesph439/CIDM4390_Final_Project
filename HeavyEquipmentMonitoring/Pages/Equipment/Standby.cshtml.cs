using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class StandbyModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public StandbyModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<HeavyEquipmentMonitoring.Models.Equipment> 
        StandbyEquipment { get; set; } = new();

    public async Task OnGetAsync()
    {
        StandbyEquipment = await _context.Equipment
            .AsNoTracking()
           .Where(e => !e.IsSold && !e.IsOutOfService && !_context.ProjectEquipment.Any(pe => pe.EquipmentID == e.EquipmentID && pe.RemovedDate == null))
            .OrderBy(e => e.EquipmentNumber)
            .ToListAsync();
    }
}
