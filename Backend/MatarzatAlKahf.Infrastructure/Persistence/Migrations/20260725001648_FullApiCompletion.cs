using System;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1861
#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FullApiCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProofFilePath",
                schema: "sales",
                table: "Payments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                schema: "sales",
                table: "Payments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAtUtc",
                schema: "sales",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedAtUtc",
                schema: "customers",
                table: "OtpVerifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSentAtUtc",
                schema: "customers",
                table: "OtpVerifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaximumAttempts",
                schema: "customers",
                table: "OtpVerifications",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "VerificationTokenHash",
                schema: "customers",
                table: "OtpVerifications",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryLocationId",
                schema: "sales",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "InventoryReleasedAtUtc",
                schema: "sales",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                schema: "system",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                schema: "system",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAtUtc",
                schema: "system",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "system",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AdministrativeNotes",
                schema: "customers",
                table: "Customers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "content",
                table: "ContactMessages",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AddColumn<string>(
                name: "AdministrativeReply",
                schema: "content",
                table: "ContactMessages",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorType",
                schema: "system",
                table: "AuditLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                schema: "system",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Carts",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GuestTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "promotions",
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Carts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckoutIdempotencies",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Scope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutIdempotencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckoutIdempotencies_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "sales",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCategories",
                schema: "promotions",
                columns: table => new
                {
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCategories", x => new { x.CouponId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_CouponCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponCategories_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "promotions",
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponProducts",
                schema: "promotions",
                columns: table => new
                {
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponProducts", x => new { x.CouponId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_CouponProducts_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "promotions",
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSessions",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenIdHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSessions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLocations",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CountryNameArabic = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CountryNameEnglish = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameArabic = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CashOnDeliveryAllowed = table.Column<bool>(type: "bit", nullable: false),
                    ProductPriceIncludesDelivery = table.Column<bool>(type: "bit", nullable: false),
                    AdditionalDeliveryCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumOrderAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportOperations",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RowsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ValidRows = table.Column<int>(type: "int", nullable: false),
                    CommitIdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsertedRows = table.Column<int>(type: "int", nullable: false),
                    UpdatedRows = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportOperations_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalSchema: "identity",
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ArabicTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EnglishTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ArabicBody = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    EnglishBody = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeoSettings",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BaseUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ArabicTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EnglishTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ArabicDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EnglishDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ArabicOpenGraphImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EnglishOpenGraphImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RobotsAdditionalRules = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedByAdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedByCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalSchema: "sales",
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalSchema: "catalog",
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments",
                column: "VerifiedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryLocationId",
                schema: "sales",
                table: "Orders",
                column: "DeliveryLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrderId",
                schema: "system",
                table: "Notifications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status_NextAttemptAtUtc",
                schema: "system",
                table: "Notifications",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses",
                column: "DeliveryLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductVariantId",
                schema: "sales",
                table: "CartItems",
                columns: new[] { "CartId", "ProductVariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_IsDeleted",
                schema: "sales",
                table: "CartItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantId",
                schema: "sales",
                table: "CartItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CouponId",
                schema: "sales",
                table: "Carts",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_Status",
                schema: "sales",
                table: "Carts",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ExpiresAtUtc",
                schema: "sales",
                table: "Carts",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_GuestTokenHash",
                schema: "sales",
                table: "Carts",
                column: "GuestTokenHash",
                unique: true,
                filter: "[GuestTokenHash] <> N''");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_IsDeleted",
                schema: "sales",
                table: "Carts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencies_ExpiresAtUtc",
                schema: "sales",
                table: "CheckoutIdempotencies",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencies_IsDeleted",
                schema: "sales",
                table: "CheckoutIdempotencies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencies_OrderId",
                schema: "sales",
                table: "CheckoutIdempotencies",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencies_Scope_Key",
                schema: "sales",
                table: "CheckoutIdempotencies",
                columns: new[] { "Scope", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponCategories_CategoryId",
                schema: "promotions",
                table: "CouponCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponProducts_ProductId",
                schema: "promotions",
                table: "CouponProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSessions_CustomerId_RevokedAtUtc",
                schema: "customers",
                table: "CustomerSessions",
                columns: new[] { "CustomerId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSessions_ExpiresAtUtc",
                schema: "customers",
                table: "CustomerSessions",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSessions_IsDeleted",
                schema: "customers",
                table: "CustomerSessions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSessions_TokenIdHash",
                schema: "customers",
                table: "CustomerSessions",
                column: "TokenIdHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLocations_CountryCode_NameEnglish_IsDeleted",
                schema: "sales",
                table: "DeliveryLocations",
                columns: new[] { "CountryCode", "NameEnglish", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLocations_IsActive_SortOrder",
                schema: "sales",
                table: "DeliveryLocations",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLocations_IsDeleted",
                schema: "sales",
                table: "DeliveryLocations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOperations_AdminUserId_Type_CreatedAtUtc",
                schema: "system",
                table: "ImportOperations",
                columns: new[] { "AdminUserId", "Type", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportOperations_CommitIdempotencyKey",
                schema: "system",
                table: "ImportOperations",
                column: "CommitIdempotencyKey",
                unique: true,
                filter: "[CommitIdempotencyKey] <> N''");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOperations_ExpiresAtUtc",
                schema: "system",
                table: "ImportOperations",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOperations_FileHash",
                schema: "system",
                table: "ImportOperations",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOperations_IsDeleted",
                schema: "system",
                table: "ImportOperations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Code_Channel",
                schema: "system",
                table: "NotificationTemplates",
                columns: new[] { "Code", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsDeleted",
                schema: "system",
                table: "NotificationTemplates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SeoSettings_IsDeleted",
                schema: "system",
                table: "SeoSettings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_IsDeleted",
                schema: "system",
                table: "StoredFiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Sha256",
                schema: "system",
                table: "StoredFiles",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_StorageKey",
                schema: "system",
                table: "StoredFiles",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByAdminUserId",
                schema: "system",
                table: "StoredFiles",
                column: "UploadedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByCustomerId",
                schema: "system",
                table: "StoredFiles",
                column: "UploadedByCustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddresses_DeliveryLocations_DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses",
                column: "DeliveryLocationId",
                principalSchema: "sales",
                principalTable: "DeliveryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryLocations_DeliveryLocationId",
                schema: "sales",
                table: "Orders",
                column: "DeliveryLocationId",
                principalSchema: "sales",
                principalTable: "DeliveryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AdminUsers_VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments",
                column: "VerifiedByAdminUserId",
                principalSchema: "identity",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddresses_DeliveryLocations_DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryLocations_DeliveryLocationId",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AdminUsers_VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "CheckoutIdempotencies",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "CouponCategories",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "CouponProducts",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "CustomerSessions",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "DeliveryLocations",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "ImportOperations",
                schema: "system");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "system");

            migrationBuilder.DropTable(
                name: "SeoSettings",
                schema: "system");

            migrationBuilder.DropTable(
                name: "StoredFiles",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Carts",
                schema: "sales");

            migrationBuilder.DropIndex(
                name: "IX_Payments_VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryLocationId",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_OrderId",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Status_NextAttemptAtUtc",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAddresses_DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "ProofFilePath",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VerifiedAtUtc",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VerifiedByAdminUserId",
                schema: "sales",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ConsumedAtUtc",
                schema: "customers",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "LastSentAtUtc",
                schema: "customers",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "MaximumAttempts",
                schema: "customers",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "VerificationTokenHash",
                schema: "customers",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "DeliveryLocationId",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InventoryReleasedAtUtc",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OrderId",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReadAtUtc",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "system",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AdministrativeNotes",
                schema: "customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeliveryLocationId",
                schema: "customers",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "AdministrativeReply",
                schema: "content",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ActorType",
                schema: "system",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TraceId",
                schema: "system",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "content",
                table: "ContactMessages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 10000);
        }
    }
}
