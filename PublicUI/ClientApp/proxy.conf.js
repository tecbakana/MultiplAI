const { env } = require('process');

// CMSAPI.Publica roda em http://localhost:7230
const target = env.CMSAPI_PUBLICA_URL || 'http://localhost:7230';

const PROXY_CONFIG = [
  {
    context: [
      "/api/publico",
      "/api/loja"
    ],
    proxyTimeout: 60000,
    timeout: 60000,
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
