from pathlib import Path
from playwright.sync_api import sync_playwright, expect
import argparse
import fitz
cli=argparse.ArgumentParser()
cli.add_argument("--url", default="http://127.0.0.1:5188")
cli.add_argument("--engine", choices=["firefox", "webkit"])
args=cli.parse_args()
base=args.url
root=Path("tmp")
root.mkdir(exist_ok=True)
with sync_playwright() as p:
    for engine in ([args.engine] if args.engine else ["firefox", "webkit"]):
        b=getattr(p,engine).launch()
        page=b.new_page(viewport={"width":390,"height":844},reduced_motion="reduce")
        failures=[]
        page.on("pageerror", lambda e: failures.append(str(e)))
        assert page.goto(base + "/?lang=ar").ok
        expect(page.locator("html")).to_have_attribute("dir","rtl")
        assert page.evaluate("document.documentElement.scrollWidth === innerWidth")
        page.locator("[data-open-portfolio-assistant]").press("Enter")
        expect(page.get_by_role("dialog")).to_be_visible()
        page.keyboard.press("Escape")
        expect(page.get_by_role("dialog")).to_be_hidden()
        assert page.goto(base + "/pitch?lang=ar").ok
        page.locator('.v2-pitch-nav a[href$="#slide-5"]').click()
        expect(page.locator("#slide-5")).to_be_in_viewport()
        page.screenshot(path=root / f"{engine}-mobile-pitch.png")
        assert not failures, failures
        print(engine, "Arabic mobile, portrait and pitch PASS")
        assert page.goto(base+"/cv?lang=ar").ok
        attachment = page.request.get(base + "/cv/download")
        assert attachment.ok and attachment.headers["content-disposition"].startswith("attachment;")
        with fitz.open(stream=attachment.body(), filetype="pdf") as doc:
            assert len(doc) == 2 and all(len(sheet.get_links()) == 4 for sheet in doc)
        print(engine, "PDF response PASS")
        with page.expect_download(timeout=12000) as download:
            page.locator("a[download]").click(no_wait_after=True)
        with fitz.open(download.value.path()) as doc:
            assert len(doc) == 2
        b.close()
        print(engine, "PDF download event PASS")
