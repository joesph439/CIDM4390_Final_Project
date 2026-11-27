
using System.Diagnostics;

namespace HeavyEquipmentMonitoring.Services;

public class SimulationClock
{
    private readonly object _lock = new();
    private readonly Stopwatch _stopwatch = new();
    private DateTime _baseSimulationTimeUtc;
    private int _simulationSpeed = 1;
    private bool _isInitialized;
    private readonly TimeZoneInfo _workTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    // Indicates whether the clock has been initialized

    public bool IsInitialized
    {
        get
        {
            lock (_lock)
            {
                return _isInitialized;
            }
        }
    }

    // Current simulation speed

    public int Speed
    {
        get
        {
            lock (_lock)
            {
                return _simulationSpeed;
            }
        }
    }

    // Initialize the simulation clock

    public void Initialize(DateTime simulationTimeUtc, int simulationSpeed)
    {
        ValidateSpeed(simulationSpeed);

        if (simulationTimeUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Simulation time must be UTC.");
        }

        lock (_lock)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Simulation clock is already initialized.");
            }

            _baseSimulationTimeUtc = simulationTimeUtc;

            _simulationSpeed = simulationSpeed;

            _stopwatch.Restart();

            _isInitialized = true;
        }
    }

    // Current simulated UTC time

    public DateTime UtcNow
    {
        get
        {
            lock (_lock)
            {
                EnsureInitialized();

                return CalculateCurrentTime();
            }
        }
    }

    // Whole-second simulated UTC timestamp

    public DateTime UtcNowSeconds
    {
        get
        {
            DateTime now = UtcNow;

            return new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond),DateTimeKind.Utc);
        }
    }

    // Current simulated Central Time

    public DateTime LocalNow
    {
        get
        {
            return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _workTimeZone);
        }
    }

    // Change simulation speed without resetting time

    public void SetSpeed(int newSpeed)
    {
        ValidateSpeed(newSpeed);

        lock (_lock)
        {
            EnsureInitialized();

            if (_simulationSpeed == newSpeed)
            {
                return;
            }

            // Preserve time accumulated at the old speed

            _baseSimulationTimeUtc = CalculateCurrentTime();

            // Start advancing at the new speed

            _simulationSpeed = newSpeed;

            _stopwatch.Restart();
        }
    }

    // Retrieve a consistent clock snapshot

    public (DateTime SimulationTimeUtc, int Speed)
        GetSnapshot()
    {
        lock (_lock)
        {
            EnsureInitialized();

            return (CalculateCurrentTime(), _simulationSpeed
            );
        }
    }

    // Calculate elapsed simulated time

    private DateTime CalculateCurrentTime()
    {
        double simulatedSeconds =
            _stopwatch.Elapsed.TotalSeconds *
            _simulationSpeed;

        return _baseSimulationTimeUtc.AddSeconds(
            simulatedSeconds);
    }

    // Validate supported speeds

    private static void ValidateSpeed(int speed)
    {
        if (speed != 1 &&
            speed != 10 &&
            speed != 60)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Simulation speed must be 1, 10 or 60.");
        }
    }

    // Prevent access before initialization

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Simulation clock has not been initialized.");
        }
    }
}
