using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSetLocalizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_set_localizations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_set_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_set_localizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_set_localizations_products_product_set_id",
                        column: x => x.product_set_id,
                        principalTable: "product_set",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_set_localizations_product_set_id",
                table: "product_set_localizations",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_set_localizations_product_set_id_language_code",
                table: "product_set_localizations",
                columns: new[] { "product_set_id", "language_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_set_localizations");
        }
    }
}
