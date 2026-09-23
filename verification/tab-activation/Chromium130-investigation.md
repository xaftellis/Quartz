# Quartz tab creation and activation

Reference: the local Chromium checkout at `C:\src\chromium130\src`, tag **130.0.6723.191**, commit `b872957737636278e1da4818e40a40c66b72a2b0`. Investigation and verification: 2026-09-23.

## Findings

Quartz already used WebView2's `NewWindowRequested` event. The original handler created a new Browser using `e.Uri`, inserted its tab, and unconditionally assigned `ParentTabs.SelectedTab = newtab`. Several favourites/history handlers repeated the same sequence. `Program.OpenNewWindowWithTabsFast` selected every tab in its loop.

EasyTabs itself does not select every tab on insertion. Its selection setter changes each tab's `Active` state; that state controls the child form's visibility. Existing foreground-only paths, including the plus button, duplicate and new-tab-left/right commands, already select appropriately. Those paths and EasyTabs rendering/selection code were left intact.

There is a second issue: Browser startup was tied to the WinForms `Load` event. Merely removing selection from callers would leave newly created background tabs uninitialized until first shown.

**The WebView2 limitation is missing disposition information, not a missing event.** The installed `Microsoft.Web.WebView2` 1.0.4191.47 assembly was inspected with reflection. `CoreWebView2NewWindowRequestedEventArgs` exposes these public properties: `Handled`, `IsUserInitiated`, `NewWindow`, `Uri`, `WindowFeatures`, `Name`, `OriginalSourceFrameInfo`. There is no foreground/background disposition property. The same API is documented in the [installed SDK XML](../../packages/Microsoft.Web.WebView2.1.0.4191.47/lib/net462/Microsoft.Web.WebView2.Core.xml).

## Chromium's decision chain

1. **Browser UI input:** [window_open_disposition_utils.cc:12](C:/src/chromium130/src/ui/base/window_open_disposition_utils.cc:12), `DispositionFromClick`, maps middle/Ctrl to background and middle/Ctrl plus Shift to foreground on Windows. Shift alone requests a window; Alt alone requests a download. Bookmarks use this in [bookmark_bar_view.cc:1356](C:/src/chromium130/src/chrome/browser/ui/views/bookmarks/bookmark_bar_view.cc:1356).
2. **Renderer links:** [html_anchor_element.cc:667](C:/src/chromium130/src/third_party/blink/renderer/core/html/html_anchor_element.cc:667) gets a navigation policy from the event. [navigation_policy.cc](C:/src/chromium130/src/third_party/blink/renderer/core/loader/navigation_policy.cc:61) applies the same modifiers, with additional checks against the current real input event to prevent synthetic background opens.
3. **Renderer-created windows:** [create_window.cc:316](C:/src/chromium130/src/third_party/blink/renderer/core/page/create_window.cc:316) calls `NavigationPolicyForCreateWindow`. In [navigation_policy.cc:174](C:/src/chromium130/src/third_party/blink/renderer/core/loader/navigation_policy.cc:174), an ordinary allowed script open defaults to foreground; popup features request a popup. A real user policy can override that default. Lack of a user gesture is not itself an instruction to open a background tab. Popup permission/blocking is separate from activation.
4. **Transport:** [render_frame_impl.cc:1266](C:/src/chromium130/src/content/renderer/render_frame_impl.cc:1266) converts Blink policy to `WindowOpenDisposition`. It is copied into navigation/new-window parameters at lines 5897 and 6785. [web_contents_impl.cc:5066](C:/src/chromium130/src/content/browser/web_contents/web_contents_impl.cc:5066) forwards the disposition to `AddNewContents`. [browser.cc:1948](C:/src/chromium130/src/chrome/browser/ui/browser.cc:1948) forwards it to `chrome::AddWebContents`; [browser_tabstrip.cc:88](C:/src/chromium130/src/chrome/browser/ui/browser_tabstrip.cc:88) assigns it to `NavigateParams`. Ordinary OpenURL requests similarly flow through `Browser::OpenURLFromTab` and [browser_navigator_params.cc:73](C:/src/chromium130/src/chrome/browser/ui/browser_navigator_params.cc:73).
5. **Normalization:** [browser_navigator.cc:354](C:/src/chromium130/src/chrome/browser/ui/browser_navigator.cc:354), `NormalizeDisposition`, converts a background request to foreground when the destination strip is empty. Background disposition explicitly clears `ADD_ACTIVE`; foreground sets it. This matters because `ADD_ACTIVE` is otherwise a default. [browser_navigator.cc:930](C:/src/chromium130/src/chrome/browser/ui/browser_navigator.cc:930) passes the add types to the tab model.
6. **Model and strip:** [tab_strip_model.cc:2002](C:/src/chromium130/src/chrome/browser/ui/tabs/tab_strip_model.cc:2002) computes `active = ADD_ACTIVE || empty()`. [tab_strip_model.cc:2584](C:/src/chromium130/src/chrome/browser/ui/tabs/tab_strip_model.cc:2584) adjusts selection indices on insertion and selects the inserted tab only if active. [browser_tab_strip_controller.cc:663](C:/src/chromium130/src/chrome/browser/ui/views/tabs/browser_tab_strip_controller.cc:663) adds the visual tab and applies model selection only when selection changes. The strip does not decide every inserted tab should be active.

