using System.ComponentModel.DataAnnotations;

namespace HeavyEquipmentMonitoring.Models;

public class SimulationSettings
{
    public int SimulationSettingsID {get; set;}

    // Manual, Automatic or Hybrid

    public string OperatingMode {get; set;} = "Manual";

    // Supported speeds: 1, 10 and 60

    public int SimulationSpeed {get; set;} = 1;

    // Daily construction work schedule

    public TimeSpan WorkStartTime {get; set;} = new TimeSpan(7, 0, 0);
    public TimeSpan WorkEndTime {get; set;} = new TimeSpan(17, 0, 0);

    // Working days

    public bool WorkMonday {get; set;} = true;
    public bool WorkTuesday {get; set;} = true;
    public bool WorkWednesday {get; set;} = true;
    public bool WorkThursday {get; set;} = true;
    public bool WorkFriday {get; set;} = true;
    public bool WorkSaturday {get; set;} = true;
    public bool WorkSunday {get; set;} = false;

    // Automatic running duration

    public int MinimumRunningMinutes {get; set;} = 15;
    public int MaximumRunningMinutes {get; set;} = 90;

    // Automatic idle duration

    public int MinimumIdleMinutes {get; set;} = 2;
    public int MaximumIdleMinutes {get; set;} = 15;

    // Automatic off duration

    public int MinimumOffMinutes {get; set;} = 5;
    public int MaximumOffMinutes {get; set;} = 30;

    // Persistent virtual clock

    public DateTime? SimulationTimeUtc {get; set;}

    // Last saved configuration change in real time

    public DateTime? UpdatedAtUtc {get; set;}
}
