(() => {
    const translations = {
        en: {
            launcher: "Ask AI",
            eyebrow: "PORTFOLIO GUIDE",
            title: "Ask about my work",
            close: "Close assistant",
            context: "Using this page as context",
            noStorage: "Sent to Gemini · not stored here",
            inputLabel: "Ask a question about Abd Alrahman's work",
            placeholder: "Ask about a project, skill, or service...",
            send: "Send",
            disclaimer: "Your question is sent to Gemini. Do not include sensitive information.",
            welcome: "Hi — I can explain Abd Alrahman's projects, .NET work, and site-aware AI system. What would you like to know?",
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
            launcher: "اسأل المساعد",
            eyebrow: "دليل البورتفوليو",
            title: "اسأل عن أعمالي",
            close: "إغلاق المساعد",
            context: "يستخدم هذه الصفحة كسياق",
            noStorage: "يُرسل إلى Gemini · لا يُحفظ هنا",
            inputLabel: "اكتب سؤالًا عن أعمال عبد الرحمن",
            placeholder: "اسأل عن مشروع أو مهارة أو خدمة...",
            send: "إرسال",
            disclaimer: "يُرسل سؤالك إلى Gemini. لا تكتب معلومات حساسة.",
            welcome: "أهلًا — أستطيع شرح مشاريع عبد الرحمن وأعماله بـ.NET ونظام الذكاء المدرك للمواقع. ماذا تريد أن تعرف؟",
            sources: "مصادر من البورتفوليو",
            loading: "أراجع محتوى البورتفوليو...",
            error: "تعذر تكوين إجابة الآن. حاول مرة أخرى بعد قليل.",
            unavailable: "يجري الآن تجهيز المساعد الحي. يمكنك تصفح دراسات حالة المشاريع في الموقع.",
            rateLimit: "وصلت إلى حد الاستخدام القصير. انتظر دقيقة ثم حاول مجددًا.",
            tooLong: "اجعل السؤال أقل من 500 حرف.",
            suggestions: {
                "site-ai": "كيف دمجت الذكاء الاصطناعي في مواقع قائمة؟",
                security: "لماذا يُعد المساعد المدرك للموقع آمنًا؟",
                dotnet: "أي مشروع يوضح خبرتك في Backend بـ.NET؟"
            }
        }
    };
    let initializedRoot = null;
    let disposeActiveContext = null;

    function detectQuestionLanguage(text) {
        return /[\u0600-\u06FF]/.test(text) ? "ar" : null;
    }

    function initialize() {
        const root = document.querySelector("[data-portfolio-assistant]");
        if (!root) return;
        if (root.dataset.initialized === "true") {
            root.refreshAssistantContext?.();
            return;
        }
        if (initializedRoot && initializedRoot !== root) {
            disposeActiveContext?.();
        }
        root.dataset.initialized = "true";
        initializedRoot = root;

        const pageLanguage = document.documentElement.dataset.language === "ar" ? "ar" : "en";
        const language = pageLanguage;
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
            message.className = `assistant-message assistant-message--${role}`;
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
