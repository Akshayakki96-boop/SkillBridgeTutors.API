# Dashboard System Architecture & Data Flow Diagrams

## 📊 System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT APPLICATION                      │
│  ┌──────────────────────┐              ┌──────────────────────┐ │
│  │  Student Dashboard   │              │  Teacher Dashboard   │ │
│  │                      │              │                      │ │
│  │  - Today's Classes   │              │  - Today's Schedule  │ │
│  │  - Upcoming Classes  │              │  - Upcoming Classes  │ │
│  │  - Past Classes      │              │  - All Students      │ │
│  └──────────────────────┘              └──────────────────────┘ │
│           │                                      │               │
│           └──────────────────┬───────────────────┘               │
│                              │                                   │
│                    JWT Token Management                          │
│                     (localStorage)                               │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                    HTTP Requests with JWT
                               │
┌──────────────────────────────┼──────────────────────────────────┐
│                              │     ASP.NET CORE 8 API           │
│                    ┌─────────▼─────────┐                        │
│                    │   Controllers     │                        │
│     ┌──────────────┤                   ├──────────────┐         │
│     │              │ StudentsController│              │         │
│     │              │ TeachersController              │         │
│     │              └───────────────────┘              │         │
│     │                      │                         │         │
│     ▼                      ▼                         ▼         │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │           Repositories (Data Access Layer)             │  │
│  │  ┌──────────────────┐       ┌──────────────────────┐  │  │
│  │  │StudentRepository │       │RegularClassRepository│  │  │
│  │  │                  │       │                      │  │  │
│  │  │GetById()         │       │GetByStudent()        │  │  │
│  │  │GetByEmail()      │       │GetByTeacher()        │  │  │
│  │  │Create()          │       │GetById()             │  │  │
│  │  │Update()          │       │GetUpcoming()         │  │  │
│  │  │ExistsByEmail()   │       │Create()              │  │  │
│  │  └──────────────────┘       │Update()              │  │  │
│  │                             │Delete()              │  │  │
│  │  ┌──────────────────────────────────────────────┐  │  │  │
│  │  │      TeacherRepository (existing)            │  │  │  │
│  │  │   GetByIdAsync(teacherId)                    │  │  │  │
│  │  │   GetAllAsync()                              │  │  │  │
│  │  └──────────────────────────────────────────────┘  │  │  │
│  └──────────────────────────────────────────────────────┘  │
│                       │                                     │
│                       ▼                                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        Entity Framework Core DbContext               │  │
│  │                                                       │  │
│  │  DbSet<Student>                                      │  │
│  │  DbSet<Teacher>                                      │  │
│  │  DbSet<RegularClass>                                 │  │
│  │  DbSet<DemoBooking>                                  │  │
│  │  DbSet<Lead>                                         │  │
│  │  DbSet<Payment>                                      │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────┬───────────────────────────────┘
                              │
                   SQL Server Database
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                     DATABASE TABLES                         │
│                                                             │
│  ┌──────────────┐  ┌─────────────┐  ┌──────────────────┐  │
│  │  Students    │  │   Teachers  │  │ RegularClasses   │  │
│  │              │  │             │  │                  │  │
│  │Id (PK)       │  │TeacherId(PK)│  │ClassId (PK)      │  │
│  │FullName      │  │FullName     │  │StudentId (FK)    │  │
│  │Email (UX)    │  │Email (UX)   │  │TeacherId (FK)    │  │
│  │Phone         │  │Phone        │  │Subject           │  │
│  │ClassYear     │  │Subjects     │  │StartTime         │  │
│  │Subject       │  │IsActive     │  │EndTime           │  │
│  │IsActive      │  │CreatedAt    │  │Status            │  │
│  │CreatedAt     │  └─────────────┘  │RecurrencePattern │  │
│  └──────────────┘                    │MeetingLink       │  │
│         ▲                            │CreatedAt         │  │
│         │                            └──────────────────┘  │
│         │ 1:M Relationship                  ▲              │
│         └────────────────────────────────────┘              │
│                                                             │
│  ┌──────────────┐           ┌──────────────────┐          │
│  │ DemoBookings │           │      Leads       │          │
│  │              │           │                  │          │
│  │BookingId(PK) │           │LeadId (PK)       │          │
│  │SlotId (FK)   │           │StudentName       │          │
│  │LeadId (FK)   │           │Email             │          │
│  │TeacherId(FK) │           │Phone             │          │
│  │Status        │           │Subject           │          │
│  │MeetingLink   │           │ClassYear         │          │
│  │BookedAt      │           │CreatedAt         │          │
│  └──────────────┘           └──────────────────┘          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Student Dashboard Data Flow

