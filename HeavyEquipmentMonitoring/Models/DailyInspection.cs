namespace HeavyEquipmentMonitoring.Models;

public class DailyInspection
{
    public int DailyID {get; set;}
    public int InspectionID {get; set;}
    public string CheckListItem {get; set;} = string.Empty;
    public string Status {get; set;} = string.Empty;
}