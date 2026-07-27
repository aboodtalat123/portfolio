(() => {
    const root = document.documentElement;

    function currentLanguage() {
        return root.dataset.language === "ar" ? "ar" : "en";
    }

    function updateButtons() {
        const isLight = root.dataset.theme === "light";
        const isArabic = currentLanguage() === "ar";
        const themeColor = document.querySelector('meta[name="theme-color"]');
        root.style.colorScheme = isLight ? "light" : "dark";
        if (themeColor) themeColor.setAttribute("content", isLight ? "#f0efea" : "#15181e");
        document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
            const label = isArabic
                ? (isLight ? "التبديل إلى الوضع الداكن" : "التبديل إلى الوضع الفاتح")
                : (isLight ? "Switch to dark theme" : "Switch to light theme");
            button.setAttribute("aria-label", label);
            button.setAttribute("title", label);
        });
    }

    document.addEventListener("click", (event) => {
        const languageControl = event.target.closest("[data-language-toggle]");
        if (languageControl) {
            event.preventDefault();
            const url = new URL(window.location.href);
            const nextLanguage = languageControl.dataset.language === "ar" ? "ar" : "en";
            if (nextLanguage === "ar") url.searchParams.set("lang", "ar");
            else url.searchParams.delete("lang");
            window.location.assign(`${url.pathname}${url.search}${url.hash}`);
            return;
        }

        const button = event.target.closest("[data-theme-toggle]");
        if (!button) return;

        const nextTheme = root.dataset.theme === "light" ? "dark" : "light";
        root.dataset.theme = nextTheme;
        localStorage.setItem("portfolio-theme", nextTheme);
        updateButtons();
    });

    document.addEventListener("DOMContentLoaded", updateButtons);
    updateButtons();
})();
