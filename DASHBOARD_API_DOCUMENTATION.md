# SkillBridge Tutors API - Dashboard Documentation

## Overview

This document provides comprehensive details about the Student and Teacher Dashboard endpoints, DTOs, and implementation for the SkillBridge Tutors API. It covers all necessary information for UI implementation.

---

## Table of Contents

1. [Student Dashboard](#student-dashboard)
2. [Teacher Dashboard](#teacher-dashboard)
3. [Data Models](#data-models)
4. [DTOs](#dtos)
5. [API Endpoints](#api-endpoints)
6. [Authentication](#authentication)
7. [Usage Examples](#usage-examples)
8. [Feature Summary](#feature-summary)

---

## Student Dashboard

### Overview

The Student Dashboard allows authenticated students to view their scheduled regular classes. Students can only see their own classes and cannot access other students' information.

### Key Features

- ✅ View today's scheduled classes
- ✅ View upcoming classes (next 20 classes)
- ✅ View past/completed classes
- ✅ View class details including teacher info, subject, time, and meeting link
- ✅ Filter classes by date and status
- ✅ Access to student profile information
- ✅ Total class count and completion statistics

### Access Control

- Only authenticated students can access their own dashboard
- JWT token required with `sub` claim containing student ID
- Students can only view their own data (StudentId must match JWT claim)

### Data Flow

```
Student Login → JWT Token Generated → Dashboard Access → View Regular Classes Only
```

---

## Teacher Dashboard

### Overview

The Teacher Dashboard provides teachers with a unified view of all their classes including both demo classes (from leads) and regular classes (from paid student enrollments).

### Key Features

- ✅ View both demo and regular classes in unified view
- ✅ View today's scheduled classes (combined demo + regular)
- ✅ View upcoming classes (combined demo + regular, limited to 20)
- ✅ View all unique students (from both demo and regular classes)
- ✅ Earnings tracking (placeholder for future implementation)
- ✅ Average rating display (placeholder for future implementation)
- ✅ Class type distinction (Demo vs Regular)

### Access Control

- Only authenticated teachers can access their own dashboard
- JWT token required with `sub` claim containing teacher ID
- Teachers only see their own classes and students

### Data Flow

```
Teacher Login → JWT Token Generated → Dashboard Access → View Demo + Regular Classes
```

---

## Data Models

### Student Model

```csharp
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public string? ParentFirstName { get; set; }
    public string? ParentLastName { get; set; }
    public string? ClassYear { get; set; }
    public string? Subject { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public ICollection<Payment> Payments { get; set; }
    public ICollection<RegularClass> RegularClasses { get; set; } // ✅ NEW
}
```

### Teacher Model

```csharp
public class Teacher
{
    public long TeacherId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Subjects { get; set; } // Comma-separated
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public ICollection<DemoBooking> DemoBookings { get; set; }
    public ICollection<RegularClass> RegularClasses { get; set; } // ✅ NEW
}
```

### RegularClass Model

```csharp
public class RegularClass
{
    public long ClassId { get; set; }
    public int StudentId { get; set; }
    public long TeacherId { get; set; }
    public string Subject { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? RecurrencePattern { get; set; } // Weekly, BiWeekly, Monthly, OneTime
    public string Status { get; set; } // Scheduled, Completed, Cancelled, Rescheduled
    public string ClassType { get; set; } // Regular
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
```

---

## DTOs

### Student Dashboard DTOs

#### StudentDashboardDto
Main DTO returned from `/api/students/dashboard` endpoint.

```csharp
{
    "studentId": 1,
    "studentName": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "classYear": "10th Grade",
    "subject": "Mathematics",
    "totalClasses": 24,
    "completedClasses": 12,
    "upcomingClasses": 6,
    "todaysClasses": [
        // Array of StudentClassDto
    ],
    "upcomingClassesList": [
        // Array of StudentClassDto
    ],
    "pastClasses": [
        // Array of StudentClassDto
    ]
}
```

#### StudentClassDto
Represents individual class information.

```csharp
{
    "classId": 101,
    "teacherName": "Ms. Smith",
    "subject": "Mathematics",
    "startTime": "2024-01-15T10:00:00Z",
    "endTime": "2024-01-15T11:00:00Z",
    "meetingLink": "https://meet.google.com/abc-def-ghi",
    "status": "Scheduled", // Scheduled, Completed, Cancelled, Rescheduled
    "recurrencePattern": "Weekly" // Weekly, BiWeekly, Monthly, OneTime
}
```

### Teacher Dashboard DTOs

#### TeacherDashboardDto
Main DTO returned from `/api/teachers/dashboard` endpoint.

```csharp
{
    "teacherId": 1,
    "teacherName": "Ms. Smith",
    "email": "smith@example.com",
    "phone": "+1-987-654-3210",
    "subjects": "Mathematics,Physics",
    "classesToday": 5,
    "totalStudents": 28,
    "earningsThisMonth": 2500.00,
    "averageRating": 4.9,
    "todaysSchedule": [
        // Array of ScheduledClassDto (combined Demo + Regular)
    ],
    "upcomingClasses": [
        // Array of UpcomingClassDto (combined Demo + Regular, max 20)
    ],
    "allStudents": [
        // Array of StudentDto
    ]
}
```

#### ScheduledClassDto
Represents a scheduled class in today's schedule.

```csharp
{
    "bookingId": 501,        // For demo classes
    "classId": 101,          // For regular classes
    "studentName": "John Doe",
    "studentEmail": "john@example.com",
    "studentPhone": "+1-234-567-8900",
    "subject": "Mathematics",
    "classType": "Regular",  // Demo or Regular
    "startTime": "2024-01-15T10:00:00Z",
    "endTime": "2024-01-15T11:00:00Z",
    "meetingLink": "https://meet.google.com/abc-def-ghi",
    "status": "Scheduled"
}
```

#### UpcomingClassDto
Represents upcoming class in the future schedule.

```csharp
{
    "bookingId": 502,        // For demo classes
    "classId": 102,          // For regular classes
    "studentName": "Jane Smith",
    "subject": "Physics",
    "classType": "Regular",  // Demo or Regular
    "startTime": "2024-01-20T14:00:00Z",
    "endTime": "2024-01-20T15:00:00Z",
    "status": "Scheduled"
}
```

#### StudentDto (in Teacher Dashboard)
Represents a student in the teacher's class list.

```csharp
{
    "leadId": 1001,          // For demo students
    "studentId": 1,          // For regular class students
    "studentName": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "subject": "Mathematics",
    "classYear": "10th Grade",
    "totalClasses": 8,
    "firstBookedAt": "2024-01-01T09:00:00Z"
}
```

---

## API Endpoints

### Student Endpoints

#### 1. Get Student Dashboard
**Endpoint:** `GET /api/students/dashboard`

**Authentication:** Required (JWT Bearer Token)

**Authorization:** Student ID from JWT must match the student's own ID

**Response:** `200 OK`

**Response Body:**
```json
{
    "studentId": 1,
    "studentName": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "classYear": "10th Grade",
    "subject": "Mathematics",
    "totalClasses": 24,
    "completedClasses": 12,
    "upcomingClasses": 6,
    "todaysClasses": [
        {
            "classId": 101,
            "teacherName": "Ms. Smith",
            "subject": "Mathematics",
            "startTime": "2024-01-15T10:00:00Z",
            "endTime": "2024-01-15T11:00:00Z",
            "meetingLink": "https://meet.google.com/abc-def-ghi",
            "status": "Scheduled",
            "recurrencePattern": "Weekly"
        }
    ],
    "upcomingClassesList": [],
    "pastClasses": []
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing JWT token
- `404 Not Found` - Student not found in database

---

#### 2. Get Class Details
**Endpoint:** `GET /api/students/classes/{classId}`

**Authentication:** Required (JWT Bearer Token)

**Authorization:** Student must be enrolled in the class

**Parameters:**
- `classId` (path parameter, long) - The class ID

**Response:** `200 OK`

**Response Body:**
```json
{
    "classId": 101,
    "teacherName": "Ms. Smith",
    "subject": "Mathematics",
    "startTime": "2024-01-15T10:00:00Z",
    "endTime": "2024-01-15T11:00:00Z",
    "meetingLink": "https://meet.google.com/abc-def-ghi",
    "status": "Scheduled",
    "recurrencePattern": "Weekly"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing JWT token
- `404 Not Found` - Class not found or student not enrolled

---

#### 3. Get Student Profile
**Endpoint:** `GET /api/students/profile`

**Authentication:** Required (JWT Bearer Token)

**Response:** `200 OK`

**Response Body:**
```json
{
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "classYear": "10th Grade",
    "subject": "Mathematics",
    "address": "123 Main St, City",
    "isActive": true,
    "createdAt": "2024-01-01T08:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing JWT token
- `404 Not Found` - Student not found

---

### Teacher Endpoints

#### 1. Get Teacher Dashboard
**Endpoint:** `GET /api/teachers/dashboard`

**Authentication:** Required (JWT Bearer Token)

**Authorization:** Teacher ID from JWT must match the teacher's own ID

**Response:** `200 OK`

**Response Body:**
```json
{
    "teacherId": 1,
    "teacherName": "Ms. Smith",
    "email": "smith@example.com",
    "phone": "+1-987-654-3210",
    "subjects": "Mathematics,Physics",
    "classesToday": 5,
    "totalStudents": 28,
    "earningsThisMonth": 2500.00,
    "averageRating": 4.9,
    "todaysSchedule": [
        {
            "bookingId": 501,
            "classId": 101,
            "studentName": "John Doe",
            "studentEmail": "john@example.com",
            "studentPhone": "+1-234-567-8900",
            "subject": "Mathematics",
            "classType": "Regular",
            "startTime": "2024-01-15T10:00:00Z",
            "endTime": "2024-01-15T11:00:00Z",
            "meetingLink": "https://meet.google.com/abc-def-ghi",
            "status": "Scheduled"
        },
        {
            "bookingId": 502,
            "classId": null,
            "studentName": "Jane Smith",
            "studentEmail": "jane@example.com",
            "studentPhone": "+1-345-678-9012",
            "subject": "Physics",
            "classType": "Demo",
            "startTime": "2024-01-15T12:00:00Z",
            "endTime": "2024-01-15T12:30:00Z",
            "meetingLink": "https://meet.google.com/xyz-uvw-rst",
            "status": "Booked"
        }
    ],
    "upcomingClasses": [
        {
            "bookingId": 503,
            "classId": 102,
            "studentName": "John Doe",
            "subject": "Mathematics",
            "classType": "Regular",
            "startTime": "2024-01-20T10:00:00Z",
            "endTime": "2024-01-20T11:00:00Z",
            "status": "Scheduled"
        }
    ],
    "allStudents": [
        {
            "leadId": 1001,
            "studentId": 1,
            "studentName": "John Doe",
            "email": "john@example.com",
            "phone": "+1-234-567-8900",
            "subject": "Mathematics",
            "classYear": "10th Grade",
            "totalClasses": 8,
            "firstBookedAt": "2024-01-01T09:00:00Z"
        },
        {
            "leadId": 1002,
            "studentId": null,
            "studentName": "Jane Smith",
            "email": "jane@example.com",
            "phone": "+1-345-678-9012",
            "subject": "Physics",
            "classYear": "11th Grade",
            "totalClasses": 3,
            "firstBookedAt": "2024-01-10T14:00:00Z"
        }
    ]
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing JWT token
- `404 Not Found` - Teacher not found

---

## Authentication

### JWT Token Format

All dashboard endpoints require authentication with a JWT Bearer token.

**Token Header:**
```
Authorization: Bearer <JWT_TOKEN>
```

### JWT Claims

**For Students:**
```json
{
    "sub": "1",              // Student ID (must be numeric)
    "email": "john@example.com",
    "iat": 1234567890,
    "exp": 1234571490
}
```

**For Teachers:**
```json
{
    "sub": "1",              // Teacher ID (must be numeric)
    "email": "smith@example.com",
    "iat": 1234567890,
    "exp": 1234571490
}
```

### Token Generation

1. **Student Login Endpoint:** `POST /api/student-auth/login`
   - Returns JWT token with student ID in `sub` claim

2. **Teacher Login Endpoint:** `POST /api/teacher-auth/login`
   - Returns JWT token with teacher ID in `sub` claim

---

## Usage Examples

### Frontend Implementation Examples

#### Student Dashboard - React Example

```typescript
// Fetch student dashboard
const fetchStudentDashboard = async (token: string) => {
    const response = await fetch('/api/students/dashboard', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        throw new Error('Failed to fetch dashboard');
    }

    return await response.json();
};

// Usage
const dashboard = await fetchStudentDashboard(jwtToken);
console.log(dashboard.todaysClasses);     // Today's classes
console.log(dashboard.upcomingClassesList); // Future classes
console.log(dashboard.pastClasses);        // Completed classes
```

#### Teacher Dashboard - React Example

```typescript
// Fetch teacher dashboard
const fetchTeacherDashboard = async (token: string) => {
    const response = await fetch('/api/teachers/dashboard', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        throw new Error('Failed to fetch dashboard');
    }

    return await response.json();
};

// Usage
const dashboard = await fetchTeacherDashboard(jwtToken);

// Separate classes by type
const demoClasses = dashboard.todaysSchedule.filter(c => c.classType === 'Demo');
const regularClasses = dashboard.todaysSchedule.filter(c => c.classType === 'Regular');

console.log(`Today: ${dashboard.classesToday} classes`);
console.log(`Students: ${dashboard.totalStudents}`);
console.log(`Demo Classes: ${demoClasses.length}`);
console.log(`Regular Classes: ${regularClasses.length}`);
```

### cURL Examples

#### Student Dashboard
```bash
curl -X GET http://localhost:5000/api/students/dashboard \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

#### Teacher Dashboard
```bash
curl -X GET http://localhost:5000/api/teachers/dashboard \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

#### Get Specific Class Details
```bash
curl -X GET http://localhost:5000/api/students/classes/101 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

---

## Feature Summary

### ✅ Implemented Features

#### Student Dashboard
| Feature | Status | Details |
|---------|--------|---------|
| View today's classes | ✅ | Filter by current date |
| View upcoming classes | ✅ | Limited to next 20 classes |
| View past classes | ✅ | Limited to last 20 classes |
| See teacher info | ✅ | Teacher name included |
| Meeting links | ✅ | Direct access to meeting URL |
| Class statistics | ✅ | Total, completed, upcoming count |
| Student profile | ✅ | Personal info endpoint |
| Only own data | ✅ | Authorization check in place |

#### Teacher Dashboard
| Feature | Status | Details |
|---------|--------|---------|
| View demo classes | ✅ | From DemoBookings table |
| View regular classes | ✅ | From RegularClasses table |
| Combined view | ✅ | Demo + Regular in same response |
| Today's schedule | ✅ | Combined and sorted by time |
| Upcoming schedule | ✅ | Limited to 20, combined types |
| Student list | ✅ | From both demo and regular |
| Class type filter | ✅ | Indicated in response (Demo/Regular) |
| Earnings tracking | ⏳ | Placeholder (TODO) |
| Rating system | ⏳ | Placeholder (TODO) |

### 📋 TODO - Future Enhancements

1. **Earnings Calculation** - Calculate teacher earnings from payments table
2. **Rating System** - Implement student ratings for teachers
3. **Class Attendance** - Track and display attendance
4. **Class Notes** - Add class notes and resources
5. **Notifications** - Upcoming class reminders
6. **Class History Export** - Export class records
7. **Advanced Filters** - Filter by subject, student, etc.
8. **Real-time Updates** - WebSocket updates for live dashboard

---

## Database Relationships

### Entity Relationships

```
Student 1 -- M RegularClass
  ↓
  └─→ Teacher

Teacher 1 -- M RegularClass
  ↓
  └─→ Student

Teacher 1 -- M DemoBooking
  ↓
  └─→ Lead

Student 1 -- M Payment
```

### Key Relationships Configuration

```csharp
// In ApplicationDbContext.OnModelCreating

modelBuilder.Entity<RegularClass>()
    .HasOne(c => c.Student)
    .WithMany(s => s.RegularClasses)
    .HasForeignKey(c => c.StudentId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<RegularClass>()
    .HasOne(c => c.Teacher)
    .WithMany(t => t.RegularClasses)
    .HasForeignKey(c => c.TeacherId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

## Error Handling

### Common Error Responses

#### 401 Unauthorized
**Scenario:** Missing or invalid JWT token

```json
{
    "message": "Invalid student token."
}
```

#### 404 Not Found
**Scenario:** Student/Teacher/Class not found

```json
{
    "message": "Student not found."
}
```

#### 403 Forbidden
**Scenario:** Accessing another user's data

- Student cannot access another student's classes
- Teacher cannot access another teacher's dashboard

---

## Performance Considerations

### Pagination
- **Today's Classes:** No limit (all classes)
- **Upcoming Classes:** Limited to 20 classes
- **Past Classes:** Limited to 20 classes

### Database Queries
- Use `.Include()` to eagerly load related entities
- Prevent N+1 query problems
- Filter at database level, not in memory

### Caching (Future)
- Consider caching dashboard data (5-10 minute TTL)
- Invalidate cache on class changes
- Use Redis for distributed caching

---

## Security Considerations

### Access Control
- ✅ JWT token validation on all endpoints
- ✅ Student can only view own classes
- ✅ Teacher can only view own classes and students
- ✅ Student ID/Teacher ID extracted from JWT `sub` claim

### Data Validation
- ✅ Student/Teacher ID must be numeric
- ✅ JWT must contain valid `sub` claim
- ✅ Token signature verified

### Recommendations
- Use HTTPS in production
- Implement rate limiting on dashboard endpoints
- Add request logging for audit trail
- Rotate JWT secrets regularly
- Set appropriate JWT expiration times (1-8 hours)

---

## Testing

### Unit Test Scenarios

#### Student Dashboard Tests
- [ ] Student can view own dashboard
- [ ] Student cannot view another student's dashboard
- [ ] Classes correctly sorted by date
- [ ] Today's classes filtered correctly
- [ ] Upcoming classes limited to 20
- [ ] Past classes limited to 20
- [ ] Teacher names display correctly

#### Teacher Dashboard Tests
- [ ] Teacher can view own dashboard
- [ ] Demo and regular classes combined correctly
- [ ] Classes sorted by start time
- [ ] Class type (Demo/Regular) marked correctly
- [ ] Student list includes both types
- [ ] Upcoming classes limited to 20
- [ ] Total counts calculated correctly

### Integration Test Scenarios
- [ ] JWT token validation works
- [ ] Database queries return correct data
- [ ] Navigation properties are populated
- [ ] Sorting and filtering work as expected
- [ ] Error responses return correct status codes

---

## API Versioning

**Current Version:** v1

**Endpoints:** 
- `GET /api/v1/students/dashboard`
- `GET /api/v1/teachers/dashboard`

(Currently using v1 as default route)

---

## Support & Maintenance

### Logging
- Log all dashboard requests (for monitoring)
- Log errors with stack traces
- Monitor performance metrics

### Monitoring
- Track dashboard load times
- Monitor database query performance
- Alert on high error rates

### Debugging
- Enable detailed error messages in development
- Use Entity Framework logging for query inspection
- Implement request/response logging middleware

---

## Appendix: Complete API Request/Response Examples

### Example 1: Student Dashboard with All Data

**Request:**
```http
GET /api/students/dashboard HTTP/1.1
Host: api.skillbridgetutors.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Response:**
```json
{
    "studentId": 1,
    "studentName": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "classYear": "10th Grade",
    "subject": "Mathematics",
    "totalClasses": 24,
    "completedClasses": 12,
    "upcomingClasses": 6,
    "todaysClasses": [
        {
            "classId": 101,
            "teacherName": "Ms. Smith",
            "subject": "Mathematics",
            "startTime": "2024-01-15T10:00:00Z",
            "endTime": "2024-01-15T11:00:00Z",
            "meetingLink": "https://meet.google.com/abc-def-ghi",
            "status": "Scheduled",
            "recurrencePattern": "Weekly"
        },
        {
            "classId": 102,
            "teacherName": "Ms. Johnson",
            "subject": "Mathematics",
            "startTime": "2024-01-15T15:30:00Z",
            "endTime": "2024-01-15T16:30:00Z",
            "meetingLink": "https://meet.google.com/jkl-mno-pqr",
            "status": "Scheduled",
            "recurrencePattern": "BiWeekly"
        }
    ],
    "upcomingClassesList": [
        {
            "classId": 103,
            "teacherName": "Mr. Wilson",
            "subject": "Mathematics",
            "startTime": "2024-01-16T10:00:00Z",
            "endTime": "2024-01-16T11:00:00Z",
            "meetingLink": "https://meet.google.com/stu-vwx-yz",
            "status": "Scheduled",
            "recurrencePattern": "Weekly"
        }
    ],
    "pastClasses": [
        {
            "classId": 99,
            "teacherName": "Ms. Smith",
            "subject": "Mathematics",
            "startTime": "2024-01-10T10:00:00Z",
            "endTime": "2024-01-10T11:00:00Z",
            "meetingLink": "https://meet.google.com/old-class",
            "status": "Completed",
            "recurrencePattern": "Weekly"
        }
    ]
}
```

---

**Document Version:** 1.0  
**Last Updated:** January 2024  
**Status:** Complete Implementation
