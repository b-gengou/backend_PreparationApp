using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreparationReports_Preparations_PreparationId",
                table: "PreparationReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PreparationReports",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CourseDurationDays",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "DailyObjectives",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "DirectoryLink",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "ModifiedFiles",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "NewExercises",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "ReferenceSupports",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "SecondOpinionAction",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "SecondOpinionNeed",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "SubjectsCovered",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "TechnicalIssues",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "TimeSpent",
                table: "PreparationReports");

            migrationBuilder.DropColumn(
                name: "WorkDirectoryLink",
                table: "PreparationReports");

            migrationBuilder.RenameTable(
                name: "PreparationReports",
                newName: "Reports");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Resources",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_PreparationReports_PreparationId",
                table: "Reports",
                newName: "IX_Reports_PreparationId");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations",
                column: "CreatedById",
                principalTable: "Formateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Preparations_PreparationId",
                table: "Reports",
                column: "PreparationId",
                principalTable: "Preparations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Preparations_PreparationId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "PreparationReports");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Resources",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_PreparationId",
                table: "PreparationReports",
                newName: "IX_PreparationReports_PreparationId");

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Resources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CourseDurationDays",
                table: "PreparationReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DailyObjectives",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DirectoryLink",
                table: "PreparationReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PreparationReports",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedFiles",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewExercises",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDate",
                table: "PreparationReports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReferenceSupports",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondOpinionAction",
                table: "PreparationReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondOpinionNeed",
                table: "PreparationReports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubjectsCovered",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TechnicalIssues",
                table: "PreparationReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeSpent",
                table: "PreparationReports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkDirectoryLink",
                table: "PreparationReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PreparationReports",
                table: "PreparationReports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PreparationReports_Preparations_PreparationId",
                table: "PreparationReports",
                column: "PreparationId",
                principalTable: "Preparations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations",
                column: "CreatedById",
                principalTable: "Formateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
