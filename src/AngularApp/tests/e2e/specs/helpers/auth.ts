import { Page, expect } from '@playwright/test';

export const DEMO_PASSWORD = 'Passw0rd!';

export const Users = {
  admin: 'admin@test.be',     // global admin
  alice: 'alice@test.be',     // global member, rich history
  georges: 'georges@test.be', // free member, blocked by 45€ debt
} as const;

/**
 * Performs a full OIDC Authorization-Code login through the IdentityServer-hosted
 * login page and waits until the SPA has rehydrated the authenticated session.
 */
export async function login(page: Page, email: string, password: string = DEMO_PASSWORD): Promise<void> {
  await page.goto('/auth/login');
  await page.getByRole('button', { name: /sign in/i }).click();

  // Now on the IdentityServer login page (https://localhost:5001).
  await page.waitForURL(/localhost:5001/, { timeout: 45_000 });
  await page.fill('input[name="Input.Username"]', email);
  await page.fill('input[name="Input.Password"]', password);
  await page.click('button[name="Input.Button"][value="login"]');

  // A consent screen may appear for first-party clients; approve it if so.
  await page.waitForLoadState('domcontentloaded');
  if (/\/consent/i.test(page.url())) {
    await page
      .locator('button[value="yes"], button[name="button"][value="yes"]')
      .first()
      .click()
      .catch(() => undefined);
  }

  // Back to the SPA, session established.
  await page.waitForURL('http://localhost:4200/**', { timeout: 45_000 });
  await expect(page.getByRole('link', { name: /Rejoindre|Join/i })).toBeVisible();
}

export async function logout(page: Page): Promise<void> {
  await page.locator('.user-trigger').click();
  // The dropdown logout item (distinct from the mobile drawer's logout link).
  await page.locator('.dropdown-item--danger').click();
  // Logout round-trips through IdentityServer end-session back to the SPA root;
  // wait for that to settle so a subsequent navigation isn't interrupted.
  await page.waitForURL('http://localhost:4200/', { timeout: 30_000 });
  await expect(page.getByRole('button', { name: /sign in/i }).or(
    page.locator('.user-trigger'))).toBeVisible().catch(() => undefined);
}
