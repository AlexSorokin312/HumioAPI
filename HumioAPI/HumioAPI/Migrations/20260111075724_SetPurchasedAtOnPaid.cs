using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class SetPurchasedAtOnPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION set_purchases_purchased_at()
                RETURNS trigger AS $$
                BEGIN
                    IF NEW.status = 'paid' AND NEW.purchased_at IS NULL THEN
                        NEW.purchased_at := now();
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_purchases_set_purchased_at
                BEFORE INSERT OR UPDATE OF status, purchased_at
                ON purchases
                FOR EACH ROW
                EXECUTE FUNCTION set_purchases_purchased_at();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_purchases_set_purchased_at ON purchases;
                DROP FUNCTION IF EXISTS set_purchases_purchased_at();
                """);
        }
    }
}
