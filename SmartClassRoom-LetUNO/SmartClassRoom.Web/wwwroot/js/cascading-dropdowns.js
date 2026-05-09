// Cascading Dropdowns for Department -> Level -> Section -> Group
$(document).ready(function () {
    const departmentDropdown = $('#DepartmentId');
    const levelDropdown = $('#LevelId');
    const sectionDropdown = $('#SectionId');
    const groupDropdown = $('#GroupId');

    // Initialize: Disable dependent dropdowns
    if (departmentDropdown.length) {
        const hasPreselectedDepartment = departmentDropdown.val();
        if (!hasPreselectedDepartment) {
            disableDropdown(levelDropdown);
            disableDropdown(sectionDropdown);
            disableDropdown(groupDropdown);
        }
    }

    // Department Change -> Load Levels
    departmentDropdown.on('change', function () {
        const departmentId = $(this).val();

        // Reset and disable dependent dropdowns
        clearDropdown(levelDropdown);
        clearDropdown(sectionDropdown);
        clearDropdown(groupDropdown);

        if (departmentId) {
            loadLevels(departmentId);
        } else {
            disableDropdown(levelDropdown);
            disableDropdown(sectionDropdown);
            disableDropdown(groupDropdown);
        }
    });

    // Level Change -> Load Sections
    levelDropdown.on('change', function () {
        const levelId = $(this).val();

        // Reset and disable dependent dropdowns
        clearDropdown(sectionDropdown);
        clearDropdown(groupDropdown);

        if (levelId) {
            loadSections(levelId);
        } else {
            disableDropdown(sectionDropdown);
            disableDropdown(groupDropdown);
        }
    });

    // Section Change -> Load Groups
    sectionDropdown.on('change', function () {
        const sectionId = $(this).val();

        // Reset dropdown
        clearDropdown(groupDropdown);

        if (sectionId) {
            loadGroups(sectionId);
        } else {
            disableDropdown(groupDropdown);
        }
    });

    // Load Levels for selected Department
    function loadLevels(departmentId) {
        showLoading(levelDropdown);

        $.ajax({
            url: `/api/academic/levels/${departmentId}`,
            type: 'GET',
            success: function (data) {
                populateDropdown(levelDropdown, data, 'Level');
                enableDropdown(levelDropdown);
            },
            error: function () {
                alert('Failed to load levels. Please try again.');
                disableDropdown(levelDropdown);
            }
        });
    }

    // Load Sections for selected Level
    function loadSections(levelId) {
        showLoading(sectionDropdown);

        $.ajax({
            url: `/api/academic/sections/${levelId}`,
            type: 'GET',
            success: function (data) {
                populateDropdown(sectionDropdown, data, 'Section');
                enableDropdown(sectionDropdown);
            },
            error: function () {
                alert('Failed to load sections. Please try again.');
                disableDropdown(sectionDropdown);
            }
        });
    }

    // Load Groups for selected Section
    function loadGroups(sectionId) {
        showLoading(groupDropdown);

        $.ajax({
            url: `/api/academic/groups/${sectionId}`,
            type: 'GET',
            success: function (data) {
                populateDropdown(groupDropdown, data, 'Group');
                enableDropdown(groupDropdown);
            },
            error: function () {
                alert('Failed to load groups. Please try again.');
                disableDropdown(groupDropdown);
            }
        });
    }

    // Helper: Populate dropdown with data
    function populateDropdown(dropdown, data, placeholder) {
        dropdown.empty();
        dropdown.append(`<option value="">-- Select ${placeholder} --</option>`);

        if (data && data.length > 0) {
            $.each(data, function (index, item) {
                dropdown.append(`<option value="${item.value}">${item.text}</option>`);
            });
        } else {
            dropdown.append(`<option value="" disabled>No ${placeholder}s available</option>`);
        }
    }

    // Helper: Clear dropdown
    function clearDropdown(dropdown) {
        const placeholder = dropdown.find('option:first').text();
        dropdown.empty();
        dropdown.append(`<option value="">${placeholder}</option>`);
    }

    // Helper: Show loading state
    function showLoading(dropdown) {
        dropdown.empty();
        dropdown.append('<option value="">Loading...</option>');
        dropdown.prop('disabled', true);
    }

    // Helper: Enable dropdown
    function enableDropdown(dropdown) {
        dropdown.prop('disabled', false);
    }

    // Helper: Disable dropdown
    function disableDropdown(dropdown) {
        dropdown.prop('disabled', true);
    }
});