```
┌─────────────────────────────────────────────────────┐
│ STUDENT LOGIN                                        │
│ ├─ Email: john@example.com                          │
│ └─ Password: ****                                   │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ AUTHENTICATION (StudentAuthController)              │
│ ├─ Hash password                                    │
│ ├─ Verify credentials                               │
│ └─ Generate JWT Token                               │
│    └─ Claims: { sub: "1", email, exp, iat }        │
└────────────┬────────────────────────────────────────┘
             │
             ▼ JWT Token returned to client
             │ (stored in localStorage)
             │
┌─────────────────────────────────────────────────────┐
│ CLIENT APP: Navigate to Dashboard                   │
│ ├─ Check token exists                               │
│ └─ Set Authorization header                         │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ HTTP REQUEST                                         │
│ GET /api/students/dashboard                         │
│ Header: Authorization: Bearer {jwt_token}           │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ STUDENTSCONTROLLER                                  │
│ ├─ Parse JWT token                                  │
│ ├─ Extract studentId from 'sub' claim               │
│ └─ Validate studentId is numeric                    │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ STUDENT REPOSITORY                                  │
│ └─ GetByIdAsync(studentId)                          │
│    ├─ Include RegularClasses                        │
│    └─ Return Student entity or null                 │
└────────────┬────────────────────────────────────────┘
             │
      ┌──────┴──────┐
      │             │
   ✅ Found      ❌ Not Found
      │             │
      ▼             ▼
  Continue    Return 404
      │
      ▼
┌─────────────────────────────────────────────────────┐
│ REGULAR CLASS REPOSITORY                            │
│ └─ GetByStudentAsync(studentId)                     │
│    ├─ WHERE StudentId = ? AND Status != 'Cancelled' │
│    ├─ Include Teacher                               │
│    ├─ OrderBy StartTime                             │
│    └─ Return List<RegularClass>                     │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ DATA PROCESSING                                     │
│                                                      │
│ All Classes ──┬──────────────────────────────────┐  │
│               │                                  │  │
│               ▼ Filter by date                  │  │
│         ┌─────────────────┐                     │  │
│         │ TODAY'S CLASSES │ (date == today)     │  │
│         ├─ Class 1        │                     │  │
│         ├─ Class 2        │                     │  │
│         └─ Sort: StartTime│                     │  │
│                │                                │  │
│                ▼                                │  │
│         ┌──────────────────────────┐           │  │
│         │ StudentClassDto Array    │           │  │
│         └──────────────────────────┘           │  │
│               │                                 │  │
│         ┌─────┴──────────────────────────┐    │  │
│         │  Filter: StartTime > today     │    │  │
│         ▼                                 ▼    │  │
│    ┌──────────────────────┐    ┌──────────────┤  │
│    │UPCOMING CLASSES      │    │PAST CLASSES   │  │
│    │(max 20)              │    │(max 20)       │  │
│    ├─ Class 1            │    ├─ Class 1     │  │
│    ├─ Class 2            │    ├─ Class 2     │  │
│    └─ Sort: StartTime    │    └─ Sort: DESC  │  │
│         │                 │        │          │  │
│         └─────────────────┴────────┘          │  │
│                 │                             │  │
│                 ▼                             │  │
│         ┌──────────────────────────────────┐ │  │
│         │ Combine all arrays into DTO      │ │  │
│         └──────────────────────────────────┘ │  │
│                 │                             │  │
└─────────────────┼─────────────────────────────┘  │
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ BUILD RESPONSE DTO                                  │
│                                                     │
│ StudentDashboardDto {                               │
│   studentId: 1,                                     │
│   studentName: "John Doe",                          │
│   email: "john@example.com",                        │
│   phone: "+1-234-567-8900",                         │
│   classYear: "10th Grade",                          │
│   subject: "Mathematics",                           │
│   totalClasses: 24,                                 │
│   completedClasses: 12,                             │
│   upcomingClasses: 6,                               │
│   todaysClasses: [ StudentClassDto[] ],             │
│   upcomingClassesList: [ StudentClassDto[] ],       │
│   pastClasses: [ StudentClassDto[] ]                │
│ }                                                   │
└─────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ HTTP RESPONSE: 200 OK                               │
│ Content-Type: application/json                      │
│                                                     │
│ {StudentDashboardDto serialized to JSON}            │
└─────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ CLIENT APPLICATION                                  │
│ ├─ Parse JSON response                              │
│ ├─ Update component state                           │
│ └─ Render UI                                        │
│    ├─ Display student info card                    │
│    ├─ Display tabs (Today/Upcoming/Past)           │
│    └─ Display class cards with details             │
└─────────────────────────────────────────────────────┘
```

