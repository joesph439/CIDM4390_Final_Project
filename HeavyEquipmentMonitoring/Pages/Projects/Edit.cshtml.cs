using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int ProjectID {get; set;}
    public string CurrentStatus {get; set;} = string.Empty;
    [BindProperty]
    public ProjectInput Input {get; set;} = new();
    public class ProjectInput
    {
        [Required(ErrorMessage = "Enter a project name.")]
        [StringLength(150)]
        public string ProjectName {get; set;} = string.Empty;

        [Required(ErrorMessage = "Select a project status.")]
        public string ProjectStatus {get; set;} = string.Empty;

        [Required(ErrorMessage = "Select a start date.")]
        [DataType(DataType.Date)]
        public DateTime? StartDate {get; set;}
    }

    // Retrieve existing project

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _context.Project.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectID == id);

        if (project == null || project.ProjectStatus == "Completed")
        {
            return NotFound();
        }

        ProjectID = project.ProjectID;
        CurrentStatus = project.ProjectStatus;

        Input = new ProjectInput
        {
            ProjectName = project.ProjectName,
            ProjectStatus = project.ProjectStatus,
            StartDate = project.StartDate
        };

        return Page();
    }

    // Save project changes

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var project = await _context.Project.FirstOrDefaultAsync(p => p.ProjectID == id);

        if (project == null || project.ProjectStatus == "Completed")
        {
            return NotFound();
        }

        ProjectID = project.ProjectID;
        CurrentStatus = project.ProjectStatus;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var projectName = Input.ProjectName.Trim();

        // Validate project name

        if (string.IsNullOrWhiteSpace(projectName))
        {
            ModelState.AddModelError(
                "Input.ProjectName",
                "Enter a valid project name.");

            return Page();
        }

        // Validate project status

        if (Input.ProjectStatus != "Upcoming" && Input.ProjectStatus != "Active")
        {
            ModelState.AddModelError(
                "Input.ProjectStatus",
                "Select a valid project status.");

            return Page();
        }

        // Prevent active projects from returning to Upcoming

        if (project.ProjectStatus == "Active" && Input.ProjectStatus == "Upcoming")
        {
            ModelState.AddModelError(
                "Input.ProjectStatus",
                "An active project cannot return to Upcoming.");

            return Page();
        }

        // Prevent duplicate project names

        bool projectExists = await _context.Project.AnyAsync(p => p.ProjectID != id && p.ProjectName.ToLower() == projectName.ToLower());

        if (projectExists)
        {
            ModelState.AddModelError(
                "Input.ProjectName",
                "A project with this name already exists.");

            return Page();
        }

        // Validate active project start date

        if (Input.ProjectStatus == "Active" && Input.StartDate!.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(
                "Input.StartDate",
                "An active project cannot have a future start date.");

            return Page();
        }

        // Prevent start date from conflicting with assignment history

        bool conflictingAssignment = await _context.ProjectEquipment.AnyAsync(pe => pe.ProjectID == id && pe.AssignedDate < Input.StartDate!.Value.Date);

        if (conflictingAssignment)
        {
            ModelState.AddModelError(
                "Input.StartDate",
                "The start date cannot be later than an existing equipment assignment.");

            return Page();
        }

        // Update existing project

        project.ProjectName = projectName;
        project.ProjectStatus = Input.ProjectStatus;
        project.StartDate = Input.StartDate!.Value.Date;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
