# UniTask Database Documentation

## 🚀 Database Options

### **Using .NET API + SQL Server?**
👉 **Go to [README-SQLSERVER.md](./README-SQLSERVER.md)** for complete SQL Server setup guide

### **Using Node.js/Python + PostgreSQL?**
👇 **Continue below** for PostgreSQL setup instructions

---

## Overview

UniTask is a micro-internship and freelance platform connecting students with businesses (SMEs/Startups). This database schema supports:

- User authentication (Students, Businesses, Admins)
- Job posting and application management
- Escrow-based payment system
- Two-way rating and review system
- Messaging and notifications
- Blog and content management
- Analytics and reporting

---

## Database Technology Stack

### PostgreSQL (Node.js / Python Backend)
- **Database**: PostgreSQL 13+
- **Language**: SQL with PL/pgSQL functions
- **Client**: Node.js, Python, or any PostgreSQL-compatible client
- **Schema**: [schema.sql](./schema.sql)
- **Seed Data**: [seed.sql](./seed.sql)

### SQL Server (.NET API)
- **Database**: SQL Server 2019+
- **Language**: T-SQL
- **ORM**: Entity Framework Core
- **Schema**: [schema-sqlserver.sql](./schema-sqlserver.sql)
- **Seed Data**: [seed-sqlserver.sql](./seed-sqlserver.sql)
- **Documentation**: [README-SQLSERVER.md](./README-SQLSERVER.md)
- **Setup Script**: [setup-sqlserver.ps1](./setup-sqlserver.ps1)

---

## Project Structure

```
database/
├── schema.sql                 # Complete database schema
├── seed.sql                   # Sample data for development
├── migrations/                # Database migrations (version control)
│   ├── 001_Create_Base_Tables.sql
│   ├── 002_Create_Jobs_Tables.sql
│   ├── 003_Create_Payment_Tables.sql
│   ├── 004_Create_Reviews_And_Skills.sql
│   ├── 005_Create_Messaging_And_Notifications.sql
│   └── 006_Create_Admin_And_Logging.sql
└── README.md                  # This file
```

---

## Setup Instructions

### 1. Install PostgreSQL

```bash
# Windows (using Chocolatey)
choco install postgresql

# macOS (using Homebrew)
brew install postgresql@15

# Linux (Ubuntu/Debian)
sudo apt-get install postgresql postgresql-contrib
```

### 2. Create Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create new database
CREATE DATABASE unitask;

# Verify
\l
```

### 3. Run Migrations (Recommended for Production)

```bash
# Run migrations in order
psql -U postgres -d unitask -f database/migrations/001_Create_Base_Tables.sql
psql -U postgres -d unitask -f database/migrations/002_Create_Jobs_Tables.sql
psql -U postgres -d unitask -f database/migrations/003_Create_Payment_Tables.sql
psql -U postgres -d unitask -f database/migrations/004_Create_Reviews_And_Skills.sql
psql -U postgres -d unitask -f database/migrations/005_Create_Messaging_And_Notifications.sql
psql -U postgres -d unitask -f database/migrations/006_Create_Admin_And_Logging.sql
```

### 4. Run Complete Schema (Development)

```bash
# For quick setup in development
psql -U postgres -d unitask -f database/schema.sql
```

### 5. Load Sample Data

```bash
psql -U postgres -d unitask -f database/seed.sql
```

### 6. Verify Installation

```bash
# Connect to database
psql -U postgres -d unitask

# Check tables
\dt

# Check views
\dv