## Path comparison and adaptation

| Path | Chromium 130 | Quartz result |
|---|---|---|
| Plus, Ctrl+T, New tab | Foreground; normal new tab appends | Plus retained; menu/shortcut now use explicit foreground disposition and append |
| Tab menu new left/right | Explicit foreground (`tab_strip_model.cc:1405` for right) | Existing foreground behavior retained |
| Ordinary link | Current tab unless target or modifiers require another context | WebView2 retains ordinary navigation |
| Middle-click / Ctrl+click | Background | Native gesture capture passes background disposition to tab insertion |
| Shift+middle / Ctrl+Shift+click | Foreground | Same native input mapping with Shift |
| Shift+click | New window | New Quartz window, retaining the source profile |
| Plain target blank | Foreground | Defaults foreground and provides native `NewWindow` target |
| Allowed `window.open` | Foreground or popup by default, with real input overrides | Native `NewWindow` handoff; popup features create a normal Quartz window; gesture mapping subject to limits below |
| Link context-menu new tab | Background, [render_view_context_menu.cc:3200](C:/src/chromium130/src/chrome/browser/renderer_context_menu/render_view_context_menu.cc:3200) | Explicit native custom command; added when WebView2 omits it |
| Link context-menu new window | New window | Native custom command creates a Quartz window |
| Image/audio/video context-menu new tab | Background, [render_view_context_menu.cc:3344](C:/src/chromium130/src/chrome/browser/renderer_context_menu/render_view_context_menu.cc:3344) | Recognized native menu commands routed to background disposition; not live-tested |
| Favourites and history-menu clicks | Event modifiers determine disposition | Shared policy for left/middle actions; explicit open-in-new-tab commands use background |
| Favourites open all, same window | Background, [bookmark_context_menu_controller.cc:244](C:/src/chromium130/src/chrome/browser/ui/bookmarks/bookmark_context_menu_controller.cc:244) | Every new tab stays background |
| History grid | Link open and selection actions are separate | Middle opens background (Shift foreground); explicit new-tab command background. Existing Ctrl/Shift row-selection interaction retained |
| Duplicate | Foreground and pinned state preserved, [browser_commands.cc:1214](C:/src/chromium130/src/chrome/browser/ui/browser_commands.cc:1214) | Existing activation and pinned behavior retained; Quartz still duplicates URL rather than Chromium's navigation-history clone |
| Address Enter variants | Alt+Enter foreground, Alt+Shift+Enter background, Shift+Enter window, [omnibox_view_views.cc:1614](C:/src/chromium130/src/chrome/browser/ui/views/omnibox/omnibox_view_views.cc:1614) | Added these opening dispositions while retaining Quartz's URL/search parsing |
| Multi-URL new window/startup | First active, remaining background, [startup_browser_creator_impl.cc:297](C:/src/chromium130/src/chrome/browser/ui/startup/startup_browser_creator_impl.cc:297) | Batch opener selects only the first; initializes all tabs |
| Session restore / reopen closed | Explicit selected state: [session_restore.cc:876](C:/src/chromium130/src/chrome/browser/sessions/session_restore.cc:876), [browser_tabrestore.cc:146](C:/src/chromium130/src/chrome/browser/ui/browser_tabrestore.cc:146) | No session-tab restoration or reopen-closed-tab implementation found. Window bounds restoration is unrelated |
| Extension tab creation | Extension `active` option becomes disposition/add types: [tabs_api.cc:1342](C:/src/chromium130/src/chrome/browser/extensions/api/tabs/tabs_api.cc:1342), [extension_tab_util.cc:315](C:/src/chromium130/src/chrome/browser/extensions/extension_tab_util.cc:315) | No Quartz extension tab-management API found; not implemented by this change |
| Tab tear-out / move | Transfers existing contents with an explicit selection decision | Existing EasyTabs transfer behavior retained; not a new navigation |

