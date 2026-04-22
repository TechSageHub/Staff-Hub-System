using System;
using Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    [DbContext(typeof(EmployeeAppDbContext))]
    [Migration("20260314100000_AddHrTicketComments")]
    public partial class AddHrTicketComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HrTicketComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HrTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommenterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsAdminComment = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrTicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrTicketComments_HrTickets_HrTicketId",
                        column: x => x.HrTicketId,
                        principalTable: "HrTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrTicketComments_HrTicketId",
                table: "HrTicketComments",
                column: "HrTicketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrTicketComments");
        }
    }
}
