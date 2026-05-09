# Grading System Testing Guide

## Prerequisites & Constraints

### Required Data Setup (Must exist before testing)

1. **Users**
   - At least one Admin user
   - At least one Teacher user
   - At least 3-5 Student users

2. **Academic Structure**
   - At least one Department
   - At least one Level (e.g., Year 1, Year 2)
   - At least one Section (optional but recommended)
   - At least one Term with:
     - Start Date (should be current or recent)
     - End Date (should be in future)
     - Status: Active

3. **Course Setup**
   - At least one Course with:
     - Course Code (e.g., "CS101")
     - Course Name
     - Credits (e.g., 3)
     - Department assigned

4. **Course Offering**
   - At least one Course Offering with:
     - Course assigned
     - Term assigned (Active term)
     - Teacher assigned
     - MaxStudents set (e.g., 30)
     - Status: "Active"
     - Sessions scheduled (optional)

5. **Student Enrollments**
   - At least 3-5 students enrolled in the course offering
   - Enrollment Status: "Enrolled"

### Database Constraints

- **GradeComponent.Weight**: Must sum to exactly 100% per course offering
- **Grade.Score**: Must be between 0 and GradeComponent.MaxScore
- **StudentEnrollment.Status**: Must be "Enrolled" to enter grades
- **CourseOffering.Status**: Should be "Active" for grade entry

---

## Testing Flow - Step by Step

### **Phase 1: Initial Setup & Data Verification**

#### Step 1.1: Login as Admin or Teacher
```
URL: https://localhost:5001/Admin/Login
Credentials: Use your admin or teacher account
Expected: Successful login, redirect to dashboard
```

#### Step 1.2: Verify Course Offering Exists
```
URL: https://localhost:5001/Admin/CourseOfferings
Expected: See list of course offerings
Action: Click "Details" on any active offering
Expected: See offering details page with statistics
```

#### Step 1.3: Check Student Enrollments
```
On Course Offering Details page:
Expected: See "Enrolled Students" section with at least 3-5 students
Constraint: If no students enrolled, use "Auto-Enroll" or "Add Students" button first
```

---

### **Phase 2: Access Grading System (3 Entry Points)**

#### Option A: Via Sidebar
```
Action: Click "Grades" in left sidebar under "Assessment" section
URL: /Admin/Grades/Index
Expected: See list of all course offerings you can manage
Result: Cards showing each offering with:
  - Course code and name
  - Term, teacher, enrolled count
  - Component status (configured/not configured)
  - Action buttons (Configure or Grade Sheet)
```

#### Option B: Via Course Offering Details
```
Action: On Course Offering Details page, look at top-right action buttons
Expected: See one of these buttons:
  - "Configure Grades" (orange) - if no components configured
  - "Manage Grades" (green) - if components already configured
Action: Click the button
Result: Routes to either ConfigureComponents or GradeSheet
```

#### Option C: Via Grades Index
```
Action: From Grades Index page, click on offering card
Expected: See "Configure" or "Grade Sheet" + "Edit" buttons
Action: Click appropriate button
Result: Routes to component configuration or grade sheet
```

---

### **Phase 3: Configure Grade Components**

#### Step 3.1: Navigate to Configure Components
```
URL: /Admin/Grades/ConfigureComponents/{offeringId}
Expected: See empty component form or existing components
```

#### Step 3.2: Add Grade Components
```
Action: Click "+ Add Component" button

Component 1:
  - Component Name: "Midterm Exam"
  - Weight: 30
  - Max Score: 100
  - Description: "Midterm examination"

Component 2:
  - Component Name: "Final Exam"
  - Weight: 40
  - Max Score: 100
  - Description: "Final examination"

Component 3:
  - Component Name: "Assignments"
  - Weight: 20
  - Max Score: 100
  - Description: "All assignments combined"

Component 4:
  - Component Name: "Attendance"
  - Weight: 10
  - Max Score: 100
  - Description: "Class attendance"

Expected: Total Weight shows "100.00%" in green
Constraint: Cannot save unless total = 100%
```

#### Step 3.3: Save Components
```
Action: Click "Save Components" button
Expected: Success message and redirect to Grade Sheet
Constraint: If total weight ≠ 100%, button is disabled
```

---

### **Phase 4: Enter Individual Grades**

#### Step 4.1: Open Grade Sheet
```
URL: /Admin/Grades/GradeSheet/{offeringId}
Expected: Excel-like table showing:
  - Rows: All enrolled students
  - Columns: Student name, code, and one column per component
  - Additional columns: Final Grade, Letter Grade
```

