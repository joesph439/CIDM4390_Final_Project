using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeavyEquipmentMonitoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineTrackingTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EngineStartedAt",
                table: "Equipment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EngineStatusChangedAt",
                table: "Equipment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTelemetryAt",
                table: "Equipment",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineStartedAt",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "EngineStatusChangedAt",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "LastTelemetryAt",
                table: "Equipment");
        }
    }
}
