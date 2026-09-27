using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260927124000_AddEventMediaPresentation")]
public partial class AddEventMediaPresentation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "MediaType",
            table: "Events",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "MediaUrl",
            table: "Events",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SlideshowUrlsJson",
            table: "Events",
            type: "text",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "Events"
            SET "MediaType" = 1,
                "MediaUrl" = '/events/za-galya.webp'
            WHERE "Slug" = 'blagotvoritelen-koncert-za-galya-2026'
              AND "MediaType" = 0
              AND "MediaUrl" IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "MediaType", table: "Events");
        migrationBuilder.DropColumn(name: "MediaUrl", table: "Events");
        migrationBuilder.DropColumn(name: "SlideshowUrlsJson", table: "Events");
    }
}
