using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeavyEquipmentMonitoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentTelemetry",
                columns: table => new
                {
                    TelemetryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjEquipID = table.Column<int>(type: "INTEGER", nullable: true),
                    SessionID = table.Column<int>(type: "INTEGER", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EngineStatus = table.Column<string>(type: "TEXT", nullable: false),
                    EngineOnSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalEngineHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentTelemetry", x => x.TelemetryID);
                    table.ForeignKey(
                        name: "FK_EquipmentTelemetry_EquipmentSession_SessionID",
                        column: x => x.SessionID,
                        principalTable: "EquipmentSession",
                        principalColumn: "SessionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentTelemetry_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentTelemetry_ProjectEquipment_ProjEquipID",
                        column: x => x.ProjEquipID,
                        principalTable: "ProjectEquipment",
                        principalColumn: "ProjEquipID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTelemetry_EquipmentID_StartTime",
                table: "EquipmentTelemetry",
                columns: new[] { "EquipmentID", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTelemetry_ProjEquipID_StartTime",
                table: "EquipmentTelemetry",
                columns: new[] { "ProjEquipID", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTelemetry_SessionID_StartTime",
                table: "EquipmentTelemetry",
                columns: new[] { "SessionID", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentTelemetry");
        }
    }
}
