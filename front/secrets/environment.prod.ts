export const environment = {
  production: true,
  hmr: false,
  API_URL: window['API_URL'] || 'https://pekalam.online',
  WS_URL: window['WS_URL'] || 'https://pekalam.online',
  serverDemoModeEnabled: (window.hasOwnProperty('serverDemoModeEnabled') ? window['serverDemoModeEnabled'] : true),
};