---

## 🎯 Teacher Dashboard Data Flow

```
┌─────────────────────────────────────────────────────┐
│ TEACHER LOGIN                                        │
│ ├─ Email: smith@example.com                         │
│ └─ Password: ****                                   │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ AUTHENTICATION (TeacherAuthController)              │
│ ├─ Hash password                                    │
│ ├─ Verify credentials                               │
│ └─ Generate JWT Token                               │
│    └─ Claims: { sub: "1", email, exp, iat }        │
└────────────┬────────────────────────────────────────┘
             │
             ▼ JWT Token returned to client
             │ (stored in localStorage)
             │
┌─────────────────────────────────────────────────────┐
│ CLIENT APP: Navigate to Dashboard                   │
│ ├─ Check token exists                               │
│ └─ Set Authorization header                         │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ HTTP REQUEST                                         │
│ GET /api/teachers/dashboard                         │
│ Header: Authorization: Bearer {jwt_token}           │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ TEACHERSCONTROLLER                                  │
│ ├─ Parse JWT token                                  │
│ ├─ Extract teacherId from 'sub' claim               │
│ └─ Validate teacherId is numeric                    │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ TEACHER REPOSITORY                                  │
│ └─ GetByIdAsync(teacherId)                          │
│    ├─ Include DemoBookings                          │
│    └─ Return Teacher entity or null                 │
└────────────┬────────────────────────────────────────┘
             │
      ┌──────┴──────┐
      │             │
   ✅ Found      ❌ Not Found
      │             │
      ▼             ▼
  Continue    Return 404
      │
      ▼
┌─────────────────────────────────────────────────────┐
│ DATA RETRIEVAL - TWO PARALLEL QUERIES                │
│                                                      │
│  ┌────────────────────┐  ┌──────────────────────┐   │
│  │ DEMO BOOKINGS      │  │ REGULAR CLASSES      │   │
│  │ Query              │  │ Query                │   │
│  │                    │  │                      │   │
│  │ DemoBookingRepo    │  │ RegularClassRepo     │   │
│  │ .Where(TeacherId)  │  │ .GetByTeacher()      │   │
│  │ .Include(Lead)     │  │ .Include(Student)    │   │
│  │ .Include(Slot)     │  │ .OrderBy StartTime   │   │
│  │ .OrderBy StartTime │  │                      │   │
│  │                    │  │                      │   │
│  └────────┬───────────┘  └──────────┬───────────┘   │
│           │                         │                │
│           ▼                         ▼                │
│     List<DemoBooking>        List<RegularClass>     │
│   (Status = "Booked")        (Status != "Cancelled")│
└─────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ DATA PROCESSING                                     │
│                                                      │
│ Demo Bookings ────┬──────────────────────────────┐  │
│ Regular Classes ──┤                              │  │
│                  │                               │  │
│                  ▼                               │  │
│         ┌─────────────────────┐                 │  │
│         │ TODAY'S CLASSES     │                 │  │
│         │ (date == today)     │                 │  │
│         │                     │                 │  │
│         │ Demo classes:       │                 │  │
│         │  - Class 1 (10:00)  │                 │  │
│         │                     │                 │  │
│         │ Regular classes:    │                 │  │
│         │  - Class 2 (11:30)  │                 │  │
│         │  - Class 3 (14:00)  │                 │  │
│         │                     │                 │  │
│         │ Sorted by StartTime │                 │  │
│         └──────────┬──────────┘                 │  │
│                    │                            │  │
│                    ▼                            │  │
│            ┌────────────────┐                  │  │
│            │ScheduledClassDto                 │  │
│            │ Array (Combined)                 │  │
│            └────────────────┘                  │  │
│                    │                            │  │
│         ┌──────────┴──────────┐                │  │
│         │ Filter:             │                │  │
│         │ StartTime > today   │                │  │
│         │ Max: 20 classes     │                │  │
│         ▼                      ▼                │  │
│  ┌──────────────────┐  ┌──────────────────┐   │  │
│  │UPCOMING CLASSES  │  │ALL UNIQUE STUDENTS   │   │
│  │(max 20)          │  │(from both sources)   │   │
│  │                  │  │                      │   │
│  │Demo + Regular    │  │From Demo Bookings:   │   │
│  │Combined & Sorted │  │ - Student 1 (3 demos)    │   │
│  │                  │  │                      │   │
│  │UpcomingClassDto  │  │From Regular Classes: │   │
│  │ Array            │  │ - Student 2 (5 reg)  │   │
│  │                  │  │ - Student 3 (2 reg)  │   │
│  │                  │  │                      │   │
│  │                  │  │Deduplicated List     │   │
│  │                  │  │StudentDto Array      │   │
│  └──────────────────┘  └──────────────────┘   │  │
│                                                  │  │
└──────────────────────────────────────────────────┘  │
             │
             ▼
┌─────────────────────────────────────────────────────┐
│ BUILD RESPONSE DTO                                  │
│                                                     │
│ TeacherDashboardDto {                               │
│   teacherId: 1,                                     │
│   teacherName: "Ms. Smith",                         │
│   email: "smith@example.com",                       │
│   phone: "+1-987-654-3210",                         │
│   subjects: "Mathematics,Physics",                  │
│   classesToday: 5,                                  │
│   totalStudents: 28,                                │
│   earningsThisMonth: 2500.00,                       │
│   averageRating: 4.9,                               │
│   todaysSchedule: [ ScheduledClassDto[] ],          │
│   upcomingClasses: [ UpcomingClassDto[] ],          │
│   allStudents: [ StudentDto[] ]                     │
│ }                                                   │
└─────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ HTTP RESPONSE: 200 OK                               │
│ Content-Type: application/json                      │
│                                                     │
│ {TeacherDashboardDto serialized to JSON}            │
└─────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ CLIENT APPLICATION                                  │
│ ├─ Parse JSON response                              │
│ ├─ Update component state                           │
│ └─ Render UI                                        │
│    ├─ Display teacher info card with stats          │
│    ├─ Display today's schedule (mixed types)        │
│    ├─ Display upcoming classes (mixed types)        │
│    └─ Display all students list                     │
└─────────────────────────────────────────────────────┘
```

