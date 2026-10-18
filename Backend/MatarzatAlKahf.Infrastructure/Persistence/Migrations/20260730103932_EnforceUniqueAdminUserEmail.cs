using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueAdminUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [identity].[AdminUsers]
                    WHERE [Email] IS NOT NULL
                    GROUP BY [Email]
                    HAVING COUNT_BIG(*) > 1
                )
                BEGIN
                    THROW 51001, N'Cannot enforce unique administrator emails because duplicate non-null values exist in identity.AdminUsers.Email. Resolve them manually before retrying; this migration does not delete or merge users.', 1;
                END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Email",
                schema: "identity",
                table: "AdminUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                schema: "identity",
                table: "AdminUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Email",
                schema: "identity",
                table: "AdminUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                schema: "identity",
                table: "AdminUsers",
                column: "Email",
                filter: "[Email] IS NOT NULL");
        }
    }
}
