/**
 * Sidebar Navigation JavaScript - Vision Valley Style
 * Handles sidebar toggle, collapsible menus, and responsive behavior
 */

(function () {
    'use strict';

    // Initialize sidebar when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        initSidebar();
        setActiveNav();
    });

    /**
     * Initialize sidebar toggle functionality
     */
    function initSidebar() {
        const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
        const sidebar = document.querySelector('.sidebar');
        const sidebarOverlay = document.querySelector('.sidebar-overlay');

        if (!sidebar) return;

        // Mobile menu button click
        if (mobileMenuBtn) {
            mobileMenuBtn.addEventListener('click', function () {
                toggleSidebarMobile();
            });
        }

        // Overlay click to close sidebar
        if (sidebarOverlay) {
            sidebarOverlay.addEventListener('click', function () {
                closeSidebarMobile();
            });
        }

        // Close sidebar on escape key (mobile)
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && window.innerWidth <= 991) {
                closeSidebarMobile();
            }
        });

        // Handle window resize
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                handleResize();
            }, 250);
        });
    }

    /**
     * Toggle sidebar visibility on mobile
     */
    function toggleSidebarMobile() {
        const sidebar = document.querySelector('.sidebar');
        const overlay = document.querySelector('.sidebar-overlay');

        if (sidebar) {
            sidebar.classList.toggle('open');
        }

        if (overlay) {
            overlay.classList.toggle('show');
        }

        // Prevent body scroll when sidebar is open
        document.body.style.overflow = sidebar && sidebar.classList.contains('open') ? 'hidden' : '';
    }

    /**
     * Close sidebar on mobile
     */
    function closeSidebarMobile() {
        const sidebar = document.querySelector('.sidebar');
        const overlay = document.querySelector('.sidebar-overlay');

        if (sidebar) {
            sidebar.classList.remove('open');
        }

        if (overlay) {
            overlay.classList.remove('show');
        }

        document.body.style.overflow = '';
    }

    /**
     * Handle window resize
     */
    function handleResize() {
        const isMobile = window.innerWidth <= 991;

        if (!isMobile) {
            closeSidebarMobile();
        }
    }

    /**
     * Set active navigation item based on current URL
     */
    function setActiveNav() {
        const currentPath = window.location.pathname.toLowerCase();
        const navLinks = document.querySelectorAll('.sidebar .nav-link');

        navLinks.forEach(function (link) {
            const href = link.getAttribute('href');
            if (!href || href === '#') return;

            const linkPath = href.toLowerCase();

            // Check for exact match or if current path starts with link path
            if (currentPath === linkPath ||
                (linkPath !== '/' && currentPath.startsWith(linkPath))) {

                // Add active class to link
                link.classList.add('active');
            }
        });
    }

})();
