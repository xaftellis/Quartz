# Chromium new-tab source reference

Actual, unmodified upstream HTML templates, CSS and TypeScript downloaded from
Chromium main on 2026-09-11 at revision
`3cd17c9338482b7c18fa96453ae20971b373a342`.

Source: https://github.com/chromium/chromium/tree/3cd17c9338482b7c18fa96453ae20971b373a342

Original paths:

- `chrome/browser/resources/new_tab_page/`: `app.css`, `ntp_searchbox.css`,
  `ntp_searchbox.html.ts`, `ntp_searchbox.ts`.
- `ui/webui/resources/cr_components/searchbox/`: `searchbox_input.css`,
  `searchbox_input.html.ts`, `searchbox_input.ts`, `searchbox_dropdown.css`,
  `searchbox_dropdown.html.ts`, `searchbox_match.css`.
- `ui/webui/resources/cr_components/most_visited/`: `most_visited.css`,
  `most_visited.html.ts`, `most_visited.ts`.
- Root `LICENSE`.

Chromium generates JavaScript and HTML from these TypeScript/Lit templates. They
depend on Chrome WebUI elements, generated Mojo bindings, browser processes and
feature flags; they are reference sources, not directly runnable WebView2 assets.

Quartz's runnable adaptation is `Quartz/assets/quartz.com/newtab/`. It follows the
classic single-line searchbox path (48px height, 24px radius, 337/449/561px widths,
16px text, shared suggestion surface) and most-visited tile geometry (112px tiles,
48px icon circles, 24px favicons), menu/dialog workflow, ten custom links,
HTTP(S) URL normalization, editing/removal, reordering and undo. The experimental
composebox, Google account UI, Lens, voice search and promotional modules are not
part of this minimal Quartz page. These files are not a claim of complete Chrome
feature or pixel parity across Chrome's experiments.

`NewTabPageController` adapts host messages to Quartz's profile history, cached
favicons, settings and navigation. Google text completions come from its live
Firefox-format autocomplete endpoint, not a fabricated list. An empty query
reads local history only. Unsupported suggestion providers retain history and
normal search submission. Shortcuts use WebView2's profile-scoped localStorage,
including its in-private lifetime, and storage events synchronize open tabs.

The upstream BSD license is retained here and shipped as
`assets/quartz.com/newtab/chromium-LICENSE` with the adapted page.
