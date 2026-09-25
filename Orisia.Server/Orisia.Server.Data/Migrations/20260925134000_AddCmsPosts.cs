using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925134000_AddCmsPosts")]
public class AddCmsPosts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TABLE IF EXISTS "Images" CASCADE;
            DROP TABLE IF EXISTS "OrderItems" CASCADE;
            DROP TABLE IF EXISTS "Reviews" CASCADE;
            DROP TABLE IF EXISTS "WishlistItems" CASCADE;
            DROP TABLE IF EXISTS "Orders" CASCADE;
            DROP TABLE IF EXISTS "Products" CASCADE;
            DROP TABLE IF EXISTS "Categories" CASCADE;
            """);

        migrationBuilder.CreateTable(
            name: "Posts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                TitleBg = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                TitleEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                BodyBg = table.Column<string>(type: "text", nullable: false),
                BodyEn = table.Column<string>(type: "text", nullable: false),
                ExcerptBg = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                ExcerptEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CoverMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                Featured = table.Column<bool>(type: "boolean", nullable: false),
                PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Posts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Posts_Users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Posts_AuthorId",
            table: "Posts",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_Featured_Status",
            table: "Posts",
            columns: new[] { "Featured", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Posts_Slug",
            table: "Posts",
            column: "Slug",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_Type_Status_PublishedAt",
            table: "Posts",
            columns: new[] { "Type", "Status", "PublishedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Posts");
    }
}
