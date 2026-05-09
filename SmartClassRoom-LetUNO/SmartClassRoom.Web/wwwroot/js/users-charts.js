/**
 * Users Charts JavaScript Module
 * Handles chart visualization for the Users management page
 */

(function () {
    'use strict';

    let userTypeChart = null;
    let userStatusChart = null;

    /**
     * Creates the user type distribution pie chart
     */
    function createUserTypeChart(data) {
        const ctx = document.getElementById('userTypeChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (userTypeChart) {
            userTypeChart.destroy();
        }

        userTypeChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Students', 'Teachers'],
                datasets: [{
                    data: [data.totalStudents, data.totalTeachers],
                    backgroundColor: [
                        '#28a745', // Green for students
                        '#17a2b8'  // Blue for teachers
                    ],
                    borderWidth: 2,
                    borderColor: '#fff'
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
                                size: 13
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
     * Creates the user status overview bar chart
     */
    function createUserStatusChart(data) {
        const ctx = document.getElementById('userStatusChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (userStatusChart) {
            userStatusChart.destroy();
        }

        userStatusChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Students', 'Teachers', 'All Users'],
                datasets: [{
                    label: 'Active',
                    data: [data.studentsActive, data.teachersActive, data.activeUsers],
                    backgroundColor: '#28a745',
                    borderColor: '#28a745',
                    borderWidth: 1
                }, {
                    label: 'Inactive',
                    data: [
                        data.totalStudents - data.studentsActive,
                        data.totalTeachers - data.teachersActive,
                        data.inactiveUsers
                    ],
                    backgroundColor: '#ffc107',
                    borderColor: '#ffc107',
                    borderWidth: 1
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
                                size: 13
                            }
                        }
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false
                    }
                },
                scales: {
                    x: {
                        stacked: true,
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        stacked: true,
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
            console.error('Chart.js is not loaded. Please include Chart.js before users-charts.js');
            return;
        }

        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                createUserTypeChart(data);
                createUserStatusChart(data);
            });
        } else {
            createUserTypeChart(data);
            createUserStatusChart(data);
        }
    }

    /**
     * Updates charts with new data
     */
    function updateCharts(data) {
        createUserTypeChart(data);
        createUserStatusChart(data);
    }

    // Export functions for external access
    window.UsersCharts = {
        init: init,
        update: updateCharts
    };

})();
