import { test, expect } from '@playwright/test';

test('Validar exclusao mutua de menus e identificar duplicidade', async ({ page }) => {
  await page.goto('/'); // Vá para a página inicial
  await page.waitForLoadState('networkidle');

  // Nós tentamos garantir que o menu dinamico está visível.
  // Se o Playwright encontrar o menu dinamico E o menu fixo antigo, 
  // este teste vai estourar um erro de AMBIGUIDADE (strict mode violation).
  const menuDinamico = page.locator('.nav');
  await expect(menuDinamico, 'ERRO CRÍTICO: Duplicidade de menu encontrada na tela!').toBeVisible();
});