namespace HeavyEquipmentMonitoring.Models;

public class Alert
{
    public int AlertID {get; set;}
    public int EquipmentID {get; set;}
    public string AlertCode {get; set;} = string.Empty;
    public string Message {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;}
    public DateTime? ResolvedAt {get; set;}
}