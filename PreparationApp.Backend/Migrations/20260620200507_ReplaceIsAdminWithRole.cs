using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreparationApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsAdminWithRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Formateurs");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Formateurs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Formateurs");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Formateurs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
