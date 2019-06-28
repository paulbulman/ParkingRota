using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class AddLastServiceRunTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastServiceRunTime",
                table: "SystemParameterLists",
                nullable: false,
                defaultValue: new DateTime(2019, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastServiceRunTime",
                table: "SystemParameterLists");
        }
    }
}
