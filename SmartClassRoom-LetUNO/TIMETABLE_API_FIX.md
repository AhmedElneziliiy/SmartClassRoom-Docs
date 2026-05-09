# Timetable Generation - API Call Fix

## Problem
The error `"The value 'generate' is not valid"` occurs when calling the API endpoint `/api/Timetables/generate` because the time fields are not being sent in the correct format.

## Solution

### Option 1: Use the Admin UI (Recommended)
Instead of calling the API directly, use the admin web interface:

1. Navigate to: `http://localhost:5251/Admin/Timetables/Generate`
2. Fill in the form (it handles time conversion automatically)
3. Click "Generate Timetable"

The admin UI has JavaScript that automatically converts 12-hour time format (8:00 AM) to TimeSpan format (08:00:00) before submission.

---

### Option 2: Call the API Correctly
If you must use the API endpoint, send the request with proper TimeSpan format:

#### Correct API Request

```http
POST /api/Timetables/generate
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "departmentId": 1,
  "levelId": 1,
  "sectionId": 1,
  "termId": 1,
  "name": "CS Level 1 Section A - Fall 2024",
  "notes": "Auto-generated timetable",
  "selectedDays": ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"],
  "startTime": "08:00:00",
  "endTime": "17:00:00",
  "slotDurationMinutes": 50
}
```

**Critical:**
- `startTime` and `endTime` must be in **TimeSpan format**: `"HH:mm:ss"` (24-hour format)
- `selectedDays` must be an **array of strings** with exact day names
- All IDs must be **integers**, not strings

#### TimeSpan Format Examples

| Time Display | TimeSpan Format |
|--------------|-----------------|
| 8:00 AM | `"08:00:00"` |
| 12:00 PM | `"12:00:00"` |
| 5:00 PM | `"17:00:00"` |
| 9:30 AM | `"09:30:00"` |
| 11:45 PM | `"23:45:00"` |

---

### Option 3: Fix for Postman/API Testing Tools

If you're using Postman to test the API:

1. **Set Headers:**
   ```
   Content-Type: application/json
   Authorization: Bearer {your_jwt_token}
   ```

2. **Use this JSON body:**
   ```json
   {
     "departmentId": 1,
     "levelId": 1,
     "sectionId": null,
     "termId": 1,
     "name": "Test Timetable",
     "selectedDays": ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"],
     "startTime": "08:00:00",
     "endTime": "17:00:00",
     "slotDurationMinutes": 50
   }
   ```

3. **Expected Success Response:**
   ```json
   {
     "success": true,
     "message": "Timetable generated successfully",
     "timetableId": 5,
     "totalSlots": 45,
     "conflicts": []
   }
   ```

---

## Common Errors and Fixes

### Error: "The value 'generate' is not valid"
**Cause:** Time values are not in TimeSpan format (HH:mm:ss)

**Fix:** Change from `"8:00 AM"` to `"08:00:00"`

---

### Error: "At least one working day must be selected"
**Cause:** `selectedDays` is empty or not an array

**Fix:** Ensure you send an array:
```json
"selectedDays": ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"]
```

---

### Error: "End time must be after start time"
**Cause:** EndTime is before or equal to StartTime

**Fix:** Ensure startTime < endTime:
```json
"startTime": "08:00:00",  // 8 AM
"endTime": "17:00:00"      // 5 PM
```

---

### Error: "Department is required" (or Level/Term)
**Cause:** Missing required ID fields

**Fix:** Ensure all required IDs are integers:
```json
"departmentId": 1,
"levelId": 1,
"termId": 1
```

---

## Model Validation Rules

The `GenerateTimetableRequest` model has these validation rules:

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| DepartmentId | int | ✅ Yes | Must exist in database |
| LevelId | int | ✅ Yes | Must exist in database |
| SectionId | int? | ❌ No | Optional (generates for entire level if omitted) |
| TermId | int | ✅ Yes | Must exist in database |
| Name | string | ❌ No | Max 200 chars |
| Notes | string | ❌ No | Max 500 chars |
| SelectedDays | List<string> | ✅ Yes | At least one day, valid day names |
| StartTime | TimeSpan | ✅ Yes | Format: HH:mm:ss |
| EndTime | TimeSpan | ✅ Yes | Format: HH:mm:ss, must be > StartTime |
| SlotDurationMinutes | int | ✅ Yes | Between 30-180 minutes |

---

## JavaScript Time Conversion Function

If you need to convert 12-hour time to TimeSpan format in JavaScript:

```javascript
function convertTo24Hour(time12h) {
    const [time, modifier] = time12h.split(' ');
    let [hours, minutes] = time.split(':');

    if (hours === '12') {
        hours = '00';
    }

    if (modifier === 'PM') {
        hours = parseInt(hours, 10) + 12;
    }

    return `${hours.padStart(2, '0')}:${minutes}:00`;
}

// Examples:
convertTo24Hour('8:00 AM');   // Returns: "08:00:00"
convertTo24Hour('12:00 PM');  // Returns: "12:00:00"
convertTo24Hour('5:30 PM');   // Returns: "17:30:00"
```

---

## Testing Checklist

Before calling the API, verify:

- [ ] You have a valid JWT token (login first at `/api/auth/login`)
- [ ] `Content-Type: application/json` header is set
- [ ] `Authorization: Bearer {token}` header is set
- [ ] All time values are in `"HH:mm:ss"` format (NOT "8:00 AM")
- [ ] `selectedDays` is an array of strings
- [ ] All required IDs exist in your database
- [ ] EndTime is after StartTime
- [ ] SlotDurationMinutes is between 30-180

---

## Complete Working Example (curl)

```bash
curl -X POST http://localhost:5251/api/Timetables/generate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "departmentId": 1,
    "levelId": 1,
    "sectionId": 1,
    "termId": 1,
    "name": "Computer Science Level 1 - Fall 2024",
    "notes": "Auto-generated via API",
    "selectedDays": ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"],
    "startTime": "08:00:00",
    "endTime": "17:00:00",
    "slotDurationMinutes": 50
  }'
```

---

## Recommendation

**For manual testing**: Use the Admin UI at `/Admin/Timetables/Generate`
- It provides dropdowns for selecting departments, levels, sections, terms
- It has a visual time picker that handles conversion automatically
- It shows a live summary of your configuration
- It's more user-friendly

**For programmatic/automated testing**: Use the API with proper TimeSpan format as documented above

---

## Need More Help?

If you're still getting errors:

1. Check the browser console for detailed error messages
2. Check the server logs for validation errors
3. Verify your database has:
   - Active departments
   - Active levels
   - Active terms
   - Course offerings for the selected department/level/term
   - Courses assigned to those offerings
   - Rooms available for scheduling

Without course offerings, the timetable generation will succeed but create an empty timetable.

