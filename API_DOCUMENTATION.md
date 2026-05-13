# UniTask Backend API & Database Documentation

## 📋 Mục Lục
1. [Database Overview](#database-overview)
2. [API Endpoints](#api-endpoints)
3. [Authentication](#authentication)
4. [Setup Instructions](#setup-instructions)

---

## Database Overview

### 🗄️ Database Schema (SQL Server 2019+)

#### **19 Bảng Chính**

| # | Bảng | Mục Đích | Rows |
|---|------|---------|------|
| 1 | Users | User accounts (students, businesses, admins) | 8 |
| 2 | StudentProfiles | Chi tiết hồ sơ sinh viên | 4 |
| 3 | BusinessProfiles | Chi tiết hồ sơ doanh nghiệp | 4 |
| 4 | JobCategories | Danh mục công việc | 6 |
| 5 | Jobs | Danh sách công việc/task | 9 |
| 6 | JobApplications | Ứng tuyển công việc | 4 |
| 7 | StudentWallets | Ví tiền sinh viên | 4 |
| 8 | Payments | Giao dịch thanh toán | 2 |
| 9 | WithdrawalRequests | Yêu cầu rút tiền | 0 |
| 10 | Skills | Danh sách kỹ năng | 10 |
| 11 | StudentSkills | Kỹ năng của sinh viên | 6 |
| 12 | Reviews | Đánh giá/bình luận | 1 |
| 13 | Conversations | Cuộc trò chuyện giữa users | 0 |
| 14 | Messages | Tin nhắn | 0 |
| 15 | Notifications | Thông báo cho users | 3 |
| 16 | BlogPosts | Bài viết blog | 0 |
| 17 | FAQs | Câu hỏi thường gặp | 4 |
| 18 | AdminReports | Báo cáo vi phạm | 0 |
| 19 | ActivityLogs | Nhật ký hoạt động | 0 |

---

### 📊 Entity Relationships

```
Users (8)
├── StudentProfile (4)
│   ├── StudentWallet (4)
│   ├── JobApplications (4)
│   ├── StudentSkills (6)
│   ├── Reviews (as ToUser)
│   └── WithdrawalRequests (0)
│
├── BusinessProfile (4)
│   ├── Jobs (9)
│   │   ├── JobApplications (4)
│   │   │   └── Payments (2)
│   │   │       └── Reviews (1)
│   │   └── JobCategories (6)
│   ├── Payments (2)
│   └── Reviews (as ToUser)
│
├── Conversations (0)
│   └── Messages (0)
│
├── BlogPosts (0)
├── ActivityLogs (0)
└── Notifications (3)
```

---

### 🔐 User Types & Roles

| Role | Permissions | Fields |
|------|-----------|--------|
| **student** | - Tìm kiếm job<br>- Ứng tuyển<br>- Gửi tin nhắn<br>- Nhận review | StudentProfile, Wallet, Skills |
| **business** | - Tạo job<br>- Quản lý ứng tuyển<br>- Thanh toán<br>- Gửi review | BusinessProfile, Jobs, Payments |
| **admin** | - Quản lý users<br>- Moderator<br>- Xem logs | AdminReports, ActivityLogs |

---

### 💰 Payment System

**Status Flow:**
```
pending → escrow → released → (complete)
                ↓
              refunded
```

**Escrow Logic:**
- Business tạo payment → status = `pending`
- Student nhận job → status = `escrow` (tiền giữ lại)
- Job hoàn thành → status = `released` (tiền về ví)
- Student rút tiền → WithdrawalRequest (3-5 ngày)

---

### 🏷️ Status Values

**Jobs:**
- `draft` - Nháp
- `open` - Mở nhận ứng tuyển
- `in_progress` - Đang thực hiện
- `completed` - Hoàn thành
- `cancelled` - Hủy

**JobApplications:**
- `pending` - Chờ phê duyệt
- `accepted` - Chấp nhận
- `rejected` - Từ chối
- `completed` - Hoàn thành
- `cancelled` - Hủy

**Payments:**
- `pending` - Chờ xác nhận
- `escrow` - Giữ tiền
- `released` - Giải phóng tiền
- `refunded` - Hoàn tiền
- `disputed` - Tranh chấp

---

## API Endpoints

### **Base URL:** `http://localhost:5000/api`

---

## 🔑 Authentication

### **POST** `/auth/register`
Đăng ký tài khoản mới

**Request Body:**
```json
{
  "email": "student@edu.vn",
  "password": "password123",
  "fullName": "Nguyễn Văn A",
  "userType": "student"
}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "student@edu.vn",
  "fullName": "Nguyễn Văn A",
  "userType": "student",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### **POST** `/auth/login`
Đăng nhập

**Request Body:**
```json
{
  "email": "student@edu.vn",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "email": "student@edu.vn",
    "fullName": "Nguyễn Văn A",
    "userType": "student"
  }
}
```

---

### **POST** `/auth/refresh-token`
Làm mới token

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### **POST** `/auth/logout`
Đăng xuất (cần Authorization header)

---

## 👥 Users Module

### **GET** `/users/{id}`
Lấy thông tin user

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "student1@edu.vn",
  "fullName": "Nguyễn Văn A",
  "phone": "0987654321",
  "bio": "Sinh viên năm 4, chuyên ngành Frontend",
  "userType": "student",
  "isVerified": true,
  "isActive": true,
  "createdAt": "2024-05-10T10:00:00Z",
  "avatarUrl": "https://..."
}
```

---

### **PUT** `/users/{id}`
Cập nhật thông tin user

**Request Body:**
```json
{
  "fullName": "Nguyễn Văn A",
  "phone": "0987654321",
  "bio": "Updated bio",
  "avatarUrl": "https://..."
}
```

---

### **POST** `/users/{id}/verify-email`
Xác thực email

**Request Body:**
```json
{
  "verificationCode": "123456"
}
```

---

## 🎓 Student Profile Module

### **GET** `/students/{userId}`
Lấy hồ sơ sinh viên

**Response:**
```json
{
  "id": "650e8400-e29b-41d4-a716-446655440001",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "studentEmail": "student1@hust.edu.vn",
  "university": "Đại học Bách Khoa Hà Nội",
  "major": "Công Nghệ Thông Tin",
  "graduationYear": 2025,
  "isVerified": true,
  "verifiedAt": "2024-05-10T10:00:00Z",
  "completedJobs": 5,
  "totalEarnings": 12500000,
  "bio": "Sinh viên năm 4, chuyên ngành Frontend",
  "portfolioUrl": "https://portfolio.com",
  "cvUrl": "https://..."
}
```

---

### **PUT** `/students/{userId}`
Cập nhật hồ sơ sinh viên

**Request Body:**
```json
{
  "university": "Đại học Bách Khoa Hà Nội",
  "major": "Công Nghệ Thông Tin",
  "graduationYear": 2025,
  "bio": "Updated bio",
  "portfolioUrl": "https://portfolio.com",
  "cvUrl": "https://..."
}
```

---

### **POST** `/students/{userId}/verify`
Xác thực là sinh viên (upload thẻ sinh viên)

**Request (Form Data):**
```
studentEmail: student1@hust.edu.vn
studentIdCard: [file]
```

---

### **GET** `/students/{userId}/dashboard`
Lấy dashboard sinh viên

**Response:**
```json
{
  "student": { ... },
  "wallet": {
    "balance": 2500000,
    "totalEarned": 12500000,
    "totalWithdrawn": 10000000
  },
  "stats": {
    "completedJobs": 5,
    "activeApplications": 2,
    "pendingApplications": 1,
    "totalEarnings": 12500000,
    "averageRating": 4.8
  },
  "recentJobs": [ ... ],
  "notifications": [ ... ]
}
```

---

## 🏢 Business Profile Module

### **GET** `/businesses/{userId}`
Lấy hồ sơ doanh nghiệp

**Response:**
```json
{
  "id": "750e8400-e29b-41d4-a716-446655440001",
  "userId": "550e8400-e29b-41d4-a716-446655440010",
  "companyName": "TechNova VN",
  "companyEmail": "info@technova.vn",
  "companyWebsite": "https://technova.vn",
  "companySize": "sme",
  "industry": "Software Development",
  "isVerified": true,
  "verifiedAt": "2024-05-10T10:00:00Z",
  "completedProjects": 24,
  "totalSpent": 245000000,
  "rating": 4.8
}
```

---

### **PUT** `/businesses/{userId}`
Cập nhật hồ sơ doanh nghiệp

**Request Body:**
```json
{
  "companyName": "TechNova VN",
  "companyEmail": "info@technova.vn",
  "companyWebsite": "https://technova.vn",
  "companySize": "sme",
  "industry": "Software Development",
  "description": "...",
  "logoUrl": "https://...",
  "address": "Hà Nội"
}
```

---

### **GET** `/businesses/{userId}/dashboard`
Lấy dashboard doanh nghiệp

**Response:**
```json
{
  "business": { ... },
  "stats": {
    "openJobs": 5,
    "totalApplications": 12,
    "pendingApplications": 3,
    "totalSpent": 245000000,
    "completedProjects": 24,
    "averageRating": 4.8
  },
  "recentApplications": [ ... ],
  "openJobs": [ ... ],
  "notifications": [ ... ]
}
```

---

## 📋 Job Categories Module

### **GET** `/categories`
Lấy danh sách danh mục

**Response:**
```json
[
  {
    "id": "850e8400-e29b-41d4-a716-446655440001",
    "name": "IT & Lập Trình",
    "slug": "it-lap-trinh",
    "description": "Công việc liên quan đến phát triển phần mềm, web",
    "jobCount": 3
  },
  {
    "id": "850e8400-e29b-41d4-a716-446655440002",
    "name": "Thiết Kế Đồ Họa",
    "slug": "thiet-ke",
    "description": "Thiết kế UI/UX, branding, đồ họa",
    "jobCount": 2
  }
]
```

---

## 💼 Jobs Module

### **GET** `/jobs`
Lấy danh sách công việc

**Query Parameters:**
- `status` - draft, open, in_progress, completed, cancelled
- `categoryId` - Lọc theo danh mục
- `isRemote` - true/false
- `isFeatured` - true/false
- `page` - Số trang (default: 1)
- `limit` - Số item/trang (default: 10)

**Response:**
```json
{
  "total": 9,
  "page": 1,
  "limit": 10,
  "data": [
    {
      "id": "950e8400-e29b-41d4-a716-446655440001",
      "title": "Frontend Developer (React + Tailwind)",
      "description": "Cần nhà phát triển Frontend...",
      "categoryId": "850e8400-e29b-41d4-a716-446655440001",
      "categoryName": "IT & Lập Trình",
      "businessId": "750e8400-e29b-41d4-a716-446655440001",
      "companyName": "TechNova VN",
      "tags": ["React", "Frontend", "E-commerce"],
      "status": "open",
      "salaryMin": 2500000,
      "salaryMax": 4000000,
      "currency": "VND",
      "durationType": "short-term",
      "durationDays": 14,
      "requiredSkills": ["React", "TypeScript", "Tailwind CSS"],
      "experienceLevel": "intermediate",
      "spotsTotal": 2,
      "spotsFilled": 1,
      "location": "Hồ Chí Minh",
      "isRemote": true,
      "isFeatured": true,
      "deadline": "2026-05-18T00:00:00Z",
      "createdAt": "2026-05-13T10:00:00Z",
      "updatedAt": "2026-05-13T10:00:00Z",
      "publishedAt": "2026-05-13T10:00:00Z"
    }
  ]
}
```

---

### **POST** `/jobs`
Tạo công việc mới (Business only)

**Request Body:**
```json
{
  "title": "Frontend Developer (React + Tailwind)",
  "description": "Cần nhà phát triển Frontend...",
  "categoryId": "850e8400-e29b-41d4-a716-446655440001",
  "tags": ["React", "Frontend", "E-commerce"],
  "salaryMin": 2500000,
  "salaryMax": 4000000,
  "durationType": "short-term",
  "durationDays": 14,
  "requiredSkills": ["React", "TypeScript", "Tailwind CSS"],
  "experienceLevel": "intermediate",
  "spotsTotal": 2,
  "location": "Hồ Chí Minh",
  "isRemote": true,
  "deadline": "2026-05-18T00:00:00Z"
}
```

---

### **GET** `/jobs/{id}`
Lấy chi tiết công việc

**Response:**
```json
{
  "id": "950e8400-e29b-41d4-a716-446655440001",
  "title": "Frontend Developer (React + Tailwind)",
  "description": "Cần nhà phát triển Frontend...",
  "category": {
    "id": "850e8400-e29b-41d4-a716-446655440001",
    "name": "IT & Lập Trình"
  },
  "business": {
    "id": "750e8400-e29b-41d4-a716-446655440001",
    "companyName": "TechNova VN",
    "rating": 4.8
  },
  "tags": ["React", "Frontend", "E-commerce"],
  "status": "open",
  "salaryMin": 2500000,
  "salaryMax": 4000000,
  "requiredSkills": ["React", "TypeScript", "Tailwind CSS"],
  "spotsTotal": 2,
  "spotsFilled": 1,
  "applications": [
    {
      "id": "a50e8400-e29b-41d4-a716-446655440001",
      "status": "accepted",
      "student": {
        "id": "650e8400-e29b-41d4-a716-446655440001",
        "name": "Nguyễn Văn A",
        "rating": 4.8
      },
      "appliedAt": "2026-05-11T10:00:00Z"
    }
  ],
  "createdAt": "2026-05-13T10:00:00Z"
}
```

---

### **PUT** `/jobs/{id}`
Cập nhật công việc (Business only)

**Request Body:** (Giống tạo job)

---

### **DELETE** `/jobs/{id}`
Xóa công việc (Business only)

---

### **PUT** `/jobs/{id}/publish`
Công khai công việc từ draft → open

---

## 📝 Job Applications Module

### **GET** `/jobs/{jobId}/applications`
Lấy danh sách ứng tuyển cho một job (Business only)

**Query Parameters:**
- `status` - pending, accepted, rejected, completed, cancelled
- `page` - Số trang

**Response:**
```json
{
  "total": 4,
  "data": [
    {
      "id": "a50e8400-e29b-41d4-a716-446655440001",
      "jobId": "950e8400-e29b-41d4-a716-446655440001",
      "studentId": "650e8400-e29b-41d4-a716-446655440001",
      "student": {
        "id": "650e8400-e29b-41d4-a716-446655440001",
        "name": "Nguyễn Văn A",
        "university": "Đại học Bách Khoa Hà Nội",
        "rating": 4.8,
        "completedJobs": 5
      },
      "status": "pending",
      "coverLetter": "Mình có kinh nghiệm 2 năm với React...",
      "proposedTimeline": "2 tuần",
      "appliedAt": "2026-05-11T10:00:00Z"
    }
  ]
}
```

---

### **POST** `/jobs/{jobId}/apply`
Ứng tuyển công việc (Student only)

**Request Body:**
```json
{
  "coverLetter": "Mình có kinh nghiệm 2 năm với React...",
  "proposedTimeline": "2 tuần"
}
```

**Response:**
```json
{
  "id": "a50e8400-e29b-41d4-a716-446655440001",
  "jobId": "950e8400-e29b-41d4-a716-446655440001",
  "studentId": "650e8400-e29b-41d4-a716-446655440001",
  "status": "pending",
  "appliedAt": "2026-05-13T10:00:00Z"
}
```

---

### **GET** `/applications/{id}`
Lấy chi tiết ứng tuyển

---

### **PUT** `/applications/{id}/accept`
Chấp nhận ứng tuyển (Business only)

**Request Body:**
```json
{
  "startDate": "2026-05-15T00:00:00Z"
}
```

---

### **PUT** `/applications/{id}/reject`
Từ chối ứng tuyển (Business only)

**Request Body:**
```json
{
  "rejectionReason": "Chúng tôi đã chọn ứng viên khác"
}
```

---

### **PUT** `/applications/{id}/complete`
Đánh dấu hoàn thành (Business only)

---

### **GET** `/my-applications`
Lấy danh sách ứng tuyển của student hiện tại

**Response:**
```json
[
  {
    "id": "a50e8400-e29b-41d4-a716-446655440001",
    "job": {
      "id": "950e8400-e29b-41d4-a716-446655440001",
      "title": "Frontend Developer (React + Tailwind)",
      "company": "TechNova VN",
      "salary": "2.5M - 4M"
    },
    "status": "accepted",
    "appliedAt": "2026-05-11T10:00:00Z",
    "acceptedAt": "2026-05-12T10:00:00Z"
  }
]
```

---

## 💳 Payments Module

### **GET** `/payments`
Lấy danh sách thanh toán

**Query Parameters:**
- `status` - pending, escrow, released, refunded, disputed
- `userId` - Lọc theo user
- `page`

**Response:**
```json
{
  "total": 2,
  "data": [
    {
      "id": "c50e8400-e29b-41d4-a716-446655440001",
      "jobApplicationId": "a50e8400-e29b-41d4-a716-446655440001",
      "amount": 3000000,
      "currency": "VND",
      "status": "released",
      "paymentMethod": "bank_transfer",
      "createdAt": "2026-05-12T10:00:00Z",
      "releasedAt": "2026-05-13T10:00:00Z"
    }
  ]
}
```

---

### **POST** `/payments`
Tạo thanh toán (Business only)

**Request Body:**
```json
{
  "jobApplicationId": "a50e8400-e29b-41d4-a716-446655440001",
  "amount": 3000000,
  "paymentMethod": "bank_transfer"
}
```

---

### **POST** `/payments/{id}/release`
Giải phóng tiền từ escrow (Business only)

---

### **POST** `/payments/{id}/refund`
Hoàn tiền (Admin/Business)

---

## 👛 Wallets Module

### **GET** `/wallets/my-wallet`
Lấy ví tiền của sinh viên hiện tại

**Response:**
```json
{
  "id": "b50e8400-e29b-41d4-a716-446655440001",
  "studentId": "650e8400-e29b-41d4-a716-446655440001",
  "balance": 2500000,
  "totalEarned": 12500000,
  "totalWithdrawn": 10000000,
  "updatedAt": "2026-05-13T10:00:00Z"
}
```

---

### **POST** `/wallets/withdraw`
Yêu cầu rút tiền

**Request Body:**
```json
{
  "amount": 1000000,
  "bankAccountName": "Nguyễn Văn A",
  "bankAccountNumber": "0987654321",
  "bankName": "Agribank"
}
```

**Response:**
```json
{
  "id": "withdrawal_123",
  "amount": 1000000,
  "status": "pending",
  "requestedAt": "2026-05-13T10:00:00Z"
}
```

---

### **GET** `/wallets/withdrawal-history`
Lấy lịch sử rút tiền

**Response:**
```json
[
  {
    "id": "withdrawal_123",
    "amount": 1000000,
    "status": "pending",
    "requestedAt": "2026-05-13T10:00:00Z",
    "completedAt": null
  }
]
```

---

## ⭐ Reviews Module

### **GET** `/reviews`
Lấy danh sách review

**Query Parameters:**
- `userId` - Review của user
- `jobId` - Review của job
- `page`

**Response:**
```json
{
  "total": 1,
  "data": [
    {
      "id": "d50e8400-e29b-41d4-a716-446655440001",
      "jobId": "950e8400-e29b-41d4-a716-446655440001",
      "applicationId": "a50e8400-e29b-41d4-a716-446655440001",
      "rating": 5,
      "comment": "Code rất sạch, giao hàng đúng hạn. Sẽ hợp tác lại!",
      "skillEndorsements": ["React", "TypeScript", "Professional"],
      "createdAt": "2026-05-13T10:00:00Z"
    }
  ]
}
```

---

### **POST** `/reviews`
Tạo review (Student or Business)

**Request Body:**
```json
{
  "jobApplicationId": "a50e8400-e29b-41d4-a716-446655440001",
  "rating": 5,
  "comment": "Code rất sạch, giao hàng đúng hạn. Sẽ hợp tác lại!",
  "skillEndorsements": ["React", "TypeScript", "Professional"]
}
```

---

### **GET** `/users/{userId}/reviews`
Lấy review của một user

---

## 🏆 Skills Module

### **GET** `/skills`
Lấy danh sách tất cả kỹ năng

**Response:**
```json
[
  {
    "id": "e50e8400-e29b-41d4-a716-446655440001",
    "name": "React",
    "category": "Frontend",
    "iconUrl": "https://..."
  }
]
```

---

### **GET** `/students/{userId}/skills`
Lấy kỹ năng của sinh viên

**Response:**
```json
[
  {
    "id": "f50e8400-e29b-41d4-a716-446655440001",
    "skillId": "e50e8400-e29b-41d4-a716-446655440001",
    "skillName": "React",
    "proficiency": "advanced",
    "endorsementCount": 5,
    "addedAt": "2026-04-01T10:00:00Z"
  }
]
```

---

### **POST** `/students/me/skills`
Thêm kỹ năng cho sinh viên hiện tại

**Request Body:**
```json
{
  "skillId": "e50e8400-e29b-41d4-a716-446655440001",
  "proficiency": "advanced"
}
```

---

### **DELETE** `/students/me/skills/{skillId}`
Xóa kỹ năng

---

### **POST** `/skills/{skillId}/endorse`
Xác thực kỹ năng của sinh viên khác

**Request Body:**
```json
{
  "studentId": "650e8400-e29b-41d4-a716-446655440001"
}
```

---

## 💬 Messages Module

### **GET** `/conversations`
Lấy danh sách cuộc trò chuyện

**Response:**
```json
[
  {
    "id": "conv_123",
    "otherUser": {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "name": "TechNova VN Team",
      "avatar": "https://..."
    },
    "lastMessage": "Bạn rất phù hợp cho việc này!",
    "lastMessageAt": "2026-05-13T10:00:00Z",
    "unreadCount": 2
  }
]
```

---

### **GET** `/conversations/{conversationId}/messages`
Lấy tin nhắn trong cuộc trò chuyện

**Query Parameters:**
- `page`
- `limit`

**Response:**
```json
{
  "total": 10,
  "data": [
    {
      "id": "msg_123",
      "conversationId": "conv_123",
      "sender": {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "name": "Nguyễn Văn A"
      },
      "content": "Xin chào, tôi rất hứng thú với việc này",
      "attachmentUrl": null,
      "isRead": true,
      "createdAt": "2026-05-13T10:00:00Z"
    }
  ]
}
```

---

### **POST** `/conversations/{conversationId}/messages`
Gửi tin nhắn

**Request Body:**
```json
{
  "content": "Xin chào, tôi rất hứng thú với việc này",
  "attachmentUrl": null
}
```

---

### **POST** `/conversations/start`
Bắt đầu cuộc trò chuyện mới

**Request Body:**
```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440010"
}
```

---

## 🔔 Notifications Module

### **GET** `/notifications`
Lấy danh sách thông báo

**Query Parameters:**
- `isRead` - true/false
- `type` - application_accepted, job_applied, new_job_matched, etc.
- `page`

**Response:**
```json
[
  {
    "id": "950e8400-e29b-41d4-a716-000000000001",
    "type": "application_accepted",
    "title": "Ứng dụng được chấp nhận",
    "message": "Ứng dụng của bạn cho vị trí Frontend Developer đã được chấp nhận!",
    "relatedJobId": "950e8400-e29b-41d4-a716-446655440001",
    "isRead": false,
    "createdAt": "2026-05-13T10:00:00Z"
  }
]
```

---

### **PUT** `/notifications/{id}/read`
Đánh dấu thông báo là đã đọc

---

### **PUT** `/notifications/read-all`
Đánh dấu tất cả thông báo là đã đọc

---

## 📚 FAQs Module

### **GET** `/faqs`
Lấy danh sách FAQ

**Query Parameters:**
- `category` - verification, payment, application, etc.

**Response:**
```json
[
  {
    "id": "a60e8400-e29b-41d4-a716-446655440001",
    "question": "Làm thế nào để xác thực tài khoản sinh viên?",
    "answer": "Bạn cần cung cấp email đuôi .edu hoặc ảnh thẻ sinh viên...",
    "category": "verification",
    "viewCount": 100
  }
]
```

---

## 📰 Blog Module

### **GET** `/blog/posts`
Lấy danh sách bài blog

**Query Parameters:**
- `category`
- `status` - draft, published, archived
- `page`

**Response:**
```json
{
  "total": 0,
  "data": []
}
```

---

### **GET** `/blog/posts/{slug}`
Lấy chi tiết bài blog

---

### **POST** `/blog/posts`
Tạo bài blog (Admin only)

---

## 🚀 Setup Instructions

### 1. Database Setup

```powershell
cd database
.\setup-sqlserver.ps1 -Password "YourPassword123!" -SeedData
```

### 2. .NET Project Structure

```csharp
// Program.cs
using UniTask.Infrastructure.Data;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddDbContext<UniTaskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UniTaskDbContext>();
    db.Database.Migrate();
}

app.Run();
```

### 3. Create Controllers

Các controller cần tạo:
- `AuthController` - Login, Register, Refresh Token
- `UsersController` - Get/Update user info
- `StudentsController` - Student profiles & dashboard
- `BusinessesController` - Business profiles & dashboard
- `JobsController` - CRUD jobs
- `ApplicationsController` - Job applications
- `PaymentsController` - Payment management
- `WalletsController` - Student wallets
- `ReviewsController` - Reviews & ratings
- `SkillsController` - Skills management
- `MessagesController` - Conversations & messages
- `NotificationsController` - Notifications
- `FAQController` - FAQ entries
- `BlogController` - Blog posts

### 4. Environment Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=unitask;User Id=sa;Password=YourPassword123!;Encrypt=false;"
  },
  "Jwt": {
    "Secret": "your-very-long-secret-key-at-least-32-characters-long",
    "ExpirationMinutes": 1440
  },
  "AllowedOrigins": "http://localhost:5173,http://localhost:3000"
}
```

---

## 📊 Database Views (Pre-built Queries)

### 1. `StudentDashboardView`
```sql
SELECT * FROM [dbo].[StudentDashboardView]
WHERE StudentId = @studentId
```

Trả về: Student info + wallet balance + active jobs count

### 2. `BusinessDashboardView`
```sql
SELECT * FROM [dbo].[BusinessDashboardView]
WHERE BusinessId = @businessId
```

Trả về: Business info + spending + pending applications

### 3. `JobDetailsView`
```sql
SELECT * FROM [dbo].[JobDetailsView]
WHERE [Status] = 'open'
ORDER BY [CreatedAt] DESC
```

Trả về: Job + company info + category + total applications

---

## 🔐 Authentication Flow

1. **Register** → POST `/auth/register`
   - Tạo user account
   - Gửi verification email

2. **Verify Email** → POST `/users/{id}/verify-email`
   - Nhập verification code

3. **Login** → POST `/auth/login`
   - Nhận JWT token + refresh token

4. **Requests** → Gửi `Authorization: Bearer {token}`

5. **Refresh Token** → POST `/auth/refresh-token`
   - Khi JWT hết hạn

---

## 💾 Sample Data

**Users:**
- student1@edu.vn (password: password123)
- technova@company.vn (password: password123)

**Jobs:**
- 9 jobs mở (Frontend, Design, Content, Marketing)
- 4 job categories
- Mixed salary ranges (800k - 6M VND)

**Applications:**
- 4 ứng tuyển (pending, accepted)
- 2 payments (released, escrow)

---

## 🎯 Key Features

✅ **User Authentication** - JWT + Refresh tokens
✅ **Job Management** - CRUD + Status tracking
✅ **Application System** - Apply, Accept, Reject, Complete
✅ **Escrow Payments** - Secure payment system
✅ **Rating System** - Two-way reviews
✅ **Messaging** - Direct messages between users
✅ **Notifications** - Real-time updates
✅ **Skill Endorsement** - Community validation
✅ **Student Wallets** - Balance tracking & withdrawals
✅ **Admin Tools** - User management, moderation

---

## 📖 Documentation Files

- [README-SQLSERVER.md](./database/README-SQLSERVER.md) - Detailed database docs
- [QUICKSTART-SQLSERVER.md](./QUICKSTART-SQLSERVER.md) - Quick start guide
- [schema-sqlserver.sql](./database/schema-sqlserver.sql) - Database schema
- [seed-sqlserver.sql](./database/seed-sqlserver.sql) - Sample data

---

**Created:** May 2024
**Tech Stack:** .NET 6+, SQL Server 2019+, Entity Framework Core
**Last Updated:** May 13, 2026
