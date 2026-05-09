# Mobile Attendance API Documentation

## Base URL
```
/api/mobile/attendance
```

## Authentication
All endpoints require JWT Bearer authentication.

### Headers (Required for all endpoints)
| Header | Value | Description |
|--------|-------|-------------|
| `Authorization` | `Bearer {token}` | JWT access token obtained from `/api/auth/login` |
| `Content-Type` | `application/json` | Required for POST requests |

---

## Endpoints

### 1. Student Check-In

**POST** `/api/mobile/attendance/check-in`

Allows a student to check into an active session.

#### Request Body
```json
{
  "udid": "string",           // Required: Device unique identifier (must match user's registered UDID)
  "sessionId": 0,             // Required: The session ID to check into
  "deviceIds": ["string"],    // Required: List of ESP device IDs detected in range
  "latitude": 0.0,            // Optional: GPS latitude
  "longitude": 0.0            // Optional: GPS longitude
}
```

#### Request Example
```json
{
  "udid": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
  "sessionId": 15,
  "deviceIds": ["ESP32-001", "ESP32-002"],
  "latitude": 30.0444,
  "longitude": 31.2357
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Check-in successful.",
  "errorCode": null,
  "data": {
    "attendanceId": 123,
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "courseName": "Database Systems",
    "courseCode": "CS301",
    "checkInTime": "2024-01-15T09:05:00Z",
    "status": "Present",
    "isLate": false,
    "roomName": "Lab A-101",
    "roomNumber": "A-101",
    "sessionStartTime": "09:00:00",
    "sessionEndTime": "10:30:00"
  }
}
```

#### Late Check-In Response (200 OK)
```json
{
  "success": true,
  "message": "Check-in successful. You are marked as late (arrived after 15 minutes grace period).",
  "errorCode": null,
  "data": {
    "attendanceId": 124,
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "courseName": "Database Systems",
    "courseCode": "CS301",
    "checkInTime": "2024-01-15T09:20:00Z",
    "status": "Late",
    "isLate": true,
    "roomName": "Lab A-101",
    "roomNumber": "A-101",
    "sessionStartTime": "09:00:00",
    "sessionEndTime": "10:30:00"
  }
}
```

#### Error Responses

| HTTP Code | Error Code | Message |
|-----------|------------|---------|
| 400 | `INVALID_REQUEST` | Invalid request data. |
| 400 | `SESSION_NOT_FOUND` | Session not found. |
| 400 | `SESSION_NOT_STARTED` | Session has not been started by the teacher yet. |
| 400 | `DEVICE_NOT_IN_RANGE` | You are not within range of the classroom device. |
| 400 | `ROOM_NO_DEVICE` | Room has no registered device for proximity check. |
| 401 | `NOT_AUTHENTICATED` | User not authenticated. |
| 403 | `UDID_MISMATCH` | Device not authorized. UDID does not match. |
| 403 | `NOT_ENROLLED` | You are not enrolled in this course. |
| 403 | `NOT_A_STUDENT` | Only students can check in to sessions. |
| 409 | `ALREADY_CHECKED_IN` | You are already checked in to this session. |
| 409 | `ALREADY_CHECKED_OUT` | You have already checked out. Re-entry is not allowed. |

#### Error Response Example
```json
{
  "success": false,
  "message": "Session has not been started by the teacher yet.",
  "errorCode": "SESSION_NOT_STARTED",
  "data": null
}
```

---

### 2. Student Check-Out

**POST** `/api/mobile/attendance/check-out`

Allows a student to check out from a session they are checked into.

#### Request Body
```json
{
  "udid": "string",           // Required: Device unique identifier
  "sessionId": 0,             // Required: The session ID to check out from
  "deviceIds": ["string"]     // Optional: ESP device IDs (only if proximity required for checkout)
}
```

#### Request Example
```json
{
  "udid": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
  "sessionId": 15
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Check-out successful.",
  "errorCode": null,
  "data": {
    "attendanceId": 123,
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "courseName": "Database Systems",
    "courseCode": "CS301",
    "checkInTime": "2024-01-15T09:05:00Z",
    "checkOutTime": "2024-01-15T10:30:00Z",
    "duration": "1h 25m",
    "status": "Present"
  }
}
```

#### Error Responses

