/**
 * Attendances Charts JavaScript Module
 * Handles chart visualization for the Attendances management page
 * Supports both student and teacher attendance data
 */

(function () {
    'use strict';

    let statusChart = null;
    let trendChart = null;

    /**
     * Creates the attendance status distribution doughnut chart
     */
    function createStatusDistributionChart(data) {
        const ctx = document.getElementById('attendanceStatusChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (statusChart) {
            statusChart.destroy();
        }

        statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Present', 'Absent', 'Late', 'Excused'],
                datasets: [{
                    label: 'Count',
                    data: [data.present, data.absent, data.late, data.excused],
                    backgroundColor: [
                        '#28a745',  // Green for Present
                        '#dc3545',  // Red for Absent
                        '#fd7e14',  // Orange for Late
                        '#17a2b8'   // Teal for Excused
                    ],
                    borderColor: '#ffffff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 15,
                            font: {
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Creates the 30-day attendance trend line chart
     * Shows separate lines for students and teachers
     */
    function createAttendanceTrendChart(data) {
        const ctx = document.getElementById('attendanceTrendChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (trendChart) {
            trendChart.destroy();
        }

        // Extract labels and datasets
        const labels = data.map(item => item.Date);
        const studentPresentData = data.map(item => item.StudentPresent);
        const teacherPresentData = data.map(item => item.TeacherPresent);

        trendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Students Present',
                        data: studentPresentData,
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 3,
                        pointHoverRadius: 5
                    },
                    {
                        label: 'Teachers Present',
                        data: teacherPresentData,
                        borderColor: '#fd7e14',
                        backgroundColor: 'rgba(253, 126, 20, 0.1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 3,
                        pointHoverRadius: 5
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            padding: 15,
                            font: {
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return context.dataset.label + ': ' + context.parsed.y;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: {
                                size: 10
                            },
                            maxRotation: 45,
                            minRotation: 0
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            precision: 0
                        },
                        grid: {
                            color: 'rgba(0, 0, 0, 0.05)'
                        }
                    }
                }
            }
        });
    }

    /**
     * Initializes all charts with provided data
     */
    function init(data) {
        // Wait for Chart.js to be loaded
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded. Please include Chart.js before attendances-charts.js');
            return;
        }

        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                createStatusDistributionChart(data.statusData);
                createAttendanceTrendChart(data.trendData);
            });
        } else {
            createStatusDistributionChart(data.statusData);
            createAttendanceTrendChart(data.trendData);
        }
    }

    /**
     * Updates charts with new data
     */
    function updateCharts(data) {
        createStatusDistributionChart(data.statusData);
        createAttendanceTrendChart(data.trendData);
    }

    // Export functions for external access
    window.AttendancesCharts = {
        init: init,
        update: updateCharts
    };

})();
