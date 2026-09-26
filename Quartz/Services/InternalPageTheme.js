function (update) {
    // The C# caller accepts only bundled pages. Recheck in the document because
    // a navigation may have completed while ExecuteScriptAsync was queued.
    if (location.href !== update.source || location.origin !== 'https://quartz.com') return false;
    if (window.__quartzAppliedTheme === update.theme) return true;
    const template = new DOMParser().parseFromString(update.html, 'text/html');
    if (!document.body || !template.body) return false;

    // Use the authored theme assets without replacing the document or its form
    // controls. Search text, selection, custom search engines and handlers survive.
    let base = document.querySelector('base[data-quartz-theme]');
    if (!base) {
        base = document.createElement('base');
        base.dataset.quartzTheme = '';
        document.head.prepend(base);
    }
    base.href = update.url;
    for (const link of document.querySelectorAll('link[href]')) {
        const url = new URL(link.getAttribute('href'), update.url);
        if (url.origin === location.origin && /^\/(light|dark)\/assets\//.test(url.pathname)) {
            url.pathname = url.pathname.replace(/^\/[^/]+\//, '/' + update.theme + '/');
            link.href = url.href;
        }
    }
    let styles = document.getElementById('quartz-live-theme');
    if (!styles) {
        styles = document.createElement('style');
        styles.id = 'quartz-live-theme';
        document.head.append(styles);
    }
    styles.textContent = Array.from(template.head.querySelectorAll('style'), s => s.textContent).join('\n');

    const current = [document.body, ...document.body.querySelectorAll('*:not(script)')];
    const themed = [template.body, ...template.body.querySelectorAll('*:not(script)')];
    // All bundled variants share the same body structure. Skip structural
    // mismatches instead of assigning colours to an unrelated dynamic element.
    if (current.length === themed.length && current.every((node, i) => node.tagName === themed[i].tagName)) {
        current.forEach((node, i) => {
            for (const property of ['color', 'background-color', 'border-color', 'background-image']) {
                const value = themed[i].style.getPropertyValue(property);
                if (value) node.style.setProperty(property, value);
                else node.style.removeProperty(property);
            }
            if (node.tagName === 'IMG' && themed[i].hasAttribute('src'))
                node.src = new URL(themed[i].getAttribute('src'), update.url).href;
        });
    }
    window.__quartzAppliedTheme = update.theme;
    return true;
}
