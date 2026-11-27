using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class AssignEquipmentModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AssignEquipmentModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Project Project {get; set;} = new();

    public List<HeavyEquipmentMonitoring.Models.Equipment> AvailableEquipment {get; set;} = new();

    public List<string> CrewTypes {get;} = new()
    {
        "DIRT",
        "ASPHALT",
        "CONCRETE",
        "UTILITIES"
    };

    [BindProperty]
    public AssignmentInput Input {get; set;} = new();

    public class AssignmentInput
    {
        [Required(ErrorMessage = "Select equipment.")]
        [Range(1, int.MaxValue, ErrorMessage = "Select valid equipment.")]
        public int? EquipmentID { get; set; }

        [Required(ErrorMessage = "Select a crew type.")]
        public string CrewType {get; set;} = string.Empty;
    }

    // Retrieve eligible standby equipment

    private IQueryable<HeavyEquipmentMonitoring.Models.Equipment>
        GetAvailableEquipment()
    {
        return _context.Equipment.AsNoTracking().Where(e => !e.IsSold && !e.IsOutOfService && e.EngineStatus == "Off" &&

                !_context.ProjectEquipment.Any(pe =>
                    pe.EquipmentID == e.EquipmentID &&
                    pe.RemovedDate == null) &&

                !_context.EquipmentSession.Any(es =>
                    es.EquipmentID == e.EquipmentID &&
                    es.EndTime == null));
    }

    // Populate equipment dropdown

    private async Task LoadAvailableEquipmentAsync()
    {
        AvailableEquipment = await GetAvailableEquipment().OrderBy(e => e.EquipmentType).ThenBy(e => e.EquipmentNumber).ToListAsync();
    }

    // Load assignment page

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _context.Project.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectID == id);

        if (project == null)
        {
            return NotFound();
        }

        if (project.ProjectStatus != "Active")
        {
            return RedirectToPage("./Details", new { id });
        }

        Project = project;

        await LoadAvailableEquipmentAsync();

        return Page();
    }

    // Save equipment assignment

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var project = await _context.Project.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectID == id);

        if (project == null)
        {
            return NotFound();
        }

        if (project.ProjectStatus != "Active")
        {
            return RedirectToPage("./Details", new { id });
        }

        Project = project;

        await LoadAvailableEquipmentAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate crew type

        if (!CrewTypes.Contains(Input.CrewType))
        {
            ModelState.AddModelError(
                "Input.CrewType",
                "Select a valid crew type.");

            return Page();
        }

        // Verify equipment is still available

        var equipment = await GetAvailableEquipment().FirstOrDefaultAsync(e => e.EquipmentID == Input.EquipmentID);

        if (equipment == null)
        {
            ModelState.AddModelError(
                "Input.EquipmentID",
                "This equipment is no longer available for assignment.");

            return Page();
        }

        // Create project equipment assignment

        var assignment = new ProjectEquipment
        {
            ProjectID = project.ProjectID,

            EquipmentID = equipment.EquipmentID,

            CrewType = Input.CrewType,

            AssignedDate = DateTime.UtcNow,

            RemovedDate = null
        };

        _context.ProjectEquipment.Add(assignment);

        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id });
    }
}
