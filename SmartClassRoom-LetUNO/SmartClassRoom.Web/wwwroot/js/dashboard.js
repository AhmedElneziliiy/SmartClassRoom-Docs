/**
 * Dashboard JavaScript Module
 * Handles charts, statistics, and real-time updates for the admin dashboard
 */

(function () {
    'use strict';

    // Chart instances
    let attendanceChart = null;
    let attendancePieChart = null;

    // Configuration
    const config = {
        refreshInterval: 60000, // 60 seconds
        apiEndpoints: {
            todaySessions: '/api/dashboard/today-sessions',
            attendanceStats: '/api/dashboard/attendance-stats'
        }
    };

    /**
     * Creates the attendance line chart showing trends over time
     */
    function createAttendanceChart() {
        const ctx = document.getElementById('attendanceChart');
        if (!ctx) return;

        // Sample data - replace with actual API call
        const data = {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
            datasets: [{
                label: 'Present',
                data: [85, 92, 88, 90, 87, 0, 0],
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                tension: 0.4,
                fill: true
            }, {
                label: 'Late',
                data: [8, 5, 7, 6, 9, 0, 0],
                borderColor: '#ffc107',
                backgroundColor: 'rgba(255, 193, 7, 0.1)',
                tension: 0.4,
                fill: true
            }, {
                label: 'Absent',
                data: [7, 3, 5, 4, 4, 0, 0],
                borderColor: '#dc3545',
                backgroundColor: 'rgba(220, 53, 69, 0.1)',
                tension: 0.4,
                fill: true
            }]
        };

        attendanceChart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 10
                        }
                    }
                }
            }
        });
    }

    /**
     * Creates the attendance pie chart showing today's status distribution
     */
    function createAttendancePieChart() {
        const ctx = document.getElementById('attendancePieChart');
        if (!ctx) return;

        const data = {
            labels: ['Present', 'Late', 'Absent', 'Excused'],
            datasets: [{
                data: [87, 6, 4, 3],
                backgroundColor: [
                    '#28a745',
                    '#ffc107',
                    '#dc3545',
                    '#17a2b8'
                ],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        };

        attendancePieChart = new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return context.label + ': ' + context.parsed + '%';
                            }
                        }
                    }
                }
            }
        });

        // Update legend values
        updatePieChartLegend(87, 6, 4, 3);
    }

    /**
     * Updates the custom legend for the pie chart
     */
    function updatePieChartLegend(present, late, absent, excused) {
        const presentEl = document.getElementById('presentCount');
        const lateEl = document.getElementById('lateCount');
        const absentEl = document.getElementById('absentCount');
        const excusedEl = document.getElementById('excusedCount');

        if (presentEl) presentEl.textContent = present + '%';
        if (lateEl) lateEl.textContent = late + '%';
        if (absentEl) absentEl.textContent = absent + '%';
        if (excusedEl) excusedEl.textContent = excused + '%';
    }

    /**
     * Loads today's sessions from the API
     */
    async function loadTodaySessions() {
        const container = document.getElementById('todaySessionsContainer');
        if (!container) return;

        try {
            const response = await fetch(config.apiEndpoints.todaySessions);

            if (!response.ok) {
                throw new Error('Failed to fetch sessions');
            }

            const sessions = await response.json();

            if (sessions.length === 0) {
                container.innerHTML = `
                    <div class="text-center py-5">
                        <i class="bi bi-calendar-x" style="font-size: 3rem; color: #dee2e6;"></i>
                        <h5 class="text-muted mt-3">No sessions scheduled for today</h5>
                    </div>
                `;
                return;
            }

            const tableHtml = buildSessionsTable(sessions);
            container.innerHTML = tableHtml;
        } catch (error) {
            console.error('Error loading sessions:', error);
            container.innerHTML = `
                <div class="alert alert-warning m-3">
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    Unable to load today's sessions. Please refresh the page.
                </div>
            `;
        }
    }

    /**
     * Builds the HTML table for sessions
     */
    function buildSessionsTable(sessions) {
        let html = '<div class="table-responsive"><table class="table table-hover mb-0">';
        html += '<thead class="table-light"><tr>';
        html += '<th>Time</th><th>Course</th><th>Teacher</th><th>Room</th><th>Status</th><th>Attendance</th>';
        html += '</tr></thead><tbody>';

        sessions.forEach(session => {
            const statusBadge = getStatusBadge(session.status);
            const attendancePercent = session.enrolledStudents > 0
                ? Math.round((session.attendedStudents / session.enrolledStudents) * 100)
                : 0;

            html += `<tr>
                <td><i class="bi bi-clock me-1"></i>${session.startTime} - ${session.endTime}</td>
                <td><strong>${session.courseCode}</strong><br><small class="text-muted">${session.courseName}</small></td>
                <td><i class="bi bi-person me-1"></i>${session.teacherName}</td>
                <td><i class="bi bi-geo-alt me-1"></i>${session.roomName || 'N/A'}</td>
                <td>${statusBadge}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <small class="me-2">${session.attendedStudents}/${session.enrolledStudents}</small>
                        <div class="progress" style="width: 60px; height: 6px;">
                            <div class="progress-bar bg-success" style="width: ${attendancePercent}%"></div>
                        </div>
                    </div>
                </td>
            </tr>`;
        });

        html += '</tbody></table></div>';
        return html;
    }

    /**
     * Returns the appropriate status badge HTML
     */
    function getStatusBadge(status) {
        const badges = {
            'Scheduled': '<span class="badge bg-secondary">Scheduled</span>',
            'InProgress': '<span class="badge bg-success"><i class="bi bi-circle-fill me-1" style="font-size: 0.5rem;"></i>In Progress</span>',
            'Completed': '<span class="badge bg-primary">Completed</span>',
            'Cancelled': '<span class="badge bg-danger">Cancelled</span>'
        };
        return badges[status] || '<span class="badge bg-secondary">Unknown</span>';
    }

    /**
     * Sets up period change handlers for the attendance chart
     */
    function setupPeriodChangeHandlers() {
        const buttons = document.querySelectorAll('#attendanceChartPeriod button');

        buttons.forEach(button => {
            button.addEventListener('click', function () {
                // Remove active class from all buttons
                buttons.forEach(btn => btn.classList.remove('active'));

                // Add active class to clicked button
                this.classList.add('active');

                // Get the selected period
                const period = this.dataset.period;

                // TODO: Reload chart data based on selected period
                console.log('Load data for period:', period);

                // You can add an API call here to fetch data for the selected period
                // Example: loadAttendanceData(period);
            });
        });
    }

    /**
     * Starts auto-refresh for dashboard data
     */
    function startAutoRefresh() {
        setInterval(function () {
            loadTodaySessions();
            // You can add more refresh functions here
        }, config.refreshInterval);
    }

    /**
     * Initializes the dashboard
     */
    function init() {
        // Wait for Chart.js to be loaded
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded. Please include Chart.js before dashboard.js');
            return;
        }

        // Create charts
        createAttendanceChart();
        createAttendancePieChart();

        // Load sessions
        loadTodaySessions();

        // Setup event handlers
        setupPeriodChangeHandlers();

        // Start auto-refresh
        startAutoRefresh();
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Export functions for external access if needed
    window.Dashboard = {
        refresh: loadTodaySessions,
        updateCharts: function() {
            createAttendanceChart();
            createAttendancePieChart();
        }
    };

})();