---

## 📱 Database Schema Relationships

```
┌─────────────────────────────────────────────────────────────────┐
│                        DATABASE SCHEMA                          │
└─────────────────────────────────────────────────────────────────┘

Students Table
┌────────────────────────────────────────┐
│ Id (PK)                                │
│ FullName                               │
│ Email (UNIQUE)                         │
│ PasswordHash                           │
│ Phone                                  │
│ ParentFirstName                        │
│ ParentLastName                         │
│ ClassYear                              │
│ Subject                                │
│ Address                                │
│ IsActive                               │
│ CreatedAt                              │
└────────────────────────────────────────┘
           │
           │ 1:M
           │
           ▼
┌────────────────────────────────────────┐
│ RegularClasses Table                   │
│                                        │
│ ClassId (PK)                           │
│ StudentId (FK) ──────────────┐         │
│ TeacherId (FK) ──────────┐   │         │
│ Subject                   │   │         │
│ StartTime                 │   │         │
│ EndTime                   │   │         │
│ RecurrencePattern         │   │         │
│ Status                    │   │         │
│ ClassType                 │   │         │
│ MeetingLink               │   │         │
│ Notes                     │   │         │
│ CreatedAt                 │   │         │
└────────────────────────────────────────┘
           │                 │
           │                 │ 1:M
           │                 │
           │                 ▼
           │        ┌────────────────────────────────────────┐
           │        │ Teachers Table                         │
           │        │                                        │
           │        │ TeacherId (PK)                         │
           │        │ FullName                               │
           │        │ Email (UNIQUE)                         │
           │        │ Phone                                  │
           │        │ Subjects                               │
           │        │ PasswordHash                           │
           │        │ IsActive                               │
           │        │ CreatedAt                              │
           │        └────────────────────────────────────────┘
           │                 ▲
           │                 │ 1:M
           │                 │
           │        ┌────────┴────────────────────────┐
           │        │                                 │
           │        ▼                                 ▼
           │  ┌──────────────────────┐      ┌──────────────────────┐
           │  │ DemoBookings Table   │      │ Payment Table        │
           │  │                      │      │                      │
           │  │ BookingId (PK)       │      │ PaymentId (PK)       │
           │  │ SlotId (FK)          │      │ StudentId (FK) ──────┘
           │  │ LeadId (FK)          │      │ Amount               │
           │  │ TeacherId (FK) ──────┼──→ (Teacher)              │
           │  │ Status               │      │ Date                 │
           │  │ MeetingLink          │      │ Status               │
           │  │ BookedAt             │      │ Type                 │
           │  └──────────────────────┘      │ CreatedAt            │
           │           ▲                    └──────────────────────┘
           │           │ 1:M
           │           │
           │     ┌─────┴───────────────────────┐
           │     │                             │
           │     ▼                             ▼
           │ ┌──────────────┐          ┌──────────────┐
           │ │ DemoSlots    │          │ Leads Table  │
           │ │              │          │              │
           │ │ SlotId (PK)  │          │ LeadId (PK)  │
           │ │ StartTime    │          │ StudentName  │
           │ │ EndTime      │          │ Email        │
           │ │ IsAvailable  │          │ Phone        │
           │ │ CreatedAt    │          │ Subject      │
           │ └──────────────┘          │ ClassYear    │
           │                           │ CreatedAt    │
           │                           └──────────────┘
           │
           └─────────────────────────────────────────────────

LEGEND:
━━━━   = Primary Key (PK)
──→    = Foreign Key (FK)
1:M    = One-to-Many relationship
```

