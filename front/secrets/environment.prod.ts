export const environment = {
  production: true,
  hmr: false,
  API_URL: window['API_URL'] || 'https://pekalam.me',
  WS_URL: window['WS_URL'] || 'https://pekalam.me',
  serverDemoModeEnabled: (window.hasOwnProperty('serverDemoModeEnabled') ? window['serverDemoModeEnabled'] : true),
};
