using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260927013000_AddDanceGroups")]
public class AddDanceGroups : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DanceGroups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                NameBg = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                NameEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                DescriptionBg = table.Column<string>(type: "text", nullable: false),
                DescriptionEn = table.Column<string>(type: "text", nullable: false),
                Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_DanceGroups", x => x.Id));

        migrationBuilder.CreateTable(
            name: "DanceGroupSchedules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DanceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DanceGroupSchedules", x => x.Id);
                table.ForeignKey(
                    name: "FK_DanceGroupSchedules_DanceGroups_DanceGroupId",
                    column: x => x.DanceGroupId,
                    principalTable: "DanceGroups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DanceGroups_Active_SortOrder",
            table: "DanceGroups",
            columns: new[] { "Active", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_DanceGroups_Slug",
            table: "DanceGroups",
            column: "Slug",
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_DanceGroupSchedules_DanceGroupId_DayOfWeek_StartTime",
            table: "DanceGroupSchedules",
            columns: new[] { "DanceGroupId", "DayOfWeek", "StartTime" },
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.Sql(
            """
            INSERT INTO "DanceGroups"
                ("Id", "Slug", "NameBg", "NameEn", "DescriptionBg", "DescriptionEn", "Location", "Active", "SortOrder", "CreatedOn", "ModifiedOn", "IsDeleted")
            VALUES
                ('8f0e1c0b-3fd7-4f62-a6cb-3097db8e1184', 'makamlii', 'Макамлии', 'Makamlii', 'Танцова група „Макамлии“.', 'Makamlii folk dance group.', 'гр. Русе, ул. Родина 80', TRUE, 0, TIMESTAMPTZ '2026-09-27 00:00:00+00', TIMESTAMPTZ '2026-09-27 00:00:00+00', FALSE);

            INSERT INTO "DanceGroupSchedules"
                ("Id", "DanceGroupId", "DayOfWeek", "StartTime", "DurationMinutes", "CreatedOn", "ModifiedOn", "IsDeleted")
            VALUES
                ('2bd27ce5-3014-4a26-8299-65c283087099', '8f0e1c0b-3fd7-4f62-a6cb-3097db8e1184', 4, TIME '19:00', 90, TIMESTAMPTZ '2026-09-27 00:00:00+00', TIMESTAMPTZ '2026-09-27 00:00:00+00', FALSE),
                ('9e14c847-9c75-47bd-a6d8-3256bdb47abf', '8f0e1c0b-3fd7-4f62-a6cb-3097db8e1184', 6, TIME '19:00', 90, TIMESTAMPTZ '2026-09-27 00:00:00+00', TIMESTAMPTZ '2026-09-27 00:00:00+00', FALSE);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DanceGroupSchedules");
        migrationBuilder.DropTable(name: "DanceGroups");
    }
}
