(() => {
    // Exact and locked by live-analysis-progress-runtime-contract.md §13. None
    // configurable, none derived from elapsed time.
    const POLL_INTERVAL_MS = 2000;
    const INITIAL_UNKNOWN_LIMIT = 5;
    const TRANSPORT_FAILURE_LIMIT = 3;
    const PROGRESS_ENDPOINT = "/analysis-progress";
    const TOKEN_FIELD = "AnalysisProgressToken";

    const forms =
        document.querySelectorAll("[data-analysis-form]");

    const controllers = [];

    const languageForm = document.querySelector("[data-language-form]");

    // A single source of analysis-busy truth: the landing progress form's own
    // existing submitting state (p10 §8.1). No new flag.
    const landingAnalysisActive = () =>
        Array.from(forms).some(candidate =>
            candidate.hasAttribute("data-analysis-progress")
            && candidate.dataset.submitting === "true");

    // While an analysis is in flight the EN/TR controls become inert and
    // perceivably disabled — accessibly (aria-disabled) and for the keyboard
    // (removed from the tab order) as well as the pointer. Restored on reset.
    const setLanguageControlsLocked = locked => {
        if (!languageForm) {
            return;
        }

        languageForm.toggleAttribute("data-analysis-locked", locked);

        languageForm
            .querySelectorAll("button[name=\"Culture\"]")
            .forEach(choice => {
                if (locked) {
                    choice.setAttribute("aria-disabled", "true");
                    choice.setAttribute("tabindex", "-1");
                } else {
                    choice.removeAttribute("aria-disabled");
                    choice.removeAttribute("tabindex");
                }
            });
    };

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

        // Only the landing progress form owns the language lock; a drill-down
        // form's reset never re-enables EN/TR on its own.
        if (form.hasAttribute("data-analysis-progress")) {
            setLanguageControlsLocked(false);
        }
    };

    forms.forEach(form => {
        reset(form);

        // Only the landing form observes live progress. The Profile drill-down
        // forms carry no token, register no run and get no stage UI (§8).
        const progress = form.hasAttribute("data-analysis-progress")
            ? createProgressController(form)
            : null;

        if (progress) {
            controllers.push(progress);
        }

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

            // Token generation must never throw out of the submit handler: on
            // failure the form submits without a token, no run is registered,
            // and the visitor gets the canonical indeterminate state (§8, §6).
            progress?.start();

            // Protect this in-flight synchronous analysis from being abandoned
            // by a language navigation (p10 §8.1).
            if (form.hasAttribute("data-analysis-progress")) {
                setLanguageControlsLocked(true);
            }
        });
    });

    // Keyboard (Enter/Space on a focused choice) and pointer both reach the
    // form through its submit event; blocking it here stops navigation to
    // /Language and the culture-cookie POST while a landing analysis is active.
    languageForm?.addEventListener("submit", event => {
        if (landingAnalysisActive()) {
            event.preventDefault();
        }
    });

    function createProgressController(form) {
        const status = form.querySelector("[data-analysis-status]");
        const live = form.querySelector("[data-analysis-live]");
        const stageLists = form.querySelectorAll("[data-analysis-stages]");
        const indeterminateSentence = live ? live.textContent : "";

        let token = null;
        let timerId = null;
        let polling = false;
        let sawKnown = false;
        let unknownStreak = 0;
        let failureStreak = 0;
        let announcedStage = null;

        const clearTimer = () => {
            if (timerId !== null) {
                window.clearInterval(timerId);
                timerId = null;
            }
        };

        const startTimer = () => {
            clearTimer();
            timerId = window.setInterval(poll, POLL_INTERVAL_MS);
        };

        const stopPolling = () => {
            polling = false;
            clearTimer();
        };

        const renderIndeterminate = () => {
            stageLists.forEach(list => {
                list.hidden = true;
                list
                    .querySelectorAll("[data-stage]")
                    .forEach(item => { item.dataset.state = "pending"; });
            });
            if (live) {
                live.textContent = indeterminateSentence;
            }
            announcedStage = null;
        };

        const resetState = () => {
            stopPolling();
            token = null;
            sawKnown = false;
            unknownStreak = 0;
            failureStreak = 0;
            if (status) {
                status.hidden = true;
            }
            renderIndeterminate();
        };

        const render = snapshot => {
            stageLists.forEach(list => {
                const applies = list.dataset.analysisStages === snapshot.target;
                list.hidden = !applies;

                if (!applies) {
                    return;
                }

                list.querySelectorAll("[data-stage]").forEach(item => {
                    const id = item.dataset.stage;

                    if (snapshot.completed.includes(id)) {
                        item.dataset.state = "completed";
                    } else if (snapshot.active === id) {
                        item.dataset.state = "active";
                    } else {
                        item.dataset.state = "pending";
                    }
                });

                // The live region carries exactly the active stage label, read
                // from the server-rendered markup — never a string built here.
                // A terminal snapshot (active === null) leaves it unchanged
                // (p10 §8.3.6).
                if (live
                    && snapshot.active !== null
                    && snapshot.active !== announcedStage) {
                    const activeLabel = list.querySelector(
                        `[data-stage="${snapshot.active}"] .analysis-progress__stage-label`);

                    if (activeLabel) {
                        live.textContent = activeLabel.textContent;
                        announcedStage = snapshot.active;
                    }
                }
            });
        };

        const handleKnown = snapshot => {
            sawKnown = true;
            unknownStreak = 0;
            failureStreak = 0;
            render(snapshot);

            // A terminal snapshot is the last thing that will ever change; the
            // result page is about to replace the whole view.
            if (snapshot.active === null && snapshot.completed.length > 0) {
                stopPolling();
            }
        };

        const handleUnknown = () => {
            if (sawKnown) {
                // The run ended, or an instance flip / TTL eviction. Keep the
                // last snapshot visible; never revert to indeterminate (§15).
                stopPolling();
                return;
            }

            // A valid known:false is not a transport failure — it breaks any
            // run of transport failures. The limits are "consecutive" (§13).
            failureStreak = 0;
            unknownStreak += 1;

            if (unknownStreak >= INITIAL_UNKNOWN_LIMIT) {
                // Silent fallback to the canonical indeterminate state (§10).
                stopPolling();
                renderIndeterminate();
            }
        };

        const handleTransportFailure = () => {
            // A transport failure breaks any run of known:false responses.
            unknownStreak = 0;
            failureStreak += 1;

            if (failureStreak >= TRANSPORT_FAILURE_LIMIT) {
                stopPolling();

                if (!sawKnown) {
                    renderIndeterminate();
                }
            }
        };

        async function poll() {
            if (!polling || token === null || document.hidden) {
                return;
            }

            let response;

            try {
                response = await fetch(
                    `${PROGRESS_ENDPOINT}?run=${encodeURIComponent(token)}`,
                    { cache: "no-store", headers: { "Accept": "application/json" } });
            } catch {
                handleTransportFailure();
                return;
            }

            if (!response.ok) {
                // A non-200, including the HTTP 400 for a malformed token,
                // counts against the transport-failure stop limit (§10.4).
                handleTransportFailure();
                return;
            }

            let payload;

            try {
                payload = await response.json();
            } catch {
                handleTransportFailure();
                return;
            }

            if (payload && payload.known === true
                && (payload.target === "repository" || payload.target === "profile")
                && Array.isArray(payload.completed)) {
                handleKnown({
                    target: payload.target,
                    active: typeof payload.active === "string" ? payload.active : null,
                    completed: payload.completed
                });
            } else if (payload && payload.known === false) {
                handleUnknown();
            } else {
                handleTransportFailure();
            }
        }

        return {
            start() {
                let generated = null;

                try {
                    generated = window.crypto?.randomUUID?.() ?? null;
                } catch {
                    generated = null;
                }

                if (generated === null) {
                    return;
                }

                token = generated;

                const hidden = document.createElement("input");
                hidden.type = "hidden";
                hidden.name = TOKEN_FIELD;
                hidden.value = token;
                form.appendChild(hidden);

                polling = true;
                sawKnown = false;
                unknownStreak = 0;
                failureStreak = 0;
                announcedStage = null;

                // Immediate first poll, then every 2000 ms (§13).
                poll();
                startTimer();
            },
            onVisibilityChange() {
                if (!polling) {
                    return;
                }

                if (document.hidden) {
                    clearTimer();
                } else {
                    poll();
                    startTimer();
                }
            },
            resetState
        };
    }

    // Polling pauses while the document is hidden and resumes on visibility;
    // the interval never compounds while hidden (§13).
    document.addEventListener("visibilitychange", () => {
        controllers.forEach(controller => controller.onVisibilityChange());
    });

    // pageshow / BFCache restore resets every stage to PENDING and re-hides the
    // region (§15, p10 §8.3.6).
    window.addEventListener("pageshow", () => {
        forms.forEach(reset);
        controllers.forEach(controller => controller.resetState());
    });
})();
