// 


import { test as setup, expect } from '@playwright/test';

const authFile = 'playwright/.auth/admin.json';

setup('authenticate as admin', async ({ page }) => {
  page.on('response', async (response) => {
    if (response.url().includes('/api/v1/auth/login')) {
      console.log('LOGIN STATUS:', response.status());
      console.log('LOGIN URL:', response.url());

      try {
        console.log('LOGIN BODY:', await response.text());
      } catch {
        console.log('Could not read login response body');
      }
    }
  });

  await page.goto('/login');

  await page.getByLabel('Username').fill(process.env.TMS_ADMIN_USER!);
  await page.getByLabel('Password').fill(process.env.TMS_ADMIN_PASS!);

  await page.getByRole('button', { name: 'Login' }).click();

  await expect(page).toHaveURL(/\/admin\/courses/);

const authSessionStorage = await page.evaluate(() => {
  return JSON.stringify(window.sessionStorage);
});

await require('fs').promises.writeFile(
  'playwright/.auth/admin-session.json',
  authSessionStorage
);

await page.context().storageState({
  path: authFile,
});
});