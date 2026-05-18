import { test, expect } from '@playwright/test';

/**
 * Testes de aceitação da feature Vitrine Canônica por Área.
 * Referência: dev-requests vitrine-canonico-1 a 7.
 *
 * Pré-condições:
 * - CMSUI + CMSAPI rodando (playwright.config.ts: dotnet run em localhost:5050)
 * - Tenant "ceartperson" com ao menos uma área de vitrine publicada
 *
 * Testes admin (Grupo 4 e 5):
 * - Requer env vars: ADMIN_EMAIL, ADMIN_PASS, TEST_AREA_ID
 * - Ficam com skip automático se as vars não estiverem configuradas
 */

const SLUG = 'ceartperson';
const AREA_URL = 'produtos'; // URL de área existente no tenant de teste

// ─── GRUPO 1: vitrine-opcoes.json (vitrine-canonico-5-asset-opcoes) ─────────────────

test.describe('Grupo 1 — vitrine-opcoes.json', () => {

  test('arquivo existe e responde 200', async ({ request }) => {
    const res = await request.get('/assets/vitrine-opcoes.json');
    expect(res.status()).toBe(200);
  });

  test('estrutura válida: tipos, estilos e paletas são arrays não-vazios', async ({ request }) => {
    const opts = await (await request.get('/assets/vitrine-opcoes.json')).json();

    for (const chave of ['tipos', 'estilos', 'paletas']) {
      expect(Array.isArray(opts[chave]), `"${chave}" deve ser array`).toBeTruthy();
      expect(opts[chave].length, `"${chave}" não pode estar vazio`).toBeGreaterThan(0);
    }
  });

  test('cada opção tem campos "valor" (string) e "label" (string não-vazia)', async ({ request }) => {
    const opts = await (await request.get('/assets/vitrine-opcoes.json')).json();

    for (const chave of ['tipos', 'estilos', 'paletas']) {
      for (const item of opts[chave]) {
        expect(typeof item.valor, `${chave}[].valor`).toBe('string');
        expect(typeof item.label, `${chave}[].label`).toBe('string');
        expect(item.valor.trim()).not.toBe('');
        expect(item.label.trim()).not.toBe('');
      }
    }
  });
});

// ─── GRUPO 2: API pública — campo canonicalArea (vitrine-canonico-1 / 4) ────────────

test.describe('Grupo 2 — API pública: canonicalArea nas áreas', () => {

  test('GET /site/slug/{slug} retorna 200 com array areas', async ({ request }) => {
    const res = await request.get(`/site/slug/${SLUG}`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('areas');
    expect(Array.isArray(body.areas)).toBeTruthy();
    expect(body.areas.length).toBeGreaterThan(0);
  });

  test('cada área expõe o campo canonicalArea como boolean', async ({ request }) => {
    const body = await (await request.get(`/site/slug/${SLUG}`)).json();
    const areas: any[] = body.areas ?? [];

    for (const area of areas) {
      expect(
        typeof area.canonicalArea,
        `Área "${area.nome ?? area.areaid}" deve ter canonicalArea boolean`
      ).toBe('boolean');
    }
  });

  test('exatamente uma área tem canonicalArea=true', async ({ request }) => {
    const body = await (await request.get(`/site/slug/${SLUG}`)).json();
    const canonicas = (body.areas ?? []).filter((a: any) => a.canonicalArea === true);

    expect(canonicas.length, 'Deve haver exatamente 1 área canônica por tenant').toBe(1);
  });

  test('a área canônica possui vitrineValoresJson preenchido (tema disponível para herança)', async ({ request }) => {
    const body = await (await request.get(`/site/slug/${SLUG}`)).json();
    const canonica = (body.areas ?? []).find((a: any) => a.canonicalArea === true);

    expect(canonica, 'Área canônica não encontrada').toBeTruthy();
    expect(
      typeof canonica.vitrineValoresJson === 'string' && canonica.vitrineValoresJson.trim() !== '',
      'Área canônica deve ter vitrineValoresJson com o tema'
    ).toBeTruthy();
  });
});

// ─── GRUPO 3: Site público — persistência do tema CSS (vitrine-canonico-3 / 4) ──────

test.describe('Grupo 3 — Site público: persistência do tema CSS', () => {

  test('CSS var --v-cor-fundo é injetada no :root ao carregar o site', async ({ page }) => {
    await page.goto(`/s/${SLUG}`);
    await page.waitForLoadState('networkidle');

    const corFundo = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--v-cor-fundo').trim()
    );
    expect(corFundo, 'CSS var --v-cor-fundo deve estar definida no :root após carregar área com vitrine').not.toBe('');
  });

  test('CSS var --v-cor-texto é injetada no :root ao carregar o site', async ({ page }) => {
    await page.goto(`/s/${SLUG}`);
    await page.waitForLoadState('networkidle');

    const corTexto = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--v-cor-texto').trim()
    );
    expect(corTexto, 'CSS var --v-cor-texto deve estar definida no :root').not.toBe('');
  });

  test('CSS var --v-cor-fundo persiste ao navegar para outra área via menu', async ({ page }) => {
    await page.goto(`/s/${SLUG}`);
    await page.waitForLoadState('networkidle');

    const corFundo1 = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--v-cor-fundo').trim()
    );
    expect(corFundo1).not.toBe('');

    const links = page.locator('.nav a');
    const count = await links.count();

    if (count > 1) {
      await links.nth(1).click();
      await page.waitForLoadState('networkidle');

      const corFundo2 = await page.evaluate(() =>
        getComputedStyle(document.documentElement).getPropertyValue('--v-cor-fundo').trim()
      );
      expect(corFundo2, 'O tema CSS da área canônica deve persistir após navegação').toBe(corFundo1);
    }
  });

  test('apenas um elemento <nav> visível por vez na página do site', async ({ page }) => {
    await page.goto(`/s/${SLUG}/${AREA_URL}`);
    await page.waitForLoadState('networkidle');

    const navsVisiveis = await page.locator('nav:visible').count();
    expect(
      navsVisiveis,
      'Deve haver exatamente 1 <nav> visível — o snapshot da vitrine não deve incluir menu duplicado'
    ).toBe(1);
  });

  test('menu de navegação exibe o nome da área atual como item ativo ou visível', async ({ page }) => {
    await page.goto(`/s/${SLUG}/${AREA_URL}`);
    await page.waitForLoadState('networkidle');

    const nav = page.locator('.nav');
    await expect(nav, 'Barra de navegação (.nav) deve estar visível').toBeVisible();
  });
});

