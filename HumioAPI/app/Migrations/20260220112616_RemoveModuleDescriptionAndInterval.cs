using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModuleDescriptionAndInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_modules_interval_count_positive",
                table: "modules");

            migrationBuilder.DropColumn(
                name: "description",
                table: "modules");

            migrationBuilder.DropColumn(
                name: "interval_count",
                table: "modules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "modules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "interval_count",
                table: "modules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "ck_modules_interval_count_positive",
                table: "modules",
                sql: "interval_count > 0");
        }
    }
}
