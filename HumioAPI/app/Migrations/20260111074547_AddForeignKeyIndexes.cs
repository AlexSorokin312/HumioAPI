using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_user_module_access_user_id",
                table: "user_module_access",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_promocode_usages_user_id",
                table: "promocode_usages",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_module_access_user_id",
                table: "user_module_access");

            migrationBuilder.DropIndex(
                name: "ix_promocode_usages_user_id",
                table: "promocode_usages");
        }
    }
}
