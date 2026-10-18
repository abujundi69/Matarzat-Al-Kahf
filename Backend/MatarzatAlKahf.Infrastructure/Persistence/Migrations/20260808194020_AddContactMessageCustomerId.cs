using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMessageCustomerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "content",
                table: "ContactMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CustomerId",
                schema: "content",
                table: "ContactMessages",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_Customers_CustomerId",
                schema: "content",
                table: "ContactMessages",
                column: "CustomerId",
                principalSchema: "customers",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_Customers_CustomerId",
                schema: "content",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_CustomerId",
                schema: "content",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "content",
                table: "ContactMessages");
        }
    }
}
