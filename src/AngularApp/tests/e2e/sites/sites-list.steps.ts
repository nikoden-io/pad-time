import {Given, Then, Before, After, setDefaultTimeout} from '@cucumber/cucumber';
import {chromium, Browser, Page, expect} from '@playwright/test';

setDefaultTimeout(30_000);

let browser: Browser;
let page: Page;

Before(async function () {
  browser = await chromium.launch({headless: true});
  const context = await browser.newContext();
  page = await context.newPage();

  // Force English locale before navigating
  await page.goto('http://localhost:4200');
  await page.evaluate(() => {
    localStorage.setItem('preferredLanguage', 'en');
  });
});

After(async function () {
  await browser?.close();
});

Given('I navigate to {string}', async function (path: string) {
  await page.goto(`http://localhost:4200${path}`);
  await page.waitForLoadState('networkidle');

  // Debug: Check current URL (might be redirected to login)
  const currentUrl = page.url();
  console.log('Current URL after navigation:', currentUrl);

  // Debug: Take screenshot
  await page.screenshot({path: 'debug-screenshot.png'});
});

Then('I should see {string} heading', async function (text: string) {
  // Wait a bit for Angular to render
  await page.waitForTimeout(2000);

  // Debug logs
  const allH1 = await page.locator('h1').allTextContents();
  console.log('════════ DEBUG ════════');
  console.log('All H1 texts found:', allH1);
  console.log('Looking for:', text);
  console.log('═══════════════════════');

  const heading = page.locator('h1', {hasText: text});
  await expect(heading).toBeVisible({timeout: 10000});
});

Then('I should see a table with sites', async function () {
  const table = page.locator('p-table');
  await expect(table).toBeVisible({timeout: 10000});
});
