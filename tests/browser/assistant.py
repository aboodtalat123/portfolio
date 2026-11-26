from playwright.sync_api import sync_playwright, expect
import os
base = os.environ.get("PORTFOLIO_TEST_URL", "http://127.0.0.1:5188")
with sync_playwright() as p:
    b=p.chromium.launch()
    page=b.new_page(reduced_motion="reduce")
    page.add_init_script("""window.SpeechRecognition = class {
      start() { queueMicrotask(() => { this.onerror({error:'not-allowed'}); this.onend(); }); }
      abort() { this.onend(); }
    }""")
    page.goto(base)
    page.locator("[data-assistant-launcher]").click()
    page.once("dialog", lambda d: d.accept())
    page.get_by_role("button",name="Microphone",exact=True).click()
    expect(page.get_by_text("Microphone unavailable.",exact=False)).to_be_visible()
    page.locator("[data-assistant-input]").fill("How does SiteAware work?")
    page.evaluate("""() => {
        const fetchOriginal = window.fetch;
        window.fetch = (url, options) => {
          window.fetch = fetchOriginal;
          return new Promise((_, reject) => options.signal.addEventListener("abort",
            () => reject(new DOMException("Aborted", "AbortError")), { once: true }));
        };
    }""")
    page.locator("[data-assistant-input]").press("Enter")
    page.get_by_role("button",name="Cancel answer").click()
    expect(page.get_by_text("Answer cancelled.")).to_be_visible()

    page.get_by_role("button",name="Retry").click()
    expect(page.locator(".assistant-sources a").last).to_have_attribute("href",base + "/work/siteaware-ai#engineering")
    page.get_by_role("button",name="Clear chat").click()
    expect(page.locator(".assistant-message")).to_have_count(1)
    page.route("**/api/assistant/chat", lambda route: route.fulfill(json={
      "answer":"<b>plain text</b>","confidence":"mock",
      "sources":[{"url":"/admin","title":"Unapproved route"}],
      "navigation":{"kind":"openCv","target":"https://example.invalid","label":"External"}
    }))
    page.locator("[data-assistant-input]").fill("Show my CV")
    page.locator("[data-assistant-input]").press("Enter")
    expect(page.get_by_text("<b>plain text</b>",exact=True)).to_be_visible()
    expect(page.locator(".assistant-action,.assistant-sources a")).to_have_count(0)
    separate = b.new_page(reduced_motion="reduce")
    separate.goto(base)
    separate.locator("[data-assistant-launcher]").click()
    expect(separate.locator(".assistant-message")).to_have_count(1)
    expect(separate.get_by_text("<b>plain text</b>",exact=True)).to_have_count(0)
    assert page.request.post(base+"/api/assistant/chat",data={"question":"How does SiteAware work?"},
        headers={"Origin":"https://example.invalid","Content-Type":"application/json"}).status == 403
    assert page.request.post(base+"/api/assistant/chat",data={"question":"x"*13000},
        headers={"Origin":base,"Content-Type":"application/json"}).status == 413
    for _ in range(10):
        limited = page.request.post(base+"/api/assistant/chat",data={"question":"Show my CV"},
            headers={"Origin":base,"Content-Type":"application/json"})
        if limited.status == 429:
            break
        assert limited.status == 200
    assert limited.status == 429 and limited.json()["error"] == "rate_limited"
    print("Mock microphone denial, cancellation, retry, reset, session isolation, text rendering, link validation, origin, payload and rate-limit gates PASS")
