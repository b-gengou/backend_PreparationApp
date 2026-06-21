using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Resources",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CreatedById",
                table: "Resources",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Formateurs_CreatedById",
                table: "Resources",
                column: "CreatedById",
                principalTable: "Formateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Formateurs_CreatedById",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_CreatedById",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Resources");
        }
    }
}
