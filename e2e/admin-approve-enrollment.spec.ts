import { test, expect } from '@playwright/test';
import fs from 'fs/promises';

test('admin approves a pending enrollment', async ({ page }) => {
  const authSessionStorage = JSON.parse(
    await fs.readFile(
      'playwright/.auth/admin-session.json',
      'utf-8'
    )
  );

  await page.addInitScript((storage) => {
    for (const [key, value] of Object.entries(storage)) {
      window.sessionStorage.setItem(key, value as string);
    }
  }, authSessionStorage);

  await page.goto('/instructor');

  await expect(
    page.getByRole('button', { name: 'Approve' }).first()
  ).toBeVisible();

  const firstApprove = page
    .getByRole('button', { name: 'Approve' })
    .first();

  await firstApprove.click();

  await expect(
    page.getByText('Approved').first()
  ).toBeVisible();
});