#### Step 4.2: Enter Grades for Student 1
```
Student: John Doe
  - Midterm Exam: 85
  - Final Exam: 90
  - Assignments: 88
  - Attendance: 95

Action: Type score in each input field
Expected:
  - Input border turns yellow (saving)
  - Then green (saved successfully)
  - Auto-saves via AJAX
Constraint: Score must be between 0 and MaxScore for that component
```

#### Step 4.3: Enter Grades for Remaining Students
```
Repeat for all enrolled students
Constraint: Each student must have grades in ALL components before final calculation
```

#### Step 4.4: Verify Grade Entry
```
Action: Refresh page
Expected: All grades persist and display correctly
```

---

### **Phase 5: Calculate Final Grades**

#### Step 5.1: Trigger Calculation
```
Action: On Grade Sheet page, click "Calculate Final Grades" button
Expected: Confirmation dialog appears
Action: Confirm
Expected: Processing and redirect back to Grade Sheet
```

#### Step 5.2: Verify Calculation Results
```
Expected Results:
For Student with:
  - Midterm (30%): 85/100 = 85% × 30% = 25.5
  - Final (40%): 90/100 = 90% × 40% = 36.0
  - Assignments (20%): 88/100 = 88% × 20% = 17.6
  - Attendance (10%): 95/100 = 95% × 10% = 9.5
  Total = 88.6%

Expected Letter Grade: B+ (85-89% = 3.3 GPA)

Check:
  - Final Grade column shows calculated percentage (88.60)
  - Letter Grade column shows badge with letter (B+)
  - StudentEnrollment.Status changed to "Completed"
```

#### Step 5.3: Handle Incomplete Grades
```
Scenario: If any student missing grades in any component
Expected: Warning message appears listing students with missing grades
Result: Those students NOT calculated, others proceed
Constraint: All components must have grades for calculation to succeed
```

---

### **Phase 6: Verify GPA Calculation**

#### Step 6.1: Check Student GPA (Admin View)
```
URL: /Admin/Students/Details/{studentId}
Expected: Student.CurrentGPA field updated with cumulative GPA
```

#### Step 6.2: Test GPA API (Student View)
```
Login as Student
API Endpoint: GET /api/Grades/gpa
Expected JSON Response:
{
  "studentId": 1,
  "studentName": "John Doe",
  "cumulativeGPA": 3.45,
  "termGPA": 3.30,
  "totalCredits": 12,
  "termCredits": 3,
  "totalQualityPoints": 41.4,
  "termId": 1
}

Calculation:
  - Course 1: B+ (3.3) × 3 credits = 9.9 points
  - Course 2: A (4.0) × 3 credits = 12.0 points
  - Course 3: B (3.0) × 3 credits = 9.0 points
  - Total: 30.9 points / 9 credits = 3.43 GPA
```

---

### **Phase 7: Test Component Templates (Optional)**

#### Step 7.1: Create Template
```
URL: /Admin/Grades/Templates
Action: Click "Create Template" button
Template Data:
  - Template Name: "Standard Exam Structure"
  - Description: "30% Mid, 40% Final, 20% Assignments, 10% Attendance"
  - Components: (same as above)
Action: Save template
Expected: Template appears in list
```

#### Step 7.2: Apply Template to New Offering
```
Action: Configure components for a new course offering
Action: Click "Load Template" button
Action: Select "Standard Exam Structure"
Expected: All 4 components auto-populated with correct weights
Action: Save components
Expected: Components saved successfully
```

---

### **Phase 8: Generate Transcript PDF**

#### Step 8.1: Test Transcript API (Student)
```
Login as Student with completed courses
API Endpoint: GET /api/Grades/transcript
Expected: PDF file downloads
File Name: Transcript_{studentId}_{date}.pdf
```

#### Step 8.2: Verify PDF Content
```
PDF Should Include:
  - Header: "Official Academic Transcript"
  - Student Information:
    - Name, Code, Email
    - Department, Level
  - Course Table:
    - Course Code, Name
    - Credits, Letter Grade, Term
  - Footer:
    - Cumulative GPA
    - Total Credits
    - Generation Date
```

---

## Common Errors & Solutions

### Error 1: "No grade components configured"
**Cause**: Trying to access Grade Sheet before configuring components
**Solution**: First configure components via ConfigureComponents action

