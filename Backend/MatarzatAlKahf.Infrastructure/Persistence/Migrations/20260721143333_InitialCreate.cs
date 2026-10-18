using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// EF Core generates inline column-name arrays for migration operations.
#pragma warning disable CA1861

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "identity");

        migrationBuilder.EnsureSchema(
            name: "system");

        migrationBuilder.EnsureSchema(
            name: "content");

        migrationBuilder.EnsureSchema(
            name: "catalog");

        migrationBuilder.EnsureSchema(
            name: "promotions");

        migrationBuilder.EnsureSchema(
            name: "customers");

        migrationBuilder.EnsureSchema(
            name: "sales");

        migrationBuilder.CreateTable(
            name: "AdminUsers",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                PreferredLanguage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdminUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Banners",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Position = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Banners", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Categories",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Categories_Categories_ParentCategoryId",
                    column: x => x.ParentCategoryId,
                    principalSchema: "catalog",
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ContactMessages",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                Subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                RepliedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContactMessages", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContentPages",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentPages", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Coupons",
            schema: "promotions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                MinimumOrderAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                MaximumDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                TotalUsageLimit = table.Column<int>(type: "int", nullable: true),
                UsageLimitPerCustomer = table.Column<int>(type: "int", nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Coupons", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            schema: "customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                PreferredLanguage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OtpVerifications",
            schema: "customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CodeHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Purpose = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OtpVerifications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Permissions",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                IsSystemRole = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SocialMediaLinks",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Platform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SocialMediaLinks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "StoreSettings",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsLocalized = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StoreSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditLogs_AdminUsers_AdminUserId",
                    column: x => x.AdminUserId,
                    principalSchema: "identity",
                    principalTable: "AdminUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "BannerTranslations",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                BannerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                DesktopImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                MobileImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                LinkUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BannerTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_BannerTranslations_Banners_BannerId",
                    column: x => x.BannerId,
                    principalSchema: "content",
                    principalTable: "Banners",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CategoryTranslations",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                MetaTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CategoryTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_CategoryTranslations_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalSchema: "catalog",
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsDeliveryIncluded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalSchema: "catalog",
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ContentPageTranslations",
            schema: "content",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ContentPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MetaTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentPageTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContentPageTranslations_ContentPages_ContentPageId",
                    column: x => x.ContentPageId,
                    principalSchema: "content",
                    principalTable: "ContentPages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CustomerAddresses",
            schema: "customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Area = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                AddressLine = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                TaxNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DeliveryNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                table.ForeignKey(
                    name: "FK_CustomerAddresses_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalSchema: "customers",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Recipient = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                ProviderReference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_Notifications_AdminUsers_AdminUserId",
                    column: x => x.AdminUserId,
                    principalSchema: "identity",
                    principalTable: "AdminUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Notifications_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalSchema: "customers",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            schema: "sales",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                OrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Area = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                AddressLine = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                TaxNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DeliveryNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DeliveryAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                SelectedPaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_Coupons_CouponId",
                    column: x => x.CouponId,
                    principalSchema: "promotions",
                    principalTable: "Coupons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Orders_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalSchema: "customers",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "AdminUserRoles",
            schema: "identity",
            columns: table => new
            {
                AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdminUserRoles", x => new { x.AdminUserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AdminUserRoles_AdminUsers_AdminUserId",
                    column: x => x.AdminUserId,
                    principalSchema: "identity",
                    principalTable: "AdminUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AdminUserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "identity",
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            schema: "identity",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalSchema: "identity",
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "identity",
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StoreSettingTranslations",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                StoreSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StoreSettingTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_StoreSettingTranslations_StoreSettings_StoreSettingId",
                    column: x => x.StoreSettingId,
                    principalSchema: "system",
                    principalTable: "StoreSettings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductImages",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                ThumbnailUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductImages", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductImages_Products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "catalog",
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductTranslations",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Story = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MetaTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductTranslations_Products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "catalog",
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductVariants",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Size = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                WeightInGrams = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                Capacity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                PriceOverride = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariants", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductVariants_Products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "catalog",
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CouponUsages",
            schema: "promotions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CouponUsages", x => x.Id);
                table.ForeignKey(
                    name: "FK_CouponUsages_Coupons_CouponId",
                    column: x => x.CouponId,
                    principalSchema: "promotions",
                    principalTable: "Coupons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CouponUsages_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalSchema: "customers",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_CouponUsages_Orders_OrderId",
                    column: x => x.OrderId,
                    principalSchema: "sales",
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderStatusHistory",
            schema: "sales",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PreviousStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ChangedByAdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderStatusHistory_AdminUsers_ChangedByAdminUserId",
                    column: x => x.ChangedByAdminUserId,
                    principalSchema: "identity",
                    principalTable: "AdminUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_OrderStatusHistory_Orders_OrderId",
                    column: x => x.OrderId,
                    principalSchema: "sales",
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            schema: "sales",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                ProviderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                TransactionReference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                ProviderResponseReference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_Orders_OrderId",
                    column: x => x.OrderId,
                    principalSchema: "sales",
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductImageTranslations",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProductImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductImageTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductImageTranslations_ProductImages_ProductImageId",
                    column: x => x.ProductImageId,
                    principalSchema: "catalog",
                    principalTable: "ProductImages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InventoryItems",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                QuantityOnHand = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                ReservedQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                ReorderLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_InventoryItems_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalSchema: "catalog",
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderItems",
            schema: "sales",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SkuSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                VariantDescriptionSnapshot = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderItems_Orders_OrderId",
                    column: x => x.OrderId,
                    principalSchema: "sales",
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderItems_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalSchema: "catalog",
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_OrderItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalSchema: "catalog",
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "InventoryTransactions",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                InventoryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                Reference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_InventoryTransactions_InventoryItems_InventoryItemId",
                    column: x => x.InventoryItemId,
                    principalSchema: "catalog",
                    principalTable: "InventoryItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AdminUserRoles_RoleId",
            schema: "identity",
            table: "AdminUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_Email",
            schema: "identity",
            table: "AdminUsers",
            column: "Email",
            filter: "[Email] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_IsDeleted",
            schema: "identity",
            table: "AdminUsers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_PhoneNumber",
            schema: "identity",
            table: "AdminUsers",
            column: "PhoneNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Action_CreatedAtUtc",
            schema: "system",
            table: "AuditLogs",
            columns: new[] { "Action", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_AdminUserId",
            schema: "system",
            table: "AuditLogs",
            column: "AdminUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_EntityName_EntityId",
            schema: "system",
            table: "AuditLogs",
            columns: new[] { "EntityName", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_IsDeleted",
            schema: "system",
            table: "AuditLogs",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Banners_IsActive_Position_SortOrder",
            schema: "content",
            table: "Banners",
            columns: new[] { "IsActive", "Position", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Banners_IsDeleted",
            schema: "content",
            table: "Banners",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Banners_StartsAtUtc_EndsAtUtc",
            schema: "content",
            table: "Banners",
            columns: new[] { "StartsAtUtc", "EndsAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_BannerTranslations_BannerId_Language",
            schema: "content",
            table: "BannerTranslations",
            columns: new[] { "BannerId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BannerTranslations_IsDeleted",
            schema: "content",
            table: "BannerTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_IsActive_SortOrder",
            schema: "catalog",
            table: "Categories",
            columns: new[] { "IsActive", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Categories_IsDeleted",
            schema: "catalog",
            table: "Categories",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ParentCategoryId",
            schema: "catalog",
            table: "Categories",
            column: "ParentCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_CategoryTranslations_CategoryId_Language",
            schema: "catalog",
            table: "CategoryTranslations",
            columns: new[] { "CategoryId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CategoryTranslations_IsDeleted",
            schema: "catalog",
            table: "CategoryTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CategoryTranslations_Language_Slug",
            schema: "catalog",
            table: "CategoryTranslations",
            columns: new[] { "Language", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContactMessages_Email",
            schema: "content",
            table: "ContactMessages",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_ContactMessages_IsDeleted",
            schema: "content",
            table: "ContactMessages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ContactMessages_Status_CreatedAtUtc",
            schema: "content",
            table: "ContactMessages",
            columns: new[] { "Status", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ContentPages_IsDeleted",
            schema: "content",
            table: "ContentPages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ContentPages_Type_IsPublished",
            schema: "content",
            table: "ContentPages",
            columns: new[] { "Type", "IsPublished" });

        migrationBuilder.CreateIndex(
            name: "IX_ContentPageTranslations_ContentPageId_Language",
            schema: "content",
            table: "ContentPageTranslations",
            columns: new[] { "ContentPageId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContentPageTranslations_IsDeleted",
            schema: "content",
            table: "ContentPageTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ContentPageTranslations_Language_Slug",
            schema: "content",
            table: "ContentPageTranslations",
            columns: new[] { "Language", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Coupons_Code",
            schema: "promotions",
            table: "Coupons",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Coupons_IsDeleted",
            schema: "promotions",
            table: "Coupons",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Coupons_Status_StartsAtUtc_EndsAtUtc",
            schema: "promotions",
            table: "Coupons",
            columns: new[] { "Status", "StartsAtUtc", "EndsAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_CouponUsages_CouponId",
            schema: "promotions",
            table: "CouponUsages",
            column: "CouponId");

        migrationBuilder.CreateIndex(
            name: "IX_CouponUsages_CustomerId",
            schema: "promotions",
            table: "CouponUsages",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_CouponUsages_IsDeleted",
            schema: "promotions",
            table: "CouponUsages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CouponUsages_OrderId",
            schema: "promotions",
            table: "CouponUsages",
            column: "OrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CustomerAddresses_CustomerId_IsDefault",
            schema: "customers",
            table: "CustomerAddresses",
            columns: new[] { "CustomerId", "IsDefault" });

        migrationBuilder.CreateIndex(
            name: "IX_CustomerAddresses_IsDeleted",
            schema: "customers",
            table: "CustomerAddresses",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Email",
            schema: "customers",
            table: "Customers",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_IsDeleted",
            schema: "customers",
            table: "Customers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_PhoneNumber",
            schema: "customers",
            table: "Customers",
            column: "PhoneNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InventoryItems_IsDeleted",
            schema: "catalog",
            table: "InventoryItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryItems_ProductVariantId",
            schema: "catalog",
            table: "InventoryItems",
            column: "ProductVariantId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InventoryTransactions_InventoryItemId",
            schema: "catalog",
            table: "InventoryTransactions",
            column: "InventoryItemId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryTransactions_IsDeleted",
            schema: "catalog",
            table: "InventoryTransactions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryTransactions_Reference",
            schema: "catalog",
            table: "InventoryTransactions",
            column: "Reference",
            filter: "[Reference] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_AdminUserId",
            schema: "system",
            table: "Notifications",
            column: "AdminUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_CustomerId",
            schema: "system",
            table: "Notifications",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_IsDeleted",
            schema: "system",
            table: "Notifications",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_ProviderReference",
            schema: "system",
            table: "Notifications",
            column: "ProviderReference");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_Status_Channel_CreatedAtUtc",
            schema: "system",
            table: "Notifications",
            columns: new[] { "Status", "Channel", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_IsDeleted",
            schema: "sales",
            table: "OrderItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId",
            schema: "sales",
            table: "OrderItems",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_ProductId",
            schema: "sales",
            table: "OrderItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_ProductVariantId",
            schema: "sales",
            table: "OrderItems",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CouponId",
            schema: "sales",
            table: "Orders",
            column: "CouponId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerId",
            schema: "sales",
            table: "Orders",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_IsDeleted",
            schema: "sales",
            table: "Orders",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_OrderNumber",
            schema: "sales",
            table: "Orders",
            column: "OrderNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_Status_CreatedAtUtc",
            schema: "sales",
            table: "Orders",
            columns: new[] { "Status", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistory_ChangedByAdminUserId",
            schema: "sales",
            table: "OrderStatusHistory",
            column: "ChangedByAdminUserId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistory_IsDeleted",
            schema: "sales",
            table: "OrderStatusHistory",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistory_OrderId_CreatedAtUtc",
            schema: "sales",
            table: "OrderStatusHistory",
            columns: new[] { "OrderId", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_OtpVerifications_ExpiresAtUtc",
            schema: "customers",
            table: "OtpVerifications",
            column: "ExpiresAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_OtpVerifications_IsDeleted",
            schema: "customers",
            table: "OtpVerifications",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_OtpVerifications_PhoneNumber_Purpose_Status",
            schema: "customers",
            table: "OtpVerifications",
            columns: new[] { "PhoneNumber", "Purpose", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Payments_IsDeleted",
            schema: "sales",
            table: "Payments",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_OrderId",
            schema: "sales",
            table: "Payments",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_Status_CreatedAtUtc",
            schema: "sales",
            table: "Payments",
            columns: new[] { "Status", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Payments_TransactionReference",
            schema: "sales",
            table: "Payments",
            column: "TransactionReference");

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Code",
            schema: "identity",
            table: "Permissions",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_IsDeleted",
            schema: "identity",
            table: "Permissions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProductImages_IsDeleted",
            schema: "catalog",
            table: "ProductImages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProductImages_ProductId_IsPrimary",
            schema: "catalog",
            table: "ProductImages",
            columns: new[] { "ProductId", "IsPrimary" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductImages_ProductId_SortOrder",
            schema: "catalog",
            table: "ProductImages",
            columns: new[] { "ProductId", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductImageTranslations_IsDeleted",
            schema: "catalog",
            table: "ProductImageTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProductImageTranslations_ProductImageId_Language",
            schema: "catalog",
            table: "ProductImageTranslations",
            columns: new[] { "ProductImageId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryId",
            schema: "catalog",
            table: "Products",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_IsDeleted",
            schema: "catalog",
            table: "Products",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Products_PublishedAtUtc",
            schema: "catalog",
            table: "Products",
            column: "PublishedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Status_IsFeatured",
            schema: "catalog",
            table: "Products",
            columns: new[] { "Status", "IsFeatured" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductTranslations_IsDeleted",
            schema: "catalog",
            table: "ProductTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProductTranslations_Language_Slug",
            schema: "catalog",
            table: "ProductTranslations",
            columns: new[] { "Language", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductTranslations_ProductId_Language",
            schema: "catalog",
            table: "ProductTranslations",
            columns: new[] { "ProductId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariants_IsDeleted",
            schema: "catalog",
            table: "ProductVariants",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariants_ProductId_IsActive",
            schema: "catalog",
            table: "ProductVariants",
            columns: new[] { "ProductId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariants_Sku",
            schema: "catalog",
            table: "ProductVariants",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            schema: "identity",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_Roles_IsDeleted",
            schema: "identity",
            table: "Roles",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Roles_Name",
            schema: "identity",
            table: "Roles",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SocialMediaLinks_IsActive_SortOrder",
            schema: "content",
            table: "SocialMediaLinks",
            columns: new[] { "IsActive", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_SocialMediaLinks_IsDeleted",
            schema: "content",
            table: "SocialMediaLinks",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_SocialMediaLinks_Platform",
            schema: "content",
            table: "SocialMediaLinks",
            column: "Platform");

        migrationBuilder.CreateIndex(
            name: "IX_StoreSettings_Group",
            schema: "system",
            table: "StoreSettings",
            column: "Group");

        migrationBuilder.CreateIndex(
            name: "IX_StoreSettings_IsDeleted",
            schema: "system",
            table: "StoreSettings",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_StoreSettings_Key",
            schema: "system",
            table: "StoreSettings",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StoreSettingTranslations_IsDeleted",
            schema: "system",
            table: "StoreSettingTranslations",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_StoreSettingTranslations_StoreSettingId_Language",
            schema: "system",
            table: "StoreSettingTranslations",
            columns: new[] { "StoreSettingId", "Language" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AdminUserRoles",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "AuditLogs",
            schema: "system");

        migrationBuilder.DropTable(
            name: "BannerTranslations",
            schema: "content");

        migrationBuilder.DropTable(
            name: "CategoryTranslations",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "ContactMessages",
            schema: "content");

        migrationBuilder.DropTable(
            name: "ContentPageTranslations",
            schema: "content");

        migrationBuilder.DropTable(
            name: "CouponUsages",
            schema: "promotions");

        migrationBuilder.DropTable(
            name: "CustomerAddresses",
            schema: "customers");

        migrationBuilder.DropTable(
            name: "InventoryTransactions",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "Notifications",
            schema: "system");

        migrationBuilder.DropTable(
            name: "OrderItems",
            schema: "sales");

        migrationBuilder.DropTable(
            name: "OrderStatusHistory",
            schema: "sales");

        migrationBuilder.DropTable(
            name: "OtpVerifications",
            schema: "customers");

        migrationBuilder.DropTable(
            name: "Payments",
            schema: "sales");

        migrationBuilder.DropTable(
            name: "ProductImageTranslations",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "ProductTranslations",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "SocialMediaLinks",
            schema: "content");

        migrationBuilder.DropTable(
            name: "StoreSettingTranslations",
            schema: "system");

        migrationBuilder.DropTable(
            name: "Banners",
            schema: "content");

        migrationBuilder.DropTable(
            name: "ContentPages",
            schema: "content");

        migrationBuilder.DropTable(
            name: "InventoryItems",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "AdminUsers",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "Orders",
            schema: "sales");

        migrationBuilder.DropTable(
            name: "ProductImages",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "Permissions",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "Roles",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "StoreSettings",
            schema: "system");

        migrationBuilder.DropTable(
            name: "ProductVariants",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "Coupons",
            schema: "promotions");

        migrationBuilder.DropTable(
            name: "Customers",
            schema: "customers");

        migrationBuilder.DropTable(
            name: "Products",
            schema: "catalog");

        migrationBuilder.DropTable(
            name: "Categories",
            schema: "catalog");
    }
}

#pragma warning restore CA1861
