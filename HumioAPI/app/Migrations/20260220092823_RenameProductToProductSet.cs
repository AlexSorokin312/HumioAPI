using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductToProductSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history");

            migrationBuilder.DropForeignKey(
                name: "fk_promocodes_products_product_id",
                table: "promocodes");

            migrationBuilder.DropForeignKey(
                name: "fk_purchases_products_product_id",
                table: "purchases");

            migrationBuilder.DropForeignKey(
                name: "fk_products_modules_module_id",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "pk_products",
                table: "products");

            migrationBuilder.RenameTable(
                name: "products",
                newName: "product_set");

            migrationBuilder.RenameIndex(
                name: "ix_products_module_id",
                table: "product_set",
                newName: "ix_product_set_module_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_product_set",
                table: "product_set",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_set_modules_module_id",
                table: "product_set",
                column: "module_id",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history",
                column: "product_id",
                principalTable: "product_set",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promocodes_products_product_id",
                table: "promocodes",
                column: "product_id",
                principalTable: "product_set",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchases_products_product_id",
                table: "purchases",
                column: "product_id",
                principalTable: "product_set",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history");

            migrationBuilder.DropForeignKey(
                name: "fk_promocodes_products_product_id",
                table: "promocodes");

            migrationBuilder.DropForeignKey(
                name: "fk_purchases_products_product_id",
                table: "purchases");

            migrationBuilder.DropForeignKey(
                name: "fk_product_set_modules_module_id",
                table: "product_set");

            migrationBuilder.DropPrimaryKey(
                name: "pk_product_set",
                table: "product_set");

            migrationBuilder.RenameTable(
                name: "product_set",
                newName: "products");

            migrationBuilder.RenameIndex(
                name: "ix_product_set_module_id",
                table: "products",
                newName: "ix_products_module_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_products",
                table: "products",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_modules_module_id",
                table: "products",
                column: "module_id",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_admin_access_history_products_product_id",
                table: "admin_access_history",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promocodes_products_product_id",
                table: "promocodes",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchases_products_product_id",
                table: "purchases",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
