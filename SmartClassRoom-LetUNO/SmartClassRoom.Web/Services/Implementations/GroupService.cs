using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Groups;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;

    public GroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Group>> GetGroupsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? sectionId = null,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        var query = await _unitOfWork.Groups
            .GetAllAsync("Section", "Section.Level", "Section.Level.Department", "Students");

        // Apply filters
        var filtered = query.AsEnumerable();

        if (sectionId.HasValue)
            filtered = filtered.Where(g => g.SectionId == sectionId.Value);

        if (levelId.HasValue)
            filtered = filtered.Where(g => g.Section.LevelId == levelId.Value);

        if (departmentId.HasValue)
            filtered = filtered.Where(g => g.Section.Level.DepartmentId == departmentId.Value);

        if (isActive.HasValue)
            filtered = filtered.Where(g => g.IsActive == isActive.Value);

        // Apply sorting
        IEnumerable<Group> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(g => g.Name),
            ("name", false) => filtered.OrderByDescending(g => g.Name),
            ("code", true) => filtered.OrderBy(g => g.Code),
            ("code", false) => filtered.OrderByDescending(g => g.Code),
            ("section", true) => filtered.OrderBy(g => g.Section.Name),
            ("section", false) => filtered.OrderByDescending(g => g.Section.Name),
            ("level", true) => filtered.OrderBy(g => g.Section.Level.Name),
            ("level", false) => filtered.OrderByDescending(g => g.Section.Level.Name),
            ("capacity", true) => filtered.OrderBy(g => g.Capacity),
            ("capacity", false) => filtered.OrderByDescending(g => g.Capacity),
            ("status", true) => filtered.OrderBy(g => g.IsActive),
            ("status", false) => filtered.OrderByDescending(g => g.IsActive),
            _ => filtered.OrderBy(g => g.Name) // Default sort
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Group>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        return await _unitOfWork.Groups.GetByIdAsync(id);
    }

    public async Task<Group?> GetGroupWithDetailsAsync(int id)
    {
        return await _unitOfWork.Groups.GetGroupWithDetailsAsync(id);
    }

    public async Task<GroupStatistics> GetGroupStatisticsAsync(int id)
    {
        return await _unitOfWork.Groups.GetGroupStatisticsAsync(id);
    }

    public async Task<(bool Success, string Message)> CreateGroupAsync(CreateGroupRequest request)
    {
        // Validate section exists and is active
        var section = await _unitOfWork.Sections.GetByIdAsync(request.SectionId);
        if (section == null)
            return (false, "Section not found.");

        if (!section.IsActive)
            return (false, $"Cannot create group. Section '{section.Name}' is inactive.");

        // Validate code uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            if (await CodeExistsAsync(request.Code))
                return (false, $"Group code '{request.Code}' already exists.");
        }

        var group = new Group
        {
            Name = request.Name,
            Code = request.Code,
            Capacity = request.Capacity,
            SectionId = request.SectionId,
            IsActive = request.IsActive
        };

        await _unitOfWork.Groups.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Group created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateGroupAsync(EditGroupRequest request)
    {
        var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(request.Id);
        if (group == null)
            return (false, "Group not found.");

        // Validate section exists and is active
        var section = await _unitOfWork.Sections.GetByIdAsync(request.SectionId);
        if (section == null)
            return (false, "Section not found.");

        if (!section.IsActive)
            return (false, $"Cannot assign to inactive section '{section.Name}'.");

        // Validate code uniqueness if changed
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != group.Code)
        {
            if (await CodeExistsAsync(request.Code, request.Id))
                return (false, $"Group code '{request.Code}' already exists.");
        }

        // Validate capacity if specified
        if (request.Capacity.HasValue)
        {
            var currentStudentsCount = group.Students.Count;
            if (request.Capacity.Value < currentStudentsCount)
                return (false, $"Cannot set capacity to {request.Capacity.Value}. Group has {currentStudentsCount} student(s).");
        }

        // Update properties
        group.Name = request.Name;
        group.Code = request.Code;
        group.Capacity = request.Capacity;
        group.SectionId = request.SectionId;
        group.IsActive = request.IsActive;

        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Group updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeactivateGroupAsync(int id)
    {
        var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(id);
        if (group == null)
            return (false, "Group not found.");

        if (!group.IsActive)
            return (false, "Group is already inactive.");

        // Check for active students
        var activeStudents = group.Students.Count(s => s.IsActive);
        if (activeStudents > 0)
            return (false, $"Cannot deactivate group. It has {activeStudents} active student(s).");

        group.IsActive = false;
        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Group deactivated successfully.");
    }

    public async Task<(bool Success, string Message)> ActivateGroupAsync(int id)
    {
        var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(id);
        if (group == null)
            return (false, "Group not found.");

        if (group.IsActive)
            return (false, "Group is already active.");

        // Validate parent hierarchy is active (Section → Level → Department)
        if (!group.Section.IsActive)
            return (false, $"Cannot activate group. Parent section '{group.Section.Name}' is inactive.");

        if (!group.Section.Level.IsActive)
            return (false, $"Cannot activate group. Parent level '{group.Section.Level.Name}' is inactive.");

        if (!group.Section.Level.Department.IsActive)
            return (false, $"Cannot activate group. Parent department '{group.Section.Level.Department.Name}' is inactive.");

        group.IsActive = true;
        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Group activated successfully.");
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var group = await _unitOfWork.Groups.GetByCodeAsync(code);
        if (group == null)
            return false;

        if (excludeId.HasValue && group.Id == excludeId.Value)
            return false;

        return true;
    }
}
