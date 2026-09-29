using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Orisia.Server.Data;

namespace Orisia.Server.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260929104500_AddProductionSecurity")]
public sealed class AddProductionSecurity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "PasswordResetTokens" (
                "TokenHash" bytea PRIMARY KEY,
                "UserId" uuid NOT NULL,
                "ExpiresAtUtc" timestamp with time zone NOT NULL
            );
            CREATE INDEX IF NOT EXISTS "IX_PasswordResetTokens_ExpiresAtUtc"
                ON "PasswordResetTokens" ("ExpiresAtUtc");

            ALTER TABLE "ContactInquiries" ENABLE ROW LEVEL SECURITY;
            ALTER TABLE "ContactInquiries" FORCE ROW LEVEL SECURITY;

            DROP POLICY IF EXISTS "ContactInquiries_Select" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Insert" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Update" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Delete" ON "ContactInquiries";

            CREATE POLICY "ContactInquiries_Select" ON "ContactInquiries"
            FOR SELECT USING (
                current_setting('app.current_is_system', true) = 'true'
                OR current_setting('app.current_is_admin', true) = 'true'
            );

            CREATE POLICY "ContactInquiries_Insert" ON "ContactInquiries"
            FOR INSERT WITH CHECK (true);

            CREATE POLICY "ContactInquiries_Update" ON "ContactInquiries"
            FOR UPDATE USING (
                current_setting('app.current_is_system', true) = 'true'
                OR current_setting('app.current_is_admin', true) = 'true'
            ) WITH CHECK (
                current_setting('app.current_is_system', true) = 'true'
                OR current_setting('app.current_is_admin', true) = 'true'
            );

            CREATE POLICY "ContactInquiries_Delete" ON "ContactInquiries"
            FOR DELETE USING (
                current_setting('app.current_is_system', true) = 'true'
                OR current_setting('app.current_is_admin', true) = 'true'
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "ContactInquiries" DISABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS "ContactInquiries_Select" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Insert" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Update" ON "ContactInquiries";
            DROP POLICY IF EXISTS "ContactInquiries_Delete" ON "ContactInquiries";
            DROP TABLE IF EXISTS "PasswordResetTokens";
            """);
    }
}
