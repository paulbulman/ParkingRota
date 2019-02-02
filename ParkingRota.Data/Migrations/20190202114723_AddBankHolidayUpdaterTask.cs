namespace ParkingRota.Data.Migrations
{
    using System;
    using Business.Model;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddBankHolidayUpdaterTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.InsertData(
                table: "ScheduledTasks",
                columns: new[] { "ScheduledTaskType", "NextRunTime" },
                values: new object[,]
                {
                    { (int)ScheduledTaskType.BankHolidayUpdater, new DateTime(2019, 1, 28) }
                });

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DeleteData(
                table: "ScheduledTasks",
                keyColumn: "ScheduledTaskType",
                keyValues: new object[] { (int)ScheduledTaskType.BankHolidayUpdater });
    }
}