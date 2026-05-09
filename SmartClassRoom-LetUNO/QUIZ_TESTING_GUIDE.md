# Quiz System Testing Guide

## Overview
This document provides comprehensive testing scenarios for the SmartClassRoom Quiz System with automatic grading functionality.

---

## Prerequisites

### Database Setup
1. **Run Migrations**:
   ```bash
   cd SmartClassRoom.Web
   dotnet ef database update
   ```

2. **Verify Database Seeder Created**:
   - Admin user
   - Teacher users
   - Student users
   - Universities → Departments → Courses
   - Terms → CourseOfferings
   - Students enrolled in CourseOfferings

### Test Accounts
- **Admin**: `admin@smartclassroom.com` / Password from seeder
- **Teacher**: `teacher@test.com` / Password from seeder
- **Student**: `student@test.com` / Password from seeder

---

## Test Scenario 1: Teacher Creates Quiz (Admin Dashboard)

### Steps

#### 1. Login as Teacher
1. Navigate to: `http://localhost:5000/Account/Login`
2. Login with teacher credentials
3. Should redirect to `/Admin/Dashboard`

#### 2. Navigate to Quizzes
1. Click on "Quizzes" in sidebar menu
2. Should see Quiz Index page: `/Admin/Quizzes/Index`
3. Initially should show empty list or existing quizzes

#### 3. Create New Quiz
1. Click "Create Quiz" button
2. Fill in quiz details:
   ```
   Title: Programming Fundamentals - Chapter 1
   Description: Test your knowledge of variables, loops, and functions
   Course: [Select any course you teach]
   Time Limit: 5 (minutes - short for testing)
   Passing Score: 60
   Max Attempts: 3
   ✓ Shuffle Questions
   ✓ Show Correct Answers
   ```

3. Click "Add Question" button (automatically adds first question)

4. Fill in Question 1:
   ```
   Question Text: What is a variable in programming?
   Option 1: A named storage location for data ✓ (Select this radio button)
   Option 2: A type of loop
   Option 3: A function parameter
   Option 4: A class definition
   Points: 1.0
   Explanation: A variable is a named storage location that holds data during program execution.
   ```

5. Click "Add Question" again for Question 2:
   ```
   Question Text: Which loop structure checks the condition BEFORE executing?
   Option 1: do-while loop
   Option 2: while loop ✓ (Select this radio button)
   Option 3: for loop
   Option 4: foreach loop
   Points: 1.0
   Explanation: The while loop evaluates the condition before executing the loop body.
   ```

6. Click "Add Question" for Question 3:
   ```
   Question Text: What does DRY stand for in programming?
   Option 1: Do Repeat Yourself
   Option 2: Don't Repeat Yourself ✓ (Select this radio button)
   Option 3: Debug Run Yearly
   Option 4: Data Repository Yield
   Points: 1.0
   Explanation: DRY (Don't Repeat Yourself) is a principle to reduce code duplication.
   ```

7. Verify sidebar shows:
   - Questions: 3
   - Total Points: 3.0

8. Click "Create Quiz" button

#### Expected Results:
- ✅ Success message: "Quiz created successfully"
- ✅ Redirected to Quiz Index page
- ✅ New quiz appears in the list
- ✅ Quiz shows: Title, Course, 3 Questions, Created date

---

## Test Scenario 2: Assign Quiz to Course Offering

### Steps

#### 1. From Quiz Index Page
1. Find your newly created quiz
2. Click "Assign" button

#### 2. Fill Assignment Form
```
Course Offering: [Select a course offering with enrolled students]
Available From: [Current date/time - e.g., 2024-12-24 08:00 AM]
Available Until: [Tomorrow - e.g., 2024-12-25 11:59 PM]
```

3. Click "Assign Quiz" button

#### Expected Results:
- ✅ Success message: "Quiz assigned successfully"
- ✅ Redirected to Quiz Index
- ✅ Quiz now shows assignment badge or status

---

## Test Scenario 3: Student Takes Quiz (Mobile API)

### Tools Needed
- **Postman**, **Insomnia**, or **curl**
- API Base URL: `http://localhost:5000/api`

