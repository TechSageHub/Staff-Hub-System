// StaffHub global client behavior: sidebar + theme toggle.
(function () {
    'use strict';

    var THEME_KEY = 'staffhub.theme';
    var root = document.documentElement;

    function currentTheme() {
        return root.getAttribute('data-bs-theme') === 'dark' ? 'dark' : 'light';
    }

    function applyTheme(theme, persist) {
        theme = theme === 'dark' ? 'dark' : 'light';
        root.setAttribute('data-bs-theme', theme);
        if (persist) {
            try { localStorage.setItem(THEME_KEY, theme); } catch (e) { /* ignore */ }
        }
        syncThemeToggleUi(theme);
        document.dispatchEvent(new CustomEvent('sh:theme-change', { detail: { theme: theme } }));
    }

    function syncThemeToggleUi(theme) {
        var btns = document.querySelectorAll('[data-sh-theme-toggle]');
        btns.forEach(function (btn) {
            var isDark = theme === 'dark';
            btn.setAttribute('aria-pressed', isDark ? 'true' : 'false');
            btn.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
            btn.setAttribute('title', isDark ? 'Switch to light mode' : 'Switch to dark mode');
            var icon = btn.querySelector('i');
            if (icon) {
                icon.classList.remove('bi-sun', 'bi-moon-stars', 'bi-moon');
                icon.classList.add(isDark ? 'bi-sun' : 'bi-moon-stars');
            }
        });
    }

    function initThemeToggle() {
        syncThemeToggleUi(currentTheme());
        document.addEventListener('click', function (e) {
            var btn = e.target.closest && e.target.closest('[data-sh-theme-toggle]');
            if (!btn) return;
            e.preventDefault();
            applyTheme(currentTheme() === 'dark' ? 'light' : 'dark', true);
        });

        // Respond to OS preference changes only when user hasn't explicitly chosen.
        var userChoice = null;
        try { userChoice = localStorage.getItem(THEME_KEY); } catch (e) { /* ignore */ }
        if (!userChoice && window.matchMedia) {
            var mq = window.matchMedia('(prefers-color-scheme: dark)');
            var listener = function (evt) { applyTheme(evt.matches ? 'dark' : 'light', false); };
            if (mq.addEventListener) mq.addEventListener('change', listener);
            else if (mq.addListener) mq.addListener(listener);
        }
    }

    function initSidebar() {
        var sidebar = document.getElementById('mainSidebar');
        var overlay = document.getElementById('sidebarOverlay');
        var toggle = document.getElementById('toggleSidebar');
        var close = document.getElementById('closeSidebar');
        if (!sidebar) return;

        function open() {
            sidebar.classList.add('show');
            if (overlay) overlay.classList.add('show');
            document.body.classList.add('overflow-hidden');
        }
        function shut() {
            sidebar.classList.remove('show');
            if (overlay) overlay.classList.remove('show');
            document.body.classList.remove('overflow-hidden');
        }
        if (toggle) toggle.addEventListener('click', open);
        if (close) close.addEventListener('click', shut);
        if (overlay) overlay.addEventListener('click', shut);
    }

    function initPasswordToggles() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest && e.target.closest('[data-sh-toggle-password]');
            if (!btn) return;
            var targetSel = btn.getAttribute('data-sh-toggle-password');
            var field = targetSel ? document.querySelector(targetSel) : null;
            if (!field) return;
            var reveal = field.getAttribute('type') === 'password';
            field.setAttribute('type', reveal ? 'text' : 'password');
            var icon = btn.querySelector('i');
            if (icon) {
                icon.classList.remove('bi-eye', 'bi-eye-slash');
                icon.classList.add(reveal ? 'bi-eye-slash' : 'bi-eye');
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initSidebar();
            initThemeToggle();
            initPasswordToggles();
        });
    } else {
        initSidebar();
        initThemeToggle();
        initPasswordToggles();
    }

    // Expose small API for views that need it (e.g., ApexCharts re-theming).
    window.StaffHub = window.StaffHub || {};
    window.StaffHub.currentTheme = currentTheme;
    window.StaffHub.applyTheme = applyTheme;
})();
