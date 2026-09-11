# Moving tabs between windows

Reference: Chromium's current `main` branch, consulted September 11, 2026.
The copyright year at the top of a source file is its original creation year,
not the revision used here.

- [Tab menu](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/chrome/browser/ui/tabs/tab_menu_model.cc):
  direct new-window command when no other eligible window exists; destination
  submenu otherwise.
- [Window submenu](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/chrome/browser/ui/tabs/existing_window_sub_menu_model.cc)
  and [base submenu](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/chrome/browser/ui/tabs/existing_base_sub_menu_model.cc):
  new-window item, separator, eligible same-profile windows, and literal window
  names without mnemonic interpretation. The consulted window-submenu blob was
  `2a8e3cd610566914cf0028f709f43b945f26f406`.
- [MoveTabsToWindowImpl and CanMoveTabsToNewWindow](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/chrome/browser/ui/browser_commands.cc):
  detach and insert live contents, preserve pins, activate the moved tab,
  append normal tabs and show the destination. New-window moves are disabled
  when every source tab would move.
- [WindowMetadataController](https://raw.githubusercontent.com/chromium/chromium/main/chrome/browser/ui/window_metadata/window_metadata_controller.cc):
  prefer the user's window name; otherwise use the active page title and the
  number of other tabs. Elide the title to fit a 400-pixel menu label, reserving
  room for the tab-count suffix.

Quartz implements the single-tab form of this command. It lists compatible
windows in activation order, including minimized windows. Menu actions retain
the destination object and validate it again when clicked, so closing or
reordering windows while a menu is open cannot redirect the transfer.

`TitleBarTabs.MoveTabToWindow` reparents the existing content form and transfers
only the owning window's subscriptions. It preserves the tab wrapper, page,
native handle, loading clock, favicon and pin state. An empty source can close
only after the destination owns its content. Pin state remains session-only.

`TabContextMenu.WindowMenu.cs` handles menu construction and window activation.
New AppContainers finish Load before tab selection, so Quartz's browser-parent
reference and selection handlers update for the transferred page. A minimized
destination restores its last normal/maximized state.

The regression harness covers active/background and pinned/normal transfers,
repeated moves, content identity, closing and cancellation, source-window
closure, both menu forms, stale destinations, activation ordering and labels.
It uses hidden WinForms contents, without starting WebView2. For a live smoke
test, move a page with history, scroll position and entered text to an existing
window and a new window; repeat with a pin and a minimized destination.
