
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

    public List<HeavyEquipmentMonitoring.Models.Equipment> EquipmentList { get; set; } = new();

    public async Task OnGetAsync()
    {
        EquipmentList = await _context.Equipment.AsNoTracking().OrderBy(e => e.EquipmentNumber).ToListAsync();
    }
}
