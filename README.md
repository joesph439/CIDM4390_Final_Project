## Real-Time Equipment Simulation

The application will simulate heavy equipment operating in real time without requiring physical IoT hardware. Users will be able to start and stop simulated equipment, monitor engine hours, assign operators, and view equipment activity through a live web dashboard.

The backend will automatically update equipment operating hours and generate simulated diagnostic alerts. The system will also track preventive maintenance schedules, daily inspections, and equipment usage history.

The simulation will use ASP.NET Core background services, Entity Framework Core, SQLite, and SignalR to process and display equipment information in real time.

### Daily Equipment Inspection and Alert System

Each piece of equipment will require one completed daily inspection before regular operation. Once the inspection is completed, it will remain valid for the rest of the equipment's workday, even if multiple operators use the machine.

If equipment begins operating without a completed inspection, the system will allow a 60-minute grace period for equipment warmup or inspection completion. If the inspection is still missing after 60 minutes, the application will automatically generate an inspection violation alert (INSP-001) and notify the assigned supervisor and crew lead.

The system will continuously track simulated engine hours regardless of inspection status. Inspection records, missing-inspection violations, and notifications will be stored in the database for accountability and reporting.
