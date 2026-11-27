using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeavyEquipmentMonitoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticSimulationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManualOverride",
                table: "Equipment",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualOverrideAt",
                table: "Equipment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextEngineTransitionAt",
                table: "Equipment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SimulationSettings",
                columns: table => new
                {
                    SimulationSettingsID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperatingMode = table.Column<string>(type: "TEXT", nullable: false),
                    SimulationSpeed = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkStartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WorkEndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WorkMonday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkTuesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkWednesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkThursday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkFriday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkSaturday = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkSunday = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinimumRunningMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumRunningMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumIdleMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumIdleMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumOffMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumOffMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    SimulationTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationSettings", x => x.SimulationSettingsID);
                    table.CheckConstraint("CK_SimulationSettings_Mode", "OperatingMode IN ('Manual', 'Automatic', 'Hybrid')");
                    table.CheckConstraint("CK_SimulationSettings_SingleRecord", "SimulationSettingsID = 1");
                    table.CheckConstraint("CK_SimulationSettings_Speed", "SimulationSpeed IN (1, 10, 60)");
                });

            migrationBuilder.InsertData(
                table: "SimulationSettings",
                columns: new[] { "SimulationSettingsID", "MaximumIdleMinutes", "MaximumOffMinutes", "MaximumRunningMinutes", "MinimumIdleMinutes", "MinimumOffMinutes", "MinimumRunningMinutes", "OperatingMode", "SimulationSpeed", "SimulationTimeUtc", "UpdatedAtUtc", "WorkEndTime", "WorkFriday", "WorkMonday", "WorkSaturday", "WorkStartTime", "WorkSunday", "WorkThursday", "WorkTuesday", "WorkWednesday" },
                values: new object[] { 1, 15, 30, 90, 2, 5, 15, "Manual", 1, null, null, new TimeSpan(0, 17, 0, 0, 0), true, true, true, new TimeSpan(0, 7, 0, 0, 0), false, true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimulationSettings");

            migrationBuilder.DropColumn(
                name: "IsManualOverride",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "ManualOverrideAt",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "NextEngineTransitionAt",
                table: "Equipment");
        }
    }
}
