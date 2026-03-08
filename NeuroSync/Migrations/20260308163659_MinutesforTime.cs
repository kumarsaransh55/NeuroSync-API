using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeuroSync__API.Migrations
{
    /// <inheritdoc />
    public partial class MinutesforTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "TaskSteps");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "TaskSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMinutes",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "TaskSteps");

            migrationBuilder.DropColumn(
                name: "TotalMinutes",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "EstimatedTime",
                table: "TaskSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
