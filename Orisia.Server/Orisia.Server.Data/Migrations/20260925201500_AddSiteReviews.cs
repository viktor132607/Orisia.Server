using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925201500_AddSiteReviews")]
public class AddSiteReviews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SiteReviews",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Featured = table.Column<bool>(type: "boolean", nullable: false),
                ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ModeratedById = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteReviews", x => x.Id);
                table.ForeignKey(
                    name: "FK_SiteReviews_Users_ModeratedById",
                    column: x => x.ModeratedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_SiteReviews_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SiteReviews_ModeratedById",
            table: "SiteReviews",
            column: "ModeratedById");

        migrationBuilder.CreateIndex(
            name: "IX_SiteReviews_Status_Featured_CreatedOn",
            table: "SiteReviews",
            columns: new[] { "Status", "Featured", "CreatedOn" });

        migrationBuilder.CreateIndex(
            name: "IX_SiteReviews_UserId",
            table: "SiteReviews",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SiteReviews");
    }
}
