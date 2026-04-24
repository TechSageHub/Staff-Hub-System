using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeOffboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOffboarded",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OffboardedAt",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeOffboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastWorkingDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOffboardings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOffboardings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OffboardingChecklistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffboardingChecklistItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOffboardingProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeOffboardingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffboardingChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOffboardingProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOffboardingProgress_EmployeeOffboardings_EmployeeOffboardingId",
                        column: x => x.EmployeeOffboardingId,
                        principalTable: "EmployeeOffboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeOffboardingProgress_OffboardingChecklistItems_OffboardingChecklistItemId",
                        column: x => x.OffboardingChecklistItemId,
                        principalTable: "OffboardingChecklistItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOffboardingProgress_EmployeeOffboardingId_OffboardingChecklistItemId",
                table: "EmployeeOffboardingProgress",
                columns: new[] { "EmployeeOffboardingId", "OffboardingChecklistItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOffboardingProgress_OffboardingChecklistItemId",
                table: "EmployeeOffboardingProgress",
                column: "OffboardingChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOffboardings_EmployeeId",
                table: "EmployeeOffboardings",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OffboardingChecklistItems_Key",
                table: "OffboardingChecklistItems",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeOffboardingProgress");

            migrationBuilder.DropTable(
                name: "EmployeeOffboardings");

            migrationBuilder.DropTable(
                name: "OffboardingChecklistItems");

            migrationBuilder.DropColumn(
                name: "IsOffboarded",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OffboardedAt",
                table: "Employees");
        }
    }
}