# Exit
\q
```

---

## Table Reference

### 1. Authentication & Users

#### `users`
Main user table for all account types (students, businesses, admins)

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| email | VARCHAR(255) | Unique, Required |
| password_hash | VARCHAR(255) | Bcrypt hashed |
| full_name | VARCHAR(255) | Required |
| avatar_url | TEXT | Profile picture URL |
| user_type | ENUM | 'student', 'business', 'admin' |
| is_verified | BOOLEAN | Email verified status |
| is_active | BOOLEAN | Account active status |
| created_at | TIMESTAMP | Account creation date |
| last_login | TIMESTAMP | Last login timestamp |

**Key Constraints:**
- `email` is unique across the platform
- `user_type` determines which profile table applies

---

#### `student_profiles`
Detailed profile information for student users

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| user_id | UUID | FK → users |
| student_email | VARCHAR(255) | University email (optional) |
| university | VARCHAR(255) | Institution name |
| major | VARCHAR(255) | Field of study |
| graduation_year | INT | Expected graduation year |
| cv_url | TEXT | Resume/CV URL |
| is_verified | BOOLEAN | Student identity verified |
| skills | TEXT[] | Array of skill names |
| completed_jobs | INT | Count of finished jobs |
| total_earnings | DECIMAL | Total money earned |

**Key Constraints:**
- One profile per user (UNIQUE constraint on user_id)
- `skills` is a PostgreSQL array type

---

#### `business_profiles`
Company/business information for business users

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| user_id | UUID | FK → users |
| company_name | VARCHAR(255) | Official company name |
| company_size | ENUM | 'startup', 'sme', 'large' |
| industry | VARCHAR(255) | Business sector |
| logo_url | TEXT | Company logo |
| is_verified | BOOLEAN | Company identity verified |
| completed_projects | INT | Count of completed jobs |
| total_spent | DECIMAL | Total amount spent |
| rating | DECIMAL(3,2) | Average rating (1-5) |

---

### 2. Job Management

#### `jobs`
Job/task postings by businesses

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| business_id | UUID | FK → business_profiles |
| title | VARCHAR(255) | Job title |
| description | TEXT | Full job description |
| category | VARCHAR(100) | Job category slug |
| tags | TEXT[] | Array of keywords |
| status | ENUM | 'draft', 'open', 'in_progress', 'completed', 'cancelled' |
| salary_min | DECIMAL | Minimum payment |
| salary_max | DECIMAL | Maximum payment |
| duration_type | ENUM | 'micro' (1-7 days), 'short-term' (1-4 weeks), 'project' (1+ month) |
| experience_level | ENUM | 'beginner', 'intermediate', 'advanced' |
| spots_total | INT | Total positions |
| spots_filled | INT | Filled positions |
| is_featured | BOOLEAN | Featured/priority listing |
| deadline | DATE | Application deadline |

**Common Queries:**
```sql
-- Featured jobs
SELECT * FROM jobs WHERE is_featured = TRUE AND status = 'open' ORDER BY created_at DESC;

-- Jobs by category
SELECT * FROM jobs WHERE category = 'it-lap-trinh' AND status = 'open';

