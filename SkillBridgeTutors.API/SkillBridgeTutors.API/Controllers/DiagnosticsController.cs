using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using System.Data;

namespace SkillBridgeTutors.API.Controllers
{
    /// <summary>
    /// Diagnostic endpoint for troubleshooting database issues
    /// </summary>
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(ApplicationDbContext context, ILogger<DiagnosticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get database connection info
        /// </summary>
        [HttpGet("db-info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();

                return Ok(new
                {
                    database = connection.Database,
                    dataSource = connection.DataSource,
                    state = connection.State.ToString(),
                    connectionString = _context.Database.GetConnectionString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if Students table has GoogleId and AuthProvider columns
        /// </summary>
        [HttpGet("check-schema")]
        public async Task<IActionResult> CheckSchema()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            COLUMN_NAME,
                            DATA_TYPE,
                            IS_NULLABLE
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Students'
                        ORDER BY ORDINAL_POSITION";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var columns = new List<dynamic>();
                        while (await reader.ReadAsync())
                        {
                            columns.Add(new
                            {
                                columnName = reader["COLUMN_NAME"],
                                dataType = reader["DATA_TYPE"],
                                isNullable = reader["IS_NULLABLE"]
                            });
                        }

                        var hasGoogleId = columns.Any(c => c.columnName.ToString() == "GoogleId");
                        var hasAuthProvider = columns.Any(c => c.columnName.ToString() == "AuthProvider");

                        return Ok(new
                        {
                            totalColumns = columns.Count,
                            hasGoogleId,
                            hasAuthProvider,
                            columns
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking schema");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Manually add missing columns to Students table
        /// </summary>
        [HttpPost("fix-schema")]
        public async Task<IActionResult> FixSchema()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var results = new List<string>();

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = 120;
                    command.CommandType = System.Data.CommandType.Text;

                    // Add GoogleId
                    command.CommandText = @"
                        IF NOT EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'GoogleId'
                        )
                        BEGIN
                            ALTER TABLE dbo.Students ADD GoogleId nvarchar(max) NULL;
                        END";

                    await command.ExecuteNonQueryAsync();
                    results.Add("GoogleId column processed");

                    // Add AuthProvider
                    command.CommandText = @"
                        IF NOT EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'AuthProvider'
                        )
                        BEGIN
                            ALTER TABLE dbo.Students ADD AuthProvider nvarchar(max) NULL;
                        END";

                    await command.ExecuteNonQueryAsync();
                    results.Add("AuthProvider column processed");
                }

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }

                return Ok(new
                {
                    message = "Schema fix completed",
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing schema");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
