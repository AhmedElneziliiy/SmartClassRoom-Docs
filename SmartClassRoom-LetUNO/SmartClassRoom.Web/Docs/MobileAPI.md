# SmartClassRoom Mobile API Documentation

## Base URL
```
Production: https://your-domain.com/api
Development: http://localhost:5000/api
```

## Authentication

All endpoints (except `/api/auth/login`) require JWT Bearer token authentication.

### Headers Required for Protected Endpoints
```
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

---

## 1. Authentication Endpoints

### 1.1 Login
Authenticate user and get JWT token.

**Endpoint:** `POST /api/auth/login`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "student100@smart.edu",
  "password": "Pass@123",
  "udid": "optional-device-unique-id"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| email | string | Yes | User's email address |
| password | string | Yes | User's password |
| udid | string | No | Device unique identifier (for mobile device binding) |

**Success Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "pC8o1aWjvkd6/7rNr/30NVQY6DZD880iBZnRD+0+NPH...",
  "expiresAt": "2026-01-19T08:36:34.2745252Z",
  "user": {
    "id": 6,
    "fullName": "Student 100",
    "email": "student100@smart.edu",
    "userType": "Student",
    "roles": ["Student"]
  }
}
```

**Error Responses:**

| Status | Response |
|--------|----------|
| 401 | `{"success": false, "message": "Invalid email or password"}` |
| 401 | `{"success": false, "message": "Account is locked. Please try again later."}` |
| 401 | `{"success": false, "message": "Device not authorized for this account. Please contact administrator."}` |

---

### 1.2 Refresh Token
Get new access token using refresh token.

**Endpoint:** `POST /api/auth/refresh-token`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "accessToken": "expired_access_token_here",
  "refreshToken": "your_refresh_token_here"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "accessToken": "new_access_token...",
  "refreshToken": "new_refresh_token...",
  "expiresAt": "2026-01-19T08:39:07.5257758Z",
  "user": {
    "id": 6,
    "fullName": "Student 100",
    "email": "student100@smart.edu",
    "userType": "Student",
    "roles": ["Student"]
  }
}
```

---

### 1.3 Change Password
Change password using current password verification.

**Endpoint:** `POST /api/auth/change-password`

**Headers:**
```
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "currentPassword": "Pass@123",
  "newPassword": "NewPass@123",
  "confirmPassword": "NewPass@123"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| currentPassword | string | Yes | Must match current password |
| newPassword | string | Yes | Min 6 characters |
| confirmPassword | string | Yes | Must match newPassword |

**Success Response (200):**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"success": false, "message": "Current password is incorrect"}` |
| 400 | `{"success": false, "message": "Failed to change password", "errors": ["..."]}` |
| 401 | `{"success": false, "message": "User not authenticated"}` |

---

### 1.4 Reset Password
Reset password to a new value (self-service).

**Endpoint:** `POST /api/auth/reset-password`

**Headers:**
```
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "newPassword": "NewPass@123",
  "confirmPassword": "NewPass@123"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| newPassword | string | Yes | Min 6 characters |
| confirmPassword | string | Yes | Must match newPassword |

