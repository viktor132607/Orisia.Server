using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925210000_AddDances")]
public class AddDances : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Dances",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                TitleBg = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                TitleEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                DescriptionBg = table.Column<string>(type: "text", nullable: false),
                DescriptionEn = table.Column<string>(type: "text", nullable: false),
                Region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                Rhythm = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                VideoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ThumbnailMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Dances", x => x.Id);
                table.ForeignKey(
                    name: "FK_Dances_Media_ThumbnailMediaId",
                    column: x => x.ThumbnailMediaId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Dances_Active_SortOrder",
            table: "Dances",
            columns: new[] { "Active", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Dances_Region",
            table: "Dances",
            column: "Region");

        migrationBuilder.CreateIndex(
            name: "IX_Dances_Slug",
            table: "Dances",
            column: "Slug",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_Dances_ThumbnailMediaId",
            table: "Dances",
            column: "ThumbnailMediaId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Dances");
    }
}
