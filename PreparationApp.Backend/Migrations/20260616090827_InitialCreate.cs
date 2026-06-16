using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Formateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GoogleCalendarId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Preparations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormateurId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GoogleEventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preparations", x => x.Id);
                    table.CheckConstraint("CK_Preparation_EndDate_After_StartDate", "\r\n                [EndDate] > [StartDate]\r\n            ");
                    table.ForeignKey(
                        name: "FK_Preparations_Formateurs_FormateurId",
                        column: x => x.FormateurId,
                        principalTable: "Formateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparationId = table.Column<int>(type: "int", nullable: false),
                    SubjectsCovered = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DailyObjectives = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReferenceSupports = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DirectoryLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModifiedFiles = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NewExercises = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    WorkDirectoryLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseDurationDays = table.Column<int>(type: "int", nullable: false),
                    TechnicalIssues = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SecondOpinionNeed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondOpinionAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TimeSpent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationReports_Preparations_PreparationId",
                        column: x => x.PreparationId,
                        principalTable: "Preparations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationResources",
                columns: table => new
                {
                    PreparationId = table.Column<int>(type: "int", nullable: false),
                    ResourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationResources", x => new { x.PreparationId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_PreparationResources_Preparations_PreparationId",
                        column: x => x.PreparationId,
                        principalTable: "Preparations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreparationResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreparationReports_PreparationId",
                table: "PreparationReports",
                column: "PreparationId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationResources_PreparationId_ResourceId",
                table: "PreparationResources",
                columns: new[] { "PreparationId", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PreparationResources_ResourceId",
                table: "PreparationResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Preparations_FormateurId",
                table: "Preparations",
                column: "FormateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreparationReports");

            migrationBuilder.DropTable(
                name: "PreparationResources");

            migrationBuilder.DropTable(
                name: "Preparations");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Formateurs");
        }
    }
}
