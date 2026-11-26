
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Equipment;

public class ArchiveModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ArchiveModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<HeavyEquipmentMonitoring.Models.Equipment>
        ArchivedEquipment {get; set;} = new();

    public async Task OnGetAsync()
    {
        ArchivedEquipment = await _context.Equipment
            .AsNoTracking()
            .Where(e => e.IsSold || e.IsOutOfService)
            .OrderByDescending(e => e.SoldDate ?? e.OutOfServiceDate)
            .ThenBy(e => e.EquipmentNumber)
            .ToListAsync();
    }
}
