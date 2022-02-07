export const environment = {
  production: true,
  hmr: false,
  API_URL: window['API_URL'] || 'https://localhost:10000',
  WS_URL: window['WS_URL'] || 'https://localhost:10000',
  serverDemoModeEnabled: window['serverDemoModeEnabled'] || true,
};
