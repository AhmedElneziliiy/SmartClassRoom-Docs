# Grading System Testing Checklist

## Quick Start Guide

### Step 1: Verify Prerequisites (5 minutes)

Run the SQL script to check your data:
```sql
-- Open SSMS and run: GRADING_SYSTEM_TEST_DATA.sql
```

**Required Results:**
- [ ] ✅ At least 1 Active Term exists
- [ ] ✅ At least 1 Active Course Offering exists
- [ ] ✅ At least 3-5 students enrolled in that offering
- [ ] ✅ Teacher assigned to the offering
- [ ] ✅ Course has Credits > 0

**If Missing Data:**
1. Create Active Term: `/Admin/Terms/Create`
2. Create Course: `/Admin/Courses/Create`
3. Create Course Offering: `/Admin/CourseOfferings/Create`
4. Enroll Students: From offering details, click "Add Students" or "Auto-Enroll"

---

### Step 2: Start Application (2 minutes)

```bash
cd "c:\Users\Kareem Usama\Desktop\SMARTCLASSROOMDOCS\SmartClassRoom-LetUNO\SmartClassRoom.Web"
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Application started. Press Ctrl+C to shut down.
```

**Browser URL:** `https://localhost:5001`

---

### Step 3: Login (1 minute)

- [ ] Navigate to `/Admin/Login`
- [ ] Enter Admin or Teacher credentials
- [ ] Verify dashboard loads successfully

---

### Step 4: Access Grading System (1 minute)

**Test all 3 entry points:**

#### Option A: Sidebar
- [ ] Click "Grades" in left sidebar
- [ ] URL changes to `/Admin/Grades/Index`
- [ ] See list of course offerings

#### Option B: Course Offering Details
- [ ] Navigate to `/Admin/CourseOfferings`
- [ ] Click "Details" on any offering
- [ ] See "Configure Grades" or "Manage Grades" button at top

#### Option C: Direct Navigation
- [ ] From Grades Index, click on offering card
- [ ] See action buttons (Configure or Grade Sheet)

---

### Step 5: Configure Grade Components (5 minutes)

- [ ] Click "Configure Grades" button
- [ ] URL: `/Admin/Grades/ConfigureComponents/{id}`

**Add Components:**
- [ ] Click "+ Add Component"
- [ ] Component 1: Midterm Exam, 30%, Max: 100
- [ ] Component 2: Final Exam, 40%, Max: 100
- [ ] Component 3: Assignments, 20%, Max: 100
- [ ] Component 4: Attendance, 10%, Max: 100

**Validation:**
- [ ] Total Weight shows "100.00%" in GREEN
- [ ] "Save Components" button is ENABLED
- [ ] Click "Save Components"
- [ ] Success message appears
- [ ] Redirects to Grade Sheet

**Common Issues:**
- ❌ Total weight = 99% → Button disabled (adjust weights)
- ❌ Duplicate names → Error message (rename components)

---

### Step 6: Enter Individual Grades (10 minutes)

- [ ] You should be on Grade Sheet page (`/Admin/Grades/GradeSheet/{id}`)
- [ ] See Excel-like table with students and components

**For Each Student (at least 3):**

Student 1:
- [ ] Midterm: `85`
- [ ] Final: `90`
- [ ] Assignments: `88`
- [ ] Attendance: `95`
- [ ] Each input turns yellow (saving), then green (saved)

Student 2:
- [ ] Midterm: `75`
- [ ] Final: `78`
- [ ] Assignments: `80`
- [ ] Attendance: `90`

Student 3:
- [ ] Midterm: `92`
- [ ] Final: `88`
- [ ] Assignments: `85`
- [ ] Attendance: `100`

**Validation:**
- [ ] Each save shows green border (success)
- [ ] Refresh page → grades persist
- [ ] Try invalid score (e.g., 105) → Error message

---

### Step 7: Calculate Final Grades (3 minutes)

