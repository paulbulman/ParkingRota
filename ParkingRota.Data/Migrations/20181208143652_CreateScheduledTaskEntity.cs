namespace ParkingRota.Data.Migrations
{
    using System;
    using Business.Model;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class CreateScheduledTaskEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduledTaskType = table.Column<int>(nullable: false),
                    NextRunTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTasks_ScheduledTaskType",
                table: "ScheduledTasks",
                column: "ScheduledTaskType",
                unique: true);

            migrationBuilder.InsertData(
                table: "ScheduledTasks",
                columns: new[] { "ScheduledTaskType", "NextRunTime" },
                values: new object[,]
                {
                    { (int)ScheduledTaskType.ReservationReminder, new DateTime(2018, 12, 1) },
                    { (int)ScheduledTaskType.RequestReminder, new DateTime(2018, 12, 1) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable(name: "ScheduledTasks");
    }
}
