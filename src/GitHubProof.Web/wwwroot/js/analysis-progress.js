(() => {
    const forms =
        document.querySelectorAll("[data-analysis-form]");

    const reset = form => {
        form.dataset.submitting = "false";
        form.setAttribute("aria-busy", "false");

        const action =
            form.querySelector("[data-analysis-submit]");
        const target =
            form.elements.namedItem("GitHubTarget");
        const status =
            form.querySelector("[data-analysis-status]");

        action?.removeAttribute("aria-disabled");

        if (target) {
            target.readOnly = false;
            target.removeAttribute("aria-readonly");
        }

        if (status) {
            status.hidden = true;
        }
    };

    forms.forEach(form => {
        reset(form);

        form.addEventListener("submit", event => {
            if (event.defaultPrevented || !form.checkValidity()) {
                return;
            }

            if (form.dataset.submitting === "true") {
                event.preventDefault();
                return;
            }

            form.dataset.submitting = "true";
            form.setAttribute("aria-busy", "true");

            const action =
                form.querySelector("[data-analysis-submit]");
            const target =
                form.elements.namedItem("GitHubTarget");
            const status =
                form.querySelector("[data-analysis-status]");

            // Readonly keeps GitHubTarget visible and interaction-wise passive
            // while preserving its value as a successful control in native
            // form serialization. Duplicate submit remains form-state-owned.
            if (target) {
                target.readOnly = true;
                target.setAttribute("aria-readonly", "true");
            }

            action?.setAttribute("aria-disabled", "true");

            if (status) {
                status.hidden = false;
            }
        });
    });

    window.addEventListener("pageshow", () => {
        forms.forEach(reset);
    });
})();
