using System;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1861
#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRevocableAdminSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminSessions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenIdHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminSessions_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalSchema: "identity",
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_AdminUserId_RevokedAtUtc",
                schema: "identity",
                table: "AdminSessions",
                columns: new[] { "AdminUserId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_ExpiresAtUtc",
                schema: "identity",
                table: "AdminSessions",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_IsDeleted",
                schema: "identity",
                table: "AdminSessions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_TokenIdHash",
                schema: "identity",
                table: "AdminSessions",
                column: "TokenIdHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminSessions",
                schema: "identity");
        }
    }
}
