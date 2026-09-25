using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925183000_AddMediaLibrary")]
public class AddMediaLibrary : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Media",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                ThumbnailStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Extension = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                AltBg = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                AltEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                UploadedById = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Media", x => x.Id);
                table.ForeignKey(
                    name: "FK_Media_Users_UploadedById",
                    column: x => x.UploadedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Media_CreatedOn",
            table: "Media",
            column: "CreatedOn");

        migrationBuilder.CreateIndex(
            name: "IX_Media_Sha256",
            table: "Media",
            column: "Sha256");

        migrationBuilder.CreateIndex(
            name: "IX_Media_StorageKey",
            table: "Media",
            column: "StorageKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Media_UploadedById",
            table: "Media",
            column: "UploadedById");

        migrationBuilder.CreateIndex(
            name: "IX_Events_CoverMediaId",
            table: "Events",
            column: "CoverMediaId");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_CoverMediaId",
            table: "Posts",
            column: "CoverMediaId");

        migrationBuilder.AddForeignKey(
            name: "FK_Events_Media_CoverMediaId",
            table: "Events",
            column: "CoverMediaId",
            principalTable: "Media",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_Posts_Media_CoverMediaId",
            table: "Posts",
            column: "CoverMediaId",
            principalTable: "Media",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Events_Media_CoverMediaId",
            table: "Events");

        migrationBuilder.DropForeignKey(
            name: "FK_Posts_Media_CoverMediaId",
            table: "Posts");

        migrationBuilder.DropIndex(
            name: "IX_Events_CoverMediaId",
            table: "Events");

        migrationBuilder.DropIndex(
            name: "IX_Posts_CoverMediaId",
            table: "Posts");

        migrationBuilder.DropTable(name: "Media");
    }
}
