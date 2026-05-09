/**
 * Quizzes Charts JavaScript Module
 * Handles chart visualization for the Quizzes management page
 */

(function () {
    'use strict';

    let courseChart = null;

    /**
     * Creates the quizzes by course bar chart
     */
    function createQuizzesByCourseChart(data) {
        const ctx = document.getElementById('quizzesByCourseChart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (courseChart) {
            courseChart.destroy();
        }

        // Extract labels and data
        const labels = data.map(item => item.Course);
        const counts = data.map(item => item.Count);

        // Generate gradient colors
        const colors = [
            '#667eea',
            '#28a745',
            '#17a2b8',
            '#ffc107',
            '#dc3545',
            '#6610f2',
            '#fd7e14',
            '#20c997'
        ];

        const backgroundColors = counts.map((_, index) => colors[index % colors.length]);

        courseChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Number of Quizzes',
                    data: counts,
                    backgroundColor: backgroundColors,
                    borderColor: backgroundColors,
                    borderWidth: 1,
                    borderRadius: 8
                }]
            },
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
                                return 'Quizzes: ' + context.parsed.y;
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
                                size: 11
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
            console.error('Chart.js is not loaded. Please include Chart.js before quizzes-charts.js');
            return;
        }

        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                createQuizzesByCourseChart(data.quizzesByCourse);
            });
        } else {
            createQuizzesByCourseChart(data.quizzesByCourse);
        }
    }

    /**
     * Updates charts with new data
     */
    function updateCharts(data) {
        createQuizzesByCourseChart(data.quizzesByCourse);
    }

    // Export functions for external access
    window.QuizzesCharts = {
        init: init,
        update: updateCharts
    };

})();