-- Jobs with available spots
SELECT * FROM jobs WHERE spots_filled < spots_total AND status = 'open';
```

---

#### `job_applications`
Student applications for jobs

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| job_id | UUID | FK → jobs |
| student_id | UUID | FK → student_profiles |
| status | ENUM | 'pending', 'accepted', 'rejected', 'completed', 'cancelled' |
| applied_at | TIMESTAMP | Application submission time |
| cover_letter | TEXT | Application message |
| accepted_at | TIMESTAMP | When business accepted |
| started_at | TIMESTAMP | When work started |
| completed_at | TIMESTAMP | When work finished |

---

#### `job_categories`
Category lookup table for job organization

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| name | VARCHAR(100) | Display name (e.g., "IT & Lập Trình") |
| slug | VARCHAR(100) | URL-safe identifier |
| description | TEXT | Category description |
| icon_url | TEXT | Category icon/emoji |

---

### 3. Payment & Wallet System

#### `student_wallets`
Balance tracking for each student

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| student_id | UUID | FK → student_profiles, UNIQUE |
| balance | DECIMAL | Current available balance |
| total_earned | DECIMAL | Lifetime earnings |
| total_withdrawn | DECIMAL | Total amount withdrawn |

---

#### `payments`
All financial transactions (escrow system)

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| job_id | UUID | FK → jobs |
| job_application_id | UUID | FK → job_applications |
| business_id | UUID | FK → business_profiles |
| student_id | UUID | FK → student_profiles |
| amount | DECIMAL | Payment amount |
| status | ENUM | 'pending', 'escrow', 'released', 'refunded', 'disputed' |
| payment_method | VARCHAR(100) | e.g., 'bank_transfer', 'wallet' |
| released_at | TIMESTAMP | When money was released |

**Escrow Flow:**
1. Business deposits money → status = 'escrow'
2. Student completes job
3. Business approves → status = 'released'
4. Money added to student wallet

---

#### `withdrawal_requests`
Student requests to withdraw money to bank

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| student_id | UUID | FK → student_profiles |
| amount | DECIMAL | Withdrawal amount |
| status | ENUM | 'pending', 'approved', 'rejected', 'completed' |
| bank_account_name | VARCHAR(255) | Account holder name |
| bank_account_number | VARCHAR(50) | Bank account number |
| bank_name | VARCHAR(255) | Bank name |
| requested_at | TIMESTAMP | When requested |
| completed_at | TIMESTAMP | When processed |

---

### 4. Ratings & Reviews

#### `reviews`
Two-way feedback system after job completion

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| job_id | UUID | FK → jobs |
| job_application_id | UUID | FK → job_applications |
| from_user_id | UUID | Reviewer (FK → users) |
| to_user_id | UUID | Reviewee (FK → users) |
| rating | INT | 1-5 stars (CHECK constraint) |
| comment | TEXT | Review text |
| skill_endorsements | TEXT[] | Endorsed skills |
| is_anonymous | BOOLEAN | Hide reviewer identity |
| created_at | TIMESTAMP | Review date |

---

#### `skills`
Skill reference table

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| name | VARCHAR(255) | Skill name, UNIQUE |
| category | VARCHAR(100) | Category (e.g., 'Frontend', 'Backend') |
| icon_url | TEXT | Skill icon/emoji |

---

#### `student_skills`
Skills possessed by each student with endorsement count

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| student_id | UUID | FK → student_profiles |
| skill_id | UUID | FK → skills |
| proficiency | ENUM | 'beginner', 'intermediate', 'advanced', 'expert' |
| endorsement_count | INT | Number of endorsements |
| added_at | TIMESTAMP | When skill was added |

**Unique Constraint:** (student_id, skill_id) - One skill per student

---

### 5. Communication

#### `conversations`
Chat conversations between two users

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| user_1_id | UUID | First participant (FK → users) |
| user_2_id | UUID | Second participant (FK → users) |
| last_message_at | TIMESTAMP | Most recent message time |
| created_at | TIMESTAMP | Conversation start |

---

#### `messages`
Individual messages within conversations

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| conversation_id | UUID | FK → conversations |
| sender_id | UUID | Message author (FK → users) |
| content | TEXT | Message text |
| attachment_url | TEXT | File/image attachment |
| is_read | BOOLEAN | Read status |
| created_at | TIMESTAMP | Send time |
| read_at | TIMESTAMP | When read |

---

#### `notifications`
System notifications for users

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| user_id | UUID | Recipient (FK → users) |
| type | VARCHAR(100) | Notification type |
| title | VARCHAR(255) | Short title |
| message | TEXT | Notification message |
| related_job_id | UUID | Related job (if applicable) |
| is_read | BOOLEAN | Read status |
| created_at | TIMESTAMP | Creation time |

**Notification Types:**
- `job_applied` - Someone applied for your job
- `application_accepted` - Your application was accepted
- `application_rejected` - Your application was rejected
- `payment_received` - Payment released
- `new_message` - New message received
- `review_received` - You received a review

---

### 6. Content & Blog

#### `blog_posts`
Blog articles by users

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| author_id | UUID | FK → users |
| title | VARCHAR(255) | Post title |
| slug | VARCHAR(255) | URL-safe title, UNIQUE |
| content | TEXT | Full post content (markdown) |
| excerpt | TEXT | Short preview |
| featured_image_url | TEXT | Cover image |
| category | VARCHAR(100) | Post category |
| tags | TEXT[] | Array of tags |
| status | ENUM | 'draft', 'published', 'archived' |
| view_count | INT | Number of views |
| like_count | INT | Number of likes |
| published_at | TIMESTAMP | Publication date |

---

#### `faqs`
Frequently Asked Questions

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| question | TEXT | Question text |
| answer | TEXT | Answer text |
| category | VARCHAR(100) | FAQ category |
| view_count | INT | Number of views |
| helpful_count | INT | Helpful votes |
| order_index | INT | Display order |

---

### 7. Admin & Moderation

#### `admin_reports`
User/job reports by community members

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| reported_by_id | UUID | Reporter (FK → users) |
| reported_user_id | UUID | Reported user (FK → users, nullable) |
| reported_job_id | UUID | Reported job (FK → jobs, nullable) |
| report_type | VARCHAR(100) | Type of violation |
| reason | TEXT | Report details |
| status | ENUM | 'pending', 'investigating', 'resolved', 'dismissed' |
| action_taken | TEXT | Admin action notes |
| created_at | TIMESTAMP | Report date |
| resolved_at | TIMESTAMP | When resolved |

---

#### `activity_logs`
Audit trail for user actions

| Field | Type | Notes |
|-------|------|-------|
| id | UUID | Primary Key |
| user_id | UUID | User who performed action (FK → users, nullable) |
| action_type | VARCHAR(100) | Type of action (e.g., 'login', 'job_created') |
| entity_type | VARCHAR(100) | What was affected ('job', 'user', 'payment') |
| entity_id | UUID | ID of affected entity |
| description | TEXT | Additional details |
| ip_address | VARCHAR(45) | User IP |
| user_agent | TEXT | Browser info |
| created_at | TIMESTAMP | Action timestamp |

---

## Database Views

### `student_dashboard_view`
Complete student profile with statistics

```sql
SELECT 
  u.id, email, full_name,
  sp.university, major, completed_jobs, total_earnings,
  sw.balance,
  pending_applications, active_jobs
