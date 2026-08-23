(() => {
    const MOBILE_BREAKPOINT_QUERY = "(max-width: 60rem)";

    const rails = document.querySelectorAll(
        ".profile-first-read > .profile-result__section--starting-point"
    );

    if (rails.length === 0) {
        return;
    }

    const mobileQuery = window.matchMedia(MOBILE_BREAKPOINT_QUERY);

    // CSS grid's own auto row sizing is driven by whichever column is
    // naturally taller, which is not always the left (primary) column —
    // several full-length repository descriptions can make the rail itself
    // the taller one. Explicitly matching the primary column's own rendered
    // height keeps it authoritative, so the rail's bottom edge always aligns
    // with it and its own repository list scrolls internally instead of
    // growing the row.
    const sync = rail => {
        const grid = rail.closest(".profile-first-read");
        const primary = grid?.querySelector(".profile-first-read__primary");

        if (!primary) {
            return;
        }

        if (mobileQuery.matches) {
            // The single-column breakpoint stacks both columns; there is no
            // sibling height left to match, so the rail returns to its own
            // natural content height.
            rail.style.removeProperty("height");
            return;
        }

        rail.style.height = `${primary.getBoundingClientRect().height}px`;
    };

    const syncAll = () => rails.forEach(sync);

    syncAll();

    window.addEventListener("resize", syncAll);
    window.addEventListener("pageshow", syncAll);
    mobileQuery.addEventListener("change", syncAll);

    if (window.ResizeObserver) {
        const observer = new ResizeObserver(syncAll);

        rails.forEach(rail => {
            const primary = rail
                .closest(".profile-first-read")
                ?.querySelector(".profile-first-read__primary");

            if (primary) {
                observer.observe(primary);
            }
        });
    }
})();
