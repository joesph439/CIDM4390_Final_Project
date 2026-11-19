namespace HeavyEquipmentMonitoring.Models;

public class AlertRecipient
{
    public int RecipientID {get; set;}
    public int AlertID {get; set;}
    public string UserID {get; set;} = string.Empty;
    public bool IsRead {get; set;}
}