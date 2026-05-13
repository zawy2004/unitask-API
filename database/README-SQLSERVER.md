# UniTask SQL Server Database Setup Guide

## Overview
Complete SQL Server 2019+ database schema for UniTask - a micro-internship platform connecting students with businesses for micro-tasks.

## Files Included

### 1. `schema-sqlserver.sql`
- **Purpose**: Complete database schema with 19 tables
- **Includes**: 
  - Core tables (Users, StudentProfiles, BusinessProfiles)
  - Job management (JobCategories, Jobs, JobApplications)
  - Payment system (StudentWallets, Payments, WithdrawalRequests)
  - Social features (Reviews, Conversations, Messages, Notifications)
  - Content management (BlogPosts, FAQs)
  - Admin features (AdminReports, ActivityLogs)
  - Skills system (Skills, StudentSkills)
- **Features**:
  - Automatic triggers for UpdatedAt timestamps
  - 3 pre-built views for common queries
  - Proper indexes on frequently queried columns
  - CHECK constraints for status fields
  - Foreign key relationships with cascade rules

### 2. `seed-sqlserver.sql`
- **Purpose**: Sample data for development and testing
- **Includes**:
  - 8 users (4 students + 4 businesses)
  - 4 student profiles with earnings data
  - 4 business profiles with project history
  - 6 job categories
  - 9 sample jobs across different categories
  - 4 job applications with various statuses
  - 4 student wallets with balance tracking
  - 2 sample payments
  - 1 review with 5-star rating
  - 10 skills entries
  - 6 student-skill associations
  - 3 notifications
  - 4 FAQ entries
- **Test Accounts**: All users have password: `password123`
  - Bcrypt Hash: `$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm`

## Setup Instructions

### Option 1: Using SQL Server Management Studio (SSMS)

1. **Open SQL Server Management Studio**
2. **Connect** to your SQL Server instance
3. **Run** the schema file:
   ```
   File > Open > Select schema-sqlserver.sql
   Execute (F5)
   ```
4. **Run** the seed data file:
   ```
   File > Open > Select seed-sqlserver.sql
   Execute (F5)
   ```

### Option 2: Using SQL Server Command Line (sqlcmd)

```powershell
# Create database and tables
sqlcmd -S localhost -U sa -P YourPassword123! -i database/schema-sqlserver.sql

# Insert seed data
sqlcmd -S localhost -U sa -P YourPassword123! -d unitask -i database/seed-sqlserver.sql
```

### Option 3: Using PowerShell Script

```powershell
cd database
.\setup-sqlserver.ps1 -Password "YourPassword123!" -SeedData
```

## .NET API Integration

### 1. Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=unitask;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

### 2. Entity Framework Core Setup

Install NuGet packages:
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 3. DbContext Configuration

Example Program.cs:
```csharp
using UniTask.Infrastructure.Data;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<UniTaskDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo")
    );
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UniTaskDbContext>();
    context.Database.Migrate();
}

app.Run();
```

## Database Schema Overview

### Core Tables

| Table | Purpose | Rows |
|-------|---------|------|
| Users | User accounts (students, businesses, admins) | 8 |
| StudentProfiles | Student profile details | 4 |
| BusinessProfiles | Business profile details | 4 |

### Job Management

| Table | Purpose | Rows |
|-------|---------|------|
| JobCategories | Job categories (IT, Design, etc.) | 6 |
| Jobs | Job postings | 9 |
| JobApplications | Student applications to jobs | 4 |

### Financial System

| Table | Purpose | Rows |
|-------|---------|------|
| StudentWallets | Student account balances | 4 |
| Payments | Payment transactions | 2 |
| WithdrawalRequests | Withdrawal requests | 0 |

### Social & Review System

| Table | Purpose | Rows |
|-------|---------|------|
| Skills | Available skills | 10 |
| StudentSkills | Student skill proficiency | 6 |
| Reviews | Job reviews and ratings | 1 |
| Conversations | Direct messages between users | 0 |
| Messages | Individual messages | 0 |

### Content & Notifications

| Table | Purpose | Rows |
|-------|---------|------|
| BlogPosts | Blog articles | 0 |
| FAQs | FAQ entries | 4 |
| Notifications | User notifications | 3 |

### Admin & Logging

| Table | Purpose | Rows |
|-------|---------|------|
| AdminReports | Content moderation reports | 0 |
| ActivityLogs | User activity audit trail | 0 |

## Key Features

