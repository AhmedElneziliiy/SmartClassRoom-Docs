# SmartClassRoom

A full-featured **ASP.NET Core MVC + REST API** platform for managing smart classrooms in educational institutions — covering courses, student attendance (with face verification), quizzes, grades, timetables, and a mobile API for student apps.

## What it does

SmartClassRoom digitalizes the academic workflow from course creation to final grading. Instructors manage sessions and record attendance; students check in via face recognition. The system supports quizzes with automatic grading, course material uploads, and rich admin controls for managing departments, levels, groups, and academic terms.

## Key Features

- **Face-based Attendance** — Students check in to live sessions using face verification; the system logs attendance status (Present / Late / Absent)
- **Course & Section Management** — Organize courses into sections, assign instructors, enroll students
- **Session Management** — Instructors open/close live sessions; attendance is only recorded during an active session
- **Quizzes & Auto-grading** — Create quizzes per course offering; students take them online and receive instant results
- **Grades Management** — Record and manage student grades per course with grading history
- **Material Upload** — Upload course materials (PDFs, files) accessible to enrolled students
- **Academic Structure** — Full hierarchy: Departments → Levels → Terms → Groups → Course Offerings → Sections
- **Student Mobile API** — Dedicated API endpoints for a companion mobile application
- **Admin Dashboard** — Manage all entities from a centralized web interface
- **Reports** — Attendance and performance reports per course, group, or student

## Tech Stack

- ASP.NET Core 8 (MVC + Web API)
- Entity Framework Core + SQL Server
- ASP.NET Identity + JWT Bearer (API) + Cookie Auth (MVC)
- Face Recognition integration (external API)
- Swagger / OpenAPI

## Project Structure

```
SmartClassRoom.Web/
  Areas/
    Admin/Controllers/    → Admin management controllers
      AttendancesController, CoursesController, QuizzesController,
      GradesController, SessionsController, StudentsController, ...
    Student/              → Student-facing MVC views
  Controllers/API/        → Mobile REST API endpoints
  Views/                  → Razor views for the web dashboard
  wwwroot/                → Static assets (CSS, JS, uploads)
```

## Core Entities

| Entity | Description |
|---|---|
| `Department` | Academic department |
| `Level` | Year/level within a department |
| `Term` | Academic semester/term |
| `Group` | Student group within a level |
| `Course` | A subject or module |
| `CourseOffering` | A course run in a specific term with an instructor |
| `Section` | A subdivison of a course offering |
| `Session` | A live class session (attendance window) |
| `Attendance` | Student presence record per session |
| `Quiz` | Assessment linked to a course offering |
| `Grade` | Final grade for a student in a course |

## Getting Started

1. Set the SQL Server connection string in `appsettings.json`
2. Configure JWT and face recognition API settings
3. Run: `dotnet ef database update`
4. Run the project and log in as Admin to begin setting up departments, courses, and users
5. Access the API docs at `/swagger`
