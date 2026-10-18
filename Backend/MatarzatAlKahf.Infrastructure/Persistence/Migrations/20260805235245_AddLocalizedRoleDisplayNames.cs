using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedRoleDisplayNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayNameArabic",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNameEnglish",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [identity].[Roles]
                SET
                    [DisplayNameArabic] = CASE [Name]
                        WHEN N'SuperAdmin' THEN N'مدير النظام الأعلى'
                        WHEN N'CatalogManager' THEN N'مدير الكتالوج'
                        WHEN N'InventoryManager' THEN N'مدير المخزون'
                        WHEN N'OrderManager' THEN N'مدير الطلبات'
                        WHEN N'ContentManager' THEN N'مدير المحتوى'
                        WHEN N'CustomerSupport' THEN N'دعم العملاء'
                        WHEN N'ReportsViewer' THEN N'عارض التقارير'
                        WHEN N'DataEntry' THEN N'إدخال البيانات'
                        WHEN N'StoreManager' THEN N'مدير المتجر'
                        WHEN N'OrderEmployee' THEN N'موظف الطلبات'
                    END,
                    [DisplayNameEnglish] = CASE [Name]
                        WHEN N'SuperAdmin' THEN N'SuperAdmin'
                        WHEN N'CatalogManager' THEN N'Catalog Manager'
                        WHEN N'InventoryManager' THEN N'Inventory Manager'
                        WHEN N'OrderManager' THEN N'Order Manager'
                        WHEN N'ContentManager' THEN N'Content Manager'
                        WHEN N'CustomerSupport' THEN N'Customer Support'
                        WHEN N'ReportsViewer' THEN N'Reports Viewer'
                        WHEN N'DataEntry' THEN N'Data Entry'
                        WHEN N'StoreManager' THEN N'Store Manager'
                        WHEN N'OrderEmployee' THEN N'Order Employee'
                    END
                WHERE [Name] IN
                (
                    N'SuperAdmin',
                    N'CatalogManager',
                    N'InventoryManager',
                    N'OrderManager',
                    N'ContentManager',
                    N'CustomerSupport',
                    N'ReportsViewer',
                    N'DataEntry',
                    N'StoreManager',
                    N'OrderEmployee'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayNameArabic",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DisplayNameEnglish",
                schema: "identity",
                table: "Roles");
        }
    }
}
