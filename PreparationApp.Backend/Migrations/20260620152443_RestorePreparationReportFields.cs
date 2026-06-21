using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RestorePreparationReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "CourseDurationDays",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DailyObjectives",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DirectoryLink",
                table: "Reports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Reports",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedFiles",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewExercises",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDate",
                table: "Reports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReferenceSupports",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondOpinionAction",
                table: "Reports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondOpinionNeed",
                table: "Reports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubjectsCovered",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TechnicalIssues",
                table: "Reports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeSpent",
                table: "Reports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkDirectoryLink",
                table: "Reports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseDurationDays",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DailyObjectives",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DirectoryLink",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ModifiedFiles",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "NewExercises",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReferenceSupports",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SecondOpinionAction",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SecondOpinionNeed",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SubjectsCovered",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TechnicalIssues",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TimeSpent",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WorkDirectoryLink",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