**Success Response (200):**
```json
{
  "success": true,
  "message": "Password reset successfully"
}
```

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"success": false, "message": "Failed to reset password", "errors": ["..."]}` |
| 401 | `{"success": false, "message": "User not authenticated"}` |

---

## 2. Timetable Endpoints

### 2.1 Get Today's Schedule
Get the student's schedule for today.

**Endpoint:** `GET /api/timetables/today`

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Success Response (200):**
```json
{
  "studentId": 6,
  "studentName": "Student 100",
  "date": "2026-01-16T00:00:00+02:00",
  "dayName": "Friday",
  "classes": [
    {
      "sessionId": 15,
      "courseOfferingId": 1,
      "courseCode": "CS101",
      "courseName": "Introduction to Programming",
      "teacherName": "Dr. Teacher 1",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "roomName": "Lecture Hall 101",
      "roomBuilding": "Building A",
      "sessionStatus": "Scheduled",
      "attendanceStatus": null
    }
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| sessionId | int? | Session ID (null if no session created yet) |
| courseOfferingId | int | Course offering ID |
| courseCode | string | Course code |
| courseName | string | Course name |
| teacherName | string | Teacher's full name |
| startTime | string | Class start time (HH:mm:ss) |
| endTime | string | Class end time (HH:mm:ss) |
| roomName | string? | Room name |
| roomBuilding | string? | Building name |
| sessionStatus | string | "Scheduled", "InProgress", "Completed", "Cancelled" |
| attendanceStatus | string? | "Present", "Absent", "Late", "Excused", or null |

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"message": "This endpoint is for students only"}` |
| 401 | `{"message": "User not authenticated"}` |
| 404 | `{"message": "Student record not found"}` |

---

### 2.2 Get Weekly Schedule
Get the student's full weekly schedule.

**Endpoint:** `GET /api/timetables/week`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| termId | int | No | Term ID (defaults to current term) |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/timetables/week?termId=1`

**Success Response (200):**
```json
{
  "studentId": 6,
  "studentName": "Student 100",
  "termId": 1,
  "termName": "Fall 2025",
  "schedule": [
    {
      "sessionId": 0,
      "courseOfferingId": 1,
      "courseCode": "CS101",
      "courseName": "Introduction to Programming",
      "teacherName": "Dr. Teacher 1",
      "dayOfWeek": 0,
      "dayName": "Sunday",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "roomName": "Lecture Hall 101",
      "roomBuilding": "Building A"
    },
    {
      "sessionId": 0,
      "courseOfferingId": 1,
      "courseCode": "CS101",
      "courseName": "Introduction to Programming",
      "teacherName": "Dr. Teacher 1",
      "dayOfWeek": 2,
      "dayName": "Tuesday",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "roomName": "Lecture Hall 101",
      "roomBuilding": "Building A"
    }
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| dayOfWeek | int | 0=Sunday, 1=Monday, ..., 6=Saturday |
| dayName | string | Day name in English |

---

### 2.3 Get Schedule by Date
Get the student's schedule for a specific date.

**Endpoint:** `GET /api/timetables/date/{date}`

**Path Parameters:**

| Parameter | Type | Format | Description |
|-----------|------|--------|-------------|
| date | string | yyyy-MM-dd | The date to get schedule for |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/timetables/date/2026-01-20`

**Success Response (200):**
```json
{
  "studentId": 6,
  "studentName": "Student 100",
  "date": "2026-01-20T00:00:00",
  "dayName": "Tuesday",
  "classes": [
    {
      "sessionId": 20,
      "courseOfferingId": 1,
      "courseCode": "CS101",
      "courseName": "Introduction to Programming",
      "teacherName": "Dr. Teacher 1",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "roomName": "Lecture Hall 101",
      "roomBuilding": "Building A",
      "sessionStatus": "Scheduled",
      "attendanceStatus": null
    }
  ]
}
```

---

## 3. Sessions Endpoints

### 3.1 Get Session Details
Get detailed information about a specific session.

**Endpoint:** `GET /api/sessions/{id}`

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Session ID |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/sessions/15`

**Success Response (200):**
```json
{
  "id": 15,
  "courseOfferingId": 1,
  "courseCode": "CS101",
  "courseName": "Introduction to Programming",
  "teacherName": "Dr. Teacher 1",
  "sessionDate": "2026-01-16T00:00:00",
  "startTime": "08:00:00",
  "endTime": "09:30:00",
  "sessionType": "Lecture",
  "status": "Scheduled",
  "startedAt": null,
  "endedAt": null,
  "room": {
    "id": 1,
    "name": "Lecture Hall 101",
    "building": "Building A",
    "capacity": 50
  },
  "attendanceCount": 25
}
```

| Field | Type | Description |
|-------|------|-------------|
| sessionType | string | "Lecture", "Lab", "Tutorial", "Regular" |
| status | string | "Scheduled", "InProgress", "Completed", "Cancelled" |
| startedAt | datetime? | When session actually started |
| endedAt | datetime? | When session actually ended |
| attendanceCount | int | Number of attendance records |

**Error Responses:**

| Status | Response |
|--------|----------|
| 404 | `{"message": "Session not found"}` |

---

### 3.2 Get Sessions by Course Offering
Get all sessions for a specific course offering.

**Endpoint:** `GET /api/sessions/offering/{offeringId}`

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| offeringId | int | Course offering ID |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/sessions/offering/1`

**Success Response (200):**
```json
[
  {
    "id": 15,
    "sessionDate": "2026-01-16T00:00:00",
    "startTime": "08:00:00",
    "endTime": "09:30:00",
    "sessionType": "Lecture",
    "status": "Completed",
    "roomName": "Lecture Hall 101"
  },
  {
    "id": 16,
    "sessionDate": "2026-01-18T00:00:00",
    "startTime": "08:00:00",
    "endTime": "09:30:00",
    "sessionType": "Lecture",
    "status": "Scheduled",
    "roomName": "Lecture Hall 101"
  }
]
```

---

### 3.3 Get Today's Sessions
Get all sessions for today (for student or teacher).

**Endpoint:** `GET /api/sessions/today`

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Success Response (200):**
```json
[
  {
    "id": 15,
    "courseOfferingId": 1,
    "sessionDate": "2026-01-16T00:00:00",
    "startTime": "08:00:00",
    "endTime": "09:30:00",
    "sessionType": "Lecture",
    "status": "Scheduled",
    "roomName": "Lecture Hall 101"
  }
]
```

**Note:** This endpoint works for both students and teachers. For students, it returns sessions from their enrolled courses. For teachers, it returns their teaching sessions.

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"message": "Invalid user type"}` |
| 401 | `{"message": "User not authenticated"}` |
| 404 | `{"message": "Student record not found"}` |
| 404 | `{"message": "Teacher record not found"}` |

---

## 4. Attendance Endpoints

### 4.1 Get My Courses (with Attendance Summary)
Get list of enrolled courses with attendance statistics.

**Endpoint:** `GET /api/attendance/my-courses`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| termId | int | No | Term ID (defaults to current term) |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/attendance/my-courses?termId=1`