FROM student_dashboard_view;
```

### `business_dashboard_view`
Complete business profile with statistics

```sql
SELECT 
  u.id, email,
  bp.company_name, industry, completed_projects, total_spent, rating,
  open_jobs, pending_applications
FROM business_dashboard_view;
```

### `job_details_view`
Job with application counts

```sql
SELECT 
  j.id, title, category, salary_min, salary_max,
  bp.company_name,
  total_applications, pending_applications
FROM job_details_view;
```

---

## Common SQL Queries

### Student Dashboard
```sql
SELECT u.id, u.full_name, sp.completed_jobs, sp.total_earnings, sw.balance
FROM users u
JOIN student_profiles sp ON u.id = sp.user_id
JOIN student_wallets sw ON sp.id = sw.student_id
WHERE u.id = $1;
```

### Available Jobs for Student
```sql
SELECT j.* FROM jobs j
WHERE j.status = 'open' 
  AND j.spots_filled < j.spots_total
  AND j.deadline > NOW()
  AND j.category = $1
ORDER BY j.is_featured DESC, j.created_at DESC;
```

### Student Applications Status
```sql
SELECT ja.id, j.title, ja.status, ja.applied_at, p.amount, p.status as payment_status
FROM job_applications ja
JOIN jobs j ON ja.job_id = j.id
LEFT JOIN payments p ON ja.id = p.job_application_id
WHERE ja.student_id = $1
ORDER BY ja.applied_at DESC;
```

### Business Job Analytics
```sql
SELECT 
  j.id, j.title,
  COUNT(ja.id) as application_count,
  SUM(CASE WHEN ja.status = 'completed' THEN 1 ELSE 0 END) as completed,
  SUM(p.amount) as total_paid
FROM jobs j
LEFT JOIN job_applications ja ON j.id = ja.job_id
LEFT JOIN payments p ON ja.id = p.job_application_id
WHERE j.business_id = $1
GROUP BY j.id, j.title;
```

### Student Earnings by Job
```sql
SELECT 
  j.title,
  p.amount,
  p.status,
  p.released_at,
  r.rating as job_rating
FROM payments p
JOIN job_applications ja ON p.job_application_id = ja.id
JOIN jobs j ON ja.job_id = j.id
LEFT JOIN reviews r ON ja.id = r.job_application_id AND r.to_user_id = $1
WHERE p.student_id = $1
ORDER BY p.created_at DESC;
```

---

## Indexes

The schema includes optimized indexes for common queries:

| Table | Index | Purpose |
|-------|-------|---------|
| users | email, user_type | Fast lookups by email/type |
| jobs | business_id, category, status, deadline | Job filtering |
| job_applications | job_id, student_id, status | Application tracking |
| payments | job_id, student_id, status | Payment queries |
| notifications | user_id, is_read | Notification filtering |
| blog_posts | author_id, status, slug | Blog queries |
| messages | conversation_id, sender_id | Chat history |
| activity_logs | user_id, created_at | Audit trails |

---

## Security Best Practices

1. **Password Storage**: Always hash passwords with bcrypt (min. 10 rounds)
2. **Foreign Keys**: All references have CASCADE delete for data integrity
3. **Enums**: Use PostgreSQL ENUMS for fixed value sets
4. **Indexes**: Created on frequently queried columns
5. **Constraints**: CHECK constraints on rating (1-5), amounts (positive)
6. **Audit Trail**: activity_logs table tracks all important actions
7. **Soft Deletes**: Consider adding `deleted_at` timestamps for sensitive data

---

## Backup & Maintenance

### Full Database Backup
```bash
pg_dump -U postgres -d unitask > unitask_backup.sql
```

### Restore from Backup
```bash
psql -U postgres -d unitask < unitask_backup.sql
```

### Analyze Performance
```sql
ANALYZE;
VACUUM ANALYZE;
```

### Check Table Sizes
```sql
SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

---

## TypeScript Integration

TypeScript types are available in `src/types/database.ts`:

```typescript
import { 
  User, 
  StudentProfile, 
  Job, 
  Payment 
} from '@/types/database';

// Type-safe queries
const student: StudentProfile = await fetchStudent(studentId);
const jobs: Job[] = await searchJobs(filters);
```

---

## Future Enhancements

- [ ] Full-text search on job descriptions and blog posts
- [ ] Recommendation engine (job-student matching)
- [ ] Dispute resolution system
- [ ] Subscription/premium features
- [ ] Integration with payment gateways (Stripe, PayPal)
- [ ] Two-factor authentication audit logs
- [ ] GDPR compliance (data export/deletion)

---

## Support & Questions

For database-related questions or improvements, please refer to the main project documentation or contact the development team.

---

**Last Updated**: May 13, 2025  
**Database Version**: 1.0  
**PostgreSQL Version**: 13+