| HTTP Code | Error Code | Message |
|-----------|------------|---------|
| 400 | `INVALID_REQUEST` | Invalid request data. |
| 400 | `NOT_CHECKED_IN` | You have not checked in to this session. |
| 400 | `DEVICE_NOT_IN_RANGE` | You are not within range of the classroom device. |
| 401 | `NOT_AUTHENTICATED` | User not authenticated. |
| 403 | `UDID_MISMATCH` | Device not authorized. UDID does not match. |
| 409 | `ALREADY_CHECKED_OUT` | You have already checked out from this session. |

---

### 3. Get Active Sessions (Student)

**GET** `/api/mobile/attendance/active-sessions`

Returns all active/upcoming sessions for the authenticated student based on their enrollments.

#### Request
No request body required.

#### Success Response (200 OK)
```json
{
  "success": true,
  "data": [
    {
      "sessionId": 15,
      "courseOfferingId": 5,
      "courseCode": "CS301",
      "courseName": "Database Systems",
      "teacherName": "",
      "sessionDate": "2024-01-15",
      "startTime": "09:00:00",
      "endTime": "10:30:00",
      "roomName": "Lab A-101",
      "roomNumber": null,
      "roomDeviceId": "ESP32-001",
      "status": "InProgress",
      "isStarted": true,
      "canCheckIn": true,
      "hasCheckedIn": false,
      "hasCheckedOut": false,
      "attendanceStatus": null,
      "checkInTime": null,
      "checkOutTime": null,
      "enrolledCount": 0,
      "checkedInCount": 0
    },
    {
      "sessionId": 16,
      "courseOfferingId": 6,
      "courseCode": "CS302",
      "courseName": "Software Engineering",
      "teacherName": "",
      "sessionDate": "2024-01-15",
      "startTime": "11:00:00",
      "endTime": "12:30:00",
      "roomName": "Room B-205",
      "roomNumber": null,
      "roomDeviceId": "ESP32-002",
      "status": "Scheduled",
      "isStarted": false,
      "canCheckIn": false,
      "hasCheckedIn": false,
      "hasCheckedOut": false,
      "attendanceStatus": null,
      "checkInTime": null,
      "checkOutTime": null,
      "enrolledCount": 0,
      "checkedInCount": 0
    }
  ]
}
```

---

### 4. Get Attendance Status

**GET** `/api/mobile/attendance/status/{sessionId}`

Returns the current attendance status for a specific session.

#### URL Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| `sessionId` | int | The session ID |

#### Success Response - Checked In (200 OK)
```json
{
  "success": true,
  "data": {
    "attendanceId": 123,
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "courseName": "Database Systems",
    "status": "Present",
    "checkInTime": "2024-01-15T09:05:00Z",
    "checkOutTime": null,
    "isCheckedOut": false,
    "duration": null
  }
}
```

#### Success Response - Not Checked In (200 OK)
```json
{
  "success": true,
  "message": "No attendance record found for this session.",
  "data": null
}
```

---

### 5. Start Session (Teacher/Admin)

**POST** `/api/mobile/attendance/session/start`

Starts a session, allowing students to check in. Only the assigned teacher or an admin can start the session.

#### Request Body
```json
{
  "sessionId": 0,      // Required: The session ID to start
  "udid": "string"     // Optional: Device UDID for verification
}
```

#### Request Example
```json
{
  "sessionId": 15,
  "udid": "TEACHER-DEVICE-UDID-12345"
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Session started successfully. Students can now check in.",
  "errorCode": null,
  "data": {
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "status": "InProgress",
    "startedAt": "2024-01-15T09:00:00Z",
    "endedAt": null,
    "courseName": "Database Systems",
    "courseCode": "",
    "roomName": "Lab A-101",
    "roomNumber": "",
    "enrolledCount": 35,
    "checkedInCount": 0
  }
}
```

#### Error Responses

| HTTP Code | Error Code | Message |
|-----------|------------|---------|
| 400 | `INVALID_REQUEST` | Invalid request data. |
| 400 | `SESSION_NOT_FOUND` | Session not found. |
| 401 | `NOT_AUTHENTICATED` | User not authenticated. |
| 403 | `UDID_MISMATCH` | Device not authorized. UDID does not match. |
| 403 | `NOT_AUTHORIZED` | You are not authorized to manage this session. |
| 409 | `INVALID_STATUS` | Session is already in progress. |
| 409 | `INVALID_STATUS` | Session has already been completed. |
| 409 | `INVALID_STATUS` | Session has been cancelled. |

---

