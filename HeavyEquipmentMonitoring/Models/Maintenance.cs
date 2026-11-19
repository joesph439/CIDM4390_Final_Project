namespace HeavyEquipmentMonitoring.Models;

public class Maintenance
{
    public int MaintenanceID {get; set;}
    public int EquipmentID {get; set;}
    public DateTime ServiceDate {get; set;}
    public double TotalEngineHours {get; set;}
}