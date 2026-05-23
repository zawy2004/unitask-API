<#
.SYNOPSIS
  Apply schema (and optionally seed data) to an Azure SQL Database.

.DESCRIPTION
  Wraps sqlcmd so you can run schema + seed against Azure SQL from your laptop
  without juggling connection strings. Prerequisites:
    - sqlcmd installed (winget install Microsoft.SqlCmd  or  winget install Microsoft.SqlServer.Tools)
    - Your laptop IP whitelisted in the Azure SQL firewall.
    - The target database created in Azure (e.g. UnitaskExe).

.EXAMPLE
  .\scripts\Apply-AzureSqlSchema.ps1 -Server unitask.database.windows.net -User unitask -Database UnitaskExe -SeedData
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory)] [string] $Server,
  [Parameter(Mandatory)] [string] $User,
  [Parameter(Mandatory)] [string] $Database,
  [string] $Password,
  [switch] $SeedData,
  [string] $SchemaPath = (Join-Path $PSScriptRoot '..\database\schema-sqlserver.sql'),
  [string] $SeedPath   = (Join-Path $PSScriptRoot '..\database\seed-sqlserver.sql')
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
  throw "sqlcmd not found. Install with: winget install Microsoft.SqlCmd"
}

if (-not $Password) {
  $secure   = Read-Host -AsSecureString "Password for $User@$Server"
  $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
}

Write-Host "Connecting to $Server as $User..." -ForegroundColor Cyan

# Schema script uses 'USE UnitaskExe;' but the DB must already exist in Azure (portal).
Write-Host "Applying schema -> $Database" -ForegroundColor Cyan
sqlcmd -S $Server -U $User -P $Password -d $Database -N -C -b -i $SchemaPath
if ($LASTEXITCODE -ne 0) { throw "Schema failed (exit $LASTEXITCODE)" }
Write-Host "Schema applied OK." -ForegroundColor Green

if ($SeedData) {
  Write-Host "Applying seed -> $Database" -ForegroundColor Cyan
  sqlcmd -S $Server -U $User -P $Password -d $Database -N -C -b -i $SeedPath
  if ($LASTEXITCODE -ne 0) { throw "Seed failed (exit $LASTEXITCODE)" }
  Write-Host "Seed applied OK." -ForegroundColor Green
}

Write-Host "Done." -ForegroundColor Green
