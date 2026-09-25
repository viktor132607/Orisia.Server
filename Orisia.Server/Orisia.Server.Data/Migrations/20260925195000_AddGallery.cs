using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925195000_AddGallery")]
public class AddGallery : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GalleryAlbums",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                TitleBg = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                TitleEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                DescriptionBg = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CoverMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                Featured = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GalleryAlbums", x => x.Id);
                table.ForeignKey(
                    name: "FK_GalleryAlbums_Media_CoverMediaId",
                    column: x => x.CoverMediaId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "GalleryMedia",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                GalleryAlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                CaptionBg = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CaptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GalleryMedia", x => x.Id);
                table.ForeignKey(
                    name: "FK_GalleryMedia_GalleryAlbums_GalleryAlbumId",
                    column: x => x.GalleryAlbumId,
                    principalTable: "GalleryAlbums",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GalleryMedia_Media_MediaId",
                    column: x => x.MediaId,
                    principalTable: "Media",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GalleryAlbums_Active_Featured_SortOrder",
            table: "GalleryAlbums",
            columns: new[] { "Active", "Featured", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_GalleryAlbums_CoverMediaId",
            table: "GalleryAlbums",
            column: "CoverMediaId");

        migrationBuilder.CreateIndex(
            name: "IX_GalleryAlbums_Slug",
            table: "GalleryAlbums",
            column: "Slug",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_GalleryMedia_GalleryAlbumId_MediaId",
            table: "GalleryMedia",
            columns: new[] { "GalleryAlbumId", "MediaId" },
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_GalleryMedia_GalleryAlbumId_SortOrder",
            table: "GalleryMedia",
            columns: new[] { "GalleryAlbumId", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_GalleryMedia_MediaId",
            table: "GalleryMedia",
            column: "MediaId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "GalleryMedia");
        migrationBuilder.DropTable(name: "GalleryAlbums");
    }
}
