using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductDeliveryFlagAddOrderAreaSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeliveryIncluded",
                schema: "catalog",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAreaNameArabic",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAreaNameEnglish",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAreaNameArabic",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAreaNameEnglish",
                schema: "sales",
                table: "Orders");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeliveryIncluded",
                schema: "catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