**Success Response (200):**
```json
[
  {
    "courseOfferingId": 1,
    "courseCode": "CS101",
    "courseName": "Introduction to Programming",
    "termName": "Fall 2025",
    "teacherName": "Dr. Teacher 1",
    "totalSessions": 20,
    "attendedSessions": 18,
    "attendancePercentage": 90.0
  },
  {
    "courseOfferingId": 2,
    "courseCode": "CS102",
    "courseName": "Data Structures",
    "termName": "Fall 2025",
    "teacherName": "Dr. Teacher 2",
    "totalSessions": 15,
    "attendedSessions": 14,
    "attendancePercentage": 93.33
  }
]
```

| Field | Type | Description |
|-------|------|-------------|
| attendedSessions | int | Present + Late count |
| attendancePercentage | decimal | Percentage (0-100) |

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"message": "This endpoint is for students only"}` |
| 401 | `{"message": "User not authenticated"}` |
| 404 | `{"message": "Student record not found"}` |

---

### 4.2 Get Course Attendance Report
Get detailed attendance report for a specific course.

**Endpoint:** `GET /api/attendance/course/{offeringId}`

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| offeringId | int | Course offering ID |

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Example:** `GET /api/attendance/course/1`

**Success Response (200):**
```json
{
  "studentId": 6,
  "studentName": "Student 100",
  "courseOfferingId": 1,
  "courseCode": "CS101",
  "courseName": "Introduction to Programming",
  "termName": "Fall 2025",
  "totalSessions": 20,
  "presentCount": 15,
  "absentCount": 2,
  "lateCount": 3,
  "excusedCount": 0,
  "attendancePercentage": 90.0,
  "records": [
    {
      "sessionId": 20,
      "sessionDate": "2026-01-16T00:00:00",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "status": "Present",
      "checkInTime": "2026-01-16T08:05:00",
      "checkOutTime": "2026-01-16T09:25:00",
      "notes": null
    },
    {
      "sessionId": 19,
      "sessionDate": "2026-01-14T00:00:00",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "status": "Late",
      "checkInTime": "2026-01-14T08:20:00",
      "checkOutTime": "2026-01-14T09:30:00",
      "notes": "Arrived 20 minutes late"
    }
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| status | string | "Present", "Absent", "Late", "Excused" |
| checkInTime | datetime? | When student checked in |
| checkOutTime | datetime? | When student checked out |
| notes | string? | Additional notes |

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"message": "This endpoint is for students only"}` |
| 401 | `{"message": "User not authenticated"}` |
| 403 | `{"message": "You are not enrolled in this course"}` |
| 404 | `{"message": "Student record not found"}` |
| 404 | `{"message": "Course offering not found"}` |

---

### 4.3 Get Attendance Summary
Get overall attendance summary across all courses.

**Endpoint:** `GET /api/attendance/summary`

**Headers:**
```
Authorization: Bearer <your_jwt_token>
```

**Success Response (200):**
```json
{
  "studentId": 6,
  "studentName": "Student 100",
  "totalSessions": 50,
  "presentCount": 40,
  "absentCount": 5,
  "lateCount": 5,
  "excusedCount": 0,
  "attendancePercentage": 90.0
}
```

**Error Responses:**

| Status | Response |
|--------|----------|
| 400 | `{"message": "This endpoint is for students only"}` |
| 401 | `{"message": "User not authenticated"}` |
| 404 | `{"message": "Student record not found"}` |

---

## Error Handling

### Common HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 400 | Bad Request - Invalid input data |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error |

### Token Expiration

- Access tokens expire after **3 days**
- When token expires, use the refresh token endpoint to get a new access token
- If refresh fails, redirect user to login

### Error Response Format

All errors follow this format:
```json
{
  "message": "Error description here"
}
```

Or for validation errors:
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Error 1", "Error 2"]
}
```

---

## Implementation Notes

### Date/Time Handling
- All dates are in ISO 8601 format
- Times are in 24-hour format (HH:mm:ss)
- Server timezone should be considered when displaying times

### Caching Recommendations
- Cache weekly schedule (refresh daily or on pull-to-refresh)
- Cache course list (refresh on app launch)
- Don't cache today's schedule for long (refresh every few minutes)

### Offline Support
- Store last fetched schedule locally
- Queue password changes for when online
- Show cached attendance data with "last updated" timestamp

---

## Sample Code (Dart/Flutter)

### HTTP Client Setup
```dart
class ApiClient {
  static const String baseUrl = 'http://localhost:5000/api';
  String? _accessToken;

  Map<String, String> get _headers => {
    'Content-Type': 'application/json',
    if (_accessToken != null) 'Authorization': 'Bearer $_accessToken',
  };

  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    final data = jsonDecode(response.body);
    if (data['success'] == true) {
      _accessToken = data['accessToken'];
    }
    return data;
  }

  Future<Map<String, dynamic>> getTodaySchedule() async {
    final response = await http.get(
      Uri.parse('$baseUrl/timetables/today'),
      headers: _headers,
    );
    return jsonDecode(response.body);
  }
}
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-16 | Initial release with auth, timetables, sessions, and attendance endpoints |
