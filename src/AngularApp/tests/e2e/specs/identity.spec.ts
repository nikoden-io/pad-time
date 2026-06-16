import { test, expect } from '@playwright/test';

/**
 * E2E checks against the running Pad'Time IdentityServer (https://localhost:5001).
 * These do not depend on the SPA dev server and validate the real OIDC auth surface.
 */
test.describe('IdentityServer (OIDC authority)', () => {
  test('serves the OpenID Connect discovery document', async ({ request }) => {
    const res = await request.get('https://localhost:5001/.well-known/openid-configuration');
    expect(res.ok()).toBeTruthy();
    const doc = await res.json();
    expect(doc.issuer).toContain('localhost:5001');
    expect(doc.authorization_endpoint).toContain('/connect/authorize');
    expect(doc.token_endpoint).toContain('/connect/token');
  });

  test('renders the credential login form', async ({ page }) => {
    await page.goto('https://localhost:5001/Account/Login');
    await expect(page.locator('input[name="Input.Username"]')).toBeVisible();
    await expect(page.locator('input[name="Input.Password"]')).toBeVisible();
    await expect(page.locator('button[name="Input.Button"][value="login"]')).toBeVisible();
  });
});
