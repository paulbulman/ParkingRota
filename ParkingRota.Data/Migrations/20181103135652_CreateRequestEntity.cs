using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class CreateRequestEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BankHoliday",
                table: "BankHoliday");

            migrationBuilder.RenameTable(
                name: "BankHoliday",
                newName: "BankHolidays");

            migrationBuilder.RenameIndex(
                name: "IX_BankHoliday_Date",
                table: "BankHolidays",
                newName: "IX_BankHolidays_Date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankHolidays",
                table: "BankHolidays",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicationUserId = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ApplicationUserId_Date",
                table: "Requests",
                columns: new[] { "ApplicationUserId", "Date" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankHolidays",
                table: "BankHolidays");

            migrationBuilder.RenameTable(
                name: "BankHolidays",
                newName: "BankHoliday");

            migrationBuilder.RenameIndex(
                name: "IX_BankHolidays_Date",
                table: "BankHoliday",
                newName: "IX_BankHoliday_Date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankHoliday",
                table: "BankHoliday",
                column: "Id");
        }
    }
}
