export const environment = {
  production: true,
  hmr: false,
  API_URL: window['API_URL'] || 'https://auctionhouse.pekalam.store',
  WS_URL: window['WS_URL'] || 'https://auctionhouse.pekalam.store',
  serverDemoModeEnabled: (window.hasOwnProperty('serverDemoModeEnabled') ? window['serverDemoModeEnabled'] : true),
};
