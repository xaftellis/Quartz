# Browser keyboard shortcuts

`Quartz/ShortcutManager.cs` defines the supported keys. Each `AppContainer` owns
one router. WinForms messages and each tab's WebView2 `KeyDown`/`KeyUp` events
feed that router; no JavaScript or global keyboard hook is used.

Keyboard commands target the selected tab in the foreground browser window.
The tab overlay and that browser's menus are included. Separate forms, their
controls, owned/native dialogs, other browser windows and background windows
are excluded. Menu clicks retain their explicit target (including a tab that
was right-clicked without selecting it).

## Definitions

| Action | Keys |
| --- | --- |
| New tab / window | Ctrl+T / Ctrl+N |
| Close tab | Ctrl+W, Ctrl+F4 |
| Close window | Alt+F4, Ctrl+Shift+W |
| Reload | Ctrl+R, F5 |
| Reload ignoring cache | Ctrl+Shift+R, Ctrl+F5, Shift+F5 |
| Find / print / open file | Ctrl+F / Ctrl+P / Ctrl+O |
| History / downloads | Ctrl+H / Ctrl+J |
| Clear browsing data | Ctrl+Shift+Delete |
| Profiles | Ctrl+Shift+M |
| Add favourite / favourite all tabs | Ctrl+D / Ctrl+Shift+D |
| Toggle favourites bar | Ctrl+Shift+B |
| Developer tools | F12, Ctrl+Shift+I |
| Task manager | Shift+Esc |
| Fullscreen | F11 |
| Focus and select the address | Ctrl+L, Alt+D, F6 |
| Next tab | Ctrl+Tab, Ctrl+PageDown |
| Previous tab | Ctrl+Shift+Tab, Ctrl+PageUp |
| Select tab 1–8 / last tab | Ctrl+1–8 / Ctrl+9 (also numpad) |
| Back / forward | Alt+Left / Alt+Right |
| Zoom in | Ctrl+=, Ctrl+Plus, Ctrl+numpad Plus |
| Zoom out / reset zoom | Ctrl+Minus / Ctrl+0 (also numpad) |

Plain typing, text editing, Tab/Shift+Tab navigation, Escape and unregistered
keys pass through to the focused control or page. Tab cycling and zoom repeat
while held; other registered commands execute once per press. Address focus
exits fullscreen because the address bar is otherwise hidden.

The existing WebView2 accelerator setting still disables keyboard Find, print,
reload, developer tools, navigation and zoom. The zoom-control setting also
applies to keyboard zoom. Menu actions and Quartz tab/window commands remain
available. Developer tools and other command availability are checked again
when executing, independently of whether a menu has been opened.

## Implementation

- `ShortcutManager` owns key definitions, menu shortcut text, window/menu scope,
  held-key tracking and deferred keyboard dispatch. It is removed when its
  window is disposed. A deferred key is discarded if activation or the selected
  tab has changed before execution.
- `Browser.Shortcuts.cs` binds menus and WebView2 events and implements shared
  command availability/actions. WebView2 callbacks only recognise and handle
  the key; browser APIs and dialogs execute later on the UI thread.
- Menu `ShortcutKeys` are cleared and `ShortcutKeyDisplayString` comes from the
  registry. This avoids a second WinForms execution path and permits Shift+Esc,
  which .NET Framework rejects as a `ToolStripMenuItem.ShortcutKeys` value.
- Find opens the native WebView2 Find bar with `CoreWebView2.Find.StartAsync`
  and an empty search term. It does not replay Ctrl+F. This API requires a
  compatible runtime (introduced in stable SDK/runtime 139). Microsoft notes
  limitations for programmatic next/previous PDF matches; Quartz leaves match
  navigation to the native Find UI. See [Microsoft's Find API documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2find.startasync?view=webview2-dotnet-1.0.4191.47)
  and [PDF limitations](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/overview-features-apis#find).
- `AppContainer.FullScreen.cs` retains restore bounds/state for the window,
  across tab switches, new tabs and closure of the original tab. Browser
  fullscreen notifications and the fullscreen menu button use this same state.
- EasyTabs rendering, activation and menu animation code is unchanged.

To add a shortcut, add a `BrowserCommand` and binding, implement its availability
and action in `Browser.Shortcuts.cs`, and bind any menu with `BindShortcutMenu`
or `SetMenuShortcut`. Avoid introducing independent key handlers or replaying
the shortcut with `SendKeys`.

## Verification

Run `verification/shortcuts/Run.ps1`. The harness compiles the production router
and fullscreen state code against small host doubles. It uses hidden WinForms
handles to check scope and menu ownership, and checks key definitions, deferred
work, repeat suppression, explicit command targets and fullscreen restoration.
It never opens a WebView2 session or reads/writes Quartz profiles. Passing it
does not establish live WebView2 keyboard delivery or visual behaviour.

Manual checks in Quartz:

1. From a page input, the address bar, a toolbar button, the tab strip and an open
   submenu: try Ctrl+T, Ctrl+W, Ctrl+L, Ctrl+Tab and F11. Each press should act once.
2. With two windows, confirm shortcuts affect only the foreground window. Move
   a tab between windows and repeat. Right-click another tab, dismiss the menu,
   then check that Ctrl+W closes the selected tab; menu Close still closes the
   right-clicked tab.
3. In Settings, Add Favourite, Profiles, History and the native Open File dialog,
   verify typing and Alt+F4 stay local and Ctrl+T/F11 do not affect the browser.
   Cancel Open File and confirm the page does not navigate.
4. Hold F11/Ctrl+W and confirm no repeated toggles/closes. Hold tab cycling or
   zoom and confirm repeats. Switch away and release a key, then return and
   confirm the next press works.
5. Enter fullscreen, switch/create/close tabs, then exit. Confirm the original
   window bounds/state and the selected tab's toolbar are restored.
6. Check Find on HTML, local text and a PDF, Print, Downloads and Task Manager.
   Test before opening their menus. Check disabled accelerator/zoom/devtools
   settings and normal copy/paste/undo in page inputs and the address bar.
