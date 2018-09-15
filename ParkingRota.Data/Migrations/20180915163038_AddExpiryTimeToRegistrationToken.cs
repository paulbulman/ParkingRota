using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingRota.Data.Migrations
{
    public partial class AddExpiryTimeToRegistrationToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RegistrationTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryTime",
                table: "RegistrationTokens",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryTime",
                table: "RegistrationTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RegistrationTokens",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