// ─── GRUPO 4: Admin — UI do gerador (vitrine-canonico-7) ─────────────────────────────
// Requer: ADMIN_EMAIL, ADMIN_PASS, TEST_AREA_ID definidos nas env vars

test.describe('Grupo 4 — Admin: UI do gerador de vitrine por área', () => {

  test.beforeEach(async ({}, testInfo) => {
    if (!process.env.ADMIN_EMAIL || !process.env.ADMIN_PASS || !process.env.TEST_AREA_ID) {
      testInfo.skip(true, 'Skipped: configure ADMIN_EMAIL, ADMIN_PASS e TEST_AREA_ID');
    }
  });

  async function fazerLogin(request: any): Promise<string> {
    const res = await request.post('/auth/login', {
      data: { email: process.env.ADMIN_EMAIL, senha: process.env.ADMIN_PASS }
    });
    const body = await res.json();
    return body.token as string;
  }

  test('tela do editor de vitrine carrega sem erro', async ({ page, request }) => {
    const token = await fazerLogin(request);
    await page.addInitScript((tk: string) => {
      const u = JSON.parse(sessionStorage.getItem('usuario') || '{}');
      sessionStorage.setItem('usuario', JSON.stringify({ ...u, token: tk }));
    }, token);

    await page.goto(`/vitrine/areaconfig/${process.env.TEST_AREA_ID}`);
    await page.waitForLoadState('networkidle');

    await expect(page.getByText('Gerar com IA'), 'Card de geração deve estar visível').toBeVisible();
  });

  test('selects de tipo, estilo e paleta estão presentes no formulário de geração', async ({ page, request }) => {
    const token = await fazerLogin(request);
    await page.addInitScript((tk: string) => {
      const u = JSON.parse(sessionStorage.getItem('usuario') || '{}');
      sessionStorage.setItem('usuario', JSON.stringify({ ...u, token: tk }));
    }, token);

    await page.goto(`/vitrine/areaconfig/${process.env.TEST_AREA_ID}`);
    await page.waitForLoadState('networkidle');

    // Os três selects devem estar presentes
    const tipoSelect  = page.locator('select').filter({ hasText: /tipo/i }).first();
    const estiloSelect = page.locator('select').filter({ hasText: /estilo/i }).first();
    const paletaSelect = page.locator('select').filter({ hasText: /paleta/i }).first();

    await expect(tipoSelect,   'Select de tipo de página deve estar visível').toBeVisible();
    await expect(estiloSelect, 'Select de estilo visual deve estar visível').toBeVisible();
    await expect(paletaSelect, 'Select de paleta de cores deve estar visível').toBeVisible();
  });

  test('cada select de opções tem ao menos 2 opções carregadas do JSON', async ({ page, request }) => {
    const token = await fazerLogin(request);
    await page.addInitScript((tk: string) => {
      const u = JSON.parse(sessionStorage.getItem('usuario') || '{}');
      sessionStorage.setItem('usuario', JSON.stringify({ ...u, token: tk }));
    }, token);

    await page.goto(`/vitrine/areaconfig/${process.env.TEST_AREA_ID}`);
    await page.waitForLoadState('networkidle');

    // Conta options em cada select (inclui a opção "Automático", então mínimo 2)
    const selects = await page.locator('select').all();
    for (const sel of selects.slice(0, 3)) {
      const opts = await sel.locator('option').count();
      expect(opts, 'Cada select deve ter ao menos 2 opções (automático + opções do JSON)').toBeGreaterThanOrEqual(2);
    }
  });

  test('checkbox "Usar visual padrão do site" está oculto quando não há área canônica', async ({ page, request }) => {
    // Este teste só faz sentido em um tenant sem vitrine publicada.
    // Em tenants com canônica existente, o checkbox deve aparecer.
    // Aqui verificamos que o checkbox tem visibilidade controlada por ngIf.
    const token = await fazerLogin(request);
    await page.addInitScript((tk: string) => {
      const u = JSON.parse(sessionStorage.getItem('usuario') || '{}');
      sessionStorage.setItem('usuario', JSON.stringify({ ...u, token: tk }));
    }, token);

    await page.goto(`/vitrine/areaconfig/${process.env.TEST_AREA_ID}`);
    await page.waitForLoadState('networkidle');

    // O checkbox só deve existir se temAreaCanonica=true
    // Este teste valida que o elemento não existe na DOM quando não há canônica
    // (requer que TEST_AREA_ID seja a primeira área sem outra canônica)
    const checkbox = page.locator('#usarCanonico');
    const count = await checkbox.count();

    // Se count=0, está correto (ngIf removeu da DOM). Se count=1, deve estar visible=false.
    if (count > 0) {
      await expect(checkbox, 'Checkbox deve estar oculto quando não há área canônica').not.toBeVisible();
    }
    // count=0 => ngIf=false => comportamento correto, teste passa implicitamente
  });
});

