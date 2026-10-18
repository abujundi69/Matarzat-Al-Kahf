using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedPhoneAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_PhoneNumber",
                schema: "customers",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_PhoneNumber",
                schema: "identity",
                table: "AdminUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "customers",
                table: "Customers",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhoneNumber",
                schema: "customers",
                table: "Customers",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE [customers].[Customers]
                SET [NormalizedPhoneNumber] = [PhoneNumber];
                """);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "identity",
                table: "AdminUsers",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhoneNumber",
                schema: "identity",
                table: "AdminUsers",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE [identity].[AdminUsers]
                SET [NormalizedPhoneNumber] = [PhoneNumber];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedPhoneNumber",
                schema: "customers",
                table: "Customers",
                column: "NormalizedPhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_NormalizedPhoneNumber",
                schema: "identity",
                table: "AdminUsers",
                column: "NormalizedPhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NormalizedPhoneNumber",
                schema: "customers",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_NormalizedPhoneNumber",
                schema: "identity",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedPhoneNumber",
                schema: "customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NormalizedPhoneNumber",
                schema: "identity",
                table: "AdminUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "customers",
                table: "Customers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "identity",
                table: "AdminUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                schema: "customers",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_PhoneNumber",
                schema: "identity",
                table: "AdminUsers",
                column: "PhoneNumber",
                unique: true);
        }
    }
}
