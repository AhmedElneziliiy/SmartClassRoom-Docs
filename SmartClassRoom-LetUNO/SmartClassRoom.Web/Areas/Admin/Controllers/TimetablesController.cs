using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.ViewModels.Timetables;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class TimetablesController : Controller
{
    private readonly ITimetableService _timetableService;
    private readonly IUnitOfWork _unitOfWork;

    public TimetablesController(
        ITimetableService timetableService,
        IUnitOfWork unitOfWork)
    {
        _timetableService = timetableService;
        _unitOfWork = unitOfWork;
    }

    // GET: Admin/Timetables
    public async Task<IActionResult> Index(
        int? departmentId = null,
        int? levelId = null,
        int? sectionId = null,
        int? termId = null,
        string? status = null)
    {
        // Get filter dropdowns
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
        var levels = await _unitOfWork.Repository<Level>().GetAllAsync();
        var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
        var terms = await _unitOfWork.Repository<Term>().GetAllAsync();

        ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive), "Id", "Name", departmentId);
        ViewBag.Levels = new SelectList(levels.Where(l => l.IsActive), "Id", "Name", levelId);
        ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive), "Id", "Name", sectionId);
        ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive), "Id", "Name", termId);
        ViewBag.Statuses = new SelectList(new[] { "Draft", "Published", "Archived" }, status);

        // Get filtered timetables
        var timetables = await _unitOfWork.Timetables.GetAllAsync();

        var filteredTimetables = timetables.Where(t =>
            (!departmentId.HasValue || t.DepartmentId == departmentId) &&
            (!levelId.HasValue || t.LevelId == levelId) &&
            (!sectionId.HasValue || t.SectionId == sectionId) &&
            (!termId.HasValue || t.TermId == termId) &&
            (string.IsNullOrEmpty(status) || t.Status == status)
        ).OrderByDescending(t => t.CreatedAt).ToList();

        return View(filteredTimetables);
    }

    // GET: Admin/Timetables/Generate
    public async Task<IActionResult> Generate()
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
        var levels = await _unitOfWork.Repository<Level>().GetAllAsync();
        var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
        var terms = await _unitOfWork.Repository<Term>().GetAllAsync();

        ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive), "Id", "Name");
        ViewBag.Levels = new SelectList(levels.Where(l => l.IsActive), "Id", "Name");
        ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive), "Id", "Name");
        ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive && t.Status == "Active"), "Id", "Name");

        return View(new GenerateTimetableRequest());
    }

    // POST: Admin/Timetables/Generate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(GenerateTimetableRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
            var levels = await _unitOfWork.Repository<Level>().GetAllAsync();
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
            var terms = await _unitOfWork.Repository<Term>().GetAllAsync();

            ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive), "Id", "Name", request.DepartmentId);
            ViewBag.Levels = new SelectList(levels.Where(l => l.IsActive), "Id", "Name", request.LevelId);
            ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive), "Id", "Name", request.SectionId);
            ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive), "Id", "Name", request.TermId);

            return View(request);
        }

        var result = await _timetableService.GenerateTimetableAsync(request);

        if (!result.Success)
        {
            // Add error to ModelState so it displays with form
            ModelState.AddModelError("", result.Message);

            // Reload dropdowns for form display
            var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
            var levels = await _unitOfWork.Repository<Level>().GetAllAsync();
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
            var terms = await _unitOfWork.Repository<Term>().GetAllAsync();

            ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive), "Id", "Name", request.DepartmentId);
            ViewBag.Levels = new SelectList(levels.Where(l => l.IsActive), "Id", "Name", request.LevelId);
            ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive), "Id", "Name", request.SectionId);
            ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive), "Id", "Name", request.TermId);

            // Return view with model to preserve form data
            return View(request);
        }

        if (result.Conflicts.Any())
        {
            TempData["WarningMessage"] = $"Timetable generated with {result.Conflicts.Count} conflicts. Please review and fix before publishing.";
        }
        else
        {
            TempData["SuccessMessage"] = "Timetable generated successfully with no conflicts!";
        }

        return RedirectToAction(nameof(Details), new { id = result.TimetableId });
    }

    // GET: Admin/Timetables/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(id);
        if (timetable == null)
        {
            TempData["ErrorMessage"] = "Timetable not found.";
            return RedirectToAction(nameof(Index));
        }

        // Detect conflicts
        var conflicts = await _timetableService.DetectConflictsAsync(id);
        ViewBag.Conflicts = conflicts;
        ViewBag.HasConflicts = conflicts.Any();

        // Pass term dates for calendar initialization
        ViewBag.TermStartDate = timetable.Term.StartDate.ToString("yyyy-MM-dd");
        ViewBag.TermEndDate = timetable.Term.EndDate.ToString("yyyy-MM-dd");

        // Calculate earliest and latest times from slots for calendar display
        var slots = timetable.ScheduledSlots;
        if (slots.Any())
        {
            var earliestTime = slots.Min(s => s.StartTime);
            var latestTime = slots.Max(s => s.EndTime);

            // Add 30-minute buffer for better display
            ViewBag.EarliestTime = earliestTime.Add(TimeSpan.FromMinutes(-30)).ToString(@"hh\:mm\:ss");
            ViewBag.LatestTime = latestTime.Add(TimeSpan.FromMinutes(30)).ToString(@"hh\:mm\:ss");
        }
        else
        {
            // Defaults if no slots
            ViewBag.EarliestTime = "08:00:00";
            ViewBag.LatestTime = "17:00:00";
        }

        // FIXED: Always hide Friday (day 5) - show Saturday(6) through Thursday(4)
        ViewBag.HiddenDays = new List<int> { 5 }; // Friday is hidden

        // Pass the original selected working days to frontend for visual styling
        ViewBag.SelectedWorkingDays = timetable.WorkingDays;

        // Also pass time configuration for display
        ViewBag.StartTime = timetable.StartTime.ToString(@"hh\:mm\:ss");
        ViewBag.EndTime = timetable.EndTime.ToString(@"hh\:mm\:ss");
        ViewBag.SlotDuration = timetable.SlotDurationMinutes;

        // Prepare events for FullCalendar - Generate recurring events for all weeks in the term
        var events = new List<object>();
        int eventCounter = 0;

        Console.WriteLine($"========== GENERATING EVENTS FOR TERM ==========");
        Console.WriteLine($"Term: {timetable.Term.Name}");
        Console.WriteLine($"Start Date: {timetable.Term.StartDate:yyyy-MM-dd}");
        Console.WriteLine($"End Date: {timetable.Term.EndDate:yyyy-MM-dd}");
        Console.WriteLine($"Total Scheduled Slots: {timetable.ScheduledSlots.Count}");

        foreach (var slot in timetable.ScheduledSlots
            .Where(slot => slot.CourseOffering != null &&
                          slot.CourseOffering.Course != null &&
                          slot.CourseOffering.Teacher != null))
        {
            // Generate events for each week in the term
            var allDatesForSlot = GetAllDatesForSlot(timetable.Term.StartDate, timetable.Term.EndDate, slot.DayOfWeek, slot.StartTime, slot.EndTime);

            Console.WriteLine($"Slot: {slot.CourseOffering.Course.Name} on {slot.DayOfWeek} at {slot.StartTime} - Generated {allDatesForSlot.Count} events");

            foreach (var (start, end) in allDatesForSlot)
            {
                eventCounter++;
                events.Add(new
                {
                    id = $"slot-{slot.Id}-evt-{eventCounter}", // Unique ID for each recurring instance
                    title = $"{slot.CourseOffering.Course.Name}",
                    start = start,
                    end = end,
                    slotId = slot.Id, // Keep original slot ID as separate property
                    courseOfferingId = slot.CourseOfferingId,
                    courseName = slot.CourseOffering.Course.Name,
                    courseCode = slot.CourseOffering.Course.Code,
                    teacherName = slot.CourseOffering.Teacher.FullName,
                    roomId = slot.RoomId,
                    roomNumber = slot.Room?.Number ?? "TBD",
                    roomName = slot.Room?.Name ?? "TBD",
                    dayOfWeek = slot.DayOfWeek,
                    startTime = slot.StartTime.ToString(@"hh\:mm"),
                    endTime = slot.EndTime.ToString(@"hh\:mm")
                });
            }
        }

        Console.WriteLine($"Total events generated: {events.Count}");
        Console.WriteLine($"================================================");

        ViewBag.Events = events;

        return View(timetable);
    }

    // GET: Admin/Timetables/Conflicts/5
    public async Task<IActionResult> Conflicts(int id)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(id);
        if (timetable == null)
        {
            TempData["ErrorMessage"] = "Timetable not found.";
            return RedirectToAction(nameof(Index));
        }

        var conflicts = await _timetableService.DetectConflictsAsync(id);

        ViewBag.Timetable = timetable;
        return View(conflicts);
    }

    // POST: Admin/Timetables/Publish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var result = await _timetableService.PublishTimetableAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = $"Timetable published successfully! {result.SessionsCreated} sessions created.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Admin/Timetables/UpdateSlot
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateSlot([FromBody] UpdateSlotRequest request)
    {
        var result = await _timetableService.UpdateSlotAsync(request);
        return Json(new { success = result.Success, message = result.Message });
    }

    // POST: Admin/Timetables/DeleteSlot/5
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        var result = await _timetableService.DeleteSlotAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }

    // POST: Admin/Timetables/SwapSlots
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SwapSlots([FromBody] SwapSlotsRequest request)
    {
        var result = await _timetableService.SwapSlotsAsync(request);
        return Json(new { success = result.Success, message = result.Message });
    }

    // POST: Admin/Timetables/AddSlot
    [HttpPost]
    public async Task<IActionResult> AddSlot(int timetableId, int courseOfferingId, [FromBody] TimeSlotDto timeSlot, int? roomId)
    {
        var result = await _timetableService.AddSlotAsync(timetableId, courseOfferingId, timeSlot, roomId);
        return Json(new { success = result.Success, message = result.Message });
    }

    // DELETE: Admin/Timetables/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(id);
        if (timetable == null)
        {
            TempData["ErrorMessage"] = "Timetable not found.";
            return RedirectToAction(nameof(Index));
        }

        // If published, delete all associated sessions first
        if (timetable.Status == "Published")
        {
            var sessions = await _unitOfWork.Repository<Session>()
                .FindAsync(s => s.TimetableId == id);

            foreach (var session in sessions)
            {
                _unitOfWork.Repository<Session>().Delete(session);
            }
        }

        // Delete all scheduled slots to avoid foreign key constraint violation
        foreach (var slot in timetable.ScheduledSlots.ToList())
        {
            _unitOfWork.Repository<ScheduledSlot>().Delete(slot);
        }

        _unitOfWork.Timetables.Delete(timetable);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Timetable deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Timetables/GetSlots/5 - Returns slots as JSON for calendar
    [HttpGet]
    public async Task<IActionResult> GetSlots(int id)
    {
        var timetable = await _unitOfWork.Timetables.GetByIdWithSlotsAsync(id);
        if (timetable == null)
        {
            return NotFound();
        }

        var events = timetable.ScheduledSlots
            .Where(slot => slot.CourseOffering != null &&
                          slot.CourseOffering.Course != null &&
                          slot.CourseOffering.Teacher != null)
            .Select(slot => new
            {
                id = slot.Id.ToString(),
                // Make title unique to prevent FullCalendar grouping
                title = $"{slot.CourseOffering.Course.Name} - {slot.DayOfWeek} {slot.StartTime:hh\\:mm}",
                start = GetDateTimeForSlot(timetable.Term.StartDate, slot.DayOfWeek, slot.StartTime),
                end = GetDateTimeForSlot(timetable.Term.StartDate, slot.DayOfWeek, slot.EndTime),
                groupId = (string?)null,
                slotId = slot.Id,
                courseOfferingId = slot.CourseOfferingId,
                courseName = slot.CourseOffering.Course.Name,
                courseCode = slot.CourseOffering.Course.Code,
                teacherName = slot.CourseOffering.Teacher.FullName,
                roomId = slot.RoomId,
                roomNumber = slot.Room?.Number ?? "TBD",
                roomName = slot.Room?.Name ?? "TBD",
                dayOfWeek = slot.DayOfWeek,
                startTime = slot.StartTime.ToString(@"hh\:mm"),
                endTime = slot.EndTime.ToString(@"hh\:mm")
            }).ToList();

        return Json(events);
    }

    private DateTime GetDateTimeForSlot(DateTime termStartDate, string dayOfWeek, TimeSpan time)
    {
        // Find first occurrence of the day in the term
        var targetDay = dayOfWeek switch
        {
            "Sunday" => DayOfWeek.Sunday,
            "Monday" => DayOfWeek.Monday,
            "Tuesday" => DayOfWeek.Tuesday,
            "Wednesday" => DayOfWeek.Wednesday,
            "Thursday" => DayOfWeek.Thursday,
            _ => DayOfWeek.Sunday
        };

        var date = termStartDate;
        while (date.DayOfWeek != targetDay)
        {
            date = date.AddDays(1);
        }

        return date.Date.Add(time);
    }

    private List<(DateTime start, DateTime end)> GetAllDatesForSlot(DateTime termStartDate, DateTime termEndDate, string dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        var dates = new List<(DateTime start, DateTime end)>();

        // Convert day of week string to DayOfWeek enum
        var targetDay = dayOfWeek switch
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

        // Find first occurrence of the target day on or after term start
        var currentDate = termStartDate;
        while (currentDate.DayOfWeek != targetDay)
        {
            currentDate = currentDate.AddDays(1);
        }

        // If the first occurrence is already after term end, return empty list
        if (currentDate > termEndDate)
        {
            return dates;
        }

        // Generate events for all weeks - keep going until we pass the term end date
        while (currentDate <= termEndDate)
        {
            dates.Add((
                start: currentDate.Date.Add(startTime),
                end: currentDate.Date.Add(endTime)
            ));

            // Move to next week (same day, 7 days later)
            currentDate = currentDate.AddDays(7);
        }

        return dates;
    }
}