- [ ] Click "Calculate Final Grades" button (green button at top)
- [ ] Confirmation dialog appears
- [ ] Click "Yes" or "Confirm"
- [ ] Processing message
- [ ] Page reloads

**Expected Results:**

Student 1 (85, 90, 88, 95):
- [ ] Final Grade: `88.60`
- [ ] Letter Grade: `B+` (green badge)

Student 2 (75, 78, 80, 90):
- [ ] Final Grade: `79.10`
- [ ] Letter Grade: `C+` (blue badge)

Student 3 (92, 88, 85, 100):
- [ ] Final Grade: `89.60`
- [ ] Letter Grade: `B+` (green badge)

**Calculation Verification:**
```
Student 1:
  Midterm: 85/100 × 30% = 25.5
  Final: 90/100 × 40% = 36.0
  Assignments: 88/100 × 20% = 17.6
  Attendance: 95/100 × 10% = 9.5
  Total = 88.6% → B+ (85-89%)
```

---

### Step 8: Verify GPA Calculation (5 minutes)

**Check in Database:**
```sql
-- Student's cumulative GPA should be updated
SELECT Id, Name, Email, CurrentGPA
FROM Users
WHERE Id IN (SELECT StudentId FROM StudentEnrollments);
```

**Expected:**
- [ ] `CurrentGPA` field updated for enrolled students
- [ ] GPA calculated as: (GradePoints × Credits) / TotalCredits

**Test API (if Student API implemented):**
- [ ] Login as student
- [ ] Call: `GET /api/Grades/gpa`
- [ ] Response includes: `cumulativeGPA`, `totalCredits`, `termGPA`

---

### Step 9: Verify Course Offering Details (2 minutes)

- [ ] Navigate back to Course Offering Details
- [ ] Check "Enrolled Students" table
- [ ] Grade column shows:
  - [ ] Final grade badge (blue)
  - [ ] Letter grade badge (green)
- [ ] Top button changed from "Configure Grades" to "Manage Grades"

---

### Step 10: Test Templates (Optional, 5 minutes)

**Create Template:**
- [ ] Navigate to `/Admin/Grades/Templates`
- [ ] Click "Create Template"
- [ ] Enter template name: "Standard Exam Structure"
- [ ] Add same 4 components
- [ ] Save template

**Apply Template:**
- [ ] Create a new course offering (different course)
- [ ] Go to Configure Components
- [ ] Click "Load Template"
- [ ] Select "Standard Exam Structure"
- [ ] Components auto-populated
- [ ] Save components

---

### Step 11: Generate Transcript PDF (Optional, 3 minutes)

**Prerequisites:**
- [ ] Student must have at least 1 completed course

**Test:**
- [ ] Login as student (or use API tool like Postman)
- [ ] Call: `GET /api/Grades/transcript`
- [ ] PDF file downloads
- [ ] Open PDF and verify:
  - [ ] Header: "Official Academic Transcript"
  - [ ] Student name, code, email
  - [ ] Course list with grades
  - [ ] Cumulative GPA at bottom
  - [ ] Generation date

---

## Edge Cases to Test

### Test Invalid Data:
- [ ] Enter grade > MaxScore (e.g., 105/100) → Error
- [ ] Enter negative grade → Error
- [ ] Save components with total ≠ 100% → Button disabled
- [ ] Calculate final with missing grades → Warning message

### Test Workflow Issues:
- [ ] Access Grade Sheet before configuring components → Info message
- [ ] Refresh during grade entry → Data persists
- [ ] Multiple teachers entering grades → No conflicts

### Test Role-Based Access:
- [ ] Teacher sees only their offerings (not all)
- [ ] Admin sees all offerings
- [ ] Student can only access their grades via API

---

## Performance Checks

- [ ] Grade Sheet loads in < 3 seconds with 50 students
- [ ] AJAX save completes in < 1 second
- [ ] Final grade calculation completes in < 5 seconds
- [ ] PDF generation completes in < 3 seconds
- [ ] No JavaScript errors in browser console

