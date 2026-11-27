using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Project Project {get; set;} = new();
    public List<ProjectEquipmentRow> EquipmentList {get; set;} = new();

    // Project summary counts

    public int AssignedEquipment {get; set;}
    public int RunningEquipment {get; set;}
    public int IdleEquipment {get; set;}
    public int MaintenanceEquipment {get; set;}
    public int OffEquipment {get; set;}

    // Equipment table structure

    public class ProjectEquipmentRow
    {
        public int ProjEquipID {get; set;}
        public int EquipmentID {get; set;}
        public string EquipmentNumber {get; set;} = string.Empty;
        public string EquipmentType {get; set;} = string.Empty;
        public string EquipmentModel {get; set;} = string.Empty;
        public string CrewType {get; set;} = string.Empty;
        public double TotalEngineHours {get; set;}
        public string EngineStatus {get; set;} = string.Empty;
        public DateTime AssignedDate {get; set;}
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Retrieve selected project

        var project = await _context.Project.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectID == id);

        if (project == null)
        {
            return NotFound();
        }

        Project = project;

        // Retrieve equipment currently assigned
        // to the selected project

        EquipmentList = await
            (from pe in _context.ProjectEquipment.AsNoTracking()

             join e in _context.Equipment.AsNoTracking()

             on pe.EquipmentID equals e.EquipmentID

             where pe.ProjectID == id
                   && pe.RemovedDate == null
                   && !e.IsSold
                   && !e.IsOutOfService

             orderby pe.CrewType, e.EquipmentNumber

             select new ProjectEquipmentRow
             {
                 ProjEquipID = pe.ProjEquipID,

                 EquipmentID = e.EquipmentID,

                 EquipmentNumber = e.EquipmentNumber.ToString(),

                 EquipmentType = e.EquipmentType,

                 EquipmentModel = e.EquipmentModel,

                 CrewType = pe.CrewType,

                 TotalEngineHours = (double)e.TotalEngineHours,

                 EngineStatus = e.EngineStatus,

                 AssignedDate = pe.AssignedDate

             }).ToListAsync();

        // Calculate project equipment summary

        AssignedEquipment = EquipmentList.Count;

        RunningEquipment = EquipmentList.Count(e => e.EngineStatus == "Running");

        IdleEquipment = EquipmentList.Count(e => e.EngineStatus == "Idle");

        MaintenanceEquipment = EquipmentList.Count(e => e.EngineStatus == "Maintenance");

        OffEquipment = EquipmentList.Count(e => e.EngineStatus == "Off");

        return Page();
    }
}