### 6. End Session (Teacher/Admin)

**POST** `/api/mobile/attendance/session/end`

Ends an active session. Only the assigned teacher or an admin can end the session.

#### Request Body
```json
{
  "sessionId": 0,      // Required: The session ID to end
  "udid": "string"     // Optional: Device UDID for verification
}
```

#### Request Example
```json
{
  "sessionId": 15,
  "udid": "TEACHER-DEVICE-UDID-12345"
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Session ended successfully.",
  "errorCode": null,
  "data": {
    "sessionId": 15,
    "sessionName": "Week 5 - Database Design",
    "status": "Completed",
    "startedAt": "2024-01-15T09:00:00Z",
    "endedAt": "2024-01-15T10:35:00Z",
    "courseName": "Database Systems",
    "courseCode": "",
    "roomName": "Lab A-101",
    "roomNumber": "",
    "enrolledCount": 35,
    "checkedInCount": 32
  }
}
```

#### Error Responses

| HTTP Code | Error Code | Message |
|-----------|------------|---------|
| 400 | `INVALID_REQUEST` | Invalid request data. |
| 400 | `SESSION_NOT_FOUND` | Session not found. |
| 401 | `NOT_AUTHENTICATED` | User not authenticated. |
| 403 | `UDID_MISMATCH` | Device not authorized. UDID does not match. |
| 403 | `NOT_AUTHORIZED` | You are not authorized to manage this session. |
| 409 | `INVALID_STATUS` | Session is not in progress. Current status: {status} |

---

### 7. Get Teacher Sessions

**GET** `/api/mobile/attendance/teacher/sessions`

Returns all sessions that the authenticated teacher can manage (start/end).

#### Request
No request body required.

#### Success Response (200 OK)
```json
{
  "success": true,
  "data": [
    {
      "sessionId": 15,
      "courseOfferingId": 0,
      "courseCode": "CS301",
      "courseName": "Database Systems",
      "teacherName": "",
      "sessionDate": "2024-01-15",
      "startTime": "09:00:00",
      "endTime": "10:30:00",
      "roomName": "Lab A-101",
      "roomNumber": null,
      "roomDeviceId": "ESP32-001",
      "status": "Scheduled",
      "isStarted": false,
      "canCheckIn": false,
      "hasCheckedIn": false,
      "hasCheckedOut": false,
      "attendanceStatus": null,
      "checkInTime": null,
      "checkOutTime": null,
      "enrolledCount": 35,
      "checkedInCount": 0
    }
  ]
}
```

---

## Configuration Settings

The attendance system uses the following configurable settings (from `appsettings.json`):

```json
{
  "AttendanceSettings": {
    "LateGracePeriodMinutes": 15,
    "EarlyCheckInMinutes": 10,
    "RequireDeviceProximity": true,
    "RequireDeviceProximityForCheckout": false
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `LateGracePeriodMinutes` | 15 | Minutes after session start before marking as "Late" |
| `EarlyCheckInMinutes` | 10 | Minutes before session start that check-in is allowed |
| `RequireDeviceProximity` | true | Require ESP device detection for check-in |
| `RequireDeviceProximityForCheckout` | false | Require ESP device detection for check-out |

---

## Workflow

### Student Check-In Flow
1. **Login** → `POST /api/auth/login` (get JWT token)
2. **Get Sessions** → `GET /api/mobile/attendance/active-sessions`
3. **Wait for teacher** to start session (status must be "InProgress")
4. **Check-In** → `POST /api/mobile/attendance/check-in`
5. **Attend session**
6. **Check-Out** → `POST /api/mobile/attendance/check-out`

### Teacher Session Management Flow
1. **Login** → `POST /api/auth/login` (get JWT token)
2. **Get Sessions** → `GET /api/mobile/attendance/teacher/sessions`
3. **Start Session** → `POST /api/mobile/attendance/session/start`
4. **Monitor attendance**
5. **End Session** → `POST /api/mobile/attendance/session/end`

---

## Session Status Values

| Status | Description |
|--------|-------------|
| `Scheduled` | Session created but not started |
| `InProgress` | Session started by teacher, students can check in |
| `Completed` | Session ended |
| `Cancelled` | Session cancelled |

## Attendance Status Values

| Status | Description |
|--------|-------------|
| `Present` | Checked in within grace period |
| `Late` | Checked in after grace period |
| `Absent` | No check-in record |
| `Excused` | Manually marked as excused |
