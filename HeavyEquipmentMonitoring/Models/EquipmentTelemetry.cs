namespace HeavyEquipmentMonitoring.Models;

public class EquipmentTelemetry
{
    public int TelemetryID {get; set;}
    public int EquipmentID {get; set;}
    public int? ProjEquipID {get; set;}
    public int? SessionID {get; set;}
    public DateTime StartTime {get; set;}
    public DateTime EndTime {get; set;}
    public string EngineStatus {get; set;} = string.Empty;
    public long EngineOnSeconds {get; set;}
    public double TotalEngineHours {get; set;}
}
