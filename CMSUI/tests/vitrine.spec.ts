import { test, expect } from '@playwright/test';

test('test', async ({ page }) => {
  await page.goto('http://localhost:44455/s/ceartperson/produtos');
  await expect(page.getByText('Esta página ainda não tem')).not.toBeVisible();
  await expect(page.getByText('Home')).toBeVisible();
});