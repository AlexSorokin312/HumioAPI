using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class UseCompositeKeysForAccessAndPromocodeUsages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_user_module_access",
                table: "user_module_access");

            migrationBuilder.DropIndex(
                name: "ix_user_module_access_user_id_module_id",
                table: "user_module_access");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promocode_usages",
                table: "promocode_usages");

            migrationBuilder.DropIndex(
                name: "ix_promocode_usages_user_id_promocode_id",
                table: "promocode_usages");

            migrationBuilder.DropColumn(
                name: "id",
                table: "user_module_access");

            migrationBuilder.DropColumn(
                name: "id",
                table: "promocode_usages");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_module_access",
                table: "user_module_access",
                columns: new[] { "user_id", "module_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_promocode_usages",
                table: "promocode_usages",
                columns: new[] { "user_id", "promocode_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_user_module_access",
                table: "user_module_access");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promocode_usages",
                table: "promocode_usages");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "user_module_access",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "promocode_usages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_module_access",
                table: "user_module_access",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_promocode_usages",
                table: "promocode_usages",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_user_module_access_user_id_module_id",
                table: "user_module_access",
                columns: new[] { "user_id", "module_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promocode_usages_user_id_promocode_id",
                table: "promocode_usages",
                columns: new[] { "user_id", "promocode_id" },
                unique: true);
        }
    }
}
