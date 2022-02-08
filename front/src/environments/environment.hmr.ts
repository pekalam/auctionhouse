export const environment = {
  production: false,
  hmr: true,
  API_URL: window['API_URL'] || 'https://localhost:10000',
  WS_URL: window['WS_URL'] || 'https://localhost:10000',
  serverDemoModeEnabled: (window.hasOwnProperty('serverDemoModeEnabled') ? window['serverDemoModeEnabled'] : true),
 };
