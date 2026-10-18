using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatarzatAlKahf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyAdministrativeRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @AdminRoleId uniqueidentifier =
                (
                    SELECT TOP (1) [Id]
                    FROM [identity].[Roles]
                    WHERE [Name] = N'Admin'
                );

                IF @AdminRoleId IS NULL
                BEGIN
                    SET @AdminRoleId = 'a11d0000-0000-4000-8000-000000000001';

                    INSERT INTO [identity].[Roles]
                    (
                        [Id],
                        [Name],
                        [DisplayNameArabic],
                        [DisplayNameEnglish],
                        [Description],
                        [IsSystemRole],
                        [CreatedAtUtc],
                        [UpdatedAtUtc],
                        [IsDeleted]
                    )
                    VALUES
                    (
                        @AdminRoleId,
                        N'Admin',
                        N'مدير',
                        N'Admin',
                        N'Full day-to-day administrative access.',
                        1,
                        SYSUTCDATETIME(),
                        NULL,
                        0
                    );
                END
                ELSE
                BEGIN
                    UPDATE [identity].[Roles]
                    SET
                        [DisplayNameArabic] = N'مدير',
                        [DisplayNameEnglish] = N'Admin',
                        [Description] = N'Full day-to-day administrative access.',
                        [IsSystemRole] = 1,
                        [IsDeleted] = 0,
                        [UpdatedAtUtc] = SYSUTCDATETIME()
                    WHERE [Id] = @AdminRoleId;
                END;

                INSERT INTO [identity].[RolePermissions] ([RoleId], [PermissionId])
                SELECT @AdminRoleId, permission.[Id]
                FROM [identity].[Permissions] AS permission
                WHERE permission.[IsDeleted] = 0
                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM [identity].[RolePermissions] AS existingMapping
                        WHERE existingMapping.[RoleId] = @AdminRoleId
                            AND existingMapping.[PermissionId] = permission.[Id]
                    );

                INSERT INTO [identity].[AdminUserRoles] ([AdminUserId], [RoleId])
                SELECT adminUser.[Id], @AdminRoleId
                FROM [identity].[AdminUsers] AS adminUser
                WHERE adminUser.[IsDeleted] = 0
                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM [identity].[AdminUserRoles] AS superAdminMapping
                        INNER JOIN [identity].[Roles] AS superAdminRole
                            ON superAdminRole.[Id] = superAdminMapping.[RoleId]
                        WHERE superAdminMapping.[AdminUserId] = adminUser.[Id]
                            AND superAdminRole.[Name] = N'SuperAdmin'
                    )
                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM [identity].[AdminUserRoles] AS adminMapping
                        WHERE adminMapping.[AdminUserId] = adminUser.[Id]
                            AND adminMapping.[RoleId] = @AdminRoleId
                    );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @InsertedAdminRoleId uniqueidentifier =
                    'a11d0000-0000-4000-8000-000000000001';

                DELETE FROM [identity].[AdminUserRoles]
                WHERE [RoleId] = @InsertedAdminRoleId;

                DELETE FROM [identity].[RolePermissions]
                WHERE [RoleId] = @InsertedAdminRoleId;

                DELETE FROM [identity].[Roles]
                WHERE [Id] = @InsertedAdminRoleId
                    AND [Name] = N'Admin';
                """);
        }
    }
}
