import { test, expect } from '@playwright/test';
import { login, Users } from './helpers/auth';

test.describe('Admin access control', () => {
  test('a global admin can reach the admin area', async ({ page }) => {
    await login(page, Users.admin);

    // The admin entry point is offered in the navigation for admins.
    await expect(page.getByRole('link', { name: 'Admin', exact: true })).toBeVisible();

    await page.goto('/admin');
    await expect(page).toHaveURL(/\/admin/);
    // The admin dashboard renders KPI cards (label shown in the active UI language).
    await expect(
      page.getByText('Sites actifs').or(page.getByText('Active sites')),
    ).toBeVisible();
  });

  test('a non-admin member is kept out of the admin area', async ({ page }) => {
    await login(page, Users.alice);

    await page.goto('/admin');
    // adminGuard redirects non-admins away from /admin.
    await expect(page).not.toHaveURL(/\/admin/);
  });
});
