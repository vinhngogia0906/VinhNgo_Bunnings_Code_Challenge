import { expect, test } from '@playwright/test'

test('daily view shows the seeded top product', async ({ page }) => {
  await page.goto('/')

  // Wait for the first fetch.
  const result = page.getByTestId('daily-result')
  await expect(result).toBeVisible()
  await expect(result).toContainText(/Arlec|Ezy Storage/)
})

test('rolling view shows a 3-day window', async ({ page }) => {
  await page.goto('/rolling')

  const result = page.getByTestId('rolling-result')
  await expect(result).toBeVisible()
  await expect(result).toContainText('Ezy Storage 37L Flexi Laundry Basket - White')
})