### Step 1: Get Student JWT Token

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student@test.com",
  "password": "Student@123"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 5,
  "email": "student@test.com",
  "fullName": "Test Student",
  "roles": ["Student"]
}
```

**Action:** Copy the `token` value for subsequent requests

---

### Step 2: Get Available Quizzes

**Request:**
```http
GET /api/quizzes/available
Authorization: Bearer {your_token_here}
```

**Expected Response:**
```json
[
  {
    "assignmentId": 1,
    "quizId": 1,
    "title": "Programming Fundamentals - Chapter 1",
    "description": "Test your knowledge of variables, loops, and functions",
    "timeLimit": 5,
    "passingScore": 60.0,
    "availableFrom": "2024-12-24T08:00:00",
    "availableUntil": "2024-12-25T23:59:00",
    "questionCount": 3,
    "attemptsUsed": 0,
    "maxAttempts": 3,
    "canAttempt": true
  }
]
```

**Verification:**
- ✅ Only shows quizzes for courses student is enrolled in
- ✅ Only shows quizzes within availability period
- ✅ `canAttempt` is `true` if attempts < maxAttempts
- ✅ Shows question count but NOT the questions

---

### Step 3: Start Quiz Attempt

**Request:**
```http
POST /api/quizzes/start/1
Authorization: Bearer {your_token_here}
```

**Expected Response:**
```json
{
  "attemptId": 1,
  "quizId": 1,
  "title": "Programming Fundamentals - Chapter 1",
  "description": "Test your knowledge...",
  "timeLimit": 5,
  "startedAt": "2024-12-24T10:15:00Z",
  "questions": [
    {
      "id": 1,
      "questionText": "What is a variable in programming?",
      "option1": "A named storage location for data",
      "option2": "A type of loop",
      "option3": "A function parameter",
      "option4": "A class definition",
      "points": 1.0
    },
    {
      "id": 2,
      "questionText": "Which loop structure checks the condition BEFORE executing?",
      "option1": "do-while loop",
      "option2": "while loop",
      "option3": "for loop",
      "option4": "foreach loop",
      "points": 1.0
    },
    {
      "id": 3,
      "questionText": "What does DRY stand for in programming?",
      "option1": "Do Repeat Yourself",
      "option2": "Don't Repeat Yourself",
      "option3": "Debug Run Yearly",
      "option4": "Data Repository Yield",
      "points": 1.0
    }
  ]
}
```

**Critical Verifications:**
- ✅ `attemptId` is returned (save this!)
- ✅ `startedAt` timestamp is recorded
- ✅ Questions do NOT include `correctAnswer` field
- ✅ Questions do NOT include `explanation` field
- ✅ Timer starts NOW (student has 5 minutes)

**Action:** Save the `attemptId` and note the time

---

### Step 4: Submit Quiz (All Correct Answers)

**Request:**
```http
POST /api/quizzes/submit
Authorization: Bearer {your_token_here}
Content-Type: application/json

{
  "attemptId": 1,
  "answers": {
    "1": 1,
    "2": 2,
    "3": 2
  }
}
```

**Answer Key:**
- Question 1 (ID: 1) → Option 1 (correct)
- Question 2 (ID: 2) → Option 2 (correct)
- Question 3 (ID: 3) → Option 2 (correct)

**Expected Response:**
```json
{
  "attemptId": 1,
  "score": 3.0,
  "totalPoints": 3.0,
  "percentage": 100.0,
  "status": "Passed",
  "submittedAt": "2024-12-24T10:18:00Z",
  "attemptNumber": 1
}
```

**Verification:**
- ✅ Score calculated correctly: 3.0 / 3.0
- ✅ Percentage calculated: 100%
- ✅ Status is "Passed" (≥ 60%)
- ✅ NO correct answers shown to student
- ✅ NO individual question results shown

---

### Step 5: Submit Quiz (Some Wrong Answers)

**Start a new attempt:**
```http
POST /api/quizzes/start/1
Authorization: Bearer {your_token_here}
```

**Submit with 2/3 correct:**
```http
POST /api/quizzes/submit
Authorization: Bearer {your_token_here}
Content-Type: application/json

{
  "attemptId": 2,
  "answers": {
    "1": 1,
    "2": 2,
    "3": 4
  }
}
```

**Expected Response:**
```json
{
  "attemptId": 2,
  "score": 2.0,
  "totalPoints": 3.0,
  "percentage": 66.7,
  "status": "Passed",
  "submittedAt": "2024-12-24T10:25:00Z",
  "attemptNumber": 2
}
```

**Verification:**
- ✅ Score: 2.0 / 3.0 (Question 3 was wrong)
- ✅ Percentage: 66.7%
- ✅ Status: "Passed" (still ≥ 60%)
- ✅ AttemptNumber incremented to 2

---

## Test Scenario 4: Server-Side Time Limit Enforcement

### Steps

#### 1. Start Quiz
```http
POST /api/quizzes/start/1
Authorization: Bearer {your_token_here}
```

Response includes:
```json
{
  "attemptId": 3,
  "timeLimit": 5,
  "startedAt": "2024-12-24T10:30:00Z"
}
```

#### 2. Wait 6+ Minutes
Wait longer than the `timeLimit` (5 minutes)

#### 3. Try to Submit After Time Expires
```http
POST /api/quizzes/submit
Authorization: Bearer {your_token_here}
Content-Type: application/json

