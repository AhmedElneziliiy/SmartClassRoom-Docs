# Smart ClassRoom - Mobile API Documentation

**Version:** 1.0
**Base URL:** `http://196.204.136.246:6001/api`
**Date:** January 14, 2026

---

## Table of Contents

1. [Authentication](#1-authentication)
2. [Grades API](#2-grades-api)
3. [Quizzes API](#3-quizzes-api)
4. [Timetables API](#4-timetables-api)
5. [Academic API](#5-academic-api)
6. [Enrollments API](#6-enrollments-api)
7. [Data Models](#7-data-models)
8. [Error Handling](#8-error-handling)

---

## 1. Authentication

Base Path: `/api/auth`

### 1.1 Login

**Endpoint:** `POST /api/auth/login`
**Authorization:** None (Public)

#### Request Headers
```http
Content-Type: application/json
```

#### Request Body
```typescript
{
  email: string;           // Required, valid email format
  password: string;        // Required, min 6 characters
  rememberMe: boolean;     // Optional, default: false
  udid?: string;          // Optional, device identifier for mobile (max 255 chars)
}
```

**Example:**
```json
{
  "email": "student@example.com",
  "password": "Password123!",
  "rememberMe": false,
  "udid": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### Response (Success - 200 OK)
```typescript
{
  success: boolean;        // true
  message: string;         // "Login successful"
  accessToken: string;     // JWT token (500-1000 chars)
  refreshToken: string;    // Refresh token (200-500 chars)
  expiresAt: string;       // ISO 8601 datetime
  user: {
    id: number;           // Integer
    fullName: string;     // Max 200 chars
    email: string;        // Valid email
    userType: string;     // "Student" | "Teacher" | "Admin"
    roles: string[];      // Array of role names
  }
}
```

**Example:**
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
  "refreshToken": "b3e5a4d2-1234-5678-90ab-cdef12345678",
  "expiresAt": "2026-01-17T12:00:00Z",
  "user": {
    "id": 123,
    "fullName": "John Doe",
    "email": "student@example.com",
    "userType": "Student",
    "roles": ["Student"]
  }
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  success: boolean;        // false
  message: string;         // Error description
}
```

**Example:**
```json
{
  "success": false,
  "message": "Email and password are required"
}
```

#### Response (Error - 401 Unauthorized)
```typescript
{
  success: boolean;        // false
  message: string;         // Error description
}
```

**Possible Messages:**
- `"Invalid email or password"`
- `"Account is locked. Please try again later."`
- `"Device not authorized for this account. Please contact administrator."`

**Example:**
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

#### Status Codes
- `200 OK` - Login successful
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Invalid credentials or unauthorized device

#### Notes
- **UDID (Unique Device Identifier):** On first mobile login, the UDID is stored. Subsequent logins verify the UDID matches.
- **Web Dashboard:** UDID is not required for web logins.
- **Token Expiration:** Access tokens expire after 60 minutes. Use refresh token to get new tokens.

---

### 1.2 Refresh Token

**Endpoint:** `POST /api/auth/refresh-token`
**Authorization:** None (Public)

#### Request Headers
```http
Content-Type: application/json
```

#### Request Body
```typescript
{
  accessToken: string;     // Required, expired or valid JWT token
  refreshToken: string;    // Required, valid refresh token
}
```

**Example:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "b3e5a4d2-1234-5678-90ab-cdef12345678"
}
```

#### Response (Success - 200 OK)
```typescript
{
  success: boolean;        // true
  message: string;         // "Token refreshed successfully"
  accessToken: string;     // New JWT token
  refreshToken: string;    // New refresh token
  expiresAt: string;       // ISO 8601 datetime
  user: {
    id: number;
    fullName: string;
    email: string;
    userType: string;
    roles: string[];
  }
}
```

**Example:**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "c4f6b5e3-5678-9012-abcd-ef1234567890",
  "expiresAt": "2026-01-17T13:00:00Z",
  "user": {
    "id": 123,
    "fullName": "John Doe",
    "email": "student@example.com",
    "userType": "Student",
    "roles": ["Student"]
  }
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  success: boolean;        // false
  message: string;         // Error description
}
```

#### Response (Error - 401 Unauthorized)
```typescript
{
  success: boolean;        // false
  message: string;         // "Invalid token" | "User not found" | "Token refresh failed"
}
```

#### Status Codes
- `200 OK` - Token refreshed successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Invalid token or user not found

---

## 2. Grades API

Base Path: `/api/grades`
**Authorization:** Required (JWT Bearer Token)

### 2.1 Get My Courses with Grades

**Endpoint:** `GET /api/grades/my-courses`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

**Example:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response (Success - 200 OK)
```typescript
Array<{
  courseOfferingId: number;  // Integer
  code: string;              // Course code, max 20 chars
  courseName: string;        // Max 200 chars
  credits: number;           // Integer, 1-6
  termName: string;          // Max 100 chars
  finalGrade: number | null; // Decimal, 0-100
  letterGrade: string | null;// "A" | "A-" | "B+" | "B" | "B-" | "C+" | "C" | "C-" | "D+" | "D" | "F"
  status: string;            // "Enrolled" | "Completed" | "Dropped" | "Withdrawn"
}>
```

**Example:**
```json
[
  {
    "courseOfferingId": 1,
    "code": "CS101",
    "courseName": "Introduction to Programming",
    "credits": 3,
    "termName": "Fall 2025",
    "finalGrade": 85.5,
    "letterGrade": "B",
    "status": "Completed"
  },
  {
    "courseOfferingId": 2,
    "code": "MATH201",
    "courseName": "Calculus I",
    "credits": 4,
    "termName": "Fall 2025",
    "finalGrade": null,
    "letterGrade": null,
    "status": "Enrolled"
  }
]
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // Error description
}
```

#### Response (Error - 401 Unauthorized)
No body, just status code.

#### Response (Error - 500 Internal Server Error)
```typescript
{
  message: string;           // "Internal server error: {details}"
}
```

#### Status Codes
- `200 OK` - Courses retrieved successfully
- `400 Bad Request` - Invalid student ID
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 2.2 Get Course Grades

**Endpoint:** `GET /api/grades/course/{offeringId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
offeringId: number;        // Required, integer
```

**Example:** `GET /api/grades/course/1`

#### Response (Success - 200 OK)
```typescript
{
  studentId: number;                    // Integer
  studentName: string;                  // Max 200 chars
  courseOfferingId: number;             // Integer
  courseName: string;                   // Max 200 chars
  componentGrades: Array<{
    componentId: number;                // Integer
    componentName: string;              // Max 100 chars
    weight: number;                     // Decimal, 0-100
    maxScore: number;                   // Decimal
    score: number | null;               // Decimal, 0-maxScore
    feedback: string | null;            // Max 1000 chars
    enteredAt: string | null;           // ISO 8601 datetime
  }>;
  finalGrade: number | null;            // Decimal, 0-100
  letterGrade: string | null;           // Letter grade
}
```

**Example:**
```json
{
  "studentId": 123,
  "studentName": "John Doe",
  "courseOfferingId": 1,
  "courseName": "Introduction to Programming",
  "componentGrades": [
    {
      "componentId": 1,
      "componentName": "Midterm Exam",
      "weight": 30.0,
      "maxScore": 100.0,
      "score": 85.0,
      "feedback": "Good work! Keep it up.",
      "enteredAt": "2025-10-15T10:00:00Z"
    },
    {
      "componentId": 2,
      "componentName": "Final Exam",
      "weight": 40.0,
      "maxScore": 100.0,
      "score": 90.0,
      "feedback": null,
      "enteredAt": "2025-12-10T14:00:00Z"
    },
    {
      "componentId": 3,
      "componentName": "Assignments",
      "weight": 30.0,
      "maxScore": 100.0,
      "score": null,
      "feedback": null,
      "enteredAt": null
    }
  ],
  "finalGrade": 87.5,
  "letterGrade": "B+"
}
```

#### Response (Error - 404 Not Found)
```typescript
{
  message: string;           // "Course not found or you are not enrolled"
}
```

#### Status Codes
- `200 OK` - Grades retrieved successfully
- `400 Bad Request` - Invalid student ID
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Course not found or student not enrolled
- `500 Internal Server Error` - Server error

---

### 2.3 Get GPA

**Endpoint:** `GET /api/grades/gpa`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Query Parameters
```typescript
termId?: number;           // Optional, integer
```

**Example:** `GET /api/grades/gpa?termId=5`

#### Response (Success - 200 OK)
```typescript
{
  studentId: number;                    // Integer
  studentName: string;                  // Max 200 chars
  cumulativeGPA: number;                // Decimal, 0.00-4.00
  termGPA: number | null;               // Decimal, 0.00-4.00 (null if termId not provided)
  totalCredits: number;                 // Integer
  termCredits: number | null;           // Integer (null if termId not provided)
  totalQualityPoints: number;           // Decimal
  termId: number | null;                // Integer or null
}
```

**Example:**
```json
{
  "studentId": 123,
  "studentName": "John Doe",
  "cumulativeGPA": 3.45,
  "termGPA": 3.67,
  "totalCredits": 45,
  "termCredits": 15,
  "totalQualityPoints": 155.25,
  "termId": 5
}
```

#### Status Codes
- `200 OK` - GPA retrieved successfully
- `400 Bad Request` - Invalid student ID
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Student not found
- `500 Internal Server Error` - Server error

---

### 2.4 Get Transcript (PDF)

**Endpoint:** `GET /api/grades/transcript`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Response (Success - 200 OK)
**Content-Type:** `application/pdf`
**Content-Disposition:** `attachment; filename="Transcript_123_20260114.pdf"`
**Body:** Binary PDF file

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // Error description
}
```

#### Status Codes
- `200 OK` - PDF generated successfully
- `400 Bad Request` - Invalid student ID or validation error
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

## 3. Quizzes API

Base Path: `/api/quizzes`
**Authorization:** Required (JWT Bearer Token)

### 3.1 Get Available Quizzes

**Endpoint:** `GET /api/quizzes/available`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Response (Success - 200 OK)
```typescript
Array<{
  assignmentId: number;              // Integer
  quizId: number;                    // Integer
  title: string;                     // Max 200 chars
  description: string | null;        // Max 1000 chars
  timeLimit: number;                 // Integer, minutes
  passingScore: number;              // Decimal, 0-100
  availableFrom: string;             // ISO 8601 datetime
  availableUntil: string;            // ISO 8601 datetime
  questionCount: number;             // Integer
  attemptsUsed: number;              // Integer
  maxAttempts: number;               // Integer
  canAttempt: boolean;               // true if can take quiz now
}>
```

**Example:**
```json
[
  {
    "assignmentId": 1,
    "quizId": 10,
    "title": "Chapter 1 Quiz",
    "description": "Quiz covering basic programming concepts",
    "timeLimit": 30,
    "passingScore": 70.0,
    "availableFrom": "2026-01-10T08:00:00Z",
    "availableUntil": "2026-01-20T23:59:59Z",
    "questionCount": 10,
    "attemptsUsed": 1,
    "maxAttempts": 3,
    "canAttempt": true
  },
  {
    "assignmentId": 2,
    "quizId": 11,
    "title": "Midterm Exam",
    "description": null,
    "timeLimit": 90,
    "passingScore": 60.0,
    "availableFrom": "2026-01-15T09:00:00Z",
    "availableUntil": "2026-01-15T18:00:00Z",
    "questionCount": 30,
    "attemptsUsed": 0,
    "maxAttempts": 1,
    "canAttempt": false
  }
]
```

#### Status Codes
- `200 OK` - Quizzes retrieved successfully
- `400 Bad Request` - Invalid student ID
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 3.2 Start Quiz

**Endpoint:** `POST /api/quizzes/start/{assignmentId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
assignmentId: number;      // Required, integer
```

**Example:** `POST /api/quizzes/start/1`

#### Response (Success - 200 OK)
```typescript
{
  attemptId: number;                    // Integer
  quizId: number;                       // Integer
  title: string;                        // Max 200 chars
  description: string | null;           // Max 1000 chars
  timeLimit: number;                    // Integer, minutes
  startedAt: string;                    // ISO 8601 datetime
  questions: Array<{
    id: number;                         // Integer, question ID
    questionText: string;               // Max 1000 chars
    questionType: number;               // 0 = MultipleChoice, 1 = TrueFalse
    option1: string;                    // Max 500 chars
    option2: string;                    // Max 500 chars
    option3: string | null;             // Max 500 chars (null for TrueFalse)
    option4: string | null;             // Max 500 chars (null for TrueFalse)
    points: number;                     // Decimal
  }>;
}
```

**Example:**
```json
{
  "attemptId": 45,
  "quizId": 10,
  "title": "Chapter 1 Quiz",
  "description": "Quiz covering basic programming concepts",
  "timeLimit": 30,
  "startedAt": "2026-01-14T10:00:00Z",
  "questions": [
    {
      "id": 1,
      "questionText": "What is the output of: print(2 + 2)?",
      "questionType": 0,
      "option1": "3",
      "option2": "4",
      "option3": "22",
      "option4": "Error",
      "points": 10.0
    },
    {
      "id": 2,
      "questionText": "Python is an interpreted language.",
      "questionType": 1,
      "option1": "True",
      "option2": "False",
      "option3": null,
      "option4": null,
      "points": 5.0
    }
  ]
}
```

#### Notes
- **Question Types:**
  - `0` = Multiple Choice (4 options)
  - `1` = True/False (2 options)
- **Questions DO NOT include correct answers**
- **Time Limit:** Quiz must be submitted within the time limit from `startedAt`
- **Attempts:** Starting a quiz consumes one attempt

#### Status Codes
- `200 OK` - Quiz started successfully
- `400 Bad Request` - Cannot start quiz (max attempts reached, not available, etc.)
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 3.3 Submit Quiz

**Endpoint:** `POST /api/quizzes/submit`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Request Body
```typescript
{
  attemptId: number;                    // Required, integer
  answers: {
    [questionId: string]: number;       // Question ID (as string) → Selected option (1-4)
  };
}
```

**Example:**
```json
{
  "attemptId": 45,
  "answers": {
    "1": 2,
    "2": 1,
    "3": 4,
    "4": 1,
    "5": 3
  }
}
```

**Notes:**
- Keys in `answers` are question IDs as strings
- Values are the selected option number (1, 2, 3, or 4)
- For True/False questions: 1 = True, 2 = False

#### Response (Success - 200 OK)
```typescript
{
  attemptId: number;                    // Integer
  score: number;                        // Decimal, total points earned
  totalPoints: number;                  // Decimal, maximum possible points
  percentage: number;                   // Decimal, 0-100
  status: string;                       // "Passed" | "Failed"
  submittedAt: string;                  // ISO 8601 datetime
  attemptNumber: number;                // Integer, which attempt this was
}
```

**Example:**
```json
{
  "attemptId": 45,
  "score": 85.0,
  "totalPoints": 100.0,
  "percentage": 85.0,
  "status": "Passed",
  "submittedAt": "2026-01-14T10:30:00Z",
  "attemptNumber": 1
}
```

#### Notes
- **Result does NOT include which answers were correct/incorrect**
- **Status:** "Passed" if percentage >= passingScore, otherwise "Failed"

#### Status Codes
- `200 OK` - Quiz submitted and graded successfully
- `400 Bad Request` - Invalid request data or validation error
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 3.4 Get Quiz Result

**Endpoint:** `GET /api/quizzes/results/{attemptId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
attemptId: number;         // Required, integer
```

**Example:** `GET /api/quizzes/results/45`

#### Response (Success - 200 OK)
```typescript
{
  attemptId: number;                    // Integer
  score: number;                        // Decimal
  totalPoints: number;                  // Decimal
  percentage: number;                   // Decimal, 0-100
  status: string;                       // "Passed" | "Failed"
  submittedAt: string;                  // ISO 8601 datetime
  attemptNumber: number;                // Integer
}
```

**Example:**
```json
{
  "attemptId": 45,
  "score": 85.0,
  "totalPoints": 100.0,
  "percentage": 85.0,
  "status": "Passed",
  "submittedAt": "2026-01-14T10:30:00Z",
  "attemptNumber": 1
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Invalid student ID" | "Quiz has not been graded yet"
}
```

#### Response (Error - 403 Forbidden)
No body, just status code. Returned when trying to access another student's quiz.

#### Response (Error - 404 Not Found)
```typescript
{
  message: string;           // "Attempt not found"
}
```

#### Status Codes
- `200 OK` - Result retrieved successfully
- `400 Bad Request` - Invalid student ID or quiz not graded
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Attempting to access another student's quiz
- `404 Not Found` - Attempt not found
- `500 Internal Server Error` - Server error

---

### 3.5 Get Quiz History

**Endpoint:** `GET /api/quizzes/history/{assignmentId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
assignmentId: number;      // Required, integer
```

**Example:** `GET /api/quizzes/history/1`

#### Response (Success - 200 OK)
```typescript
Array<{
  attemptId: number;                    // Integer
  score: number;                        // Decimal
  totalPoints: number;                  // Decimal
  percentage: number;                   // Decimal, 0-100
  status: string;                       // "Passed" | "Failed"
  submittedAt: string;                  // ISO 8601 datetime
  attemptNumber: number;                // Integer
}>
```

**Example:**
```json
[
  {
    "attemptId": 50,
    "score": 92.0,
    "totalPoints": 100.0,
    "percentage": 92.0,
    "status": "Passed",
    "submittedAt": "2026-01-15T14:00:00Z",
    "attemptNumber": 2
  },
  {
    "attemptId": 45,
    "score": 85.0,
    "totalPoints": 100.0,
    "percentage": 85.0,
    "status": "Passed",
    "submittedAt": "2026-01-14T10:30:00Z",
    "attemptNumber": 1
  }
]
```

#### Notes
- **Only returns graded attempts**
- **Ordered by submission date (newest first)**

#### Status Codes
- `200 OK` - History retrieved successfully (empty array if no attempts)
- `400 Bad Request` - Invalid student ID
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

## 4. Timetables API

Base Path: `/api/timetables`
**Authorization:** Required (JWT Bearer or Cookie)

### 4.1 Generate Timetable

**Endpoint:** `POST /api/timetables/generate`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Request Body
```typescript
{
  departmentId: number;                 // Required, integer
  levelId: number;                      // Required, integer
  sectionId: number;                    // Required, integer
  termId: number;                       // Required, integer
  name: string;                         // Required, max 200 chars
  notes: string | null;                 // Optional, max 1000 chars
  selectedDays: string[];               // Required, array of day names
  startTime: string;                    // Required, format "HH:mm:ss"
  endTime: string;                      // Required, format "HH:mm:ss"
}
```

**Example:**
```json
{
  "departmentId": 1,
  "levelId": 2,
  "sectionId": 3,
  "termId": 5,
  "name": "Fall 2025 - CS Level 2A",
  "notes": "First draft - needs review",
  "selectedDays": ["Monday", "Tuesday", "Wednesday", "Thursday"],
  "startTime": "08:00:00",
  "endTime": "17:00:00"
}
```

**Valid Day Names:**
- `"Sunday"`
- `"Monday"`
- `"Tuesday"`
- `"Wednesday"`
- `"Thursday"`
- `"Friday"`
- `"Saturday"`

#### Response (Success - 200 OK)
```typescript
{
  success: boolean;                     // true if generated
  message: string;                      // Success or error message
  timetableId: number | null;           // Integer (ID of created timetable)
  totalSlots: number;                   // Integer
  scheduledSlots: number;               // Integer
  unscheduledSlots: number;             // Integer
  conflicts: Array<{
    conflictType: string;               // "Teacher" | "Section" | "Room"
    dayOfWeek: string;                  // Day name
    startTime: string;                  // "HH:mm:ss"
    endTime: string;                    // "HH:mm:ss"
    resource: string;                   // Teacher/Room name
    affectedCourses: string[];          // Array of course codes
    description: string;                // Human-readable description
  }>;
  unscheduledCourses: Array<{
    courseCode: string;                 // Max 20 chars
    courseName: string;                 // Max 200 chars
    reason: string;                     // Why it couldn't be scheduled
  }>;
  status: string;                       // "Draft" | "Published"
}
```

**Example:**
```json
{
  "success": true,
  "message": "Timetable generated successfully with 2 conflicts",
  "timetableId": 10,
  "totalSlots": 40,
  "scheduledSlots": 35,
  "unscheduledSlots": 5,
  "conflicts": [
    {
      "conflictType": "Teacher",
      "dayOfWeek": "Monday",
      "startTime": "09:00:00",
      "endTime": "10:00:00",
      "resource": "Dr. Smith",
      "affectedCourses": ["CS101", "CS201"],
      "description": "Teacher double-booked at this time"
    }
  ],
  "unscheduledCourses": [
    {
      "courseCode": "MATH301",
      "courseName": "Advanced Calculus",
      "reason": "No available time slot that fits course requirements"
    }
  ],
  "status": "Draft"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  success: boolean;                     // false
  message: string;                      // Error description
  timetableId: null;
  totalSlots: number;                   // 0
  scheduledSlots: number;               // 0
  unscheduledSlots: number;             // 0
  conflicts: [];
  unscheduledCourses: [];
  status: string;                       // "Draft"
}
```

#### Status Codes
- `200 OK` - Timetable generated (may have conflicts)
- `400 Bad Request` - Validation error or generation failed
- `401 Unauthorized` - Missing or invalid token

---

### 4.2 Get Timetable

**Endpoint:** `GET /api/timetables/{id}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer
```

**Example:** `GET /api/timetables/10`

#### Response (Success - 200 OK)
```typescript
{
  id: number;                           // Integer
  name: string;                         // Max 200 chars
  departmentName: string;               // Max 200 chars
  levelName: string;                    // Max 100 chars
  sectionName: string;                  // Max 100 chars
  termName: string;                     // Max 100 chars
  status: string;                       // "Draft" | "Published"
  createdAt: string;                    // ISO 8601 datetime
  slots: Array<{
    id: number;                         // Integer
    dayOfWeek: string;                  // Day name
    startTime: string;                  // "HH:mm:ss"
    endTime: string;                    // "HH:mm:ss"
    courseName: string;                 // Max 200 chars
    courseCode: string;                 // Max 20 chars
    teacherName: string;                // Max 200 chars
    roomNumber: string | null;          // Max 50 chars
    roomCapacity: number | null;        // Integer
  }>;
}
```

**Example:**
```json
{
  "id": 10,
  "name": "Fall 2025 - CS Level 2A",
  "departmentName": "Computer Science",
  "levelName": "Level 2",
  "sectionName": "Section A",
  "termName": "Fall 2025",
  "status": "Draft",
  "createdAt": "2026-01-14T10:00:00Z",
  "slots": [
    {
      "id": 1,
      "dayOfWeek": "Monday",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "courseName": "Introduction to Programming",
      "courseCode": "CS101",
      "teacherName": "Dr. John Smith",
      "roomNumber": "A101",
      "roomCapacity": 40
    },
    {
      "id": 2,
      "dayOfWeek": "Monday",
      "startTime": "09:00:00",
      "endTime": "10:00:00",
      "courseName": "Data Structures",
      "courseCode": "CS201",
      "teacherName": "Dr. Jane Johnson",
      "roomNumber": null,
      "roomCapacity": null
    }
  ]
}
```

#### Response (Error - 404 Not Found)
Empty response body.

#### Status Codes
- `200 OK` - Timetable retrieved successfully
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Timetable not found

---

### 4.3 Get Timetable Slots

**Endpoint:** `GET /api/timetables/{id}/slots`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer
```

**Example:** `GET /api/timetables/10/slots`

#### Response (Success - 200 OK)
```typescript
Array<{
  id: number;                           // Integer
  dayOfWeek: string;                    // Day name
  startTime: string;                    // "HH:mm:ss"
  endTime: string;                      // "HH:mm:ss"
  courseName: string;                   // Max 200 chars
  courseCode: string;                   // Max 20 chars
  teacherName: string;                  // Max 200 chars
  roomNumber: string | null;            // Max 50 chars
  roomCapacity: number | null;          // Integer
}>
```

**Example:**
```json
[
  {
    "id": 1,
    "dayOfWeek": "Monday",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "courseName": "Introduction to Programming",
    "courseCode": "CS101",
    "teacherName": "Dr. John Smith",
    "roomNumber": "A101",
    "roomCapacity": 40
  }
]
```

#### Status Codes
- `200 OK` - Slots retrieved successfully (empty array if none)
- `401 Unauthorized` - Missing or invalid token

---

### 4.4 Detect Conflicts

**Endpoint:** `GET /api/timetables/{id}/conflicts`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer
```

**Example:** `GET /api/timetables/10/conflicts`

#### Response (Success - 200 OK)
```typescript
Array<{
  conflictType: string;                 // "Teacher" | "Section" | "Room"
  dayOfWeek: string;                    // Day name
  startTime: string;                    // "HH:mm:ss"
  endTime: string;                      // "HH:mm:ss"
  resource: string;                     // Teacher/Room name
  affectedCourses: string[];            // Array of course codes
  description: string;                  // Human-readable description
}>
```

**Example:**
```json
[
  {
    "conflictType": "Teacher",
    "dayOfWeek": "Monday",
    "startTime": "09:00:00",
    "endTime": "10:00:00",
    "resource": "Dr. Smith",
    "affectedCourses": ["CS101", "CS201"],
    "description": "Teacher double-booked at this time"
  },
  {
    "conflictType": "Room",
    "dayOfWeek": "Tuesday",
    "startTime": "10:00:00",
    "endTime": "11:00:00",
    "resource": "A101",
    "affectedCourses": ["MATH101", "PHY101"],
    "description": "Room double-booked at this time"
  }
]
```

#### Status Codes
- `200 OK` - Conflicts retrieved (empty array if none)
- `401 Unauthorized` - Missing or invalid token

---

### 4.5 Publish Timetable

**Endpoint:** `POST /api/timetables/{id}/publish`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer
```

**Example:** `POST /api/timetables/10/publish`

#### Response (Success - 200 OK)
```typescript
{
  success: boolean;                     // true
  message: string;                      // "Timetable published successfully"
  sessionsCreated: number;              // Integer, number of sessions created
  publishedAt: string;                  // ISO 8601 datetime
  remainingConflicts: [];               // Empty array on success
}
```

**Example:**
```json
{
  "success": true,
  "message": "Timetable published successfully",
  "sessionsCreated": 120,
  "publishedAt": "2026-01-14T12:00:00Z",
  "remainingConflicts": []
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  success: boolean;                     // false
  message: string;                      // Error description
  sessionsCreated: number;              // 0
  publishedAt: null;
  remainingConflicts: Array<{
    conflictType: string;
    dayOfWeek: string;
    startTime: string;
    endTime: string;
    resource: string;
    affectedCourses: string[];
    description: string;
  }>;
}
```

**Example:**
```json
{
  "success": false,
  "message": "Cannot publish timetable with unresolved conflicts",
  "sessionsCreated": 0,
  "publishedAt": null,
  "remainingConflicts": [
    {
      "conflictType": "Teacher",
      "dayOfWeek": "Monday",
      "startTime": "09:00:00",
      "endTime": "10:00:00",
      "resource": "Dr. Smith",
      "affectedCourses": ["CS101", "CS201"],
      "description": "Teacher double-booked"
    }
  ]
}
```

#### Notes
- **Publishing creates all course sessions** for the term based on the timetable
- **Cannot publish if there are conflicts**
- **Published timetables cannot be edited** (must create new version)

#### Status Codes
- `200 OK` - Timetable published successfully
- `400 Bad Request` - Publishing failed (usually due to conflicts)
- `401 Unauthorized` - Missing or invalid token

---

### 4.6 Update Slot

**Endpoint:** `PUT /api/timetables/slots/{id}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Path Parameters
```typescript
id: number;                // Required, integer (slot ID)
```

#### Request Body
```typescript
{
  slotId: number;                       // Required, must match path parameter
  dayOfWeek: string;                    // Required, valid day name
  startTime: string;                    // Required, "HH:mm:ss"
  endTime: string;                      // Required, "HH:mm:ss"
  roomId: number | null;                // Optional, integer
}
```

**Example:**
```json
{
  "slotId": 1,
  "dayOfWeek": "Monday",
  "startTime": "08:00:00",
  "endTime": "09:00:00",
  "roomId": 5
}
```

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Slot updated successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Slot ID mismatch" | "Validation error" | etc.
}
```

#### Status Codes
- `200 OK` - Slot updated successfully
- `400 Bad Request` - Validation error or update failed
- `401 Unauthorized` - Missing or invalid token

---

### 4.7 Delete Slot

**Endpoint:** `DELETE /api/timetables/slots/{id}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer (slot ID)
```

**Example:** `DELETE /api/timetables/slots/1`

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Slot deleted successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Delete failed: {reason}"
}
```

#### Status Codes
- `200 OK` - Slot deleted successfully
- `400 Bad Request` - Delete failed
- `401 Unauthorized` - Missing or invalid token

---

### 4.8 Add Slot

**Endpoint:** `POST /api/timetables/{timetableId}/slots`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Path Parameters
```typescript
timetableId: number;       // Required, integer
```

#### Query Parameters
```typescript
courseOfferingId: number;  // Required, integer
roomId?: number;           // Optional, integer
```

**Example:** `POST /api/timetables/10/slots?courseOfferingId=5&roomId=3`

#### Request Body
```typescript
{
  dayOfWeek: string;                    // Required, valid day name
  startTime: string;                    // Required, "HH:mm:ss"
  endTime: string;                      // Required, "HH:mm:ss"
}
```

**Example:**
```json
{
  "dayOfWeek": "Monday",
  "startTime": "08:00:00",
  "endTime": "09:00:00"
}
```

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Slot added successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Add failed: {reason}"
}
```

#### Status Codes
- `200 OK` - Slot added successfully
- `400 Bad Request` - Add failed (conflict, validation error, etc.)
- `401 Unauthorized` - Missing or invalid token

---

### 4.9 Delete Timetable

**Endpoint:** `DELETE /api/timetables/{id}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
id: number;                // Required, integer
```

**Example:** `DELETE /api/timetables/10`

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Timetable deleted successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Delete failed: {reason}"
}
```

#### Status Codes
- `200 OK` - Timetable deleted successfully
- `400 Bad Request` - Delete failed (timetable published, etc.)
- `401 Unauthorized` - Missing or invalid token

---

## 5. Academic API

Base Path: `/api/academic`
**Authorization:** None (Public)

### 5.1 Get Levels by Department

**Endpoint:** `GET /api/academic/levels/{departmentId}`
**Authorization:** None

#### Path Parameters
```typescript
departmentId: number;      // Required, integer
```

**Example:** `GET /api/academic/levels/1`

#### Response (Success - 200 OK)
```typescript
Array<{
  value: number;                        // Integer, level ID
  text: string;                         // Level name, e.g., "Level 1"
}>
```

**Example:**
```json
[
  {
    "value": 1,
    "text": "Level 1"
  },
  {
    "value": 2,
    "text": "Level 2"
  },
  {
    "value": 3,
    "text": "Level 3"
  },
  {
    "value": 4,
    "text": "Level 4"
  }
]
```

#### Notes
- Returns only **active levels**
- Ordered by level number (ascending)
- Returns empty array if department has no levels

#### Status Codes
- `200 OK` - Levels retrieved (empty array if none)

---

### 5.2 Get Sections by Level

**Endpoint:** `GET /api/academic/sections/{levelId}`
**Authorization:** None

#### Path Parameters
```typescript
levelId: number;           // Required, integer
```

**Example:** `GET /api/academic/sections/2`

#### Response (Success - 200 OK)
```typescript
Array<{
  value: number;                        // Integer, section ID
  text: string;                         // Section name, e.g., "Section A"
}>
```

**Example:**
```json
[
  {
    "value": 1,
    "text": "Section A"
  },
  {
    "value": 2,
    "text": "Section B"
  },
  {
    "value": 3,
    "text": "Section C"
  }
]
```

#### Notes
- Returns only **active sections**
- Ordered by name (alphabetically)
- Returns empty array if level has no sections

#### Status Codes
- `200 OK` - Sections retrieved (empty array if none)

---

### 5.3 Get Groups by Section

**Endpoint:** `GET /api/academic/groups/{sectionId}`
**Authorization:** None

#### Path Parameters
```typescript
sectionId: number;         // Required, integer
```

**Example:** `GET /api/academic/groups/1`

#### Response (Success - 200 OK)
```typescript
Array<{
  value: number;                        // Integer, group ID
  text: string;                         // Group name, e.g., "Group 1"
}>
```

**Example:**
```json
[
  {
    "value": 1,
    "text": "Group 1"
  },
  {
    "value": 2,
    "text": "Group 2"
  }
]
```

#### Notes
- Returns only **active groups**
- Ordered by name (alphabetically)
- Returns empty array if section has no groups

#### Status Codes
- `200 OK` - Groups retrieved (empty array if none)

---

## 6. Enrollments API

Base Path: `/api/enrollments`
**Authorization:** Required (JWT Bearer or Cookie)

### 6.1 Enroll Student

**Endpoint:** `POST /api/enrollments`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Request Body
```typescript
{
  studentId: number;                    // Required, integer
  courseOfferingId: number;             // Required, integer
}
```

**Example:**
```json
{
  "studentId": 123,
  "courseOfferingId": 45
}
```

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Student enrolled successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // Error description
}
```

**Possible Error Messages:**
- `"Prerequisites not met: {course codes required}"`
- `"Course is full (max capacity reached)"`
- `"Student is already enrolled in this course"`
- `"Schedule conflict with course: {conflicting course}"`
- `"Course is not available for enrollment"`

**Example:**
```json
{
  "message": "Prerequisites not met: CS101, MATH101 required"
}
```

#### Notes
- **Validates prerequisites** before enrollment
- **Checks capacity** - fails if course is full
- **Checks schedule conflicts** with student's other courses
- **Creates enrollment** with status "Enrolled"

#### Status Codes
- `200 OK` - Enrollment successful
- `400 Bad Request` - Enrollment failed (validation error)
- `401 Unauthorized` - Missing or invalid token

---

### 6.2 Drop Course

**Endpoint:** `DELETE /api/enrollments/{enrollmentId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
enrollmentId: number;      // Required, integer
```

**Example:** `DELETE /api/enrollments/1`

#### Response (Success - 200 OK)
```typescript
{
  message: string;           // "Course dropped successfully"
}
```

#### Response (Error - 400 Bad Request)
```typescript
{
  message: string;           // "Drop failed: {reason}"
}
```

**Possible Error Messages:**
- `"Cannot drop course after drop deadline"`
- `"Enrollment not found"`
- `"Course already dropped"`

#### Response (Error - 404 Not Found)
```typescript
{
  message: string;           // "Enrollment not found"
}
```

#### Notes
- **Changes enrollment status** to "Dropped"
- **Does not delete** the enrollment record
- **May fail** if past drop deadline

#### Status Codes
- `200 OK` - Course dropped successfully
- `400 Bad Request` - Drop failed
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Enrollment not found

---

### 6.3 Get Student Enrollments

**Endpoint:** `GET /api/enrollments/student/{studentId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
studentId: number;         // Required, integer
```

#### Query Parameters
```typescript
termId?: number;           // Optional, integer (filter by term)
```

**Example:** `GET /api/enrollments/student/123?termId=5`

#### Response (Success - 200 OK)
```typescript
Array<{
  enrollmentId: number;                 // Integer
  studentName: string;                  // Max 200 chars
  courseName: string;                   // Max 200 chars
  termName: string;                     // Max 100 chars
  enrollmentDate: string;               // ISO 8601 datetime
  status: string;                       // "Enrolled" | "Dropped" | "Completed" | "Withdrawn"
}>
```

**Example:**
```json
[
  {
    "enrollmentId": 1,
    "studentName": "John Doe",
    "courseName": "Introduction to Programming",
    "termName": "Fall 2025",
    "enrollmentDate": "2025-08-15T10:00:00Z",
    "status": "Enrolled"
  },
  {
    "enrollmentId": 2,
    "studentName": "John Doe",
    "courseName": "Data Structures",
    "termName": "Fall 2025",
    "enrollmentDate": "2025-08-15T10:05:00Z",
    "status": "Completed"
  }
]
```

#### Notes
- **Without termId:** Returns all enrollments for the student
- **With termId:** Filters to specific term only

#### Status Codes
- `200 OK` - Enrollments retrieved (empty array if none)
- `401 Unauthorized` - Missing or invalid token

---

### 6.4 Get Offering Enrollments

**Endpoint:** `GET /api/enrollments/offering/{offeringId}`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
```

#### Path Parameters
```typescript
offeringId: number;        // Required, integer
```

**Example:** `GET /api/enrollments/offering/45`

#### Response (Success - 200 OK)
```typescript
Array<{
  enrollmentId: number;                 // Integer
  studentName: string;                  // Max 200 chars
  courseName: string;                   // Max 200 chars
  termName: string;                     // Max 100 chars
  enrollmentDate: string;               // ISO 8601 datetime
  status: string;                       // "Enrolled" | "Dropped" | "Completed" | "Withdrawn"
}>
```

**Example:**
```json
[
  {
    "enrollmentId": 1,
    "studentName": "John Doe",
    "courseName": "Introduction to Programming",
    "termName": "Fall 2025",
    "enrollmentDate": "2025-08-15T10:00:00Z",
    "status": "Enrolled"
  },
  {
    "enrollmentId": 2,
    "studentName": "Jane Smith",
    "courseName": "Introduction to Programming",
    "termName": "Fall 2025",
    "enrollmentDate": "2025-08-15T10:05:00Z",
    "status": "Enrolled"
  }
]
```

#### Notes
- Returns **all enrollments** for the specified course offering
- Useful for teachers to see their class roster

#### Status Codes
- `200 OK` - Enrollments retrieved (empty array if none)
- `401 Unauthorized` - Missing or invalid token

---

### 6.5 Validate Enrollment

**Endpoint:** `POST /api/enrollments/validate`
**Authorization:** Required

#### Request Headers
```http
Authorization: Bearer {access-token}
Content-Type: application/json
```

#### Request Body
```typescript
{
  studentId: number;                    // Required, integer
  courseOfferingId: number;             // Required, integer
}
```

**Example:**
```json
{
  "studentId": 123,
  "courseOfferingId": 45
}
```

#### Response (Success - 200 OK)
```typescript
{
  isEligible: boolean;                  // true if can enroll, false otherwise
  message: string;                      // Success or error message
}
```

**Example (Eligible):**
```json
{
  "isEligible": true,
  "message": "Student is eligible to enroll in this course"
}
```

**Example (Not Eligible - Prerequisites):**
```json
{
  "isEligible": false,
  "message": "Prerequisites not met: CS101, MATH101 required"
}
```

**Example (Not Eligible - Full):**
```json
{
  "isEligible": false,
  "message": "Course is full (max capacity: 40 students)"
}
```

**Example (Not Eligible - Schedule Conflict):**
```json
{
  "isEligible": false,
  "message": "Schedule conflict with course: Data Structures (CS201)"
}
```

#### Notes
- **Does NOT enroll** the student, only checks eligibility
- **Validates:**
  - Prerequisites completion
  - Course capacity
  - Schedule conflicts
  - Already enrolled check
  - Course availability

#### Status Codes
- `200 OK` - Validation complete (check `isEligible` field)
- `401 Unauthorized` - Missing or invalid token

---

## 7. Data Models

### 7.1 Question Types (Enum)
```typescript
enum QuestionType {
  MultipleChoice = 0,
  TrueFalse = 1
}
```

### 7.2 Enrollment Status (String)
Valid values:
- `"Enrolled"` - Currently enrolled
- `"Dropped"` - Student dropped the course
- `"Completed"` - Successfully completed
- `"Withdrawn"` - Withdrew from course

### 7.3 Letter Grades (String)
Valid values:
- `"A"`, `"A-"`
- `"B+"`, `"B"`, `"B-"`
- `"C+"`, `"C"`, `"C-"`
- `"D+"`, `"D"`
- `"F"`

### 7.4 User Types (String)
Valid values:
- `"Student"`
- `"Teacher"`
- `"Admin"`

### 7.5 Timetable Status (String)
Valid values:
- `"Draft"` - Can be edited
- `"Published"` - Locked, sessions created

### 7.6 Conflict Types (String)
Valid values:
- `"Teacher"` - Teacher double-booked
- `"Section"` - Section double-booked
- `"Room"` - Room double-booked

### 7.7 Days of Week (String)
Valid values:
- `"Sunday"`
- `"Monday"`
- `"Tuesday"`
- `"Wednesday"`
- `"Thursday"`
- `"Friday"`
- `"Saturday"`

---

## 8. Error Handling

### 8.1 HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Request successful |
| 400 | Bad Request | Validation error, invalid data |
| 401 | Unauthorized | Missing/invalid token, invalid credentials |
| 403 | Forbidden | Accessing unauthorized resource |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server error |

### 8.2 Error Response Format

Most errors return JSON:
```typescript
{
  message: string;           // Human-readable error message
  success?: boolean;         // false (for auth endpoints)
}
```

**Example:**
```json
{
  "message": "Course not found or you are not enrolled"
}
```

### 8.3 Authentication Errors

**Missing Token:**
- Status: `401 Unauthorized`
- No response body

**Invalid/Expired Token:**
- Status: `401 Unauthorized`
- Body: `{ "message": "Invalid token" }`

**Recommended Handling:**
1. If `401` on any endpoint → try refresh token
2. If refresh fails → redirect to login
3. Store new tokens from refresh response

### 8.4 Validation Errors

**Example:**
```json
{
  "message": "Email and password are required"
}
```

Always check `message` field for user-friendly error description.

---

## 9. Best Practices

### 9.1 Token Management

**Store Securely:**
- Use secure storage (Keychain on iOS, KeyStore on Android)
- Don't store in plain text or SharedPreferences

**Refresh Strategy:**
```
1. Make API call
2. If 401 → Call /api/auth/refresh-token
3. If refresh succeeds → Retry original call with new token
4. If refresh fails → Logout and redirect to login
```

**Token Expiration:**
- Access tokens expire after **60 minutes**
- Refresh tokens expire after **7 days** (default)
- Implement automatic refresh before expiration

### 9.2 UDID (Unique Device ID)

**For Mobile Apps:**
```
1. Generate UUID on first app launch
2. Store persistently in secure storage
3. Send on login as "udid" field
4. First login stores UDID on server
5. Subsequent logins verify UDID matches
```

**Important:**
- UDID locks account to specific device
- Contact admin to reset if device changes
- Web dashboard doesn't use UDID

### 9.3 Quiz Handling

**Start Quiz Flow:**
```
1. GET /api/quizzes/available → Show available quizzes
2. POST /api/quizzes/start/{assignmentId} → Get questions
3. Store startedAt timestamp locally
4. Calculate remaining time = timeLimit - (now - startedAt)
5. POST /api/quizzes/submit → Submit answers before time expires
```

**Answer Format:**
```json
{
  "attemptId": 45,
  "answers": {
    "1": 2,    // Question 1 → Option 2
    "2": 1,    // Question 2 → Option 1
    "3": 4     // Question 3 → Option 4
  }
}
```

### 9.4 Date/Time Handling

**All timestamps are UTC ISO 8601:**
- Format: `"2026-01-14T10:30:00Z"`
- Always convert to local timezone for display
- Store original UTC for calculations

**Time Format:**
- Format: `"HH:mm:ss"` (24-hour)
- Example: `"08:00:00"` = 8:00 AM
- Example: `"17:30:00"` = 5:30 PM

### 9.5 Pagination

**Not Currently Implemented:**
- All list endpoints return complete results
- No pagination parameters
- Filter on client side if needed

**Future Enhancement:**
- May add `page` and `pageSize` parameters
- Will be documented when available

### 9.6 Error Handling Example (Pseudocode)

```typescript
async function apiCall(endpoint: string, options: RequestOptions) {
  try {
    const response = await fetch(endpoint, {
      ...options,
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
        ...options.headers
      }
    });

    if (response.status === 401) {
      // Try to refresh token
      const refreshed = await refreshToken();
      if (refreshed) {
        // Retry original request with new token
        return apiCall(endpoint, options);
      } else {
        // Refresh failed, logout
        logout();
        navigateToLogin();
        return null;
      }
    }

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || `HTTP ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
}
```

---

## 10. Testing & Swagger

**Swagger UI:**
`http://196.204.136.246:6001/swagger`

**Try Endpoints:**
1. Expand any endpoint
2. Click "Try it out"
3. Fill in parameters
4. For authenticated endpoints:
   - Login first to get token
   - Click "Authorize" button at top
   - Enter: `Bearer {your-access-token}`
   - Click "Authorize"
5. Execute request

**Swagger provides:**
- Interactive API testing
- Request/response examples
- Schema definitions
- Authentication testing

---

## 11. Support & Contact

**For API Issues:**
- Check Swagger documentation first
- Verify token is valid and not expired
- Check request body matches exact format
- Review error messages carefully

**Common Issues:**
1. **401 Errors:** Token expired or invalid → refresh token
2. **400 Errors:** Request validation failed → check required fields
3. **404 Errors:** Resource not found → verify IDs are correct
4. **500 Errors:** Server error → contact backend team

---

**Document Version:** 1.0
**Last Updated:** January 14, 2026
**API Version:** 1.0

