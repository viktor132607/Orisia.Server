using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260927192000_AddPostMediaUrlAndBackfillContentImages")]
public partial class AddPostMediaUrlAndBackfillContentImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MediaUrl",
            table: "Posts",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "Posts"
            SET "MediaUrl" = '/events/za-galya.webp'
            WHERE "Type" = 0
              AND "IsDeleted" = false
              AND ("MediaUrl" IS NULL OR "MediaUrl" = '');
            """);

        migrationBuilder.Sql("""
            UPDATE "Events"
            SET "MediaType" = 1,
                "MediaUrl" = '/events/za-galya.webp'
            WHERE "IsDeleted" = false
              AND (
                    "MediaType" = 0
                    OR ("MediaType" = 1 AND ("MediaUrl" IS NULL OR "MediaUrl" = ''))
                  );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MediaUrl",
            table: "Posts");
    }
}
