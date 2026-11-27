namespace HeavyEquipmentMonitoring.Models;

public class Equipment
{
    public int EquipmentID {get; set;}
    public string EquipmentNumber {get; set;} = string.Empty;
    public string EquipmentType {get; set;} = string.Empty;
    public string EquipmentModel {get; set;} = string.Empty;
    public double TotalEngineHours {get; set;}
    public string EngineStatus {get; set;} = string.Empty;
    public DateTime? EngineStartedAt {get; set;}
    public DateTime? EngineStatusChangedAt {get; set;}
    public DateTime? LastTelemetryAt {get; set;}
    public bool IsSold {get; set;} = false;
    public DateTime? SoldDate {get; set;}
    public bool IsOutOfService {get; set;} = false;
    public DateTime? OutOfServiceDate {get; set;}
    public string? ArchiveReason {get; set;}

    // Automatic simulation properties
    public bool IsManualOverride {get; set;} = false;
    public DateTime? ManualOverrideAt {get; set;}
    public DateTime? NextEngineTransitionAt {get; set;}
}