### Error 2: "Component weights must sum to 100%"
**Cause**: Total weight is 99.5% or 100.5%
**Solution**: Adjust weights to exactly 100.00%

### Error 3: "Score must be between 0 and {MaxScore}"
**Cause**: Entered score > MaxScore (e.g., 105 when max is 100)
**Solution**: Enter valid score within range

### Error 4: "Student is not enrolled in this course offering"
**Cause**: Trying to enter grade for non-enrolled student
**Solution**: First enroll student via Course Offering Details page

### Error 5: "Missing grades for {components}"
**Cause**: Trying to calculate final grades with incomplete data
**Solution**: Enter grades for all components for all students

### Error 6: 404 Error on /Admin/Grades
**Cause**: Index action not found (before fix)
**Solution**: Already fixed - Index action now exists in GradesController

---

## Grade Scale Reference

| Letter | Percentage | GPA Points |
|--------|------------|------------|
| A      | 90-100%    | 4.0        |
| B+     | 85-89%     | 3.3        |
| B      | 80-84%     | 3.0        |
| C+     | 75-79%     | 2.3        |
| C      | 70-74%     | 2.0        |
| D      | 60-69%     | 1.0        |
| F      | <60%       | 0.0        |

---

## Test Data Checklist

### Before Starting Tests, Ensure:
- [ ] At least 1 Admin user exists
- [ ] At least 1 Teacher user exists
- [ ] At least 5 Student users exist
- [ ] At least 1 Active Term exists
- [ ] At least 1 Course exists with Credits > 0
- [ ] At least 1 Active Course Offering exists
- [ ] At least 5 students enrolled in that offering
- [ ] Enrollments have Status = "Enrolled"

### Quick SQL Verification Queries:
```sql
-- Check Terms
SELECT * FROM Terms WHERE Status = 'Active';

-- Check Course Offerings
SELECT * FROM CourseOfferings WHERE Status = 'Active';

-- Check Enrollments
SELECT co.Id, c.Code, COUNT(se.Id) as EnrolledCount
FROM CourseOfferings co
JOIN Courses c ON co.CourseId = c.Id
JOIN StudentEnrollments se ON se.CourseOfferingId = co.Id
WHERE se.Status = 'Enrolled'
GROUP BY co.Id, c.Code;

-- Check Grade Components
SELECT co.Id, c.Code, COUNT(gc.Id) as ComponentCount, SUM(gc.Weight) as TotalWeight
FROM CourseOfferings co
JOIN Courses c ON co.CourseId = c.Id
LEFT JOIN GradeComponents gc ON gc.CourseOfferingId = co.Id
GROUP BY co.Id, c.Code;
```

---

## Testing URLs Summary

| Feature | URL | Method |
|---------|-----|--------|
| Grades Index | /Admin/Grades/Index | GET |
| Configure Components | /Admin/Grades/ConfigureComponents/{id} | GET |
| Save Components | /Admin/Grades/SaveComponents | POST |
| Grade Sheet | /Admin/Grades/GradeSheet/{id} | GET |
| Save Grade (AJAX) | /Admin/Grades/SaveGrade | POST |
| Calculate Final | /Admin/Grades/CalculateFinalGrades/{id} | POST |
| Templates | /Admin/Grades/Templates | GET |
| Student GPA API | /api/Grades/gpa | GET |
| Student Transcript | /api/Grades/transcript | GET |

---

## Expected Workflow Timeline

1. **Setup Phase**: 5-10 minutes (create offering, enroll students)
2. **Configure Components**: 2-3 minutes
3. **Enter Grades**: 5-10 minutes (depends on student count)
4. **Calculate Finals**: 1 minute
5. **Verify Results**: 2-3 minutes
6. **Generate Transcript**: 1 minute

**Total**: ~20-30 minutes for complete flow test

---

## Success Criteria

✅ All 5 students have grades in all 4 components
✅ Final grades calculated correctly (weighted average)
✅ Letter grades assigned according to scale
✅ Cumulative GPA updated in Student record
✅ Transcript PDF generates with all completed courses
✅ No errors in browser console
✅ All AJAX grade saves work smoothly
✅ Template system works (optional)

---

## Next Steps After Testing

1. Test with multiple course offerings
2. Test with different term periods
3. Test GPA calculation across multiple terms
4. Test role-based access (Teacher sees only their offerings)
5. Test edge cases (decimal scores, 0% grades, 100% grades)
6. Test concurrent grade entry by multiple teachers
7. Verify database transactions and rollback on errors