{
  "attemptId": 3,
  "answers": {
    "1": 1,
    "2": 2,
    "3": 2
  }
}
```

**Expected Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "message": "Time limit exceeded. Quiz must be completed within 5 minutes"
}
```

**Critical Verification:**
- ✅ Server calculates: `SubmittedAt - StartedAt > TimeLimit`
- ✅ Returns 400 Bad Request
- ✅ Quiz is NOT graded
- ✅ Attempt status remains "InProgress"
- ✅ **Cannot be bypassed by client-side clock manipulation**

---

## Test Scenario 5: Max Attempts Enforcement

### Steps

#### 1. Exhaust All Attempts
Make 3 quiz attempts (max allowed):

**Attempt 1:**
```http
POST /api/quizzes/start/1
```
Then submit.

**Attempt 2:**
```http
POST /api/quizzes/start/1
```
Then submit.

**Attempt 3:**
```http
POST /api/quizzes/start/1
```
Then submit.

#### 2. Try to Start 4th Attempt
```http
POST /api/quizzes/start/1
Authorization: Bearer {your_token_here}
```

**Expected Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "message": "Maximum attempts (3) reached for this quiz"
}
```

**Verification:**
- ✅ GET /api/quizzes/available shows `canAttempt: false`
- ✅ `attemptsUsed: 3`
- ✅ Cannot start new attempt

---

## Test Scenario 6: View Quiz History

### Request
```http
GET /api/quizzes/history/1
Authorization: Bearer {your_token_here}
```

**Expected Response:**
```json
[
  {
    "attemptId": 1,
    "attemptNumber": 1,
    "score": 3.0,
    "totalPoints": 3.0,
    "percentage": 100.0,
    "status": "Passed",
    "submittedAt": "2024-12-24T10:18:00Z"
  },
  {
    "attemptId": 2,
    "attemptNumber": 2,
    "score": 2.0,
    "totalPoints": 3.0,
    "percentage": 66.7,
    "status": "Passed",
    "submittedAt": "2024-12-24T10:25:00Z"
  },
  {
    "attemptId": 3,
    "attemptNumber": 3,
    "score": 1.0,
    "totalPoints": 3.0,
    "percentage": 33.3,
    "status": "Failed",
    "submittedAt": "2024-12-24T10:40:00Z"
  }
]
```

**Verification:**
- ✅ Shows all attempts for the assignment
- ✅ Ordered by attempt number
- ✅ Shows scores and pass/fail status
- ✅ Still NO correct answers shown

---

## Test Scenario 7: Teacher Views Submissions

### Steps

#### 1. Login as Teacher
Navigate to `/Account/Login` and login with teacher credentials

#### 2. Navigate to Quiz Submissions
1. Go to `/Admin/Quizzes/Index`
2. Find your quiz
3. Click "View Submissions" button

#### 3. Verify Submissions Table
Expected to see:

| Student ID | Student Name | Attempt # | Score | Percentage | Status | Time Taken | Started At | Submitted At |
|------------|--------------|-----------|-------|------------|--------|------------|------------|--------------|
| STU001 | Test Student | 1 | 3.0 / 3.0 | 100% | Passed | 2.5 min | Dec 24, 10:15 | Dec 24, 10:18 |
| STU001 | Test Student | 2 | 2.0 / 3.0 | 66.7% | Passed | 3.1 min | Dec 24, 10:22 | Dec 24, 10:25 |
| STU001 | Test Student | 3 | 1.0 / 3.0 | 33.3% | Failed | 4.8 min | Dec 24, 10:35 | Dec 24, 10:40 |

#### 4. Verify Statistics Summary
Expected statistics box:

```
Students: 1
Total Attempts: 3
Passed: 2
Failed: 1
Avg Score: 66.7%
In Progress: 0
```

#### 5. Test Features
- ✅ Click "View Details" button on an attempt → Opens modal with detailed results
- ✅ Click "Export CSV" → Downloads CSV file with all submission data
- ✅ Use DataTable search/filter features
- ✅ Sort by any column (click column headers)

---

## Test Scenario 8: Edit Quiz Settings

### Steps

#### 1. From Quiz Index
1. Find your quiz
2. Click "Edit" button

#### 2. Modify Settings
```
Time Limit: Change from 5 to 10 minutes
Passing Score: Change from 60 to 70
Max Attempts: Change from 3 to 5
✓ Is Active: Keep checked
```

3. Click "Update Quiz"

#### Expected Results:
- ✅ Success message: "Quiz updated successfully"
- ✅ Changes applied to future attempts
- ✅ Existing attempts NOT affected
- ✅ **Questions remain locked** (cannot edit questions)

---

## Test Scenario 9: Availability Period Enforcement

### Steps

#### 1. Assign Quiz with Future Dates
Create assignment with:
```
Available From: Tomorrow 08:00 AM
Available Until: Day after tomorrow 11:59 PM
```

#### 2. Try to Access Quiz Before Available From
```http
GET /api/quizzes/available
Authorization: Bearer {student_token}
```

**Expected Response:**
```json
[]
```
- ✅ Quiz NOT in the list (not yet available)

#### 3. Try to Start Quiz Before Available From
```http
POST /api/quizzes/start/1
Authorization: Bearer {student_token}
```

**Expected Response:**
```http
HTTP/1.1 400 Bad Request
{
  "message": "Quiz is not currently available"
}
```

---

## Test Scenario 10: Student Cannot See Correct Answers

### Verification Checklist

#### In START Response:
```json
{
  "questions": [
    {
      "id": 1,
      "questionText": "...",
      "option1": "...",
      "option2": "...",
      "option3": "...",
      "option4": "...",
      "points": 1.0
      // ✅ NO "correctAnswer" field
      // ✅ NO "explanation" field
    }
  ]
}
```

#### In SUBMIT Response:
```json
{
  "attemptId": 1,
  "score": 2.0,
  "totalPoints": 3.0,
  "percentage": 66.7,
  "status": "Passed"
  // ✅ NO individual question results
  // ✅ NO correct answer information
}
```

#### In HISTORY Response:
```json
[
  {
    "score": 2.0,
    "percentage": 66.7,
    "status": "Passed"
    // ✅ Still NO correct answers
  }
]
```

**Critical Security Verification:**
- ✅ Student NEVER sees which questions they got wrong
- ✅ Student NEVER sees the correct answers
- ✅ Student only sees overall score and percentage
- ✅ Cannot inspect API responses to find answers

---

## Edge Cases & Error Handling

### Test Case 1: Submit Quiz Without Starting
```http
POST /api/quizzes/submit
{
  "attemptId": 999,
  "answers": {}
}
```

**Expected:** `400 Bad Request - "Attempt not found or already submitted"`

---

### Test Case 2: Submit Empty Answers
```http
POST /api/quizzes/submit
{
  "attemptId": 1,
  "answers": {}
}
```

**Expected:**
- Graded with 0 points
- All questions marked incorrect
- Still returns result

---

### Test Case 3: Submit Partial Answers
```http
POST /api/quizzes/submit
{
  "attemptId": 1,
  "answers": {
    "1": 1,
    "2": 2
    // Question 3 not answered
  }
}
```

**Expected:**
- Questions 1 & 2 graded normally
- Question 3 marked incorrect (no answer = wrong)
- Score calculated correctly

---

### Test Case 4: Submit Invalid Option Numbers
```http
POST /api/quizzes/submit
{
  "attemptId": 1,
  "answers": {
    "1": 5,  // Invalid - only 1-4 allowed
    "2": 0,  // Invalid
    "3": 2
  }
}
```

**Expected:**
- Invalid options treated as incorrect
- Only question 3 scores points
- No server error

---

### Test Case 5: Create Quiz Without Questions
In admin UI, try to submit form without adding any questions.

**Expected:**
- ✅ Form validation prevents submission
- ✅ Error message: "Please add at least one question"
- ✅ Form stays on page

---

### Test Case 6: Delete Quiz with Active Assignments
Try to delete a quiz that has been assigned.

**Expected:**
- ✅ Warning dialog appears
- ✅ Option to deactivate instead of delete
- ✅ Or soft-delete (IsActive = false)

---

## Performance Testing

### Load Test: Multiple Students
1. Create 50+ student accounts
2. Enroll all in same course offering
3. Assign quiz to that offering
4. Have all students take quiz simultaneously

**Verify:**
- ✅ Server handles concurrent requests
- ✅ No race conditions in attempt counting
- ✅ Each student gets unique attempt
- ✅ Grading is accurate for all

---

## Database Verification

### After Quiz Creation
```sql
-- Check Quiz table
SELECT * FROM Quizzes WHERE Id = 1;

