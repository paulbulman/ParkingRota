using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class AddNearbyDistanceParameter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NearbyDistance",
                table: "SystemParameterLists",
                nullable: false,
                defaultValue: 3.99m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NearbyDistance",
                table: "SystemParameterLists");
        }
    }
}
