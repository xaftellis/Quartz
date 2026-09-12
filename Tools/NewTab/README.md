# Quartz new tab

The page bundles Chromium's actual Lit search input, suggestion dropdown and
suggestion rows, most-visited shortcuts, menus, dialogs, buttons, focus behavior,
ripples and transitions. The upstream CSS, HTML templates, TypeScript and SVG
files are retained under `third_party/chromium/webui` at revision
`d228ff553e2800b1e81c1f29ddd5cbc5483e9030` (Chromium main, 2026-09-11).
`manifest.json` records the revision and SHA-256 of every consumed source.

Upstream: https://github.com/chromium/chromium/tree/d228ff553e2800b1e81c1f29ddd5cbc5483e9030

`build.mjs` performs Chromium's Windows resource preprocessing, resolves generated
CSS/HTML import wrappers, and compiles the components with Lit. Original SVGs
are copied into `Quartz/assets/quartz.com/newtab/chromium`; none are redrawn.
The original source files are not edited. The page-level stylesheet changes its
`:host` selector to `:root` because Quartz mounts it in the document.

`host.ts` supplies the browser APIs normally implemented by Chromium's C++/Mojo
backend: shortcut storage and undo, favicon lookup, and navigation.
`searchbox.ts` hosts Chromium's input and dropdown with the original
`ntp_searchbox.css`, omitting optional Lens, voice and compose controls. It adapts
Quartz's history and search responses into Chromium autocomplete matches and
keeps the previous results visible while the next response is pending.
`page.ts` applies Quartz themes and the selected search engine. The supplied
Quartz logo replaces Google's logo; the page contains no other NTP modules.

Google suggestions come from the live Google autocomplete endpoint through
`NewTabPageController` and `SearchSuggestions`. Focusing an empty input requests
only recent Quartz history. Shortcuts preserve the existing
`quartz.newtab.shortcuts.v1` profile-local storage key and synchronize across tabs.

## Build

From the repository root, install the pinned dependencies once:

```powershell
pnpm --dir Tools/NewTab install --frozen-lockfile --ignore-scripts
node Tools/NewTab/build.mjs
```

The normal component build uses the vendored sources offline. `--sync` fetches
missing files from the pinned revision. Update the revision deliberately before
refreshing the vendor tree. Generated assets are committed, so building Quartz
with MSBuild does not require Node or network access.

Chromium's BSD license and Lit's BSD license ship beside the generated bundle.
