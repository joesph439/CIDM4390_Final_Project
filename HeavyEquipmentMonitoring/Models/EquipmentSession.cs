namespace HeavyEquipmentMonitoring.Models;

public class EquipmentSession
{
    public int SessionID {get; set;}
    public int OperatorID {get; set;}
    public int EquipmentID {get; set;}
    public DateTime StartTime {get; set;}
    public DateTime? EndTime {get; set;}
    public double StartEngineHour {get; set;}
    public double? EndEngineHour {get; set;}
}