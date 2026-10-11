# Dashboard UI Implementation Guide

## Quick Reference for Frontend Developers

This guide provides step-by-step instructions for implementing the Student and Teacher dashboards in your frontend application.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Student Dashboard UI](#student-dashboard-ui)
3. [Teacher Dashboard UI](#teacher-dashboard-ui)
4. [Component Examples](#component-examples)
5. [State Management](#state-management)
6. [API Integration](#api-integration)
7. [UI Components Checklist](#ui-components-checklist)

---

## Quick Start

### Prerequisites
- JWT token stored in localStorage/sessionStorage
- API base URL configured
- HTTP client library (fetch, axios, etc.)

### Basic Setup

```typescript
// config.ts
export const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

export const getAuthHeaders = (token: string) => ({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
});
```

---

## Student Dashboard UI

### Layout Structure

```
┌─────────────────────────────────────────────────┐
│  Student Dashboard                              │
├─────────────────────────────────────────────────┤
│                                                  │
│  Student Info Card                              │
│  ├─ Name, Email, Phone                          │
│  ├─ Class Year, Subject                         │
│  └─ Stats: Total | Completed | Upcoming        │
│                                                  │
│  Tabs: [ Today ] [ Upcoming ] [ Past ]          │
│                                                  │
│  Classes List                                   │
│  ├─ Class Card 1                                │
│  │  ├─ Teacher: Ms. Smith                       │
│  │  ├─ Subject: Mathematics                     │
│  │  ├─ Time: 10:00 AM - 11:00 AM                │
│  │  ├─ Status: Scheduled                        │
│  │  └─ [Join Meeting] Button                    │
│  │                                              │
│  ├─ Class Card 2                                │
│  │  └─ ...                                      │
│  │                                              │
│  └─ Empty State (if no classes)                 │
│                                                  │
└─────────────────────────────────────────────────┘
```

### Key Information to Display

#### Student Info Section
```
Name: John Doe
Email: john@example.com
Phone: +1-234-567-8900
Grade/Year: 10th Grade
Subject: Mathematics

Statistics:
- Total Classes: 24
- Completed: 12
- Upcoming: 6
```

#### Class Card Components

**For Today's Classes:**
```
┌────────────────────────────────────┐
│ Teacher: Ms. Smith                 │
│ Subject: Mathematics               │
│ Time: 10:00 AM - 11:00 AM          │
│ Status: [Scheduled]                │
│ Recurring: Weekly                  │
│ [Join Meeting]                     │
└────────────────────────────────────┘
```

**For Upcoming Classes:**
```
┌────────────────────────────────────┐
│ Teacher: Mr. Wilson                │
│ Subject: Physics                   │
│ Date: Monday, Jan 20               │
│ Time: 2:00 PM - 3:00 PM            │
│ Status: [Scheduled]                │
│ Recurring: BiWeekly                │
└────────────────────────────────────┘
```

**For Past Classes:**
```
┌────────────────────────────────────┐
│ Teacher: Ms. Johnson               │
│ Subject: Chemistry                 │
│ Date: Friday, Jan 10               │
│ Time: 3:30 PM - 4:30 PM            │
│ Status: [Completed] ✓              │
└────────────────────────────────────┘
```

### React Component Example

```typescript
// StudentDashboard.tsx
import React, { useEffect, useState } from 'react';
import { StudentDashboardDto, StudentClassDto } from '../types';
import { fetchStudentDashboard } from '../api/studentApi';

const StudentDashboard: React.FC = () => {
    const [dashboard, setDashboard] = useState<StudentDashboardDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'today' | 'upcoming' | 'past'>('today');

    useEffect(() => {
        loadDashboard();
    }, []);

    const loadDashboard = async () => {
        try {
            const token = localStorage.getItem('studentToken');
            if (!token) throw new Error('No authentication token');

            const data = await fetchStudentDashboard(token);
            setDashboard(data);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load dashboard');
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!dashboard) return <div>No data</div>;

    const classes = {
        today: dashboard.todaysClasses,
        upcoming: dashboard.upcomingClassesList,
        past: dashboard.pastClasses
    };

    const currentClasses = classes[activeTab];

    return (
        <div className="student-dashboard">
            {/* Student Info Section */}
            <div className="student-info-card">
                <h1>{dashboard.studentName}</h1>
                <div className="info-row">
                    <span>Email: {dashboard.email}</span>
                    <span>Phone: {dashboard.phone}</span>
                </div>
                <div className="info-row">
                    <span>Grade: {dashboard.classYear}</span>
                    <span>Subject: {dashboard.subject}</span>
                </div>

                {/* Statistics */}
                <div className="statistics">
                    <div className="stat">
                        <div className="stat-value">{dashboard.totalClasses}</div>
                        <div className="stat-label">Total Classes</div>
                    </div>
                    <div className="stat">
                        <div className="stat-value">{dashboard.completedClasses}</div>
                        <div className="stat-label">Completed</div>
                    </div>
                    <div className="stat">
                        <div className="stat-value">{dashboard.upcomingClasses}</div>
                        <div className="stat-label">Upcoming</div>
                    </div>
                </div>
            </div>

            {/* Tabs */}
            <div className="tabs">
                <button
                    className={`tab ${activeTab === 'today' ? 'active' : ''}`}
                    onClick={() => setActiveTab('today')}
                >
                    Today ({dashboard.todaysClasses.length})
                </button>
                <button
                    className={`tab ${activeTab === 'upcoming' ? 'active' : ''}`}
                    onClick={() => setActiveTab('upcoming')}
                >
                    Upcoming ({dashboard.upcomingClassesList.length})
                </button>
                <button
                    className={`tab ${activeTab === 'past' ? 'active' : ''}`}
                    onClick={() => setActiveTab('past')}
                >
                    Past ({dashboard.pastClasses.length})
                </button>
            </div>

            {/* Classes List */}
            <div className="classes-container">
                {currentClasses.length > 0 ? (
                    currentClasses.map(classItem => (
                        <ClassCard key={classItem.classId} classItem={classItem} />
                    ))
                ) : (
                    <div className="empty-state">
                        No classes in this category
                    </div>
                )}
            </div>
        </div>
    );
};

// ClassCard.tsx
interface ClassCardProps {
    classItem: StudentClassDto;
}

const ClassCard: React.FC<ClassCardProps> = ({ classItem }) => {
    const formatTime = (date: string) => {
        return new Date(date).toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const formatDate = (date: string) => {
        return new Date(date).toLocaleDateString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric'
        });
    };

    return (
        <div className="class-card">
            <div className="class-header">
                <h3>{classItem.teacherName}</h3>
                <span className={`status ${classItem.status.toLowerCase()}`}>
                    {classItem.status}
                </span>
            </div>

            <div className="class-details">
                <p><strong>Subject:</strong> {classItem.subject}</p>
                <p>
                    <strong>Date & Time:</strong>
                    {' '}
                    {formatDate(classItem.startTime)}
                    {' '}
                    {formatTime(classItem.startTime)} - {formatTime(classItem.endTime)}
                </p>
                {classItem.recurrencePattern && (
                    <p><strong>Recurring:</strong> {classItem.recurrencePattern}</p>
                )}
            </div>

            {classItem.meetingLink && (
                <a
                    href={classItem.meetingLink}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn-join"
                >
                    Join Meeting
                </a>
            )}
        </div>
    );
};

export default StudentDashboard;
```

---

## Teacher Dashboard UI

### Layout Structure

```
┌──────────────────────────────────────────────────┐
│  Teacher Dashboard                               │
├──────────────────────────────────────────────────┤
│                                                   │
│  Teacher Info & Stats                            │
│  ├─ Name, Email, Phone                           │
│  ├─ Subjects                                     │
│  └─ Stats: Today(5) | Students(28) | Earnings   │
│                                                   │
│  Today's Schedule                                │
│  ├─ Class 1 (10:00 AM) - Regular                 │
│  ├─ Class 2 (11:30 AM) - Demo                    │
│  └─ Class 3 (2:00 PM) - Regular                  │
│                                                   │
│  Upcoming Classes (Next 20)                      │
│  ├─ Class 1 - Tomorrow (Regular)                 │
│  ├─ Class 2 - Wed (Demo)                         │
│  └─ ...                                          │
│                                                   │
│  All Students                                    │
│  ├─ Student 1 (8 classes total)                  │
│  ├─ Student 2 (5 classes total)                  │
│  └─ ...                                          │
│                                                   │
└──────────────────────────────────────────────────┘
```

### Key Information to Display

#### Teacher Info Section
```
Name: Ms. Smith
Email: smith@example.com
Phone: +1-987-654-3210
Subjects: Mathematics, Physics

Statistics:
- Classes Today: 5
- Total Students: 28
- Earnings This Month: $2,500.00
- Average Rating: 4.9/5.0
```

#### Class Cards in Teacher Dashboard

**Today's Schedule Card:**
```
┌──────────────────────────────────────┐
│ 10:00 AM - 11:00 AM                  │
│                                      │
│ [Regular]                            │
│ Student: John Doe                    │
│ Subject: Mathematics                 │
│ Meeting: [Join] [Copy Link]          │
│ Status: Scheduled ✓                  │
│                                      │
│ Email: john@example.com              │
│ Phone: +1-234-567-8900               │
└──────────────────────────────────────┘
```

**Upcoming Classes Card:**
```
┌──────────────────────────────────────┐
│ Tomorrow, 2:00 PM - 3:00 PM          │
│                                      │
│ [Demo]                               │
│ Student: Jane Smith                  │
│ Subject: Physics                     │
│ Status: Booked ✓                     │
└──────────────────────────────────────┘
```

**Student List Card:**
```
┌──────────────────────────────────────┐
│ John Doe                             │
│ Subject: Mathematics                 │
│ Classes: 8 total                     │
│ Started: Jan 1, 2024                 │
│ Email: john@example.com              │
│ Phone: +1-234-567-8900               │
└──────────────────────────────────────┘
```

### React Component Example

```typescript
// TeacherDashboard.tsx
import React, { useEffect, useState } from 'react';
import { TeacherDashboardDto } from '../types';
import { fetchTeacherDashboard } from '../api/teacherApi';

const TeacherDashboard: React.FC = () => {
    const [dashboard, setDashboard] = useState<TeacherDashboardDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadDashboard();
    }, []);

    const loadDashboard = async () => {
        try {
            const token = localStorage.getItem('teacherToken');
            if (!token) throw new Error('No authentication token');

            const data = await fetchTeacherDashboard(token);
            setDashboard(data);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load dashboard');
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!dashboard) return <div>No data</div>;

    return (
        <div className="teacher-dashboard">
            {/* Teacher Info Section */}
            <div className="teacher-info-card">
                <div className="header">
                    <div>
                        <h1>{dashboard.teacherName}</h1>
                        <p>{dashboard.subjects}</p>
                    </div>
                    <div className="contact">
                        <p>{dashboard.email}</p>
                        <p>{dashboard.phone}</p>
                    </div>
                </div>

                {/* Statistics */}
                <div className="statistics">
                    <div className="stat">
                        <div className="stat-value">{dashboard.classesToday}</div>
                        <div className="stat-label">Classes Today</div>
                    </div>
                    <div className="stat">
                        <div className="stat-value">{dashboard.totalStudents}</div>
                        <div className="stat-label">Total Students</div>
                    </div>
                    <div className="stat">
                        <div className="stat-value">${dashboard.earningsThisMonth}</div>
                        <div className="stat-label">Earnings This Month</div>
                    </div>
                    <div className="stat">
                        <div className="stat-value">{dashboard.averageRating}★</div>
                        <div className="stat-label">Average Rating</div>
                    </div>
                </div>
            </div>

            {/* Today's Schedule */}
            <section className="section today-schedule">
                <h2>Today's Schedule ({dashboard.todaysSchedule.length} classes)</h2>
                <div className="classes-list">
                    {dashboard.todaysSchedule.length > 0 ? (
                        dashboard.todaysSchedule.map((classItem, idx) => (
                            <TodayClassCard key={idx} classItem={classItem} />
                        ))
                    ) : (
                        <p className="empty">No classes today</p>
                    )}
                </div>
            </section>

            {/* Upcoming Classes */}
            <section className="section upcoming-classes">
                <h2>Upcoming Classes ({dashboard.upcomingClasses.length})</h2>
                <div className="classes-list">
                    {dashboard.upcomingClasses.length > 0 ? (
                        dashboard.upcomingClasses.map((classItem, idx) => (
                            <UpcomingClassCard key={idx} classItem={classItem} />
                        ))
                    ) : (
                        <p className="empty">No upcoming classes</p>
                    )}
                </div>
            </section>

            {/* All Students */}
            <section className="section students-list">
                <h2>All Students ({dashboard.allStudents.length})</h2>
                <div className="students">
                    {dashboard.allStudents.length > 0 ? (
                        dashboard.allStudents.map((student, idx) => (
                            <StudentCard key={idx} student={student} />
                        ))
                    ) : (
                        <p className="empty">No students</p>
                    )}
                </div>
            </section>
        </div>
    );
};

// TodayClassCard.tsx
const TodayClassCard: React.FC<{ classItem: ScheduledClassDto }> = ({ classItem }) => {
    const formatTime = (date: string) => {
        return new Date(date).toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const copyLink = (link: string) => {
        navigator.clipboard.writeText(link);
        alert('Link copied to clipboard');
    };

    return (
        <div className={`class-card ${classItem.classType.toLowerCase()}`}>
            <div className="time">
                {formatTime(classItem.startTime)} - {formatTime(classItem.endTime)}
            </div>

            <div className="class-type-badge">{classItem.classType}</div>

            <div className="class-details">
                <h4>{classItem.studentName}</h4>
                <p><strong>Subject:</strong> {classItem.subject}</p>
                <p><strong>Email:</strong> {classItem.studentEmail}</p>
                {classItem.studentPhone && (
                    <p><strong>Phone:</strong> {classItem.studentPhone}</p>
                )}
            </div>

            {classItem.meetingLink && (
                <div className="actions">
                    <a
                        href={classItem.meetingLink}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="btn-join"
                    >
                        Join Meeting
                    </a>
                    <button
                        className="btn-copy"
                        onClick={() => copyLink(classItem.meetingLink)}
                    >
                        Copy Link
                    </button>
                </div>
            )}
        </div>
    );
};

// UpcomingClassCard.tsx
const UpcomingClassCard: React.FC<{ classItem: UpcomingClassDto }> = ({ classItem }) => {
    const formatDateTime = (date: string) => {
        return new Date(date).toLocaleString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    return (
        <div className={`class-card upcoming ${classItem.classType.toLowerCase()}`}>
            <div className="class-type-badge">{classItem.classType}</div>

            <h4>{classItem.studentName}</h4>
            <p><strong>Subject:</strong> {classItem.subject}</p>
            <p><strong>Date & Time:</strong> {formatDateTime(classItem.startTime)}</p>
            <p className="status">{classItem.status}</p>
        </div>
    );
};

// StudentCard.tsx
interface StudentDTO {
    studentId?: number;
    studentName: string;
    email: string;
    phone?: string;
    subject: string;
    totalClasses: number;
    firstBookedAt: string;
}

const StudentCard: React.FC<{ student: StudentDTO }> = ({ student }) => {
    const firstDate = new Date(student.firstBookedAt).toLocaleDateString();

    return (
        <div className="student-card">
            <h4>{student.studentName}</h4>
            <p><strong>Subject:</strong> {student.subject}</p>
            <p><strong>Classes:</strong> {student.totalClasses} total</p>
            <p><strong>Started:</strong> {firstDate}</p>
            <p><strong>Email:</strong> {student.email}</p>
            {student.phone && <p><strong>Phone:</strong> {student.phone}</p>}
        </div>
    );
};

export default TeacherDashboard;
```

---

## Component Examples

### Shared Components

#### ClassType Badge
```typescript
interface ClassTypeBadgeProps {
    type: 'Demo' | 'Regular';
}

const ClassTypeBadge: React.FC<ClassTypeBadgeProps> = ({ type }) => {
    const colors = {
        Demo: '#FFA500',      // Orange
        Regular: '#4CAF50'    // Green
    };

    return (
        <span style={{
            backgroundColor: colors[type],
            color: 'white',
            padding: '4px 12px',
            borderRadius: '20px',
            fontSize: '12px',
            fontWeight: 'bold'
        }}>
            {type}
        </span>
    );
};
```

#### Status Badge
```typescript
interface StatusBadgeProps {
    status: 'Scheduled' | 'Completed' | 'Cancelled' | 'Rescheduled' | 'Booked';
}

const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
    const styles = {
        Scheduled: { bg: '#E3F2FD', color: '#1976D2' },
        Completed: { bg: '#E8F5E9', color: '#388E3C' },
        Cancelled: { bg: '#FFEBEE', color: '#D32F2F' },
        Rescheduled: { bg: '#FFF3E0', color: '#F57C00' },
        Booked: { bg: '#F3E5F5', color: '#7B1FA2' }
    };

    const style = styles[status];

    return (
        <span style={{
            backgroundColor: style.bg,
            color: style.color,
            padding: '6px 12px',
            borderRadius: '4px',
            fontSize: '12px',
            fontWeight: '600'
        }}>
            {status}
        </span>
    );
};
```

#### Empty State Component
```typescript
const EmptyState: React.FC<{ message: string; icon?: string }> = ({
    message,
    icon = '📭'
}) => (
    <div style={{
        textAlign: 'center',
        padding: '40px',
        color: '#999'
    }}>
        <div style={{ fontSize: '48px', marginBottom: '16px' }}>{icon}</div>
        <p>{message}</p>
    </div>
);
```

---

## State Management

### Using React Context

```typescript
// DashboardContext.tsx
import React, { createContext, useContext, useState, useEffect } from 'react';

interface DashboardContextType {
    studentDashboard: StudentDashboardDto | null;
    teacherDashboard: TeacherDashboardDto | null;
    loading: boolean;
    error: string | null;
    refresh: () => Promise<void>;
}

const DashboardContext = createContext<DashboardContextType | undefined>(undefined);

export const DashboardProvider: React.FC<{ children: React.ReactNode }> = ({
    children
}) => {
    const [studentDashboard, setStudentDashboard] = useState<StudentDashboardDto | null>(null);
    const [teacherDashboard, setTeacherDashboard] = useState<TeacherDashboardDto | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const refresh = async () => {
        const token = localStorage.getItem('token');
        const userType = localStorage.getItem('userType'); // 'student' or 'teacher'

        if (!token) return;

        setLoading(true);
        try {
            if (userType === 'student') {
                const data = await fetchStudentDashboard(token);
                setStudentDashboard(data);
            } else {
                const data = await fetchTeacherDashboard(token);
                setTeacherDashboard(data);
            }
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Error loading dashboard');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        refresh();
        // Refresh every 5 minutes
        const interval = setInterval(refresh, 5 * 60 * 1000);
        return () => clearInterval(interval);
    }, []);

    return (
        <DashboardContext.Provider
            value={{
                studentDashboard,
                teacherDashboard,
                loading,
                error,
                refresh
            }}
        >
            {children}
        </DashboardContext.Provider>
    );
};

export const useDashboard = () => {
    const context = useContext(DashboardContext);
    if (!context) {
        throw new Error('useDashboard must be used within DashboardProvider');
    }
    return context;
};
```

### Using Redux

```typescript
// dashboardSlice.ts
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export const fetchStudentDashboard = createAsyncThunk(
    'dashboard/fetchStudent',
    async (token: string) => {
        const response = await fetch('/api/students/dashboard', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        return response.json();
    }
);

const dashboardSlice = createSlice({
    name: 'dashboard',
    initialState: {
        student: null,
        teacher: null,
        loading: false,
        error: null
    },
    extraReducers: (builder) => {
        builder
            .addCase(fetchStudentDashboard.pending, (state) => {
                state.loading = true;
            })
            .addCase(fetchStudentDashboard.fulfilled, (state, action) => {
                state.student = action.payload;
                state.loading = false;
            })
            .addCase(fetchStudentDashboard.rejected, (state, action) => {
                state.error = action.error.message;
                state.loading = false;
            });
    }
});

export default dashboardSlice.reducer;
```

---

## API Integration

### Axios Setup

```typescript
// api/client.ts
import axios, { AxiosInstance } from 'axios';

export const createApiClient = (token?: string): AxiosInstance => {
    return axios.create({
        baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000',
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    });
};

// api/studentApi.ts
export const fetchStudentDashboard = async (token: string) => {
    const client = createApiClient(token);
    const response = await client.get('/api/students/dashboard');
    return response.data;
};

export const fetchClassDetails = async (token: string, classId: number) => {
    const client = createApiClient(token);
    const response = await client.get(`/api/students/classes/${classId}`);
    return response.data;
};

// api/teacherApi.ts
export const fetchTeacherDashboard = async (token: string) => {
    const client = createApiClient(token);
    const response = await client.get('/api/teachers/dashboard');
    return response.data;
};
```

### Error Handling

```typescript
// api/errorHandler.ts
export class ApiError extends Error {
    constructor(
        public status: number,
        public message: string,
        public data?: any
    ) {
        super(message);
    }
}

export const handleApiError = (error: any): ApiError => {
    if (error.response) {
        return new ApiError(
            error.response.status,
            error.response.data?.message || 'API Error',
            error.response.data
        );
    }
    return new ApiError(0, error.message || 'Unknown error');
};
```

---

## UI Components Checklist

### Student Dashboard Components
- [ ] StudentInfoCard
  - [ ] Name display
  - [ ] Email display
  - [ ] Phone display
  - [ ] Grade/Year display
  - [ ] Subject display
  - [ ] Statistics section
- [ ] ClassTabs
  - [ ] Today tab
  - [ ] Upcoming tab
  - [ ] Past tab
- [ ] StudentClassCard
  - [ ] Teacher name
  - [ ] Subject
  - [ ] Time display
  - [ ] Meeting link button
  - [ ] Status badge
  - [ ] Recurrence pattern
- [ ] EmptyState
  - [ ] No classes message
  - [ ] Icon display
- [ ] Loading state
- [ ] Error message display
- [ ] Refresh button (optional)

### Teacher Dashboard Components
- [ ] TeacherInfoCard
  - [ ] Name display
  - [ ] Email display
  - [ ] Phone display
  - [ ] Subjects display
  - [ ] Statistics section (Classes today, Students, Earnings, Rating)
- [ ] TodayScheduleSection
  - [ ] Class time range
  - [ ] Class type badge (Demo/Regular)
  - [ ] Student name
  - [ ] Subject
  - [ ] Student contact info
  - [ ] Join meeting button
  - [ ] Copy link button
- [ ] UpcomingClassesSection
  - [ ] Class date/time
  - [ ] Class type badge
  - [ ] Student name
  - [ ] Subject
  - [ ] Status
- [ ] StudentsListSection
  - [ ] Student name
  - [ ] Subject
  - [ ] Total classes count
  - [ ] Start date
  - [ ] Contact info (email, phone)
- [ ] EmptyStates
  - [ ] No classes today
  - [ ] No upcoming classes
  - [ ] No students
- [ ] Loading state
- [ ] Error message display
- [ ] Refresh button (optional)

---

## CSS/Styling Guide

### Color Scheme

```css
:root {
    /* Primary Colors */
    --primary: #1976D2;
    --primary-light: #E3F2FD;
    --primary-dark: #0D47A1;

    /* Class Type Colors */
    --demo-color: #FFA500;      /* Orange */
    --regular-color: #4CAF50;   /* Green */

    /* Status Colors */
    --scheduled: #1976D2;    /* Blue */
    --completed: #4CAF50;    /* Green */
    --cancelled: #D32F2F;    /* Red */
    --booked: #7B1FA2;       /* Purple */

    /* Neutral */
    --text-primary: #333;
    --text-secondary: #666;
    --bg-light: #F5F5F5;
    --border: #DDD;
}
```

### Dashboard Card Styles

```css
.dashboard-card {
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    padding: 24px;
    margin-bottom: 24px;
}

.info-card {
    border-left: 4px solid var(--primary);
}

.class-card {
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 12px;
    border: 1px solid var(--border);
    transition: box-shadow 0.3s;
}

.class-card:hover {
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.class-card.demo {
    border-left: 4px solid var(--demo-color);
}

.class-card.regular {
    border-left: 4px solid var(--regular-color);
}
```

---

## Best Practices

### Performance
1. **Memoize Components** - Use `React.memo()` for class cards
2. **Lazy Load Images** - Load student/teacher avatars lazily
3. **Pagination** - Implement pagination for large student lists
4. **Caching** - Cache dashboard data with appropriate TTL

### Accessibility
1. Use semantic HTML
2. Add ARIA labels to interactive elements
3. Ensure keyboard navigation works
4. Use sufficient color contrast
5. Add focus indicators

### User Experience
1. Show loading states
2. Display error messages clearly
3. Add refresh/reload option
4. Provide empty states
5. Use consistent date/time formatting
6. Show confirmation dialogs for actions

### Security
1. Store JWT securely
2. Validate token before API calls
3. Clear token on logout
4. Sanitize displayed user data
5. Use HTTPS in production

---

## Testing Examples

### Component Tests

```typescript
// StudentDashboard.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import StudentDashboard from './StudentDashboard';

describe('StudentDashboard', () => {
    it('should display student name', async () => {
        render(<StudentDashboard />);
        await waitFor(() => {
            expect(screen.getByText('John Doe')).toBeInTheDocument();
        });
    });

    it('should display today classes', async () => {
        render(<StudentDashboard />);
        await waitFor(() => {
            expect(screen.getByText('Today (2)')).toBeInTheDocument();
        });
    });

    it('should display empty state when no classes', async () => {
        // Mock fetch to return no classes
        render(<StudentDashboard />);
        await waitFor(() => {
            expect(screen.getByText(/No classes/i)).toBeInTheDocument();
        });
    });
});
```

---

**Document Version:** 1.0  
**Last Updated:** January 2024  
**Status:** Complete Implementation Guide
