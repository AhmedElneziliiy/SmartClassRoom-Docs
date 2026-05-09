using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.ViewModels.Timetables;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class TimetableService : ITimetableService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimetableService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Generation

    public async Task<TimetableGenerationResult> GenerateTimetableAsync(GenerateTimetableRequest request, int? userId = null)
    {
        // STEP 1: VALIDATION
        var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId);
        if (department == null)
            return new TimetableGenerationResult { Success = false, Message = "Department not found" };

        var level = await _unitOfWork.Levels.GetByIdAsync(request.LevelId);
        if (level == null)
            return new TimetableGenerationResult { Success = false, Message = "Level not found" };

        var term = await _unitOfWork.Terms.GetByIdAsync(request.TermId);
        if (term == null)
            return new TimetableGenerationResult { Success = false, Message = "Term not found" };

        Section? section = null;
        if (request.SectionId.HasValue)
        {
            section = await _unitOfWork.Sections.GetByIdAsync(request.SectionId.Value);
            if (section == null)
                return new TimetableGenerationResult { Success = false, Message = "Section not found" };
        }

        // STEP 1.5: CHECK FOR DUPLICATE ACTIVE TIMETABLE
        var allTimetables = await _unitOfWork.Timetables.GetAllAsync();
        var existingActiveTimetable = allTimetables.FirstOrDefault(t =>
            t.DepartmentId == request.DepartmentId &&
            t.LevelId == request.LevelId &&
            t.TermId == request.TermId &&
            (!request.SectionId.HasValue || t.SectionId == request.SectionId) &&
            t.IsActive == true);

        if (existingActiveTimetable != null)
        {
            return new TimetableGenerationResult
            {
                Success = false,
                Message = $"An active timetable already exists for this criteria. " +
                          $"Department: {department.Name}, Level: {level.Name}, " +
                          $"Term: {term.Name}" +
                          (section != null ? $", Section: {section.Name}" : "") +
                          ". Please delete or deactivate the existing timetable first, or select different criteria.",
                TimetableId = 0
            };
        }

        // STEP 2: RETRIEVE COURSE OFFERINGS
        var offerings = await GetCourseOfferingsForGeneration(
            request.DepartmentId,
            request.LevelId,
            request.SectionId,
            request.TermId);

        if (!offerings.Any())
            return new TimetableGenerationResult
            {
                Success = false,
                Message = "No active course offerings found for the specified criteria"
            };

        // STEP 2.5: VALIDATE GENERATION PARAMETERS
        var totalSessionsNeeded = offerings.Sum(o => o.SessionsPerWeek);
        var validation = ValidateGenerationParameters(request, totalSessionsNeeded);
        if (!validation.IsValid)
        {
            return new TimetableGenerationResult
            {
                Success = false,
                Message = validation.ErrorMessage
            };
        }

        // STEP 3: CREATE TIMETABLE ENTITY
        var timetableName = request.Name ??
            $"{department.Name} - {level.Name}" +
            (request.SectionId.HasValue ? $" - Section" : "") +
            $" - {term.Name}";

        var timetable = new Timetable
        {
            Name = timetableName,
            DepartmentId = request.DepartmentId,
            LevelId = request.LevelId,
            SectionId = request.SectionId,
            TermId = request.TermId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            WorkingDays = request.SelectedDays, // Store selected working days
            IsActive = true,
            Status = "Draft",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Timetables.AddAsync(timetable);
        await _unitOfWork.SaveChangesAsync(); // Save to get ID

        // STEP 4: GENERATE TIME SLOTS
        var allTimeSlots = GenerateAllTimeSlots(
            request.SelectedDays,
            request.StartTime,
            request.EndTime,
            request.SlotDurationMinutes);

        // DEBUG: Log generated slots
        Console.WriteLine($"[DEBUG] Generated {allTimeSlots.Count} total time slots");
        foreach (var day in request.SelectedDays)
        {
            var slotsForDay = allTimeSlots.Where(s => s.DayOfWeek == day).Count();
            Console.WriteLine($"  {day}: {slotsForDay} slots");
        }

        // STEP 5: SORT OFFERINGS (most constrained first)
        var sortedOfferings = offerings.OrderByDescending(o => o.SessionsPerWeek).ToList();

        // DEBUG: Log course offerings
        Console.WriteLine($"\n[DEBUG] Course offerings to schedule:");
        foreach (var offering in sortedOfferings)
        {
            Console.WriteLine($"  - {offering.Course.Name} ({offering.SessionsPerWeek} sessions/week) - Teacher: {offering.Teacher?.FullName}, Section: {offering.Section?.Name}");
        }

        // STEP 6: SCHEDULE EACH OFFERING - GLOBAL ROUND-ROBIN DISTRIBUTION
        var unscheduledCourses = new List<UnscheduledCourse>();
        var scheduledSlotsCount = 0;
        var totalRequiredSlots = sortedOfferings.Sum(o => o.SessionsPerWeek);

        Console.WriteLine($"\n[DEBUG] Total sessions needed: {totalRequiredSlots}, Total slots available: {allTimeSlots.Count}");

        // Global day rotation index for round-robin distribution across ALL courses
        int globalDayIndex = 0;

        foreach (var offering in sortedOfferings)
        {
            Console.WriteLine($"\n[DEBUG] Scheduling: {offering.Course.Name} ({offering.SessionsPerWeek} sessions)");

            var (scheduledCount, updatedDayIndex) = await ScheduleOfferingAsync(
                timetable,
                offering,
                allTimeSlots,
                term,
                request.SelectedDays,
                globalDayIndex);  // Pass current global day index

            globalDayIndex = updatedDayIndex;  // Update for next iteration

            Console.WriteLine($"[DEBUG] Successfully scheduled {scheduledCount}/{offering.SessionsPerWeek} sessions for {offering.Course.Name}");

            scheduledSlotsCount += scheduledCount;

            if (scheduledCount < offering.SessionsPerWeek)
            {
                unscheduledCourses.Add(new UnscheduledCourse
                {
                    CourseOfferingId = offering.Id,
                    CourseName = offering.Course.Name,
                    CourseCode = offering.Course.Code,
                    TeacherName = offering.Teacher.FullName,
                    SessionsPerWeek = offering.SessionsPerWeek,
                    ScheduledSessions = scheduledCount,
                    Reason = scheduledCount == 0
                        ? "No available time slots or suitable rooms"
                        : "Partial scheduling - insufficient available slots"
                });
            }
        }

        // STEP 7: DETECT CONFLICTS
        var conflicts = await DetectConflictsAsync(timetable.Id);

        // STEP 8: RETURN RESULT
        return new TimetableGenerationResult
        {
            Success = true,
            Message = conflicts.Any()
                ? "Timetable generated with conflicts. Please review and resolve before publishing."
                : "Timetable generated successfully without conflicts.",
            TimetableId = timetable.Id,
            TotalSlots = totalRequiredSlots,
            ScheduledSlots = scheduledSlotsCount,
            UnscheduledSlots = totalRequiredSlots - scheduledSlotsCount,
            Conflicts = conflicts,
            UnscheduledCourses = unscheduledCourses,
            Status = "Draft"
        };
    }

    private List<TimeSlotDto> GenerateAllTimeSlots(
        List<string> workingDays,
        TimeSpan startTime,
        TimeSpan endTime,
        int slotDurationMinutes)
    {
        var slots = new List<TimeSlotDto>();
        var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

        // Calculate number of slots per day
        var workingMinutes = (int)(endTime - startTime).TotalMinutes;
        var slotsPerDay = workingMinutes / slotDurationMinutes;

        foreach (var day in workingDays)
        {
            for (int i = 0; i < slotsPerDay; i++)
            {
                var slotStart = startTime.Add(TimeSpan.FromMinutes(i * slotDurationMinutes));
                var slotEnd = slotStart.Add(slotDuration);

                slots.Add(new TimeSlotDto
                {
                    DayOfWeek = day,
                    StartTime = slotStart,
                    EndTime = slotEnd
                });
            }
        }

        return slots;
    }

    private async Task<IEnumerable<CourseOffering>> GetCourseOfferingsForGeneration(
        int departmentId,
        int levelId,
        int? sectionId,
        int termId)
    {
        // Get all offerings for the term
        var allOfferings = await _unitOfWork.CourseOfferings.GetAllAsync(
            "Course",
            "Course.Department",
            "Teacher",
            "Section",
            "Section.Level",
            "Enrollments");

        // Filter by criteria
        var filtered = allOfferings
            .Where(o => o.TermId == termId && o.Status == "Active");

        // Filter by section if specified
        if (sectionId.HasValue)
        {
            filtered = filtered.Where(o => o.SectionId == sectionId.Value);
        }
        else
        {
            // Filter by department and level
            filtered = filtered.Where(o =>
                o.Course.DepartmentId == departmentId &&
                o.Section != null &&
                o.Section.LevelId == levelId);
        }

        return filtered.ToList();
    }

    private async Task<(int ScheduledCount, int UpdatedDayIndex)> ScheduleOfferingAsync(
        Timetable timetable,
        CourseOffering offering,
        List<TimeSlotDto> allTimeSlots,
        Term term,
        List<string> workingDays,
        int globalDayIndex)  // Global rotation index
    {
        var scheduledCount = 0;
        var sessionsNeeded = offering.SessionsPerWeek;

        // Get enrolled student count for room capacity check
        var enrolledCount = offering.Enrollments.Count(e => e.Status == "Enrolled");
        var requiredCapacity = enrolledCount > 0 ? enrolledCount : offering.MaxStudents;

        // Determine room type based on course
        var roomType = DetermineRoomType(offering.Course);

        // GLOBAL ROUND-ROBIN DISTRIBUTION
        // Schedule each session using the global day rotation index
        for (int sessionIndex = 0; sessionIndex < sessionsNeeded; sessionIndex++)
        {
            TimeSlotDto? availableSlot = null;
            string? scheduledDay = null;
            int daysChecked = 0;

            // Try to find available slot, rotating through days globally
            while (daysChecked < workingDays.Count && availableSlot == null)
            {
                string dayToCheck = workingDays[globalDayIndex % workingDays.Count];

                availableSlot = await FindAvailableSlotOnDayAsync(
                    offering,
                    timetable.Id,
                    term.Id,
                    allTimeSlots,
                    dayToCheck);

                if (availableSlot != null)
                {
                    scheduledDay = dayToCheck;
                }
                else
                {
                    // Current day full for this offering, try next day in rotation
                    globalDayIndex++;
                    daysChecked++;
                }
            }

            if (availableSlot != null && scheduledDay != null)
            {
                Console.WriteLine($"  Session {sessionIndex + 1}/{sessionsNeeded}: Scheduled on {scheduledDay} at {availableSlot.StartTime}");

                // Find suitable room
                var room = await FindSuitableRoomAsync(
                    availableSlot.DayOfWeek,
                    availableSlot.StartTime,
                    availableSlot.EndTime,
                    term.Id,
                    roomType,
                    requiredCapacity);

                // Create scheduled slot
                var scheduledSlot = new ScheduledSlot
                {
                    TimetableId = timetable.Id,
                    CourseOfferingId = offering.Id,
                    DayOfWeek = availableSlot.DayOfWeek,
                    StartTime = availableSlot.StartTime,
                    EndTime = availableSlot.EndTime,
                    RoomId = room?.Id
                };

                await _unitOfWork.ScheduledSlots.AddAsync(scheduledSlot);
                await _unitOfWork.SaveChangesAsync();

                scheduledCount++;

                // Move to next day in global rotation for next session
                globalDayIndex++;
            }
            else
            {
                // No available slot found on any day - don't throw, just break and mark as partial
                Console.WriteLine($"  [ERROR] Cannot schedule session {sessionIndex + 1}/{sessionsNeeded} - no slots available on any day!");
                break;  // Exit loop, return partial count
            }
        }

        // Return scheduled count and updated global day index
        return (scheduledCount, globalDayIndex);
    }

    private (bool IsValid, string ErrorMessage, int TotalSlots) ValidateGenerationParameters(
        GenerateTimetableRequest request,
        int totalSessionsNeeded)
    {
        // Calculate slots per day (static based on working hours)
        var workingMinutes = (int)(request.EndTime - request.StartTime).TotalMinutes;
        var slotsPerDay = workingMinutes / request.SlotDurationMinutes;

        // Calculate MINIMUM slots needed per day to fit all sessions
        var minSlotsPerDayNeeded = (int)Math.Ceiling((double)totalSessionsNeeded / request.SelectedDays.Count);

        // Check if each day can accommodate required slots (STRICT VALIDATION)
        if (slotsPerDay < minSlotsPerDayNeeded)
        {
            return (false,
                $"Each day can only fit {slotsPerDay} slots, but needs {minSlotsPerDayNeeded} slots to distribute {totalSessionsNeeded} sessions across {request.SelectedDays.Count} days. " +
                $"Please: (1) Increase working hours, (2) Reduce slot duration, or (3) Select more working days.",
                0);
        }

        // Check minimum configuration
        if (slotsPerDay < 2 || request.SelectedDays.Count < 1)
        {
            return (false,
                "Configuration provides too few time slots. Please add more days or extend hours.",
                0);
        }

        // Return the ACTUAL total sessions needed (static value based on course offerings, NOT days selected)
        return (true, string.Empty, totalSessionsNeeded);
    }

    private async Task<TimeSlotDto?> FindAvailableSlotAsync(
        CourseOffering offering,
        int timetableId,
        int termId,
        List<TimeSlotDto> availableSlots)
    {
        foreach (var slot in availableSlots)
        {
            // Check teacher conflict
            var teacherConflict = await _unitOfWork.ScheduledSlots.HasTeacherConflictAsync(
                offering.TeacherId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                termId);

            if (teacherConflict)
                continue;

            // Check section conflict
            if (offering.SectionId.HasValue)
            {
                var sectionConflict = await _unitOfWork.ScheduledSlots.HasSectionConflictAsync(
                    offering.SectionId.Value,
                    slot.DayOfWeek,
                    slot.StartTime,
                    slot.EndTime,
                    termId);

                if (sectionConflict)
                    continue;
            }

            // Slot is available - no conflicts
            return slot;
        }

        // No available slot found
        return null;
    }

    private async Task<TimeSlotDto?> FindAvailableSlotOnDayAsync(
        CourseOffering offering,
        int timetableId,
        int termId,
        List<TimeSlotDto> availableSlots,
        string preferredDay)
    {
        // Filter slots for the preferred day only
        var slotsOnDay = availableSlots.Where(s => s.DayOfWeek == preferredDay).ToList();

        // DEBUG: Log slot availability
        Console.WriteLine($"[DEBUG] FindAvailableSlotOnDayAsync for {preferredDay}:");
        Console.WriteLine($"  Total available slots in list: {availableSlots.Count}");
        Console.WriteLine($"  Slots on {preferredDay}: {slotsOnDay.Count}");
        Console.WriteLine($"  Course: {offering.Course.Name}, Teacher: {offering.Teacher?.FullName}, Section: {offering.Section?.Name}");

        if (slotsOnDay.Count == 0)
        {
            Console.WriteLine($"  [ERROR] No slots found for {preferredDay} in availableSlots list!");
            return null;
        }

        foreach (var slot in slotsOnDay)
        {
            Console.WriteLine($"  Checking slot: {slot.StartTime} - {slot.EndTime}");

            // Check teacher conflict
            var teacherConflict = await _unitOfWork.ScheduledSlots.HasTeacherConflictAsync(
                offering.TeacherId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                termId);

            if (teacherConflict)
            {
                Console.WriteLine($"    - Teacher conflict detected");
                continue;
            }

            // Check section conflict
            if (offering.SectionId.HasValue)
            {
                var sectionConflict = await _unitOfWork.ScheduledSlots.HasSectionConflictAsync(
                    offering.SectionId.Value,
                    slot.DayOfWeek,
                    slot.StartTime,
                    slot.EndTime,
                    termId);

                if (sectionConflict)
                {
                    Console.WriteLine($"    - Section conflict detected");
                    continue;
                }
            }

            // Slot is available - no conflicts
            Console.WriteLine($"    - AVAILABLE! Returning slot.");
            return slot;
        }

        // No available slot found on this day
        Console.WriteLine($"  [ERROR] All {slotsOnDay.Count} slots on {preferredDay} have conflicts!");
        return null;
    }

    private async Task<Room?> FindSuitableRoomAsync(
        string dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        int termId,
        string? roomType,
        int requiredCapacity)
    {
        var availableRooms = await _unitOfWork.Rooms.GetAvailableRoomsAsync(
            dayOfWeek,
            startTime,
            endTime,
            termId,
            roomType,
            requiredCapacity);

        // Return the smallest room that fits (most efficient use)
        return availableRooms.OrderBy(r => r.Capacity).FirstOrDefault();
    }

    private string? DetermineRoomType(Course course)
    {
        // Check course name or code for lab indicators
        var courseName = course.Name?.ToLower() ?? "";
        var courseCode = course.Code?.ToLower() ?? "";

        if (courseName.Contains("lab") || courseCode.Contains("lab"))
            return "Lab";

        if (courseName.Contains("seminar") || courseCode.Contains("sem"))
            return "Lecture";

        // Default to any room type (null = no filter)
        return null;
    }

    #endregion

    #region Conflict Detection

    public async Task<List<ConflictInfo>> DetectConflictsAsync(int timetableId)
    {
        var conflicts = new List<ConflictInfo>();

        var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(timetableId);
        if (timetable == null)
            return conflicts;

        var slots = timetable.ScheduledSlots.ToList();

        // Group slots by day and time for comparison
        var slotsByTime = slots
            .GroupBy(s => new { s.DayOfWeek, s.StartTime, s.EndTime })
            .Where(g => g.Count() > 1); // Only interested in overlapping slots

        foreach (var group in slotsByTime)
        {
            var conflictingSlots = group.ToList();

            // Check teacher conflicts
            var teacherGroups = conflictingSlots
                .GroupBy(s => s.CourseOffering.TeacherId)
                .Where(g => g.Count() > 1);

            foreach (var teacherGroup in teacherGroups)
            {
                var teacher = teacherGroup.First().CourseOffering.Teacher;
                conflicts.Add(new ConflictInfo
                {
                    ConflictType = "Teacher",
                    DayOfWeek = group.Key.DayOfWeek,
                    StartTime = group.Key.StartTime,
                    EndTime = group.Key.EndTime,
                    Resource = teacher.FullName,
                    AffectedCourses = teacherGroup
                        .Select(s => $"{s.CourseOffering.Course.Code} - {s.CourseOffering.Course.Name}")
                        .ToList(),
                    Description = $"{teacher.FullName} is scheduled for multiple courses at the same time"
                });
            }

            // Check section conflicts
            var sectionGroups = conflictingSlots
                .Where(s => s.CourseOffering.SectionId.HasValue)
                .GroupBy(s => s.CourseOffering.SectionId!.Value)
                .Where(g => g.Count() > 1);

            foreach (var sectionGroup in sectionGroups)
            {
                var section = sectionGroup.First().CourseOffering.Section;
                conflicts.Add(new ConflictInfo
                {
                    ConflictType = "Section",
                    DayOfWeek = group.Key.DayOfWeek,
                    StartTime = group.Key.StartTime,
                    EndTime = group.Key.EndTime,
                    Resource = section?.Name ?? "Unknown Section",
                    AffectedCourses = sectionGroup
                        .Select(s => $"{s.CourseOffering.Course.Code} - {s.CourseOffering.Course.Name}")
                        .ToList(),
                    Description = $"Section {section?.Name} has multiple courses scheduled at the same time"
                });
            }

            // Check room conflicts
            var roomGroups = conflictingSlots
                .Where(s => s.RoomId.HasValue)
                .GroupBy(s => s.RoomId!.Value)
                .Where(g => g.Count() > 1);

            foreach (var roomGroup in roomGroups)
            {
                var room = roomGroup.First().Room;
                conflicts.Add(new ConflictInfo
                {
                    ConflictType = "Room",
                    DayOfWeek = group.Key.DayOfWeek,
                    StartTime = group.Key.StartTime,
                    EndTime = group.Key.EndTime,
                    Resource = room?.Number ?? "Unknown Room",
                    AffectedCourses = roomGroup
                        .Select(s => $"{s.CourseOffering.Course.Code} - {s.CourseOffering.Course.Name}")
                        .ToList(),
                    Description = $"Room {room?.Number} is assigned to multiple courses at the same time"
                });
            }
        }

        return conflicts;
    }

    #endregion

    #region Publishing

    public async Task<PublishTimetableResult> PublishTimetableAsync(int timetableId, int? userId = null)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(timetableId);
            if (timetable == null)
            {
                return new PublishTimetableResult
                {
                    Success = false,
                    Message = "Timetable not found"
                };
            }

            if (timetable.Status == "Published")
            {
                return new PublishTimetableResult
                {
                    Success = false,
                    Message = "Timetable is already published"
                };
            }

            // Check for conflicts
            var conflicts = await DetectConflictsAsync(timetableId);
            if (conflicts.Any())
            {
                return new PublishTimetableResult
                {
                    Success = false,
                    Message = "Cannot publish timetable with unresolved conflicts",
                    RemainingConflicts = conflicts
                };
            }

            // Get term dates
            var term = await _unitOfWork.Terms.GetByIdAsync(timetable.TermId);
            if (term == null)
            {
                return new PublishTimetableResult
                {
                    Success = false,
                    Message = "Term not found"
                };
            }

            // Generate sessions for all scheduled slots
            var sessionsCreated = 0;

            foreach (var slot in timetable.ScheduledSlots)
            {
                var sessions = GenerateSessionsForSlot(slot, term);

                await _unitOfWork.Sessions.AddRangeAsync(sessions);
                sessionsCreated += sessions.Count;
            }

            // Update timetable status
            timetable.Status = "Published";
            timetable.PublishedBy = userId;
            timetable.PublishedAt = DateTime.UtcNow;

            _unitOfWork.Timetables.Update(timetable);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new PublishTimetableResult
            {
                Success = true,
                Message = "Timetable published successfully",
                SessionsCreated = sessionsCreated,
                PublishedAt = timetable.PublishedAt
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return new PublishTimetableResult
            {
                Success = false,
                Message = $"Error publishing timetable: {ex.Message}"
            };
        }
    }

    private List<Session> GenerateSessionsForSlot(ScheduledSlot slot, Term term)
    {
        var sessions = new List<Session>();

        // Map day name to DayOfWeek enum
        var targetDayOfWeek = slot.DayOfWeek switch
        {
            "Sunday" => DayOfWeek.Sunday,
            "Monday" => DayOfWeek.Monday,
            "Tuesday" => DayOfWeek.Tuesday,
            "Wednesday" => DayOfWeek.Wednesday,
            "Thursday" => DayOfWeek.Thursday,
            "Friday" => DayOfWeek.Friday,
            "Saturday" => DayOfWeek.Saturday,
            _ => DayOfWeek.Sunday
        };

        // Find first occurrence of target day in term
        var currentDate = term.StartDate;
        while (currentDate.DayOfWeek != targetDayOfWeek && currentDate <= term.EndDate)
        {
            currentDate = currentDate.AddDays(1);
        }

        // Generate sessions for each week
        while (currentDate <= term.EndDate)
        {
            var session = new Session
            {
                CourseOfferingId = slot.CourseOfferingId,
                SessionDate = currentDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                RoomId = slot.RoomId,
                SessionType = "Regular",
                Status = "Scheduled",
                TimetableId = slot.TimetableId,
                CreatedAt = DateTime.UtcNow
            };

            sessions.Add(session);

            // Move to next week
            currentDate = currentDate.AddDays(7);
        }

        return sessions;
    }

    #endregion

    #region Manual Editing

    public async Task<(bool Success, string Message)> UpdateSlotAsync(UpdateSlotRequest request)
    {
        var slot = await _unitOfWork.ScheduledSlots.GetByIdAsync(
            request.SlotId,
            "Timetable",
            "CourseOffering",
            "CourseOffering.Teacher",
            "CourseOffering.Section");

        if (slot == null)
            return (false, "Slot not found");

        if (slot.Timetable.Status == "Published")
            return (false, "Cannot edit published timetable. Create a new version instead.");

        // Check for conflicts with new time/room
        var teacherConflict = await _unitOfWork.ScheduledSlots.HasTeacherConflictAsync(
            slot.CourseOffering.TeacherId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            slot.Timetable.TermId);

        if (teacherConflict)
            return (false, "Teacher has a conflict at this time");

        if (slot.CourseOffering.SectionId.HasValue)
        {
            var sectionConflict = await _unitOfWork.ScheduledSlots.HasSectionConflictAsync(
                slot.CourseOffering.SectionId.Value,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                slot.Timetable.TermId);

            if (sectionConflict)
                return (false, "Section has a conflict at this time");
        }

        if (request.RoomId.HasValue)
        {
            var roomConflict = await _unitOfWork.ScheduledSlots.HasRoomConflictAsync(
                request.RoomId.Value,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                slot.Timetable.TermId);

            if (roomConflict)
                return (false, "Room is not available at this time");
        }

        // Update slot
        slot.DayOfWeek = request.DayOfWeek;
        slot.StartTime = request.StartTime;
        slot.EndTime = request.EndTime;
        slot.RoomId = request.RoomId;

        _unitOfWork.ScheduledSlots.Update(slot);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Slot updated successfully");
    }

    public async Task<(bool Success, string Message)> DeleteSlotAsync(int slotId)
    {
        var slot = await _unitOfWork.ScheduledSlots.GetByIdAsync(slotId, "Timetable");
        if (slot == null)
            return (false, "Slot not found");

        if (slot.Timetable.Status == "Published")
            return (false, "Cannot delete slots from published timetable");

        _unitOfWork.ScheduledSlots.Delete(slot);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Slot deleted successfully");
    }

    public async Task<(bool Success, string Message)> AddSlotAsync(
        int timetableId,
        int courseOfferingId,
        TimeSlotDto timeSlot,
        int? roomId)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdAsync(timetableId);
        if (timetable == null)
            return (false, "Timetable not found");

        if (timetable.Status == "Published")
            return (false, "Cannot add slots to published timetable");

        var offering = await _unitOfWork.CourseOfferings.GetByIdWithDetailsAsync(courseOfferingId);
        if (offering == null)
            return (false, "Course offering not found");

        // Check teacher conflict
        var teacherConflict = await _unitOfWork.ScheduledSlots.HasTeacherConflictAsync(
            offering.TeacherId,
            timeSlot.DayOfWeek,
            timeSlot.StartTime,
            timeSlot.EndTime,
            timetable.TermId);

        if (teacherConflict)
            return (false, "Teacher has a conflict at this time");

        // Check section conflict
        if (offering.SectionId.HasValue)
        {
            var sectionConflict = await _unitOfWork.ScheduledSlots.HasSectionConflictAsync(
                offering.SectionId.Value,
                timeSlot.DayOfWeek,
                timeSlot.StartTime,
                timeSlot.EndTime,
                timetable.TermId);

            if (sectionConflict)
                return (false, "Section has a conflict at this time");
        }

        // Check room conflict
        if (roomId.HasValue)
        {
            var roomConflict = await _unitOfWork.ScheduledSlots.HasRoomConflictAsync(
                roomId.Value,
                timeSlot.DayOfWeek,
                timeSlot.StartTime,
                timeSlot.EndTime,
                timetable.TermId);

            if (roomConflict)
                return (false, "Room is not available at this time");
        }

        var slot = new ScheduledSlot
        {
            TimetableId = timetableId,
            CourseOfferingId = courseOfferingId,
            DayOfWeek = timeSlot.DayOfWeek,
            StartTime = timeSlot.StartTime,
            EndTime = timeSlot.EndTime,
            RoomId = roomId
        };

        await _unitOfWork.ScheduledSlots.AddAsync(slot);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Slot added successfully");
    }

    public async Task<(bool Success, string Message)> SwapSlotsAsync(SwapSlotsRequest request)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Fetch both slots with related data
            var slot1 = await _unitOfWork.Repository<ScheduledSlot>()
                .GetByIdAsync(request.Slot1Id, "CourseOffering.Teacher", "CourseOffering.Course", "Timetable", "CourseOffering.Section");
            var slot2 = await _unitOfWork.Repository<ScheduledSlot>()
                .GetByIdAsync(request.Slot2Id, "CourseOffering.Teacher", "CourseOffering.Course", "Timetable", "CourseOffering.Section");

            if (slot1 == null || slot2 == null)
            {
                await transaction.RollbackAsync();
                return (false, "One or both slots not found");
            }

            // Verify both slots belong to same timetable
            if (slot1.TimetableId != request.TimetableId || slot2.TimetableId != request.TimetableId)
            {
                await transaction.RollbackAsync();
                return (false, "Slots must belong to the same timetable");
            }

            // Check if timetable is published
            var timetable = await _unitOfWork.Timetables.GetByIdAsync(request.TimetableId);
            if (timetable == null)
            {
                await transaction.RollbackAsync();
                return (false, "Timetable not found");
            }

            if (timetable.Status == "Published")
            {
                await transaction.RollbackAsync();
                return (false, "Cannot modify published timetable");
            }

            // Store slot1's original values
            var tempDay = slot1.DayOfWeek;
            var tempStart = slot1.StartTime;
            var tempEnd = slot1.EndTime;
            var tempRoom = slot1.RoomId;

            // Swap: slot1 gets slot2's position
            slot1.DayOfWeek = slot2.DayOfWeek;
            slot1.StartTime = slot2.StartTime;
            slot1.EndTime = slot2.EndTime;
            slot1.RoomId = slot2.RoomId;

            // Swap: slot2 gets slot1's original position
            slot2.DayOfWeek = tempDay;
            slot2.StartTime = tempStart;
            slot2.EndTime = tempEnd;
            slot2.RoomId = tempRoom;

            // Validate conflicts after swap
            var termId = timetable.TermId;

            // Check slot1's new position (BLOCK swap if conflicts detected)
            var slot1Conflicts = await ValidateSlotPosition(slot1, termId);
            if (slot1Conflicts != null)
            {
                await transaction.RollbackAsync();
                return (false, $"Cannot swap: {slot1.CourseOffering.Course.Name} would conflict - {slot1Conflicts}");
            }

            // Check slot2's new position (BLOCK swap if conflicts detected)
            var slot2Conflicts = await ValidateSlotPosition(slot2, termId);
            if (slot2Conflicts != null)
            {
                await transaction.RollbackAsync();
                return (false, $"Cannot swap: {slot2.CourseOffering.Course.Name} would conflict - {slot2Conflicts}");
            }

            // Update both slots
            _unitOfWork.ScheduledSlots.Update(slot1);
            _unitOfWork.ScheduledSlots.Update(slot2);
            await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, "Slots swapped successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error swapping slots: {ex.Message}");
        }
    }

    // Helper method to validate a slot's position for conflicts
    private async Task<string?> ValidateSlotPosition(ScheduledSlot slot, int termId)
    {
        var teacherId = slot.CourseOffering.TeacherId;
        var sectionId = slot.CourseOffering.SectionId;

        // Check teacher conflict (excluding this slot itself)
        var teacherConflict = await _unitOfWork.ScheduledSlots.HasTeacherConflictAsync(
            teacherId, slot.DayOfWeek, slot.StartTime, slot.EndTime, termId, slot.Id);
        if (teacherConflict)
            return "Teacher has another class at this time";

        // Check section conflict (excluding this slot itself)
        if (sectionId.HasValue)
        {
            var sectionConflict = await _unitOfWork.ScheduledSlots.HasSectionConflictAsync(
                sectionId.Value, slot.DayOfWeek, slot.StartTime, slot.EndTime, termId, slot.Id);
            if (sectionConflict)
                return "Section has another class at this time";
        }

        // Check room conflict (excluding this slot itself)
        if (slot.RoomId.HasValue)
        {
            var roomConflict = await _unitOfWork.ScheduledSlots.HasRoomConflictAsync(
                slot.RoomId.Value, slot.DayOfWeek, slot.StartTime, slot.EndTime, termId, slot.Id);
            if (roomConflict)
                return "Room is already booked at this time";
        }

        return null;
    }

    #endregion

    #region Retrieval

    public async Task<Timetable?> GetTimetableByIdAsync(int id)
    {
        return await _unitOfWork.Timetables.GetByIdWithDetailsAsync(id);
    }

    public async Task<Timetable?> GetTimetableWithSlotsAsync(int id)
    {
        return await _unitOfWork.Timetables.GetByIdWithSlotsAsync(id);
    }

    public async Task<IEnumerable<ScheduledSlotDto>> GetTimetableSlotsAsync(int id)
    {
        var slots = await _unitOfWork.ScheduledSlots.GetByTimetableAsync(id);

        return slots.Select(s => new ScheduledSlotDto
        {
            Id = s.Id,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            CourseName = s.CourseOffering.Course.Name,
            CourseCode = s.CourseOffering.Course.Code,
            TeacherName = s.CourseOffering.Teacher.FullName,
            RoomNumber = s.Room?.Number,
            RoomCapacity = s.Room?.Capacity
        }).ToList();
    }

    public async Task<(bool Success, string Message)> DeleteTimetableAsync(int id)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdAsync(id);
        if (timetable == null)
            return (false, "Timetable not found");

        if (timetable.Status == "Published")
            return (false, "Cannot delete published timetable");

        _unitOfWork.Timetables.Delete(timetable);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Timetable deleted successfully");
    }

    #endregion
}
