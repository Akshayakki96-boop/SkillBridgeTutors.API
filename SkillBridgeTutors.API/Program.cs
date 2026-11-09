using System.Text;
using System.Text;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Repository;
using SkillBridgeTutors.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // Retry up to 5 times for transient Azure SQL errors (e.g. auto-pause wake-up)
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);

            // Fail fast rather than hanging for 120s
            sqlOptions.CommandTimeout(30);
        });
});

// Repositories
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IDemoRepository, DemoRepository>();
builder.Services.AddScoped<ICallRecordRepository, CallRecordRepository>();
builder.Services.AddScoped<IRegularClassRepository, RegularClassRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherAvailabilityRepository, TeacherAvailabilityRepository>();

// Services
builder.Services.AddHttpClient<IRetellService, RetellService>();
builder.Services.AddHttpClient<IPayPalService, PayPalService>();

// Email provider - using Resend only
builder.Services.AddHttpClient<IEmailProvider, ResendEmailProvider>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<IClassSchedulingService, ClassSchedulingService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SkillBridge Tutors API", Version = "v1" });

    // Add JWT Bearer input to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://skillbridgetutors.com",
            "https://www.skillbridgetutors.com",
            "http://localhost:3000",
            "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending migrations automatically and ensure schema is up-to-date
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply any pending migrations
        dbContext.Database.Migrate();

        // Verify that required columns exist in Students table and add them if missing
        var connection = dbContext.Database.GetDbConnection();
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // Check if GoogleId column exists
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'GoogleId'
                )
                BEGIN
                    ALTER TABLE Students ADD GoogleId nvarchar(max) NULL;
                END

                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'AuthProvider'
                )
                BEGIN
                    ALTER TABLE Students ADD AuthProvider nvarchar(max) NULL;
                END";

            command.ExecuteNonQuery();
        }
        connection.Close();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying migrations or verifying schema");
    // Don't throw - allow app to start even if migration verification fails
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();