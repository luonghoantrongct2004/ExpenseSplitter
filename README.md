# 💰 **Expense Splitter - Ứng Dụng Chia Tiền Nhóm**

## 🎯 **Giới Thiệu**

Expense Splitter là ứng dụng web giúp quản lý và chia sẻ chi phí trong nhóm một cách công bằng và minh bạch. Phù hợp cho sinh viên, nhóm bạn, đồng nghiệp khi đi ăn uống, du lịch cùng nhau.

### **Vấn đề giải quyết**
- Khó khăn trong việc tính toán "ai nợ ai bao nhiêu"
- Quên ghi chép chi tiêu chung
- Tranh cãi về tiền bạc trong nhóm
- Khó theo dõi lịch sử chi tiêu

## ✨ **Tính Năng**

### **Core Features (MVP)**
- ✅ Đăng nhập với Google OAuth
- ✅ Tạo và quản lý nhóm
- ✅ Thêm chi phí và chia tiền
- ✅ Tính toán nợ tự động
- ✅ Thanh toán và quyết toán
- ✅ Lịch sử chi tiêu

### **Advanced Features**
- 📸 Scan bill (OCR) 
- 💳 QR Payment (VietQR)
- 📊 Thống kê chi tiêu
- 🔔 Thông báo realtime
- 🌐 Đa ngôn ngữ (VI/EN)

## 🏗️ **Kiến Trúc Hệ Thống**

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│   Frontend      │────▶│   Backend API   │────▶│   PostgreSQL    │
│   (Next.js)     │     │   (.NET 8)     │     │                 │
│                 │     │                 │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Google OAuth   │     │  File Storage   │
└─────────────────┘     └─────────────────┘
```

## 💻 **Tech Stack**

### **Backend (.NET 8)**
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8
- **Database**: PostgreSQL
- **Authentication**: JWT + Google OAuth
- **Real-time**: SignalR
- **Documentation**: Swagger/OpenAPI

### **Frontend (Next.js 14)**
- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State**: Zustand
- **API Client**: Axios + React Query
- **Forms**: React Hook Form + Zod

### **Infrastructure**
- **Backend Hosting**: Railway/Azure
- **Frontend Hosting**: Vercel
- **Database**: Supabase (PostgreSQL)
- **File Storage**: Supabase Storage

## 🗄️ **Database Schema**

### **Core Tables**

#### **1. Users**
```sql
- id: Guid (PK)
- googleId: string (unique)
- email: string (unique)
- name: string
- avatarUrl: string?
- isActive: boolean
- lastLoginAt: DateTime?
- createdAt: DateTime
- updatedAt: DateTime
```

#### **2. Groups**
```sql
- id: Guid (PK)
- name: string
- description: string?
- currency: string (default: 'VND')
- inviteCode: string (unique)
- createdById: Guid (FK → Users)
- isActive: boolean
- createdAt: DateTime
- updatedAt: DateTime
```

#### **3. GroupMembers**
```sql
- id: Guid (PK)
- groupId: Guid (FK → Groups)
- userId: Guid (FK → Users)
- role: enum (ADMIN, MEMBER)
- joinedAt: DateTime
- leftAt: DateTime?
- isActive: boolean
```

#### **4. Expenses**
```sql
- id: Guid (PK)
- groupId: Guid (FK → Groups)
- amount: decimal(12,2)
- currency: string
- description: string
- category: string?
- paidById: Guid (FK → Users)
- expenseDate: DateTime
- createdById: Guid (FK → Users)
- isDeleted: boolean
- createdAt: DateTime
- updatedAt: DateTime
```

#### **5. ExpenseSplits**
```sql
- id: Guid (PK)
- expenseId: Guid (FK → Expenses)
- userId: Guid (FK → Users)
- amount: decimal(12,2)
- percentage: decimal(5,2)?
- isSettled: boolean
```

#### **6. Settlements**
```sql
- id: Guid (PK)
- groupId: Guid (FK → Groups)
- fromUserId: Guid (FK → Users)
- toUserId: Guid (FK → Users)
- amount: decimal(12,2)
- paymentMethod: string?
- referenceCode: string?
- status: enum (PENDING, COMPLETED, CANCELLED)
- settledAt: DateTime?
- createdAt: DateTime
```
## 📁 **Project Structure**

### **Backend Structure**
```
backend/
├── BE.API/                 # API Layer
│   ├── Controllers/        # API Controllers
│   ├── Middleware/         # Custom Middleware
│   └── Extensions/         # Extension Methods
├── BE.Application/         # Application Layer
│   ├── DTOs/              # Data Transfer Objects
│   ├── Services/          # Business Services
│   └── Interfaces/        # Service Interfaces
├── BE.Domain/             # Domain Layer
│   ├── Entities/          # Domain Entities
│   ├── Interfaces/        # Repository Interfaces
│   ├── Specifications/    # Query Specifications
│   └── Exceptions/        # Domain Exceptions
└── BE.Infrastructure/     # Infrastructure Layer
    ├── Data/              # EF Core DbContext
    ├── Repositories/      # Repository Implementations
    └── Services/          # External Services
```

### **Frontend Structure**
```
frontend/
├── app/                   # Next.js App Router
│   ├── (auth)/           # Auth Pages
│   ├── (dashboard)/      # Protected Pages
│   └── api/              # API Routes
├── components/           # React Components
│   ├── ui/               # Base UI Components
│   ├── forms/            # Form Components
│   └── features/         # Feature Components
├── lib/                  # Utilities
│   ├── api/              # API Client
│   └── utils/            # Helper Functions
├── hooks/                # Custom React Hooks
├── store/                # Zustand Stores
└── types/                # TypeScript Types
```


**Happy Splitting! 🎉**