---

## 🔐 Access Control Flow

```
┌─────────────────────────────────────────────────────┐
│ STUDENT DASHBOARD ACCESS CONTROL                    │
└─────────────────────────────────────────────────────┘

Request: GET /api/students/dashboard
Headers: Authorization: Bearer {jwt_token}

         │
         ▼
    ┌──────────────────────┐
    │ JWT Token Validation │
    │ ├─ Token signature   │
    │ ├─ Token expiration  │
    │ └─ Token format      │
    └──────────┬───────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Valid     ❌ Invalid
        │             │
        ▼             ▼
    Continue     401 Unauthorized
        │
        ▼
    ┌──────────────────────────┐
    │ Extract Claims           │
    │ ├─ sub: StudentId        │
    │ ├─ email                 │
    │ ├─ iat (issued at)       │
    │ └─ exp (expiration)      │
    └──────────┬───────────────┘
               │
               ▼
    ┌──────────────────────────┐
    │ Parse StudentId from 'sub'
    │ └─ Must be numeric       │
    └──────────┬───────────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Valid     ❌ Invalid
        │             │
        ▼             ▼
    Continue     401 Unauthorized
        │
        ▼
    ┌──────────────────────────┐
    │ Database Lookup          │
    │ └─ GetByIdAsync(studentId)
    └──────────┬───────────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Found    ❌ Not Found
        │             │
        ▼             ▼
    Continue     404 Not Found
        │
        ▼
    ┌──────────────────────────────┐
    │ Query Student's Classes      │
    │ ├─ WHERE StudentId = ?       │
    │ ├─ AND Status != 'Cancelled' │
    │ └─ Only sees OWN classes     │
    └──────────┬───────────────────┘
               │
               ▼
    ┌──────────────────────────┐
    │ Return 200 OK            │
    │ StudentDashboardDto      │
    └──────────────────────────┘


┌─────────────────────────────────────────────────────┐
│ TEACHER DASHBOARD ACCESS CONTROL                    │
└─────────────────────────────────────────────────────┘

Request: GET /api/teachers/dashboard
Headers: Authorization: Bearer {jwt_token}

         │
         ▼
    ┌──────────────────────┐
    │ JWT Token Validation │
    └──────────┬───────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Valid     ❌ Invalid
        │             │
        ▼             ▼
    Continue     401 Unauthorized
        │
        ▼
    ┌──────────────────────────┐
    │ Extract Claims           │
    │ ├─ sub: TeacherId        │
    │ └─ ...                   │
    └──────────┬───────────────┘
               │
               ▼
    ┌──────────────────────────┐
    │ Parse TeacherId from 'sub'
    │ └─ Must be numeric       │
    └──────────┬───────────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Valid     ❌ Invalid
        │             │
        ▼             ▼
    Continue     401 Unauthorized
        │
        ▼
    ┌──────────────────────────┐
    │ Database Lookup          │
    │ └─ GetByIdAsync(teacherId)
    └──────────┬───────────────┘
               │
        ┌──────┴──────┐
        │             │
     ✅ Found    ❌ Not Found
        │             │
        ▼             ▼
    Continue     404 Not Found
        │
        ▼
    ┌──────────────────────────────────┐
    │ Query Teacher's Classes          │
    │ ├─ Demo: WHERE TeacherId = ?     │
    │ ├─ Regular: WHERE TeacherId = ?  │
    │ ├─ Combine both types            │
    │ └─ Only sees OWN classes         │
    └──────────┬───────────────────────┘
               │
               ▼
    ┌──────────────────────────┐
    │ Return 200 OK            │
    │ TeacherDashboardDto      │
    │ (Demo + Regular combined)│
    └──────────────────────────┘
```

