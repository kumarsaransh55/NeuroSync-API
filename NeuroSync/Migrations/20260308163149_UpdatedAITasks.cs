using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeuroSync__API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAITasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimatedTime",
                table: "TaskSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Heading",
                table: "TaskSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "TaskSteps");

            migrationBuilder.DropColumn(
                name: "Heading",
                table: "TaskSteps");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Tasks");
        }
    }
}
