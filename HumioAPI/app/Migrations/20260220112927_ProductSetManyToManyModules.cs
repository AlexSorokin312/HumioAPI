using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProductSetManyToManyModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_set_modules",
                columns: table => new
                {
                    product_set_id = table.Column<long>(type: "bigint", nullable: false),
                    module_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_set_modules", x => new { x.product_set_id, x.module_id });
                    table.ForeignKey(
                        name: "fk_product_set_modules_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_set_modules_product_set_product_set_id",
                        column: x => x.product_set_id,
                        principalTable: "product_set",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_set_modules_module_id",
                table: "product_set_modules",
                column: "module_id");

            migrationBuilder.Sql("""
                INSERT INTO product_set_modules (product_set_id, module_id)
                SELECT id, module_id
                FROM product_set;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_product_set_modules_module_id",
                table: "product_set");

            migrationBuilder.DropIndex(
                name: "ix_product_set_module_id",
                table: "product_set");

            migrationBuilder.DropColumn(
                name: "module_id",
                table: "product_set");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "module_id",
                table: "product_set",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE product_set AS ps
                SET module_id = map.module_id
                FROM (
                    SELECT product_set_id, MIN(module_id) AS module_id
                    FROM product_set_modules
                    GROUP BY product_set_id
                ) AS map
                WHERE ps.id = map.product_set_id;
                """);

            migrationBuilder.Sql("""
                UPDATE product_set
                SET module_id = (SELECT id FROM modules ORDER BY id LIMIT 1)
                WHERE module_id IS NULL;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "module_id",
                table: "product_set",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_set_module_id",
                table: "product_set",
                column: "module_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_set_modules_module_id",
                table: "product_set",
                column: "module_id",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable(
                name: "product_set_modules");
        }
    }
}
