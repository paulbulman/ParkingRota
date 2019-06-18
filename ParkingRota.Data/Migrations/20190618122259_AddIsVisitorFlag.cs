using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class AddIsVisitorFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisitor",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisitor",
                table: "AspNetUsers");
        }
    }
}
