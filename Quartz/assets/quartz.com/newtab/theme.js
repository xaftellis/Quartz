// Set the initial palette before paint; the host supplies live changes.
(() => {
  const theme = new URLSearchParams(location.search).get('theme');
  document.documentElement.dataset.theme = ['light', 'dark', 'black', 'aqua', 'xmas'].includes(theme) ? theme : 'light';
})();
