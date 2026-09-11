/* Quartz WebView2 adapter for Chromium's searchbox and most-visited interactions.
 * See chromium-LICENSE and third_party/chromium/new_tab/README.md. */
(() => {
  'use strict';
  const $ = id => document.getElementById(id);
  const input = $('search-input'), box = $('searchbox'), list = $('suggestions');
  const dialog = $('shortcut-dialog'), menu = $('shortcut-menu');
  const storageKey = 'quartz.newtab.shortcuts.v1';
  const engineNames = { google: 'Google', bing: 'Bing', duckduckgo: 'DuckDuckGo', yahoo: 'Yahoo', youtube: 'YouTube', wikipedia: 'Wikipedia', netflix: 'Netflix', googlemaps: 'Google Maps', ebay: 'eBay', amazon: 'Amazon', amazom: 'Amazon', ecosia: 'Ecosia' };
  let requestId = 0, pendingId = 0, debounce, composing = false, typed = '', results = [], selected = -1;
  let shortcuts = [], editingId = null, menuId = null, menuAnchor = null, dragId = null;
  let toastTimer, undoAction = null, engine = 'google';
  const icons = new Map();

  function send(type, data = {}) {
    const id = ++requestId;
    window.chrome?.webview?.postMessage({ channel: 'quartz-newtab', type, id, ...data });
    return id;
  }
  function icon(name) {
    const element = document.createElement('span');
    element.className = `icon ${name}-icon`;
    element.setAttribute('aria-hidden', 'true');
    return element;
  }
  function announce(text) { $('search-status').textContent = text; }
  function closeSuggestions() {
    clearTimeout(debounce);
    pendingId = ++requestId;
    list.hidden = true;
    box.classList.remove('open');
    input.setAttribute('aria-expanded', 'false');
    input.removeAttribute('aria-activedescendant');
    selected = -1;
  }
  function highlight(text, query) {
    const span = document.createElement('span');
    const start = query ? text.toLocaleLowerCase().indexOf(query.toLocaleLowerCase()) : -1;
    if (start < 0) { span.textContent = text; return span; }
    span.append(document.createTextNode(text.slice(0, start)));
    const strong = document.createElement('strong');
    strong.textContent = text.slice(start, start + query.length);
    span.append(strong, document.createTextNode(text.slice(start + query.length)));
    return span;
  }
  function renderSuggestions(history = [], searches = []) {
    if (document.activeElement !== input || composing) return;
    const previous = results[selected];
    const query = typed.trim();
    results = query ? [{ kind: 'input', text: query }] : [];
    results.push(...history);
    const seen = new Set([query.toLocaleLowerCase()]);
    for (const text of searches) {
      if (typeof text !== 'string' || seen.has(text.toLocaleLowerCase())) continue;
      seen.add(text.toLocaleLowerCase());
      results.push({ kind: 'search', text });
    }
    results = results.slice(0, 8);
    selected = previous ? results.findIndex(r => r.kind === previous.kind && r.text === previous.text && r.url === previous.url) : -1;
    if (selected < 0 && query) selected = 0;
    list.replaceChildren();
    results.forEach((result, index) => {
      const row = document.createElement('div');
      row.className = 'suggestion'; row.id = `suggestion-${index}`;
      row.setAttribute('role', 'option');
      row.setAttribute('aria-selected', String(index === selected));
      row.append(icon(result.kind === 'history' ? 'history' : 'search'));
      const text = document.createElement('div'); text.className = 'suggestion-text';
      text.append(highlight(result.text, query));
      if (result.url) {
        const url = document.createElement('span'); url.className = 'suggestion-url';
        url.textContent = '– ' + result.url.replace(/^https?:\/\//, '').replace(/\/$/, '');
        text.append(url);
      }
      row.append(text);
      row.addEventListener('pointerdown', e => e.preventDefault());
      row.addEventListener('click', () => navigate(result));
      list.append(row);
    });
    list.hidden = results.length === 0;
    box.classList.toggle('open', results.length > 0);
    input.setAttribute('aria-expanded', String(results.length > 0));
    if (selected >= 0) input.setAttribute('aria-activedescendant', `suggestion-${selected}`);
    else input.removeAttribute('aria-activedescendant');
    announce(`${results.length} suggestion${results.length === 1 ? '' : 's'} available.`);
  }
  function requestSuggestions(immediate = false) {
    clearTimeout(debounce);
    typed = input.value;
    pendingId = ++requestId;
    selected = -1; results = [];
    $('clear-search').hidden = !typed;
    renderSuggestions();
    const request = () => { pendingId = send('suggest', { query: typed.trim() }); };
    if (immediate || !typed.trim()) request();
    else debounce = setTimeout(request, 150);
  }
  function navigate(result) {
    if (composing) return;
    const text = result?.url || result?.text || input.value;
    if (!text.trim()) return;
    closeSuggestions();
    send('navigate', { text, forceSearch: result?.kind === 'search' });
  }
  input.addEventListener('focus', () => requestSuggestions(true));
  input.addEventListener('click', () => { if (list.hidden) requestSuggestions(true); });
  input.addEventListener('input', () => { if (!composing) requestSuggestions(); });
  input.addEventListener('compositionstart', () => { composing = true; closeSuggestions(); });
  input.addEventListener('compositionend', () => { composing = false; requestSuggestions(); });
  input.addEventListener('keydown', e => {
    if (e.isComposing || composing || e.keyCode === 229) return;
    if (e.key === 'Escape') { e.preventDefault(); input.value = typed; closeSuggestions(); return; }
    if (e.key === 'Tab') { closeSuggestions(); return; }
    if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
    e.preventDefault();
    if (list.hidden) { requestSuggestions(true); return; }
    selected += e.key === 'ArrowDown' ? 1 : -1;
    if (selected >= results.length) selected = -1;
    if (selected < -1) selected = results.length - 1;
    for (let i = 0; i < list.children.length; i++) list.children[i].setAttribute('aria-selected', String(i === selected));
    const result = results[selected];
    input.value = result ? (result.url || result.text) : typed;
    if (result) {
      input.setAttribute('aria-activedescendant', `suggestion-${selected}`);
      list.children[selected].scrollIntoView({ block: 'nearest' });
    } else input.removeAttribute('aria-activedescendant');
  });
  $('search-form').addEventListener('submit', e => { e.preventDefault(); navigate(list.hidden ? null : results[selected]); });
  $('clear-search').addEventListener('pointerdown', e => e.preventDefault());
  $('clear-search').addEventListener('click', () => { input.value = ''; input.focus(); requestSuggestions(true); });
  box.addEventListener('focusout', e => { if (!box.contains(e.relatedTarget)) closeSuggestions(); });

  // Chromium's normalizeUrl accepts only HTTP(S) and supplies a missing scheme.
  function normalizeUrl(value) {
    try {
      value = value.trim();
      if (!value || /\s/.test(value) || (/^[a-z][\w+.-]*:/i.test(value) && !value.includes('://') && !/^[^/:]+:\d+(\/|$)/.test(value))) return null;
      const url = new URL(value.includes('://') ? value : `https://${value}`);
      return ['http:', 'https:'].includes(url.protocol) && !url.username && !url.password ? url.href : null;
    } catch { return null; }
  }
  function readShortcuts() {
    const data = JSON.parse(localStorage.getItem(storageKey) || '[]');
    if (!Array.isArray(data) || data.length > 10 || data.some(s => !s || typeof s.id !== 'string' || typeof s.name !== 'string' || s.name.length > 128 || typeof s.url !== 'string' || !normalizeUrl(s.url)) || new Set(data.map(s => s.id)).size !== data.length) throw new Error('Invalid shortcut data');
    return data;
  }
  function toast(message, undo = null) {
    clearTimeout(toastTimer); undoAction = undo;
    $('toast-message').textContent = message; $('undo').hidden = !undo; $('toast').hidden = false;
    toastTimer = setTimeout(() => { if (!$('toast').contains(document.activeElement)) $('toast').hidden = true; }, 10000);
  }
  function mutateShortcuts(change, message, focusId) {
    try {
      const before = readShortcuts();
      const after = change(before.map(s => ({ ...s })));
      if (!after) return false;
      const serialized = JSON.stringify(after);
      localStorage.setItem(storageKey, serialized);
      shortcuts = after; renderShortcuts(focusId);
      toast(message, () => {
        // Don't overwrite an edit made in another tab after this action.
        if (localStorage.getItem(storageKey) !== serialized) { toast('Shortcuts changed in another tab.'); return; }
        localStorage.setItem(storageKey, JSON.stringify(before));
        shortcuts = before; renderShortcuts(focusId); toast('Shortcut change undone');
      });
      return true;
    } catch { toast('Couldn’t save shortcuts. Please try again.'); return false; }
  }
  function reloadShortcuts() {
    try { shortcuts = readShortcuts(); renderShortcuts(); }
    catch { toast('Couldn’t read saved shortcuts.'); }
  }
  function renderShortcuts(focusId) {
    const container = $('shortcuts'); container.replaceChildren();
    for (const shortcut of shortcuts) {
      const tile = document.createElement('div'); tile.className = 'tile'; tile.dataset.id = shortcut.id;
      const link = document.createElement('a'); link.href = shortcut.url; link.title = `${shortcut.name}\n${shortcut.url}`;
      link.setAttribute('aria-label', shortcut.name); link.draggable = true;
      link.addEventListener('keydown', e => {
        if (!['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(e.key)) return;
        e.preventDefault();
        const index = shortcuts.findIndex(s => s.id === shortcut.id);
        const direction = ['ArrowLeft', 'ArrowUp'].includes(e.key) ? -1 : 1;
        const next = Math.max(0, Math.min(shortcuts.length - 1, index + direction));
        if (e.altKey) moveShortcut(shortcut.id, shortcuts[next].id);
        else container.children[next]?.querySelector('a')?.focus();
      });
      link.addEventListener('dragstart', e => {
        closeMenu(false); dragId = shortcut.id; tile.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', shortcut.id);
      });
      link.addEventListener('dragend', endDrag);
      tile.addEventListener('dragover', e => { if (dragId && dragId !== shortcut.id) { e.preventDefault(); e.dataTransfer.dropEffect = 'move'; tile.classList.add('drop-target'); } });
      tile.addEventListener('dragleave', e => { if (!tile.contains(e.relatedTarget)) tile.classList.remove('drop-target'); });
      tile.addEventListener('drop', e => { if (!dragId) return; e.preventDefault(); const from = dragId; endDrag(); moveShortcut(from, shortcut.id); });
      const circle = document.createElement('span'); circle.className = 'tile-icon';
      if (icons.has(shortcut.url)) { const img = new Image(24, 24); img.src = icons.get(shortcut.url); img.alt = ''; circle.append(img); }
      else circle.append(icon('globe'));
      const label = document.createElement('span'); label.className = 'tile-title';
      const name = document.createElement('span'); name.textContent = shortcut.name; label.append(name);
      const more = document.createElement('button'); more.className = 'icon-button more'; more.append(icon('more'));
      more.setAttribute('aria-label', `More options for ${shortcut.name}`); more.setAttribute('aria-haspopup', 'menu');
      more.addEventListener('click', () => openMenu(shortcut.id, more));
      tile.append(link, circle, label, more); container.append(tile);
    }
    if (shortcuts.length < 10) {
      const add = document.createElement('button'); add.id = 'add-shortcut'; add.className = 'tile';
      const circle = document.createElement('span'); circle.className = 'tile-icon'; circle.append(icon('add'));
      const title = document.createElement('span'); title.className = 'tile-title';
      const text = document.createElement('span'); text.textContent = 'Add shortcut'; title.append(text);
      add.append(circle, title); add.addEventListener('click', () => openDialog()); container.append(add);
    }
    if (focusId) {
      const target = Array.from(container.children).find(t => t.dataset.id === focusId);
      (target?.querySelector('a') || $('add-shortcut') || container.querySelector('a'))?.focus();
    }
    const missing = shortcuts.filter(s => !icons.has(s.url)).map(s => s.url);
    if (missing.length) send('icons', { urls: missing });
  }
  function endDrag() { dragId = null; document.querySelectorAll('.dragging, .drop-target').forEach(t => t.classList.remove('dragging', 'drop-target')); }
  function moveShortcut(from, target) {
    if (from === target) return;
    mutateShortcuts(items => {
      const a = items.findIndex(s => s.id === from), b = items.findIndex(s => s.id === target);
      if (a < 0 || b < 0) return null;
      items.splice(b, 0, items.splice(a, 1)[0]); return items;
    }, 'Shortcut moved', from);
  }
  function openMenu(id, anchor) {
    closeSuggestions(); menuId = id; menuAnchor = anchor; menu.hidden = false;
    const rect = anchor.getBoundingClientRect();
    menu.style.left = `${Math.max(8, Math.min(innerWidth - menu.offsetWidth - 8, rect.right - menu.offsetWidth))}px`;
    menu.style.top = `${Math.min(innerHeight - menu.offsetHeight - 8, rect.bottom)}px`;
    anchor.setAttribute('aria-expanded', 'true'); $('edit-shortcut').focus();
  }
  function closeMenu(restore = true) {
    menu.hidden = true; menuAnchor?.setAttribute('aria-expanded', 'false');
    if (restore && menuAnchor?.isConnected) menuAnchor.focus();
  }
  menu.addEventListener('keydown', e => {
    if (e.key === 'Escape') { e.preventDefault(); closeMenu(); }
    if (e.key === 'Tab') closeMenu(false);
    if (e.key === 'ArrowDown' || e.key === 'ArrowUp') { e.preventDefault(); (document.activeElement === $('edit-shortcut') ? $('remove-shortcut') : $('edit-shortcut')).focus(); }
  });
  function openDialog(id = null) {
    closeSuggestions(); closeMenu(false); editingId = id;
    const shortcut = shortcuts.find(s => s.id === id);
    $('dialog-title').textContent = id ? 'Edit shortcut' : 'Add shortcut';
    $('shortcut-name').value = shortcut?.name || ''; $('shortcut-url').value = shortcut?.url || '';
    $('url-error').textContent = ''; $('shortcut-url').removeAttribute('aria-invalid');
    $('save-shortcut').disabled = !shortcut;
    dialog.showModal(); $('shortcut-name').focus();
  }
  function validate(showError = false) {
    const value = $('shortcut-url').value.trim(), url = normalizeUrl(value);
    const duplicate = !!url && shortcuts.some(s => s.id !== editingId && normalizeUrl(s.url) === url);
    $('save-shortcut').disabled = !url || duplicate;
    $('url-error').textContent = (showError || duplicate) && value ? duplicate ? 'Shortcut already exists' : !url ? 'Enter a valid URL' : '' : '';
    $('shortcut-url').setAttribute('aria-invalid', String(!!$('url-error').textContent));
    return url && !duplicate ? url : null;
  }
  $('shortcut-url').addEventListener('input', () => validate());
  $('shortcut-url').addEventListener('blur', () => validate(true));
  $('cancel-shortcut').addEventListener('click', () => dialog.close());
  dialog.addEventListener('close', () => {
    const tile = Array.from($('shortcuts').children).find(t => t.dataset.id === editingId);
    (tile?.querySelector('a') || $('add-shortcut') || $('shortcuts').querySelector('a'))?.focus();
  });
  $('shortcut-form').addEventListener('submit', e => {
    e.preventDefault(); const url = validate(true); if (!url) return;
    const id = editingId || crypto.randomUUID();
    const name = $('shortcut-name').value.trim() || new URL(url).hostname;
    const success = mutateShortcuts(items => {
      if (items.some(s => s.id !== id && normalizeUrl(s.url) === url)) { toast('Shortcut already exists'); return null; }
      const index = items.findIndex(s => s.id === id);
      if (editingId && index < 0) { toast('This shortcut was removed in another tab.'); return null; }
      if (index >= 0) items[index] = { id, name, url };
      else if (items.length < 10) items.push({ id, name, url });
      else { toast('You can add up to 10 shortcuts.'); return null; }
      return items;
    }, editingId ? 'Shortcut updated' : 'Shortcut added');
    if (success) { editingId = id; dialog.close(); }
  });
  $('edit-shortcut').addEventListener('click', () => openDialog(menuId));
  $('remove-shortcut').addEventListener('click', () => {
    const id = menuId; closeMenu(false);
    mutateShortcuts(items => items.filter(s => s.id !== id), 'Shortcut removed', id);
  });
  $('undo').addEventListener('click', () => { try { undoAction?.(); } catch { toast('Couldn’t undo the change.'); } });
  document.addEventListener('pointerdown', e => {
    if (!box.contains(e.target)) closeSuggestions();
    if (!menu.hidden && !menu.contains(e.target) && !menuAnchor?.contains(e.target)) closeMenu(false);
  });
  window.addEventListener('resize', () => closeMenu(false));
  window.addEventListener('storage', e => { if (e.key === storageKey || e.key === null) { closeMenu(false); reloadShortcuts(); if (dialog.open) validate(); } });
  document.addEventListener('visibilitychange', () => {
    if (!document.hidden) { send('state'); reloadShortcuts(); if (document.activeElement === input) requestSuggestions(true); }
    else closeSuggestions();
  });
  window.chrome?.webview?.addEventListener('message', ({ data }) => {
    if (data.type === 'state') {
      if (['light', 'dark', 'black', 'aqua', 'xmas'].includes(data.theme)) document.documentElement.dataset.theme = data.theme;
      const changed = engine !== data.engine; engine = data.engine || 'google';
      input.placeholder = `Search ${engineNames[engine] || 'Google'} or type a URL`;
      input.setAttribute('aria-label', input.placeholder);
      if (changed && document.activeElement === input) requestSuggestions(true);
    } else if (data.type === 'suggestions' && data.id === pendingId && data.query === typed.trim()) {
      renderSuggestions(data.history, data.searches);
    } else if (data.type === 'icons') {
      for (const [url, image] of Object.entries(data.icons || {})) {
        if (typeof image !== 'string' || !image.startsWith('data:image/x-icon;base64,')) continue;
        icons.set(url, image);
        // Update only the image so asynchronous icon replies never destroy focus/drag.
        for (const tile of $('shortcuts').children) {
          if (tile.querySelector('a')?.href !== url) continue;
          const img = new Image(24, 24); img.src = image; img.alt = '';
          tile.querySelector('.tile-icon').replaceChildren(img);
        }
      }
    }
  });
  reloadShortcuts(); send('state');
})();