---

## Success Criteria

### ✅ Phase 1: Setup
- All prerequisites exist (term, offering, enrollments)
- Application runs without errors
- Can login and access dashboard

### ✅ Phase 2: Component Configuration
- Can add/edit/delete components
- Weight validation works (must = 100%)
- Components save successfully

### ✅ Phase 3: Grade Entry
- Can enter grades for all students
- AJAX save works (green border feedback)
- Grades persist after refresh

### ✅ Phase 4: Calculation
- Final grades calculate correctly
- Letter grades assigned per scale
- GPA updates in database

### ✅ Phase 5: Display
- Grades show in Course Offering Details
- Transcript PDF generates correctly
- All badges and formatting correct

---

## Troubleshooting

### Issue: "No course offerings found"
**Solution:** Create an active term and course offering first

### Issue: "No grade components configured"
**Solution:** Configure components before accessing grade sheet

### Issue: Total weight = 99.99%
**Solution:** Adjust weights to exactly 100.00 (use 2 decimal places)

### Issue: AJAX save fails
**Check:**
- Browser console for JavaScript errors
- Network tab for 400/500 errors
- AntiForgeryToken is present in form

### Issue: Final calculation fails
**Check:**
- All students have grades in ALL components
- Component weights sum to 100%
- No null scores

### Issue: PDF generation fails
**Check:**
- QuestPDF NuGet package installed
- Student has at least 1 completed course
- No null values in transcript data

---

## Time Estimates

| Phase | Time |
|-------|------|
| Prerequisites Setup | 5 min |
| Configure Components | 5 min |
| Enter Grades (5 students) | 10 min |
| Calculate Finals | 2 min |
| Verify Results | 5 min |
| Test Templates | 5 min |
| Test Transcript | 3 min |
| **Total** | **~35 min** |

---

## Final Verification SQL

Run this at the end to verify everything:

```sql
-- Final Grade Summary
SELECT
    c.Code AS Course,
    s.Name AS Student,
    se.FinalGrade,
    se.LetterGrade,
    se.Status,
    (SELECT COUNT(*) FROM Grades WHERE StudentId = se.StudentId AND CourseOfferingId = co.Id) AS GradeCount,
    (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) AS ComponentCount
FROM StudentEnrollments se
JOIN CourseOfferings co ON se.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
JOIN Users s ON se.StudentId = s.Id
WHERE co.Status = 'Active'
ORDER BY c.Code, s.Name;

-- Student GPA Summary
SELECT
    s.Id,
    s.Name,
    s.StudentCode,
    s.CurrentGPA,
    COUNT(se.Id) AS CompletedCourses,
    SUM(c.Credits) AS TotalCredits
FROM Users s
JOIN StudentEnrollments se ON se.StudentId = s.Id
JOIN CourseOfferings co ON se.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
WHERE se.Status = 'Completed' AND se.FinalGrade IS NOT NULL
GROUP BY s.Id, s.Name, s.StudentCode, s.CurrentGPA;
```

**Expected:**
- All students have `FinalGrade` and `LetterGrade`
- `GradeCount` = `ComponentCount` for all students
- `Status` = "Completed" for calculated students
- `CurrentGPA` > 0 for students with completed courses

---

## Report Template

After testing, fill this out:

```
Testing Date: __________
Tested By: __________
Environment: Local / Dev / Staging / Production

✅ PASSED / ❌ FAILED

Phases Tested:
- [ ] Component Configuration
- [ ] Grade Entry
- [ ] Final Calculation
- [ ] GPA Calculation
- [ ] Transcript Generation
- [ ] Templates

Issues Found:
1. __________
2. __________

Recommendations:
1. __________
2. __________

Overall Status: ✅ PASS / ❌ FAIL / ⚠️ NEEDS WORK
```
