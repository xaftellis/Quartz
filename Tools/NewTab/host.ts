// Quartz implementations of Chromium's browser-process APIs. UI components
// and animations are imported from the pinned Chromium source unchanged.
export const loadTimeData = {
  data: {},
  getBoolean(name) { return !!this.data[name]; },
  getInteger(name) { return this.data[name] ?? 0; },
  getString(name) { return this.data[name] ?? name; },
  getStringF(name, ...args) { return this.getString(name).replace(/\$(\d)/g, (_,n) => args[+n-1] ?? ''); },
  valueExists(name) { return name in this.data; },
  getValue(name) { return this.data[name]; },
  overrideValues(data) { Object.assign(this.data,data); }
};
export function faviconUrl(url) { return '/newtab/chromium/cr_components/searchbox/icons/default.svg'; }
