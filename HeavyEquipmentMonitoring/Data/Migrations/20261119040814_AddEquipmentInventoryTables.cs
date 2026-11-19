using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeavyEquipmentMonitoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentInventoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentNumber = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentType = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentModel = table.Column<string>(type: "TEXT", nullable: false),
                    TotalEngineHours = table.Column<double>(type: "REAL", nullable: false),
                    EngineStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentID);
                });

            migrationBuilder.CreateTable(
                name: "Operator",
                columns: table => new
                {
                    OperatorID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operator", x => x.OperatorID);
                    table.ForeignKey(
                        name: "FK_Operator_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectStatus = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.ProjectID);
                });

            migrationBuilder.CreateTable(
                name: "Alert",
                columns: table => new
                {
                    AlertID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertCode = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alert", x => x.AlertID);
                    table.ForeignKey(
                        name: "FK_Alert_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inspection",
                columns: table => new
                {
                    InspectionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    UserID = table.Column<string>(type: "TEXT", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspection", x => x.InspectionID);
                    table.ForeignKey(
                        name: "FK_Inspection_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inspection_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Maintenance",
                columns: table => new
                {
                    MaintenanceID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalEngineHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance", x => x.MaintenanceID);
                    table.ForeignKey(
                        name: "FK_Maintenance_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentSession",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperatorID = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndeTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartEngineHour = table.Column<double>(type: "REAL", nullable: false),
                    EndEngineHour = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentSession", x => x.SessionID);
                    table.ForeignKey(
                        name: "FK_EquipmentSession_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentSession_Operator_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "Operator",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEquipment",
                columns: table => new
                {
                    ProjEquipID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RemovedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEquipment", x => x.ProjEquipID);
                    table.ForeignKey(
                        name: "FK_ProjectEquipment_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEquipment_Project_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Project",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectUser",
                columns: table => new
                {
                    ProjectUserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false),
                    UserID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUser", x => x.ProjectUserID);
                    table.ForeignKey(
                        name: "FK_ProjectUser_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectUser_Project_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Project",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertRecipient",
                columns: table => new
                {
                    RecipientID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlertID = table.Column<int>(type: "INTEGER", nullable: false),
                    UserID = table.Column<string>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRecipient", x => x.RecipientID);
                    table.ForeignKey(
                        name: "FK_AlertRecipient_Alert_AlertID",
                        column: x => x.AlertID,
                        principalTable: "Alert",
                        principalColumn: "AlertID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertRecipient_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyInspection",
                columns: table => new
                {
                    DailyID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InspectionID = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentID = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckListItem = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyInspection", x => x.DailyID);
                    table.ForeignKey(
                        name: "FK_DailyInspection_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyInspection_Inspection_InspectionID",
                        column: x => x.InspectionID,
                        principalTable: "Inspection",
                        principalColumn: "InspectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_EquipmentID",
                table: "Alert",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipient_AlertID",
                table: "AlertRecipient",
                column: "AlertID");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipient_UserID",
                table: "AlertRecipient",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyInspection_EquipmentID",
                table: "DailyInspection",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyInspection_InspectionID",
                table: "DailyInspection",
                column: "InspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSession_EquipmentID",
                table: "EquipmentSession",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSession_OperatorID",
                table: "EquipmentSession",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_EquipmentID",
                table: "Inspection",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_UserID",
                table: "Inspection",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_EquipmentID",
                table: "Maintenance",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Operator_UserID",
                table: "Operator",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEquipment_EquipmentID",
                table: "ProjectEquipment",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEquipment_ProjectID",
                table: "ProjectEquipment",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUser_ProjectID",
                table: "ProjectUser",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUser_UserID",
                table: "ProjectUser",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRecipient");

            migrationBuilder.DropTable(
                name: "DailyInspection");

            migrationBuilder.DropTable(
                name: "EquipmentSession");

            migrationBuilder.DropTable(
                name: "Maintenance");

            migrationBuilder.DropTable(
                name: "ProjectEquipment");

            migrationBuilder.DropTable(
                name: "ProjectUser");

            migrationBuilder.DropTable(
                name: "Alert");

            migrationBuilder.DropTable(
                name: "Inspection");

            migrationBuilder.DropTable(
                name: "Operator");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");
        }
    }
}
