namespace ParkingRota.Data.Migrations
{
    using System;
    using Business.Model;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddSummaryTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.InsertData(
                table: "ScheduledTasks",
                columns: new[] { "ScheduledTaskType", "NextRunTime" },
                values: new object[,]
                {
                    { (int)ScheduledTaskType.DailySummary, new DateTime(2018, 12, 1) },
                    { (int)ScheduledTaskType.WeeklySummary, new DateTime(2018, 12, 1) }
                });

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DeleteData(
                table: "ScheduledTasks",
                keyColumn: "ScheduledTaskType",
                keyValues: new object[] { (int)ScheduledTaskType.DailySummary, (int)ScheduledTaskType.WeeklySummary });
    }
}