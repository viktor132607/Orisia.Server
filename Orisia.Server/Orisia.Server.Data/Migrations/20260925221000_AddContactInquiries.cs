using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260925221000_AddContactInquiries")]
public class AddContactInquiries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ContactInquiries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                Message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AnswerText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AnsweredById = table.Column<Guid>(type: "uuid", nullable: true),
                ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContactInquiries", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContactInquiries_Users_AnsweredById",
                    column: x => x.AnsweredById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContactInquiries_AnsweredById",
            table: "ContactInquiries",
            column: "AnsweredById");

        migrationBuilder.CreateIndex(
            name: "IX_ContactInquiries_Email",
            table: "ContactInquiries",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_ContactInquiries_Status_CreatedOn",
            table: "ContactInquiries",
            columns: new[] { "Status", "CreatedOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ContactInquiries");
    }
}
