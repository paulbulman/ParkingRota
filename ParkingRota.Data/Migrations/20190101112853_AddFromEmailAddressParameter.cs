using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class AddFromEmailAddressParameter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromEmailAddress",
                table: "SystemParameterLists",
                nullable: false,
                defaultValue: "noreply@parkingrota");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromEmailAddress",
                table: "SystemParameterLists");
        }
    }
}
