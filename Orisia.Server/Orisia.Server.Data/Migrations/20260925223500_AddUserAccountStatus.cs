using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925223500_AddUserAccountStatus")]
public class AddUserAccountStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "DeactivatedAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_Users_IsActive_Role",
            table: "Users",
            columns: new[] { "IsActive", "Role" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_IsActive_Role",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DeactivatedAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Users");
    }
}
