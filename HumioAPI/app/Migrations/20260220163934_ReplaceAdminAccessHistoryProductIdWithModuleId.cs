using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAdminAccessHistoryProductIdWithModuleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "admin_access_history",
                newName: "module_id");

            migrationBuilder.RenameIndex(
                name: "ix_admin_access_history_product_id",
                table: "admin_access_history",
                newName: "ix_admin_access_history_module_id");

            migrationBuilder.AddForeignKey(
                name: "fk_admin_access_history_modules_module_id",
                table: "admin_access_history",
                column: "module_id",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_access_history_modules_module_id",
                table: "admin_access_history");

            migrationBuilder.RenameColumn(
                name: "module_id",
                table: "admin_access_history",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "ix_admin_access_history_module_id",
                table: "admin_access_history",
                newName: "ix_admin_access_history_product_id");

            migrationBuilder.AddForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history",
                column: "product_id",
                principalTable: "product_set",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
