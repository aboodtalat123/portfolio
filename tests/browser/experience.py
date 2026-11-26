from pathlib import Path
from playwright.sync_api import sync_playwright, expect
import fitz

root = Path.cwd()
import os
base = os.environ.get("PORTFOLIO_TEST_URL", "http://127.0.0.1:5188")
with sync_playwright() as p:
    browser = p.chromium.launch()
    errors = []
    for label, width, height, locale in [("desktop",1440,1000,""),("mobile",390,844,"?lang=ar"),("tablet",768,1024,"")]:
        context = browser.new_context(viewport={"width":width,"height":height}, reduced_motion="reduce", color_scheme="dark")
        page = context.new_page()
        page.on("pageerror", lambda err: errors.append(str(err)))
        assert page.goto(base + "/" + locale).ok
        framework_script = page.locator('script[src*="_framework/blazor.web"]').get_attribute("src")
        assert framework_script and page.request.get(base + "/" + framework_script.lstrip("/")).ok
        expect(page.locator("html")).to_have_attribute("dir", "rtl" if locale else "ltr")
        expect(page.locator(".v2-portrait img")).to_be_visible()
        assert page.locator(".v2-portrait img").evaluate("e => e.naturalWidth") > 0
        assert page.evaluate("document.documentElement.scrollWidth === innerWidth")
        page.screenshot(path=root / f"tmp/{label}-home.png", full_page=True)
        page.locator("[data-open-portfolio-assistant]").focus()
        page.keyboard.press("Enter")
        expect(page.get_by_role("dialog")).to_be_visible()
        text = "كيف يعمل SiteAware؟" if locale else "How does SiteAware work?"
        page.locator("[data-assistant-input]").fill(text)
        page.locator("[data-assistant-input]").press("Enter")
        expect(page.locator(".assistant-sources a").last).to_have_attribute("href", __import__("re").compile("/work/siteaware-ai"))
        page.screenshot(path=root / f"tmp/{label}-assistant.png")
        page.keyboard.press("Escape")
        expect(page.get_by_role("dialog")).to_be_hidden()
        expect(page.locator("[data-open-portfolio-assistant]")).to_be_focused()
        page.locator("[data-theme-toggle]").click()
        expect(page.locator("html")).to_have_attribute("data-theme","light")
        page.wait_for_function("""() => getComputedStyle(document.body).backgroundColor === 'rgb(247, 247, 240)'""")
        assert page.locator("h1").evaluate("e=>getComputedStyle(e).color") == 'rgb(25, 36, 25)'
        page.screenshot(path=root / f"tmp/{label}-light.png")
        page.locator("[data-assistant-launcher]").click()
        page.locator("[data-assistant-input]").fill("افتح السيرة" if locale else "Open the technical CV")
        page.locator("[data-assistant-input]").press("Enter")
        expect(page.locator(".assistant-action")).to_have_attribute("href", "/cv" + locale)
        assert page.url == base + "/" + locale
        page.locator(".assistant-action").click()
        page.wait_for_url(base + "/cv" + locale)
        expect(page.locator("h1")).to_be_visible()
        page.locator("[data-assistant-launcher]").click()
        expect(page.locator(".assistant-message--user").first).to_have_text(text)
        page.keyboard.press("Escape")
        with page.expect_download() as download:
            page.locator("a[download]").click()
        assert download.value.suggested_filename.endswith(".pdf")
        doc = fitz.open(download.value.path())
        assert len(doc) == 2 and all(len(q.get_links()) == 4 for q in doc)
        for slug in ["siteaware-ai","tebiha","dr-solution","level-up-pos","wesal-mabrouk","fairaid-gaza"]:
            assert page.goto(base + "/work/" + slug + locale).ok
            expect(page.locator("#engineering")).to_be_visible()
            assert page.evaluate("document.documentElement.scrollWidth === innerWidth")
        assert page.goto(base + "/pitch" + locale).ok
        with page.expect_download() as pitch_download:
            page.locator('a[download][href^="/pitch/download"]').click()
        assert pitch_download.value.suggested_filename.endswith("_AR.pdf" if locale else "_EN.pdf")
        with fitz.open(pitch_download.value.path()) as pitch_doc:
            assert len(pitch_doc) == 14
            assert all(sheet.get_text().strip() for sheet in pitch_doc)
        page.locator('.v2-pitch-nav a[href$="#slide-5"]').click()
        assert page.url.endswith("#slide-5")
        expect(page.locator("#slide-5")).to_be_in_viewport()
        print(label, "home, assistant, localization, theme, download, cases and pitch PASS")
        context.close()
    assert not errors, errors
