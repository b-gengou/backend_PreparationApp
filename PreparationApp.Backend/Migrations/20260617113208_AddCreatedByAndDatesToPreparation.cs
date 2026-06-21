using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByAndDatesToPreparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Preparations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Preparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Preparations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Preparations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Formateurs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Formateurs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Preparations_CreatedById",
                table: "Preparations",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations",
                column: "CreatedById",
                principalTable: "Formateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preparations_Formateurs_CreatedById",
                table: "Preparations");

            migrationBuilder.DropIndex(
                name: "IX_Preparations_CreatedById",
                table: "Preparations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Preparations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Preparations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Preparations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Preparations");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Formateurs");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Formateurs");
        }
    }
}
