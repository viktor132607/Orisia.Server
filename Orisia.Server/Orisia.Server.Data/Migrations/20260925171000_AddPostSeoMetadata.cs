using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925171000_AddPostSeoMetadata")]
public class AddPostSeoMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SeoDescriptionBg",
            table: "Posts",
            type: "character varying(180)",
            maxLength: 180,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SeoDescriptionEn",
            table: "Posts",
            type: "character varying(180)",
            maxLength: 180,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SeoTitleBg",
            table: "Posts",
            type: "character varying(70)",
            maxLength: 70,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SeoTitleEn",
            table: "Posts",
            type: "character varying(70)",
            maxLength: 70,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "SeoDescriptionBg", table: "Posts");
        migrationBuilder.DropColumn(name: "SeoDescriptionEn", table: "Posts");
        migrationBuilder.DropColumn(name: "SeoTitleBg", table: "Posts");
        migrationBuilder.DropColumn(name: "SeoTitleEn", table: "Posts");
    }
}
