using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Unitask.Application;
using Unitask.Application.Common.Settings;
using Unitask.Infrastructure;
using Unitask.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

bool IsAllowedOrigin(string origin)
{
    if (allowedOrigins.Any(value => string.Equals(value, origin, StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    var host = uri.Host;
    return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || host.Equals("::1", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".vercel.com", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".unitask.io.vn", StringComparison.OrdinalIgnoreCase)
        || host.Equals("unitask.io.vn", StringComparison.OrdinalIgnoreCase);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.SetIsOriginAllowed(IsAllowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Unitask API",
        Version = "v1"
    });

    // FIX DUPLICATE SCHEMA ID
    options.CustomSchemaIds(type => type.FullName);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException(
        "JwtSettings are missing or invalid.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddResponseCaching();

var momoSettings = builder.Configuration.GetSection("MomoAPI").Get<Unitask.Api.Services.MomoSettings>();
if (momoSettings is not null)
{
    builder.Services.AddSingleton(momoSettings);
    builder.Services.AddHttpClient<Unitask.Api.Services.MomoService>();
}
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRagServices(builder.Configuration);

// Chính sách Escrow 1.2: tự động nghiệm thu & giải ngân sau 72h doanh nghiệp không phản hồi.
builder.Services.AddHostedService<Unitask.Api.Jobs.EscrowAutoReleaseService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Unitask API" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

// Auto-create portfolio tables if they don't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Unitask.Infrastructure.Persistence.UnitaskDbContext>();
    try
    {
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PortfolioProjects')
            CREATE TABLE PortfolioProjects (
                Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                StudentId UNIQUEIDENTIFIER NOT NULL,
                Title NVARCHAR(255) NOT NULL,
                [Description] NVARCHAR(MAX) NULL,
                ImageUrl NVARCHAR(500) NULL,
                ProjectUrl NVARCHAR(500) NULL,
                GithubUrl NVARCHAR(500) NULL,
                Tags NVARCHAR(1000) NULL,
                [Role] NVARCHAR(100) NULL,
                StartDate DATETIME2 NULL,
                EndDate DATETIME2 NULL,
                IsHighlighted BIT DEFAULT 0,
                SortOrder INT DEFAULT 0,
                CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
                FOREIGN KEY (StudentId) REFERENCES StudentProfiles(Id) ON DELETE CASCADE
            );

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Educations')
            CREATE TABLE Educations (
                Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                StudentId UNIQUEIDENTIFIER NOT NULL,
                Institution NVARCHAR(255) NOT NULL,
                Degree NVARCHAR(255) NULL,
                FieldOfStudy NVARCHAR(255) NULL,
                StartYear INT NULL,
                EndYear INT NULL,
                Gpa DECIMAL(3,2) NULL,
                [Description] NVARCHAR(MAX) NULL,
                IsCurrent BIT DEFAULT 0,
                SortOrder INT DEFAULT 0,
                CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
                FOREIGN KEY (StudentId) REFERENCES StudentProfiles(Id) ON DELETE CASCADE
            );

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Certifications')
            CREATE TABLE Certifications (
                Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                StudentId UNIQUEIDENTIFIER NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                IssuingOrganization NVARCHAR(255) NULL,
                IssueDate DATETIME2 NULL,
                ExpirationDate DATETIME2 NULL,
                CredentialUrl NVARCHAR(500) NULL,
                CredentialId NVARCHAR(100) NULL,
                ImageUrl NVARCHAR(500) NULL,
                SortOrder INT DEFAULT 0,
                CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
                UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
                FOREIGN KEY (StudentId) REFERENCES StudentProfiles(Id) ON DELETE CASCADE
            );
        ");
        Console.WriteLine("[Startup] Portfolio tables ensured.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Portfolio tables check: {ex.Message}");
    }
}

app.Run();