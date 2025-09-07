import time
from playwright.sync_api import sync_playwright, Page, expect

def run_verification(page: Page):
    """
    This script automates the verification of the picking list application.
    It uploads a PDF, verifies the review page, saves the data, and verifies the details page.
    """
    base_url = "http://localhost:5100"

    # 1. Navigate to the upload page and take a screenshot
    print("Navigating to upload page...")
    page.goto(f"{base_url}/upload")
    expect(page.get_by_role("heading", name="Upload Picking List")).to_be_visible()
    page.screenshot(path="jules-scratch/verification/01_upload_page.png")
    print("Screenshot of upload page taken.")

    # 2. Upload a sample PDF. This triggers a background API call and a client-side navigation.
    print("Uploading sample PDF...")
    pdf_path = "/app/Samples/15355234.pdf"
    page.locator("input[type=file]").set_input_files(pdf_path)

    # 3. On the review page, wait for an element to appear, then verify content and take a screenshot
    print("Verifying review page...")
    # Increased timeout to 30 seconds to allow for parsing time
    expect(page.get_by_role("heading", name="Review & Edit Picking List")).to_be_visible(timeout=30000)
    expect(page.get_by_text("Sales Order: 39053467")).to_be_visible()
    expect(page.get_by_role("gridcell", name="PP2448IO/C")).to_be_visible()
    time.sleep(2)
    page.screenshot(path="jules-scratch/verification/02_review_page.png")
    print("Screenshot of review page taken.")

    # 4. Save the picking list. This also triggers a client-side navigation.
    print("Saving picking list...")
    page.get_by_role("button", name="Approve & Save").click()

    # 5. On the details page, wait for an element and take a screenshot
    print("Verifying details page...")
    expect(page.get_by_role("heading", name="Picking List Details")).to_be_visible(timeout=10000)
    expect(page.get_by_text("Sales Order: 39053467")).to_be_visible()
    expect(page.get_by_role("gridcell", name="PP2448IO/C")).to_be_visible()
    page.screenshot(path="jules-scratch/verification/03_details_page.png")
    print("Screenshot of details page taken.")

    print("Verification script completed successfully.")

if __name__ == "__main__":
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        run_verification(page)
        browser.close()
