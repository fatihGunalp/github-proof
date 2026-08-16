(() => {
    const storageKey = "github-proof-theme";
    const root = document.documentElement;
    const toggle =
        document.querySelector("[data-theme-toggle]");

    if (!toggle) {
        return;
    }

    const currentTheme = () =>
        root.dataset.theme === "dark"
            ? "dark"
            : "light";

    const updateAccessibleName = theme => {
        const nextTheme =
            theme === "dark"
                ? "light"
                : "dark";

        const label =
            `Switch to ${nextTheme} theme`;

        toggle.setAttribute(
            "aria-label",
            label
        );

        toggle.setAttribute(
            "title",
            label
        );
    };

    const applyTheme = theme => {
        root.dataset.theme = theme;

        try {
            localStorage.setItem(
                storageKey,
                theme
            );
        } catch {
            // The current-page theme still changes.
        }

        updateAccessibleName(theme);
    };

    updateAccessibleName(
        currentTheme()
    );

    toggle.addEventListener(
        "click",
        () => {
            const nextTheme =
                currentTheme() === "dark"
                    ? "light"
                    : "dark";

            applyTheme(nextTheme);
        }
    );
})();