---

## 📊 API Response Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                 API RESPONSE BUILDING                       │
└─────────────────────────────────────────────────────────────┘

STUDENT DASHBOARD RESPONSE:

┌─────────────────────────────┐
│ StudentDashboardDto         │
├─────────────────────────────┤
│ ┌──────────────┐            │
│ │ Student Info │            │
│ │ ├─ Id        │            │
│ │ ├─ Name      │            │
│ │ ├─ Email     │            │
│ │ ├─ Phone     │            │
│ │ ├─ ClassYear │            │
│ │ ├─ Subject   │            │
│ │ └─ Stats     │            │
│ └──────────────┘            │
│ ┌──────────────┐            │
│ │ TodaysClasses │ (max: all)│
│ │ ├─ ClassId   │            │
│ │ ├─ Teacher   │            │
│ │ ├─ Subject   │            │
│ │ ├─ Times     │            │
│ │ ├─ Link      │            │
│ │ └─ Status    │            │
│ └──────────────┘            │
│ ┌──────────────────┐        │
│ │ UpcomingClassesList       │ (max: 20)
│ │ └─ [StudentClassDto]      │
│ └──────────────────┘        │
│ ┌──────────────────┐        │
│ │ PastClasses      │ (max: 20)
│ │ └─ [StudentClassDto]      │
│ └──────────────────┘        │
└─────────────────────────────┘


TEACHER DASHBOARD RESPONSE:

┌─────────────────────────────┐
│ TeacherDashboardDto         │
├─────────────────────────────┤
│ ┌──────────────┐            │
│ │ Teacher Info │            │
│ │ ├─ Id        │            │
│ │ ├─ Name      │            │
│ │ ├─ Email     │            │
│ │ ├─ Phone     │            │
│ │ ├─ Subjects  │            │
│ │ └─ Stats     │            │
│ └──────────────┘            │
│ ┌──────────────────┐        │
│ │ TodaysSchedule │          │
│ │ ├─ Demo Classes  │        │
│ │ └─ Regular Classes         │
│ │    (Combined & Sorted)     │
│ │    └─ [ScheduledClassDto]  │
│ └──────────────────┘        │
│ ┌──────────────────┐        │
│ │ UpcomingClasses  │ (max:20)
│ │ ├─ Demo Classes  │        │
│ │ └─ Regular Classes         │
│ │    (Combined & Sorted)     │
│ │    └─ [UpcomingClassDto]   │
│ └──────────────────┘        │
│ ┌──────────────────┐        │
│ │ AllStudents      │        │
│ │ ├─ From Demos    │        │
│ │ ├─ From Regular  │        │
│ │ ├─ Deduplicated  │        │
│ │ └─ [StudentDto]  │        │
│ └──────────────────┘        │
└─────────────────────────────┘
```

---

## 🎯 Error Handling Flow

```
┌──────────────────────────────────────────────────────┐
│          ERROR HANDLING FLOW                         │
└──────────────────────────────────────────────────────┘

API Request
│
├─ No Token?
│  └─→ 401 Unauthorized
│      └─ "Invalid student/teacher token."
│
├─ Invalid Token Format?
│  └─→ 401 Unauthorized
│      └─ "Invalid token signature"
│
├─ Token Expired?
│  └─→ 401 Unauthorized
│      └─ "Token has expired"
│
├─ Missing 'sub' Claim?
│  └─→ 401 Unauthorized
│      └─ "Invalid student/teacher token."
│
├─ 'sub' Not Numeric?
│  └─→ 401 Unauthorized
│      └─ "Invalid student/teacher token."
│
├─ Student/Teacher Not Found?
│  └─→ 404 Not Found
│      └─ "Student/Teacher not found."
│
├─ Database Error?
│  └─→ 500 Internal Server Error
│      └─ "An error occurred" (production)
│      └─ Full exception (development)
│
└─ Success!
   └─→ 200 OK
       └─ Response DTO as JSON
```

---

**Diagrams Created:** January 2024  
**Version:** 1.0  
**Status:** Complete and Production Ready
