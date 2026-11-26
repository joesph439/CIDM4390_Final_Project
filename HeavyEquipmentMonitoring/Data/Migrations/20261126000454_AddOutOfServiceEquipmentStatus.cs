using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeavyEquipmentMonitoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutOfServiceEquipmentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "Equipment",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutOfService",
                table: "Equipment",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutOfServiceDate",
                table: "Equipment",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IsOutOfService",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "OutOfServiceDate",
                table: "Equipment");
        }
    }
}