### 1. Automatic Timestamps
All main tables have `CreatedAt` and `UpdatedAt` fields that are automatically managed:
- `CreatedAt`: Set at record creation (SQL Server: `DEFAULT GETUTCDATE()`)
- `UpdatedAt`: Updated automatically on every modification (via triggers)

### 2. Status Tracking
All status fields use CHECK constraints:
- **Job Status**: draft, open, in_progress, completed, cancelled
- **Application Status**: pending, accepted, rejected, completed, cancelled
- **Payment Status**: pending, escrow, released, refunded, disputed
- **User Type**: student, business, admin

### 3. JSON Support
Array fields stored as JSON in NVARCHAR(MAX):
- `TagsJson`: Job tags
- `RequiredSkillsJson`: Required skills for jobs
- `SkillEndorsementsJson`: Endorsed skills in reviews

### 4. Pre-built Views

#### StudentDashboardView
Query: Get student dashboard summary with earnings and active jobs
```sql
SELECT * FROM [dbo].[StudentDashboardView] 
WHERE StudentId = @studentId
```

#### BusinessDashboardView
Query: Get business dashboard summary with spending and applications
```sql
SELECT * FROM [dbo].[BusinessDashboardView] 
WHERE BusinessId = @businessId
```

#### JobDetailsView
Query: Get job with company and category information
```sql
SELECT * FROM [dbo].[JobDetailsView] 
WHERE [Status] = 'open'
ORDER BY [CreatedAt] DESC
```

## Data Relationships

```
Users (8)
├── StudentProfiles (4)
│   ├── StudentWallets (4)
│   ├── JobApplications (4)
│   ├── StudentSkills (6)
│   └── Reviews (as ToUser)
├── BusinessProfiles (4)
│   ├── Jobs (9)
│   │   ├── JobApplications (4)
│   │   ├── JobCategories (6)
│   │   └── Reviews (as Job)
│   └── Payments (as Business)
├── Conversations (0)
├── Messages (0)
├── BlogPosts (0)
├── ActivityLogs (0)
└── AdminReports (as ReportedBy)
```

## Password Information

### Test Accounts
All seed data accounts use the same password: `password123`

### Hash Format
**Bcrypt** (standard for .NET Core Identity)
- Hash: `$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm`
- Cost Factor: 10

### To Generate New Hashes (C#)
```csharp
using System.Security.Cryptography;

public static string HashPassword(string password)
{
    string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    return hash;
}
```

## Troubleshooting

### Issue: "Cannot connect to server"
- Verify SQL Server is running: `sqlcmd -S localhost -U sa`
- Check connection string spelling
- Verify password is correct

### Issue: "Database already exists"
- Comment out the DROP DATABASE line in schema-sqlserver.sql
- Or delete the existing database manually via SSMS

### Issue: "Foreign key constraint fails"
- Ensure seed data is inserted AFTER schema creation
- Follow the correct table insertion order (seeds are properly ordered)
- Check that all GUIDs match between tables

### Issue: "IDENTITY_INSERT is OFF"
- This schema uses UNIQUEIDENTIFIER (GUID), not IDENTITY
- Manual ID insertion is allowed

## Backup & Maintenance

### Create Backup
```sql
BACKUP DATABASE [unitask] 
TO DISK = 'C:\Backup\unitask.bak'
WITH INIT, DESCRIPTION = 'Full backup of UniTask database'
```

### Restore Backup
```sql
RESTORE DATABASE [unitask] 
FROM DISK = 'C:\Backup\unitask.bak'
WITH REPLACE
```

## Performance Considerations

### Indexes
The schema includes 30+ non-clustered indexes on:
- Email (Users)
- Status fields (Jobs, Applications, Payments)
- User relationships
- Created dates for sorting
- Featured jobs

### Best Practices
1. Use views for common queries
2. Index all filter columns in WHERE clauses
3. Avoid SELECT * - specify needed columns
4. Use transactions for multi-table operations
5. Monitor query performance with SQL Server Management Studio

## Next Steps

1. **Run the schema script** to create the database structure
2. **Run the seed script** to populate sample data
3. **Update appsettings.json** with your connection string
4. **Configure Entity Framework Core** in your .NET project
5. **Test the connection** by running a simple query
6. **Start building API** endpoints using the DbContext

## Support Files

- **`migrations-sqlserver/`** - Incremental migration files (for version control)
- **`setup-sqlserver.ps1`** - Automated setup script (PowerShell)
- **`README.md`** - This documentation

## License
UniTask Database Schema - 2024

---

**Last Updated**: May 2024
**SQL Server Version**: 2019+
**Entity Framework Core Version**: 7.0+