## Implementation

- `TabOpenDisposition.cs` defines the native opening policy. `Browser.TabOpening.cs` inserts and selects using that disposition, with first-tab normalization.
- `TabOpenGesture.cs` observes native mouse-up events over the visible selected WebView HWND and its child HWNDs, plus WebView Enter key events. A low-level mouse hook is needed across WebView2's child process boundary. It does not inject input or page scripts. Hidden/disposed controls remove their hook; callbacks always pass input onward.
- The event handler still uses `CoreWebView2.NewWindowRequested`. It takes a deferral, initializes a fresh target with the same environment/profile and no prior navigation, and assigns `e.NewWindow`. This follows the [documented handoff](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2newwindowrequestedeventargs.newwindow). Opening only `e.Uri` would discard the native window relationship and request details such as POST data.
- Initialization is a shared Task, explicitly started for hidden tabs. Handle creation is on the UI thread. Tab insertion selects immediately only for foreground requests; asynchronous initialization completion does not select again. Background initialization no longer requests address focus or shows its notification icon.
- Context-menu changes use WebView2's supported [native context-menu APIs](https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/context-menus). Other native commands and menu presentation remain WebView2-owned.

## Practical limits

This is not a claim of full Chromium equivalence. Chromium transports the exact disposition; this WebView2 API does not. Native gesture correlation is single-use, lasts at most 1,000 ms, and is cleared on navigation, another click, context-menu opening, or hiding. A delayed request can outlive it; a script request within the interval can be associated with it; multiple requests from one input cannot all be classified exactly. If the native hook is unavailable or Windows removes a stalled hook, uncorrelated requests fall back to foreground/popup policy. The expiry is a Quartz approximation, not a Chromium constant.

Explicit custom context-menu opens navigate a URL. They do not carry all of Chromium's internal initiator/referrer policy and request metadata; those cases require further verification. Renderer `NewWindowRequested` opens retain the native handoff. Popup windows use normal Quartz chrome, without a new popup-specific frame or exact feature sizing. Chromium's popup-blocking policy was not recreated. Allowed unprompted script opens default foreground, as in the activation policy; WebView2's popup permission behavior is separate.

Quartz's existing link placement, close selection, duplication history, Alt-click favourites navigation, history-grid selection, and cross-window tab-transfer rules were not rewritten. There is no injected JavaScript, DOM interception, or synthetic click implementation.

## Verification

- Debug solution build succeeded with Visual Studio MSBuild, using `verification\tab-activation\build\` as an isolated OutDir.
- `Run.ps1`: **32** policy/gesture assertions passed, including modifiers, first-tab behavior, gesture expiry/consumption, clock wrap, popup defaults, and address-bar dispositions.
- Existing `verification\shortcuts\Run.ps1`: **174** shortcut-routing assertions passed.
- `git diff --check` passed for the implementation snapshot.
- The real executable was verified by process path as `C:\Users\admin\source\repos\Quartz\verification\tab-activation\build\Quartz.exe`. The desktop launch API initially resolved a bare path to the installed app; that test instance was closed and the explicit process path was used. Installed-app output was not counted as validation.
- In real Quartz with the script-free local `links.html`/`target.html`: middle-click opened and loaded a background target; plain target blank selected the new target; native context-menu new tab loaded in background; Ctrl+Enter opened background; Ctrl+Shift+Enter opened foreground; Ctrl+T selected a new tab; address-bar Alt+Enter selected a new tab.
- Physical Ctrl+mouse/Shift+middle gestures, scripted delayed/multiple opens, POST/opener behavior, image/media commands, batch opening, duplicate, cross-profile windows, close-during-initialization and popup window features remain manual checks. Tests of policy are not evidence that these live paths all match Chromium.
- Automation stopped sending input when user activity was detected. Other source edits appeared after the tested snapshot; the build/test results above apply to the snapshot actually built, not subsequent concurrent edits.

To build the reviewable version, use Visual Studio on `Quartz.sln` or the same MSBuild command with `/p:OutDir=<isolated directory>`. Run `verification\tab-activation\Run.ps1` for the policy checks. Open `links.html` in that repository-built Quartz for the script-free page checks.
