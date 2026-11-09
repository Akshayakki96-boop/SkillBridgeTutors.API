using Microsoft.EntityFrameworkCore;
using System.Data;

namespace SkillBridgeTutors.API.Data
{
    /// <summary>
    /// Helper class to ensure database schema is up-to-date
    /// This runs raw SQL to add missing columns that may not have been applied by EF Core migrations
    /// </summary>
    public static class SchemaMigrationHelper
    {
        public static async Task EnsureStudentsTableSchemaAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            try
            {
                var connection = dbContext.Database.GetDbConnection();

                // Ensure connection is open
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = 60;

                    // Check and add GoogleId column
                    command.CommandText = @"
                        IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
                        BEGIN
                            IF NOT EXISTS (
                                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'GoogleId'
                            )
                            BEGIN
                                ALTER TABLE Students ADD GoogleId nvarchar(max) NULL;
                                PRINT 'Column GoogleId added to Students table';
                            END
                            ELSE
                            BEGIN
                                PRINT 'Column GoogleId already exists in Students table';
                            END
                        END";

                    await command.ExecuteNonQueryAsync();

                    // Check and add AuthProvider column
                    command.CommandText = @"
                        IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
                        BEGIN
                            IF NOT EXISTS (
                                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'AuthProvider'
                            )
                            BEGIN
                                ALTER TABLE Students ADD AuthProvider nvarchar(max) NULL;
                                PRINT 'Column AuthProvider added to Students table';
                            END
                            ELSE
                            BEGIN
                                PRINT 'Column AuthProvider already exists in Students table';
                            END
                        END";

                    await command.ExecuteNonQueryAsync();

                    // Teachers table has the same Google auth columns
                    command.CommandText = @"
                        IF OBJECT_ID(N'[dbo].[Teachers]', N'U') IS NOT NULL
                        BEGIN
                            IF COL_LENGTH('dbo.Teachers', 'GoogleId') IS NULL
                                ALTER TABLE dbo.Teachers ADD GoogleId nvarchar(max) NULL;
                            IF COL_LENGTH('dbo.Teachers', 'AuthProvider') IS NULL
                                ALTER TABLE dbo.Teachers ADD AuthProvider nvarchar(max) NULL;
                        END";

                    await command.ExecuteNonQueryAsync();

                    logger.LogInformation("Database schema verification completed successfully");
                }

                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying database schema");
                throw;
            }
        }
    }
}
