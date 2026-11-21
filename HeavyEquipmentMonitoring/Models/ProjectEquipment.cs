namespace HeavyEquipmentMonitoring.Models;

public class ProjectEquipment
{
    public int ProjEquipID {get; set;}
    public int ProjectID {get; set;}
    public int EquipmentID {get; set;}
    public string CrewType { get; set; } = string.Empty;
    public DateTime AssignedDate {get; set;}
    public DateTime? RemovedDate {get; set;}
}