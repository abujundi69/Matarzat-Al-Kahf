using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogActorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                schema: "system",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorName",
                schema: "system",
                table: "AuditLogs");
        }
    }
}
