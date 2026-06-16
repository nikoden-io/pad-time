import { test, expect } from '@playwright/test';
import { login, Users } from './helpers/auth';

test.describe('Authenticated navigation', () => {
  test('a member can open the public matches (Join) page', async ({ page }) => {
    await login(page, Users.alice);
    await page.getByRole('link', { name: /Rejoindre|Join/i }).click();
    await expect(page).toHaveURL(/\/join/);
  });

  test('a member can open the My Matches page', async ({ page }) => {
    await login(page, Users.alice);
    await page.getByRole('link', { name: /Mes matches|My matches/i }).click();
    await expect(page).toHaveURL(/\/matches/);
  });

  test('a member can open the Booking page', async ({ page }) => {
    await login(page, Users.alice);
    await page.getByRole('link', { name: /Réserver|Booking/i }).click();
    await expect(page).toHaveURL(/\/booking/);
  });
});