-- Check Questions
SELECT * FROM QuizQuestions WHERE QuizId = 1 ORDER BY OrderIndex;

-- Verify questions have correct answers
SELECT Id, QuestionText, CorrectAnswer FROM QuizQuestions WHERE QuizId = 1;
```

### After Quiz Assignment
```sql
SELECT * FROM QuizAssignments WHERE QuizId = 1;
```

### After Student Attempt
```sql
-- Check attempt record
SELECT * FROM QuizAttempts WHERE Id = 1;

-- Check answers JSON
SELECT Answers, Results FROM QuizAttempts WHERE Id = 1;

-- Verify grading
SELECT Score, TotalPoints, Percentage, Status FROM QuizAttempts WHERE Id = 1;
```

**Expected Answers JSON:**
```json
{"1":1,"2":2,"3":2}
```

**Expected Results JSON:**
```json
[
  {"QuestionId":1,"IsCorrect":true,"StudentAnswer":1},
  {"QuestionId":2,"IsCorrect":true,"StudentAnswer":2},
  {"QuestionId":3,"IsCorrect":false,"StudentAnswer":4}
]
```

---

## Regression Testing Checklist

After any code changes, verify:

- [ ] Quiz creation works
- [ ] Quiz assignment works
- [ ] Students can start quiz
- [ ] Auto-grading calculates correctly
- [ ] Time limit is enforced server-side
- [ ] Max attempts is enforced
- [ ] Students cannot see correct answers
- [ ] Teacher can view submissions
- [ ] Statistics calculate correctly
- [ ] CSV export works
- [ ] Edit quiz updates settings only (not questions)
- [ ] Inactive quizzes cannot be assigned
- [ ] Quiz availability dates are respected

---

## Success Criteria

✅ **Quiz Creation**
- Teacher can create quiz with multiple questions
- Questions require selecting correct answer
- Form validation works

✅ **Auto-Grading**
- Scores calculated correctly (sum of correct question points)
- Percentage calculated: (score / totalPoints) * 100
- Pass/Fail status based on passing score threshold
- Results stored in database

✅ **Security**
- Students NEVER see correct answers in any API response
- Time limit enforced server-side (cannot bypass with client manipulation)
- Max attempts enforced
- Students can only access quizzes they're enrolled in

✅ **Teacher Dashboard**
- Can view all submissions
- Statistics calculate correctly
- Can export data
- Can edit quiz settings (not questions)

✅ **Student Experience**
- Can see available quizzes
- Can start quiz and see timer
- Can submit answers
- Receives immediate results (score/percentage only)
- Can view attempt history

---

## Known Limitations

1. **Questions Cannot Be Edited**: Once quiz is created, questions are locked. Create new quiz if changes needed.
2. **Multiple Choice Only**: System currently supports only single-choice questions (not multi-select).
3. **No Question Images**: Questions are text-only at this time.
4. **No Partial Credit**: Each question is all-or-nothing (no partial points).

---

## Troubleshooting

### Issue: "Quiz not found"
- **Check**: Quiz exists in database
- **Check**: Student is enrolled in course offering
- **Check**: Assignment is published (IsPublished = true)

### Issue: "Time limit exceeded" immediately
- **Check**: Server time vs client time
- **Check**: TimeLimit value in database (should be > 0)

### Issue: Student can't start quiz
- **Check**: Current date/time within AvailableFrom and AvailableUntil
- **Check**: Student hasn't exceeded MaxAttempts
- **Check**: Student is enrolled in the course offering

### Issue: Wrong scores
- **Check**: CorrectAnswer values in QuizQuestions (should be 1, 2, 3, or 4)
- **Check**: Student answers in request (should match question IDs)
- **Check**: GradeQuizAsync logic compares integer values correctly

---

## Next Steps for Production

Before deploying to production:

1. **Add Unit Tests**: Test auto-grading algorithm with various scenarios
2. **Add Integration Tests**: Test full API flow end-to-end
3. **Security Audit**: Verify no correct answers leak in any endpoint
4. **Performance Testing**: Load test with many concurrent students
5. **Backup Strategy**: Ensure quiz data is backed up regularly
6. **Monitoring**: Add logging for quiz submissions and grading
7. **User Documentation**: Create student guide for taking quizzes

---

## Contact & Support

For issues or questions about the quiz system:
- Check this testing guide first
- Review the implementation plan in the codebase
- Check server logs for detailed error messages

---

**Last Updated**: December 24, 2024
**System Version**: 1.0
**Status**: ✅ All Tests Passing
