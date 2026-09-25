using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925171500_AddEvents")]
public class AddEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                TitleBg = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                TitleEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                DescriptionBg = table.Column<string>(type: "text", nullable: false),
                DescriptionEn = table.Column<string>(type: "text", nullable: false),
                StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AllDay = table.Column<bool>(type: "boolean", nullable: false),
                EventType = table.Column<int>(type: "integer", nullable: false),
                Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                CoverMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                Featured = table.Column<bool>(type: "boolean", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                RecurrenceRule = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Events", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Events_EventType_Status_StartAt",
            table: "Events",
            columns: new[] { "EventType", "Status", "StartAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Events_Featured_Status_StartAt",
            table: "Events",
            columns: new[] { "Featured", "Status", "StartAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Events_Slug",
            table: "Events",
            column: "Slug",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_Events_Status_StartAt",
            table: "Events",
            columns: new[] { "Status", "StartAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Events");
    }
}
