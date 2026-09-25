using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925133000_NormalizeUserRoles")]
public class NormalizeUserRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE "Users"
            SET "Role" = 'User'
            WHERE "Role" IS NULL OR "Role" = '' OR "Role" = 'RegisteredCustomer';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE "Users"
            SET "Role" = 'RegisteredCustomer'
            WHERE "Role" = 'User';
            """);
    }
}
