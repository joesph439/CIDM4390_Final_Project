using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HeavyEquipmentMonitoring.Data;
using HeavyEquipmentMonitoring.Models;
using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Pages.Projects;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProjectInput Input {get; set;} = new();

    public class ProjectInput
    {
        [Required(ErrorMessage = "Enter a project name.")]
        [StringLength(150)]
        public string ProjectName {get; set;} = string.Empty;

        [Required(ErrorMessage = "Select a project status.")]
        public string ProjectStatus {get; set;} = "Upcoming";

        [Required(ErrorMessage = "Select a start date.")]
        [DataType(DataType.Date)]
        public DateTime? StartDate {get; set;}
    }

    public void OnGet()
    {
        Input.StartDate = DateTime.Today;
        Input.ProjectStatus = "Upcoming";
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

        // Prevent duplicate project names

        bool projectExists = await _context.Project.AnyAsync(p => p.ProjectName.ToLower() == projectName.ToLower());

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

        // Create the project

        var project = new Project
        {
            ProjectName = projectName,
            ProjectStatus = Input.ProjectStatus,
            StartDate = Input.StartDate!.Value.Date,
            EndDate = null
        };

        _context.Project.Add(project);

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
