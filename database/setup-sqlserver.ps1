# UniTask SQL Server Setup Script
# PowerShell script to automate SQL Server database setup for UniTask project
# Requires: SQL Server 2019+, PowerShell 5.0+

param(
    [Parameter(Mandatory=$false)]
    [string]$ServerInstance = "localhost",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "unitask",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "sa",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "YourPassword123!",
    
    [Parameter(Mandatory=$false)]
    [switch]$SeedData = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ScriptPath = (Split-Path -Parent $MyInvocation.MyCommand.Path)
)

# ==========================================
# CONFIGURATION
# ==========================================

$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

# Colors
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorWarning = "Yellow"
$ColorInfo = "Cyan"

# ==========================================
# FUNCTIONS
# ==========================================

function Write-Success {
    Write-Host "✓ $args" -ForegroundColor $ColorSuccess
}

function Write-Fail {
    Write-Host "✗ $args" -ForegroundColor $ColorError
}

function Write-Warn {
    Write-Host "⚠ $args" -ForegroundColor $ColorWarning
}

function Write-Header {
    Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor $ColorInfo
    Write-Host "║ $args" -ForegroundColor $ColorInfo
    Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor $ColorInfo
}

function Test-SQLServer {
    try {
        $connectionString = "Server=$ServerInstance;User Id=$Username;Password=$Password;Connection Timeout=5;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Execute-SqlFile {
    param(
        [string]$FilePath,
        [string]$Database
    )
    
    if (-not (Test-Path $FilePath)) {
        throw "File not found: $FilePath"
    }
    
    try {
        $sqlContent = Get-Content -Path $FilePath -Raw
        $connectionString = "Server=$ServerInstance;Database=$Database;User Id=$Username;Password=$Password;Connection Timeout=30;"
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        # Split by GO (SQL Server batch separator)
        $batches = $sqlContent -split "GO\s*`n"
        
        foreach ($batch in $batches) {
            if ($batch.Trim().Length -gt 0) {
                $command = $connection.CreateCommand()
                $command.CommandText = $batch
                $command.CommandTimeout = 300
                $command.ExecuteNonQuery() | Out-Null
            }
        }
        
        $connection.Close()
        return $true
    }
    catch {
        throw $_
    }
}

function Show-Menu {
    Write-Host "`nOptions:" -ForegroundColor $ColorInfo
    Write-Host "1. Setup database schema only"
    Write-Host "2. Setup schema + seed data"
    Write-Host "3. Seed data only (database must exist)"
    Write-Host "4. Exit"
    Write-Host ""
}

# ==========================================
# MAIN EXECUTION
# ==========================================

function Main {
    Write-Header "UniTask SQL Server Setup"
    
    Write-Host "SQL Server Configuration:" -ForegroundColor $ColorInfo
    Write-Host "  Server: $ServerInstance"
    Write-Host "  Database: $DatabaseName"
    Write-Host "  Username: $Username"
    Write-Host ""
    
    # Test SQL Server connection
    Write-Host "Testing SQL Server connection..." -ForegroundColor $ColorInfo
    if (-not (Test-SQLServer)) {
        Write-Fail "Cannot connect to SQL Server at $ServerInstance"
        Write-Warn "Possible issues:"
        Write-Warn "  - SQL Server is not running"
        Write-Warn "  - Server instance name is incorrect"
        Write-Warn "  - Username or password is wrong"
        Write-Warn "  - Network connectivity issue"
        Write-Host ""
        Write-Host "How to fix:"
        Write-Host "  1. Check SQL Server is running"
        Write-Host "  2. Verify connection string is correct"
        Write-Host "  3. Run with correct parameters:"
        Write-Host "     .\setup-sqlserver.ps1 -ServerInstance 'MYCOMPUTER\SQLEXPRESS' -Password 'YourPassword'"
        exit 1
    }
    Write-Success "Connected to SQL Server"
    
    # Menu
    Show-Menu
    $choice = Read-Host "Select option (1-4)"
    
    switch ($choice) {
        "1" {
            Setup-SchemaOnly
        }
        "2" {
            Setup-SchemaAndSeed
        }
        "3" {
            Setup-SeedOnly
        }
        "4" {
            Write-Host "Exiting..." -ForegroundColor $ColorInfo
            exit 0
        }
        default {
            Write-Warn "Invalid selection. Exiting."
            exit 1
        }
    }
}

function Setup-SchemaOnly {
    Write-Header "Creating Database Schema"
    
    $schemaFile = Join-Path $ScriptPath "schema-sqlserver.sql"
    
    if (-not (Test-Path $schemaFile)) {
        Write-Fail "Schema file not found: $schemaFile"
        exit 1
    }
    
    try {
        Write-Host "Creating database and tables..." -ForegroundColor $ColorInfo
        Execute-SqlFile -FilePath $schemaFile -Database "master"
        
        Write-Success "Database schema created successfully!"
        Write-Host ""
        Write-Host "Database Summary:" -ForegroundColor $ColorInfo
        Write-Host "  - 19 tables created"
        Write-Host "  - 8 triggers for auto-UpdatedAt"
        Write-Host "  - 3 views for common queries"
        Write-Host "  - 30+ indexes for performance"
        
    }
    catch {
        Write-Fail "Failed to create schema: $_"
        exit 1
    }
}

function Setup-SchemaAndSeed {
    Write-Header "Creating Database Schema and Seeding Data"
    
    Setup-SchemaOnly
    Setup-SeedOnly
}

function Setup-SeedOnly {
    Write-Header "Seeding Sample Data"
    
    $seedFile = Join-Path $ScriptPath "seed-sqlserver.sql"
    
    if (-not (Test-Path $seedFile)) {
        Write-Fail "Seed file not found: $seedFile"
        exit 1
    }
    
    try {
        Write-Host "Inserting sample data..." -ForegroundColor $ColorInfo
        Execute-SqlFile -FilePath $seedFile -Database $DatabaseName
        
        Write-Success "Sample data inserted successfully!"
        Write-Host ""
        Write-Host "Sample Data Summary:" -ForegroundColor $ColorInfo
        Write-Host "  - 8 users (4 students, 4 businesses)"
        Write-Host "  - 4 student profiles"
        Write-Host "  - 4 business profiles"
        Write-Host "  - 6 job categories"
        Write-Host "  - 9 sample jobs"
        Write-Host "  - 4 job applications"
        Write-Host "  - 2 payments"
        Write-Host "  - 1 review"
        Write-Host "  - 10 skills"
        Write-Host "  - 6 student-skill associations"
        Write-Host "  - 3 notifications"
        Write-Host "  - 4 FAQ entries"
        Write-Host ""
        Write-Host "Test Credentials:" -ForegroundColor $ColorInfo
        Write-Host "  Password: password123"
        Write-Host "  Student Account: student1@edu.vn"
        Write-Host "  Business Account: technova@company.vn"
        
    }
    catch {
        Write-Fail "Failed to seed data: $_"
        exit 1
    }
}

# Show instructions
function Show-Instructions {
    Write-Host "`nNext Steps:" -ForegroundColor $ColorInfo
    Write-Host ""
    Write-Host "1. Update your appsettings.json connection string:"
    Write-Host '   "DefaultConnection": "Server=localhost;Database=unitask;User Id=sa;Password=YourPassword123!;Encrypt=false;"'
    Write-Host ""
    Write-Host "2. In your .NET project, install Entity Framework:"
    Write-Host "   dotnet add package Microsoft.EntityFrameworkCore.SqlServer"
    Write-Host ""
    Write-Host "3. Configure DbContext in Program.cs"
    Write-Host ""
    Write-Host "4. Run migrations (if needed):"
    Write-Host "   dotnet ef migrations add InitialCreate"
    Write-Host "   dotnet ef database update"
    Write-Host ""
    Write-Host "5. Start your API:"
    Write-Host "   dotnet run"
    Write-Host ""
}

# ==========================================
# START
# ==========================================

try {
    Main
    Show-Instructions
    Write-Success "Setup completed! Database is ready for development."
}
catch {
    Write-Fail "Setup failed: $_"
    exit 1
}
