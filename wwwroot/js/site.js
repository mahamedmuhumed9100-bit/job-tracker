// Theme toggle — mirrors the pattern used on the portfolio site and the
// algorithm visualizer, so the same interaction feels consistent everywhere.
(function () {
    var toggle = document.getElementById('themeToggle');
    if (!toggle) return;

    function applyIcon() {
        var theme = document.documentElement.getAttribute('data-theme');
        toggle.textContent = theme === 'dark' ? '☀️' : '🌙';
    }

    applyIcon();

    toggle.addEventListener('click', function () {
        var current = document.documentElement.getAttribute('data-theme');
        var next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        applyIcon();
        try {
            localStorage.setItem('theme', next);
        } catch (e) {
            // ignore if storage is unavailable
        }
    });
})();
