/**
 * Sessions Charts JavaScript Module
 * Handles chart visualization for the Sessions management page
 */

(function () {
    'use strict';

    let statusChart = null;
    let trendChart = null;

    /**
     * Creates the session status distribution doughnut chart
     */
    function createStatusDistributionChart(data) {
        const ctx = document.getElementById('sessionStatusChart');
        if (!ctx) return;

        if (statusChart) {
            statusChart.destroy();
        }

        statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Scheduled', 'In Progress', 'Completed', 'Cancelled'],
                datasets: [{
                    label: 'Sessions',
                    data: [data.scheduled, data.inProgress, data.completed, data.cancelled],
                    backgroundColor: [
                        '#0d6efd',  // Blue for Scheduled
                        '#fd7e14',  // Orange for InProgress
                        '#28a745',  // Green for Completed
                        '#dc3545'   // Red for Cancelled
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
                            font: { size: 12 }
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
     * Creates the daily sessions trend line chart
     */
    function createSessionTrendChart(data) {
        const ctx = document.getElementById('sessionTrendChart');
        if (!ctx) return;

        if (trendChart) {
            trendChart.destroy();
        }

        const labels = data.map(item => item.Date);
        const sessionCounts = data.map(item => item.SessionCount);
        const completedCounts = data.map(item => item.CompletedCount);

        trendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Total Sessions',
                        data: sessionCounts,
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 3,
                        pointHoverRadius: 5
                    },
                    {
                        label: 'Completed',
                        data: completedCounts,
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
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
                            font: { size: 12 }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            font: { size: 10 },
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
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded');
            return;
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                createStatusDistributionChart(data.statusData);
                createSessionTrendChart(data.trendData);
            });
        } else {
            createStatusDistributionChart(data.statusData);
            createSessionTrendChart(data.trendData);
        }
    }

    // Export for external access
    window.SessionsCharts = {
        init: init
    };

})();
