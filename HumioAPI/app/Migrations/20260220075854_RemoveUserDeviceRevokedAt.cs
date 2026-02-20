using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserDeviceRevokedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "users_devices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "revoked_at",
                table: "users_devices",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
