namespace HeavyEquipmentMonitoring.Models;

public class ProjectEquipment
{
    public int ProjEquipID {get; set;}
    public int ProjectID {get; set;}
    public int EquipmentID {get; set;}
    public DateTime AssignedDate {get; set;}
    public DateTime? RemovedDate {get; set;}
}