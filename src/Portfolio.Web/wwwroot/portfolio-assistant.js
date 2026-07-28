(() => {
    const translations = {
        en: {
            launcher: "Ask AI",
            eyebrow: "PORTFOLIO GUIDE",
            title: "Ask about my work",
            close: "Close assistant",
            context: "Using this page as context",
            noStorage: "Sent to Gemini \u00b7 not stored here",
            inputLabel: "Ask a question about Abd Alrahman's work",
            placeholder: "Ask about a project, skill, or service...",
            send: "Send",
            disclaimer: "Your question is sent to Gemini. Do not include sensitive information.",
            welcome: "Hi \u2014 I can explain Abd Alrahman's projects, .NET work, and site-aware AI system. What would you like to know?",
            sources: "Portfolio sources",
            loading: "Reading the portfolio...",
            error: "I could not answer right now. Please try again in a moment.",
            unavailable: "The live assistant is being configured. You can still explore the project case studies.",
            rateLimit: "You have reached the short usage limit. Please wait a minute and try again.",
            tooLong: "Please keep the question under 500 characters.",
            suggestions: {
                "site-ai": "How did you integrate AI into existing websites?",
                security: "Why is the site-aware assistant secure?",
                dotnet: "Which project best demonstrates your .NET backend work?"
            }
        },
        ar: {
            launcher: "\u0627\u0633\u0623\u0644 \u0627\u0644\u0645\u0633\u0627\u0639\u062f",
            eyebrow: "\u062f\u0644\u064a\u0644 \u0627\u0644\u0628\u0648\u0631\u062a\u0641\u0648\u0644\u064a\u0648",
            title: "\u0627\u0633\u0623\u0644 \u0639\u0646 \u0623\u0639\u0645\u0627\u0644\u064a",
            close: "\u0625\u063a\u0644\u0627\u0642 \u0627\u0644\u0645\u0633\u0627\u0639\u062f",
            context: "\u064a\u0633\u062a\u062e\u062f\u0645 \u0647\u0630\u0647 \u0627\u0644\u0635\u0641\u062d\u0629 \u0643\u0633\u064a\u0627\u0642",
            noStorage: "\u064a\u064f\u0631\u0633\u0644 \u0625\u0644\u0649 Gemini \u00b7 \u0644\u0627 \u064a\u064f\u062d\u0641\u0638 \u0647\u0646\u0627",
            inputLabel: "\u0627\u0643\u062a\u0628 \u0633\u0624\u0627\u0644\u064b\u0627 \u0639\u0646 \u0623\u0639\u0645\u0627\u0644 \u0639\u0628\u062f \u0627\u0644\u0631\u062d\u0645\u0646",
            placeholder: "\u0627\u0633\u0623\u0644 \u0639\u0646 \u0645\u0634\u0631\u0648\u0639 \u0623\u0648 \u0645\u0647\u0627\u0631\u0629 \u0623\u0648 \u062e\u062f\u0645\u0629...",
            send: "\u0625\u0631\u0633\u0627\u0644",
            disclaimer: "\u064a\u064f\u0631\u0633\u0644 \u0633\u0624\u0627\u0644\u0643 \u0625\u0644\u0649 Gemini. \u0644\u0627 \u062a\u0643\u062a\u0628 \u0645\u0639\u0644\u0648\u0645\u0627\u062a \u062d\u0633\u0627\u0633\u0629.",
            welcome: "\u0623\u0647\u0644\u064b\u0627 \u2014 \u0623\u0633\u062a\u0637\u064a\u0639 \u0634\u0631\u062d \u0645\u0634\u0627\u0631\u064a\u0639 \u0639\u0628\u062f \u0627\u0644\u0631\u062d\u0645\u0646 \u0648\u0623\u0639\u0645\u0627\u0644\u0647 \u0628\u0640.NET \u0648\u0646\u0638\u0627\u0645 \u0627\u0644\u0630\u0643\u0627\u0621 \u0627\u0644\u0645\u062f\u0631\u0643 \u0644\u0644\u0645\u0648\u0627\u0642\u0639. \u0645\u0627\u0630\u0627 \u062a\u0631\u064a\u062f \u0623\u0646 \u062a\u0639\u0631\u0641\u061f",
            sources: "\u0645\u0635\u0627\u062f\u0631 \u0645\u0646 \u0627\u0644\u0628\u0648\u0631\u062a\u0641\u0648\u0644\u064a\u0648",
            loading: "\u0623\u0631\u0627\u062c\u0639 \u0645\u062d\u062a\u0648\u0649 \u0627\u0644\u0628\u0648\u0631\u062a\u0641\u0648\u0644\u064a\u0648...",
            error: "\u062a\u0639\u0630\u0631 \u062a\u0643\u0648\u064a\u0646 \u0625\u062c\u0627\u0628\u0629 \u0627\u0644\u0622\u0646. \u062d\u0627\u0648\u0644 \u0645\u0631\u0629 \u0623\u062e\u0631\u0649 \u0628\u0639\u062f \u0642\u0644\u064a\u0644.",
            unavailable: "\u064a\u062c\u0631\u064a \u0627\u0644\u0622\u0646 \u062a\u062c\u0647\u064a\u0632 \u0627\u0644\u0645\u0633\u0627\u0639\u062f \u0627\u0644\u062d\u064a. \u064a\u0645\u0643\u0646\u0643 \u062a\u0635\u0641\u062d \u062f\u0631\u0627\u0633\u0627\u062a \u062d\u0627\u0644\u0629 \u0627\u0644\u0645\u0634\u0627\u0631\u064a\u0639 \u0641\u064a \u0627\u0644\u0645\u0648\u0642\u0639.",
            rateLimit: "\u0648\u0635\u0644\u062a \u0625\u0644\u0649 \u062d\u062f \u0627\u0644\u0627\u0633\u062a\u062e\u062f\u0627\u0645 \u0627\u0644\u0642\u0635\u064a\u0631. \u0627\u0646\u062a\u0638\u0631 \u062f\u0642\u064a\u0642\u0629 \u062b\u0645 \u062d\u0627\u0648\u0644 \u0645\u062c\u062f\u062f\u064b\u0627.",
            tooLong: "\u0627\u062c\u0639\u0644 \u0627\u0644\u0633\u0624\u0627\u0644 \u0623\u0642\u0644 \u0645\u0646 500 \u062d\u0631\u0641.",
            suggestions: {
                "site-ai": "\u0643\u064a\u0641 \u062f\u0645\u062c\u062a \u0627\u0644\u0630\u0643\u0627\u0621 \u0627\u0644\u0627\u0635\u0637\u0646\u0627\u0639\u064a \u0641\u064a \u0645\u0648\u0627\u0642\u0639 \u0642\u0627\u0626\u0645\u0629\u061f",
                security: "\u0644\u0645\u0627\u0630\u0627 \u064a\u064f\u0639\u062f \u0627\u0644\u0645\u0633\u0627\u0639\u062f \u0627\u0644\u0645\u062f\u0631\u0643 \u0644\u0644\u0645\u0648\u0642\u0639 \u0622\u0645\u0646\u064b\u0627\u061f",
                dotnet: "\u0623\u064a \u0645\u0634\u0631\u0648\u0639 \u064a\u0648\u0636\u062d \u062e\u0628\u0631\u062a\u0643 \u0641\u064a Backend \u0628\u0640.NET\u061f"
            }
        }
    };
    let initializedRoot = null;
    let disposeActiveContext = null;

    function detectQuestionLanguage(text) {
        return /[\u0600-\u06FF]/.test(text) ? "ar" : null;
    }

    function buildDOM(language) {
        const copy = translations[language];
        const root = document.createElement("aside");
        root.className = "portfolio-assistant";
        root.dataset.portfolioAssistant = "";
        root.dataset.position = "right";
        root.setAttribute("aria-label", "AI assistant");
        root.hidden = false;

        root.innerHTML =
            '<button class="assistant-launcher" type="button" data-assistant-launcher aria-controls="portfolio-assistant-panel" aria-expanded="false" title="Toggle AI assistant (Ctrl+Shift+A)">' +
            '<span class="assistant-launcher__icon" aria-hidden="true">A</span>' +
            '<span class="assistant-launcher__label" data-assistant-launcher-label>' + copy.launcher + '</span>' +
            '<i aria-hidden="true"></i></button>' +
            '<section class="assistant-panel" id="portfolio-assistant-panel" data-assistant-panel role="dialog" aria-modal="false" aria-labelledby="portfolio-assistant-title" hidden>' +
            '<header class="assistant-panel__header">' +
            '<div>' +
            '<span class="assistant-panel__eyebrow" data-assistant-eyebrow>' + copy.eyebrow + '</span>' +
            '<h2 id="portfolio-assistant-title" data-assistant-title>' + copy.title + '</h2>' +
            '</div>' +
            '<button class="assistant-close" type="button" data-assistant-close aria-label="' + copy.close + '">\u00d7</button>' +
            '</header>' +
            '<div class="assistant-context">' +
            '<span><i aria-hidden="true"></i><span data-assistant-context-label>' + copy.context + '</span></span>' +
            '<small data-assistant-no-storage>' + copy.noStorage + '</small>' +
            '</div>' +
            '<div class="assistant-messages" data-assistant-messages role="log" aria-live="polite" aria-relevant="additions text"></div>' +
            '<div class="assistant-suggestions" data-assistant-suggestions>' +
            '<button type="button" data-assistant-suggestion="site-ai">' + copy.suggestions["site-ai"] + '</button>' +
            '<button type="button" data-assistant-suggestion="security">' + copy.suggestions.security + '</button>' +
            '<button type="button" data-assistant-suggestion="dotnet">' + copy.suggestions.dotnet + '</button>' +
            '</div>' +
            '<form class="assistant-form" data-assistant-form>' +
            '<label class="visually-hidden" for="portfolio-assistant-question" data-assistant-input-label>' + copy.inputLabel + '</label>' +
            '<textarea id="portfolio-assistant-question" data-assistant-input rows="2" maxlength="500" enterkeyhint="send" autocomplete="off" placeholder="' + copy.placeholder + '"></textarea>' +
            '<button type="submit" data-assistant-send>' +
            '<span data-assistant-send-label>' + copy.send + '</span>' +
            '<span aria-hidden="true">\u2197</span></button>' +
            '</form>' +
            '<p class="assistant-disclaimer" data-assistant-disclaimer>' + copy.disclaimer + '</p>' +
            '</section>';

        document.body.append(root);

        root.openAssistant = () => {};
        root.closeAssistant = () => {};
        root.refreshAssistantContext = () => {};

        return root;
    }

    function initialize() {
        const language = document.documentElement.dataset.language === "ar" ? "ar" : "en";

        let root = document.querySelector("[data-portfolio-assistant]");
        if (root) {
            if (root.dataset.initialized === "true") {
                root.refreshAssistantContext?.();
                return;
            }
            if (initializedRoot && initializedRoot !== root) {
                disposeActiveContext?.();
            }
            root.dataset.initialized = "true";
            initializedRoot = root;
        } else {
            root = buildDOM(language);
            root.dataset.initialized = "true";
            initializedRoot = root;
        }

        const copy = translations[language];
        const launcher = root.querySelector("[data-assistant-launcher]");
        const launcherLabel = root.querySelector("[data-assistant-launcher-label]");
        const panel = root.querySelector("[data-assistant-panel]");
        const closeButton = root.querySelector("[data-assistant-close]");
        const messages = root.querySelector("[data-assistant-messages]");
        const suggestions = root.querySelector("[data-assistant-suggestions]");
        const form = root.querySelector("[data-assistant-form]");
        const input = root.querySelector("[data-assistant-input]");
        const sendButton = root.querySelector("[data-assistant-send]");
        const conversation = [];
        let pending = false;
        let activeSection = window.location.hash.replace("#", "");
        let sectionObserver = null;
        let lastFocusedElement = null;
        let assistantProgrammaticClose = false;

        launcherLabel.textContent = copy.launcher;
        launcher.setAttribute("aria-label", copy.title);
        root.querySelector("[data-assistant-eyebrow]").textContent = copy.eyebrow;
        root.querySelector("[data-assistant-title]").textContent = copy.title;
        closeButton.setAttribute("aria-label", copy.close);
        root.querySelector("[data-assistant-context-label]").textContent = copy.context;
        root.querySelector("[data-assistant-no-storage]").textContent = copy.noStorage;
        root.querySelector("[data-assistant-input-label]").textContent = copy.inputLabel;
        root.querySelector("[data-assistant-send-label]").textContent = copy.send;
        root.querySelector("[data-assistant-disclaimer]").textContent = copy.disclaimer;
        input.placeholder = copy.placeholder;

        root.querySelectorAll("[data-assistant-suggestion]").forEach((button) => {
            const suggestionKey = button.dataset.assistantSuggestion;
            button.textContent = copy.suggestions[suggestionKey];
            button.addEventListener("click", () => {
                input.value = copy.suggestions[suggestionKey];
                submitQuestion();
            });
        });

        addMessage("assistant", copy.welcome);

        function openPanel() {
            if (root.hidden) return;
            lastFocusedElement = document.activeElement;
            panel.hidden = false;
            launcher.setAttribute("aria-expanded", "true");
            root.classList.add("is-open");
            window.setTimeout(() => input.focus(), 50);
        }

        function closePanel({ restoreFocus = true } = {}) {
            assistantProgrammaticClose = true;
            panel.hidden = true;
            launcher.setAttribute("aria-expanded", "false");
            root.classList.remove("is-open");
            if (restoreFocus) {
                const focusTarget = lastFocusedElement?.isConnected
                    ? lastFocusedElement
                    : launcher;
                focusTarget.focus();
            }
            window.setTimeout(() => { assistantProgrammaticClose = false; }, 100);
        }

        function togglePanel() {
            if (panel.hidden) openPanel();
            else closePanel();
        }

        function addMessage(role, text, sources = []) {
            const message = document.createElement("article");
            message.className = "assistant-message assistant-message--" + role;
            message.setAttribute("dir", "auto");

            const body = document.createElement("p");
            body.textContent = text;
            message.append(body);

            const trustedSources = sources.filter((source) => {
                if (!source || !source.url || !source.title) return false;
                try {
                    return new URL(source.url, window.location.origin).origin === window.location.origin;
                } catch {
                    return false;
                }
            });

            if (trustedSources.length > 0) {
                const sourceGroup = document.createElement("nav");
                sourceGroup.className = "assistant-sources";
                sourceGroup.setAttribute("aria-label", copy.sources);

                const label = document.createElement("span");
                label.textContent = copy.sources;
                sourceGroup.append(label);

                trustedSources.forEach((source) => {
                    const link = document.createElement("a");
                    link.href = new URL(source.url, window.location.origin).href;
                    link.textContent = source.title;
                    sourceGroup.append(link);
                });

                message.append(sourceGroup);
            }

            messages.append(message);
            messages.scrollTop = messages.scrollHeight;
            return message;
        }

        function addLoadingMessage() {
            const message = document.createElement("article");
            message.className = "assistant-message assistant-message--assistant assistant-message--loading";
            message.setAttribute("role", "status");

            const indicator = document.createElement("i");
            indicator.setAttribute("aria-hidden", "true");
            const label = document.createElement("span");
            label.textContent = copy.loading;
            message.append(indicator, label);
            messages.append(message);
            messages.scrollTop = messages.scrollHeight;
            return message;
        }

        async function submitQuestion() {
            if (pending) return;
            const question = input.value.replace(/\s+/g, " ").trim();
            if (!question) {
                input.focus();
                return;
            }

            if (question.length > 500) {
                addMessage("assistant", copy.tooLong);
                return;
            }

            pending = true;
            input.value = "";
            input.disabled = true;
            sendButton.disabled = true;
            suggestions.hidden = true;
            addMessage("user", question);
            const loadingMessage = addLoadingMessage();
            const priorHistory = conversation.slice(-6);
            const controller = new AbortController();
            const timeout = window.setTimeout(() => controller.abort(), 30000);

            try {
                const response = await fetch("/api/assistant/chat", {
                    method: "POST",
                    credentials: "same-origin",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json"
                    },
                    body: JSON.stringify({
                        question,
                        language: detectQuestionLanguage(question) ?? language,
                        path: window.location.pathname,
                        section: activeSection,
                        pageContext: readCurrentPageContext(),
                        history: priorHistory
                    }),
                    signal: controller.signal
                });

                let payload = {};
                try {
                    payload = await response.json();
                } catch {
                    payload = {};
                }

                loadingMessage.remove();

                if (response.status === 429) {
                    addMessage("assistant", copy.rateLimit);
                    return;
                }

                if (response.status === 503) {
                    addMessage("assistant", copy.unavailable);
                    return;
                }

                if (!response.ok || typeof payload.answer !== "string") {
                    addMessage("assistant", copy.error);
                    return;
                }

                addMessage("assistant", payload.answer, Array.isArray(payload.sources) ? payload.sources : []);
                conversation.push(
                    { role: "user", content: question },
                    { role: "assistant", content: payload.answer }
                );
            } catch {
                loadingMessage.remove();
                addMessage("assistant", copy.error);
            } finally {
                window.clearTimeout(timeout);
                pending = false;
                input.disabled = false;
                sendButton.disabled = false;
                input.focus();
            }
        }

        launcher.addEventListener("click", () => togglePanel());
        closeButton.addEventListener("click", () => closePanel());
        form.addEventListener("submit", (event) => {
            event.preventDefault();
            submitQuestion();
        });
        input.addEventListener("keydown", (event) => {
            if (event.key === "Enter" && !event.shiftKey && !event.isComposing) {
                event.preventDefault();
                submitQuestion();
            }
        });

        panel.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                event.preventDefault();
                closePanel();
            }
        });

        function readCurrentPageContext() {
            const currentSection = activeSection
                ? document.getElementById(activeSection)
                : null;
            const contextRoot = currentSection ?? document.querySelector("main");
            if (!contextRoot) return "";

            return (contextRoot.innerText || "")
                .replace(/\s+/g, " ")
                .trim()
                .slice(0, 5000);
        }

        function refreshAssistantContext() {
            activeSection = window.location.hash.replace("#", "");
            const isSupportedPath = window.location.pathname !== "/work/video-goals-extractor";
            root.hidden = !isSupportedPath;
            if (!isSupportedPath && !panel.hidden) {
                closePanel({ restoreFocus: false });
            }

            sectionObserver?.disconnect();
            sectionObserver = null;
            const trackedSections = Array.from(document.querySelectorAll("main section[id]"))
                .filter((section) => ["system", "work", "about", "contact"].includes(section.id));
            if ("IntersectionObserver" in window && trackedSections.length > 0) {
                sectionObserver = new IntersectionObserver((entries) => {
                    const visible = entries
                        .filter((entry) => entry.isIntersecting)
                        .sort((left, right) => right.intersectionRatio - left.intersectionRatio)[0];
                    if (visible) activeSection = visible.target.id;
                }, {
                    rootMargin: "-25% 0px -55% 0px",
                    threshold: [0, 0.1, 0.35, 0.6]
                });
                trackedSections.forEach((section) => sectionObserver.observe(section));
            }
        }

        root.openAssistant = openPanel;
        root.closeAssistant = closePanel;
        root.refreshAssistantContext = refreshAssistantContext;
        disposeActiveContext = () => sectionObserver?.disconnect();
        refreshAssistantContext();
    }

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-open-portfolio-assistant]");
        if (!trigger) return;
        document.querySelector("[data-portfolio-assistant]")?.openAssistant?.();
    });

    document.addEventListener("keydown", (event) => {
        if (event.key !== "Escape") return;
        const root = document.querySelector("[data-portfolio-assistant]");
        const panel = root?.querySelector("[data-assistant-panel]");
        if (panel && !panel.hidden) {
            event.preventDefault();
            root.closeAssistant?.();
        }
    });

    document.addEventListener("keydown", (event) => {
        if ((event.ctrlKey || event.metaKey) && event.shiftKey && (event.key === "a" || event.key === "A")) {
            event.preventDefault();
            const root = document.querySelector("[data-portfolio-assistant]");
            if (!root) return;
            const panel = root.querySelector("[data-assistant-panel]");
            if (panel?.hidden) {
                root.openAssistant?.();
            } else {
                root.closeAssistant?.();
            }
        }
    });

    function start() {
        initialize();
        if (window.Blazor?.addEventListener && !window.__portfolioAssistantEnhancedLoad) {
            window.__portfolioAssistantEnhancedLoad = true;
            window.Blazor.addEventListener("enhancedload", initialize);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start, { once: true });
    } else {
        start();
    }
})();
