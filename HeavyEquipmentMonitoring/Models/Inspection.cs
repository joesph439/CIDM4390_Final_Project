namespace HeavyEquipmentMonitoring.Models;

public class Inspection
{
    public int InspectionID {get; set;}
    public int EquipmentID {get; set;}
    public string UserID {get; set;}= string.Empty;
    public DateTime InspectionDate {get; set;}
    public DateTime? CompletedAt {get; set;}
    public string Status {get; set;} = string.Empty;
}