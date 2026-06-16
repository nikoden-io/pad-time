import { test, expect } from '@playwright/test';
import { login, logout, Users } from './helpers/auth';

test.describe('Authentication (OIDC)', () => {
  test('a member can sign in and reach the authenticated app', async ({ page }) => {
    await login(page, Users.alice);

    await expect(page).toHaveURL('http://localhost:4200/');
    await expect(page.getByRole('link', { name: /Réserver|Booking/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /Rejoindre|Join/i })).toBeVisible();
  });

  test('a signed-in member can sign out', async ({ page }) => {
    await login(page, Users.alice);
    await logout(page);

    // After logout the guarded app is no longer reachable.
    await page.goto('/matches');
    await expect(page).toHaveURL(/\/auth\/login/);
  });
});
