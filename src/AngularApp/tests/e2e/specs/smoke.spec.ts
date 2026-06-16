import { test, expect } from '@playwright/test';

test.describe('Smoke / unauthenticated', () => {
  test('unauthenticated visit to a guarded route redirects to the login page', async ({ page }) => {
    await page.goto('/matches');
    await expect(page).toHaveURL(/\/auth\/login/);
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });

  test('login page renders the sign-in entry point', async ({ page }) => {
    await page.goto('/auth/login');
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });
});