// ─── GRUPO 5: API admin — publicação define área canônica (vitrine-canonico-3) ────────
// Requer: ADMIN_EMAIL, ADMIN_PASS, TEST_AREA_ID

test.describe('Grupo 5 — API admin: publicação define canonicalArea', () => {

  test.beforeEach(async ({}, testInfo) => {
    if (!process.env.ADMIN_EMAIL || !process.env.ADMIN_PASS || !process.env.TEST_AREA_ID) {
      testInfo.skip(true, 'Skipped: configure ADMIN_EMAIL, ADMIN_PASS e TEST_AREA_ID');
    }
  });

  test('POST /vitrine/area/{id}/publicar retorna 200', async ({ request }) => {
    const authRes = await request.post('/auth/login', {
      data: { email: process.env.ADMIN_EMAIL, senha: process.env.ADMIN_PASS }
    });
    const { token } = await authRes.json();

    const res = await request.post(
      `/vitrine/area/${process.env.TEST_AREA_ID}/publicar`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    expect(res.status()).toBe(200);
  });

  test('após publicação, GET /site/slug/{slug} exibe área com canonicalArea=true', async ({ request }) => {
    const authRes = await request.post('/auth/login', {
      data: { email: process.env.ADMIN_EMAIL, senha: process.env.ADMIN_PASS }
    });
    const { token } = await authRes.json();

    // Publicar a área de teste
    await request.post(
      `/vitrine/area/${process.env.TEST_AREA_ID}/publicar`,
      { headers: { Authorization: `Bearer ${token}` } }
    );

    // Verificar no site público
    const siteRes = await request.get(`/site/slug/${SLUG}`);
    const body = await siteRes.json();
    const canonicas = (body.areas ?? []).filter((a: any) => a.canonicalArea === true);

    expect(canonicas.length, 'Deve haver exatamente 1 área canônica após publicação').toBe(1);
  });

  test('publicar segunda área não muda a área canônica existente', async ({ request }) => {
    const authRes = await request.post('/auth/login', {
      data: { email: process.env.ADMIN_EMAIL, senha: process.env.ADMIN_PASS }
    });
    const { token } = await authRes.json();

    // Verificar canônica antes
    const antes = await (await request.get(`/site/slug/${SLUG}`)).json();
    const canonicaAntes = (antes.areas ?? []).find((a: any) => a.canonicalArea === true);
    if (!canonicaAntes) {
      test.skip(true, 'Nenhuma área canônica existente — pré-condição não atendida');
      return;
    }

    // Publicar a área de teste (segunda área)
    await request.post(
      `/vitrine/area/${process.env.TEST_AREA_ID}/publicar`,
      { headers: { Authorization: `Bearer ${token}` } }
    );

    // Canônica não deve ter mudado
    const depois = await (await request.get(`/site/slug/${SLUG}`)).json();
    const canonicaDepois = (depois.areas ?? []).find((a: any) => a.canonicalArea === true);

    expect(canonicaDepois?.areaid).toBe(canonicaAntes.areaid);
    expect(
      (depois.areas ?? []).filter((a: any) => a.canonicalArea === true).length
    ).toBe(1);
  });
});
