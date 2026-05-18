import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  // Diretório onde os testes estão localizados
  testDir: './tests',
  
  // Executa testes em arquivos diferentes em paralelo para ganhar velocidade
  fullyParallel: true,
  
  // Falha o build no CI se você esquecer um test.only no código local
  forbidOnly: !!process.env.CI,
  
  // Número de tentativas de rodar novamente um teste caso ele falhe (bom para flaky tests)
  retries: process.env.CI ? 2 : 0,
  
  // Número de workers (threads) em paralelo. No CI usa menos para evitar gargalo de CPU
  workers: process.env.CI ? 1 : undefined,
  
  // Formato do relatório de saída (html gera uma página rica com logs de falha)
  reporter: 'html',

  // Configurações globais para todos os projetos de execução
  use: {
    // URL base para evitar digitar o domínio inteiro em cada test.goto()
    baseURL: 'http://localhost:3000',

    // Coleta o rastreamento (trace) de falhas. 'on-first-retry' ajuda a debugar no CI
    trace: 'on-first-retry',
    
    // Tira screenshot automaticamente se o teste falhar
    screenshot: 'only-on-failure',
  },

  // Projetos para rodar em diferentes engines de browsers
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },

    /* Exemplo para testar em resoluções mobile se necessário */
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
  ],

  /* Se o seu front-end precisa rodar localmente antes dos testes começarem */
  // webServer: {
  //   command: 'npm run dev',
  //   url: 'http://localhost:3000',
  //   reuseExistingServer: !process.env.CI,
  // },
});