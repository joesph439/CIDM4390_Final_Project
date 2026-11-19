namespace HeavyEquipmentMonitoring.Models;

public class Project
{
    public int ProjectID {get; set;}
    public string ProjectName {get; set;} = string.Empty;
    public string ProjectStatus {get; set;} = string.Empty;
    public DateTime StartDate {get; set;}
    public DateTime? EndDate {get; set;}
}