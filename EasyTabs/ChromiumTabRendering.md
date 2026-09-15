# Chromium tab rendering and pinning

Quartz's vector tab renderer follows Chromium **86.0.4240.75** desktop (non-touch)
geometry. Pinned tabs use that same baseline so their size, overlap and transitions
fit the existing strip. Chromium-derived portions retain the license in
[Chromium-LICENSE.txt](Chromium-LICENSE.txt).

## Sources inspected

- [TabStripModel](https://chromium.googlesource.com/chromium/src/+/86.0.4240.75/chrome/browser/ui/tabs/tab_strip_model.cc):
  `SetTabPinnedImpl`, `ConstrainInsertionIndex`, and `GetIndicesClosedByCommand`.
- [TabStrip](https://chromium.googlesource.com/chromium/src/+/86.0.4240.75/chrome/browser/ui/views/tabs/tab_strip.cc):
  `SetTabData` and `StartPinnedTabAnimation` recalculate ideal bounds, then animate
  the affected tabs to those bounds.
- [Tab layout](https://chromium.googlesource.com/chromium/src/+/86.0.4240.75/chrome/browser/ui/views/tabs/tab.cc):
  `Layout`, `UpdateIconVisibility`, `ShouldRenderAsNormalTab`, and
  `MaybeAdjustLeftForPinnedTab` control the close button, title and favicon.
- [Tab strip layout](https://chromium.googlesource.com/chromium/src/+/86.0.4240.75/chrome/browser/ui/views/tabs/tab_strip_layout.cc):
  `CalculatePinnedTabBounds` retains fixed pinned widths even under pressure.
- [BoundsAnimator](https://chromium.googlesource.com/chromium/src/+/2e7bf39223623d6058af7fc58229e07248526706/ui/views/animation/bounds_animator.h):
  200 ms with `EASE_OUT`.
- [DuplicateTabAt](https://chromium.googlesource.com/chromium/src/+/0824deb64346af49f8c2899d50f55e363ac809f7/chrome/browser/ui/browser_commands.cc):
  duplicates inherit the source tab's pinned state.

## Quartz behavior

Right-click a tab and choose **Pin** or **Unpin**. Pins form a contiguous
prefix. Pinning appends to that prefix; unpinning inserts at the beginning of the
normal tabs. Selection and the live content form are preserved. New ordinary tabs
opened from pinned tabs enter the normal section. Duplicates inherit pinning.
Dragging reorders within the corresponding section, while the visual follows the
pointer across the entire tab strip. Releasing animates it back to its legal slot
over 200 ms. Moving a tab to another window keeps its pin state. This follows
Chromium's [GetAttachedDragPoint and MoveAttached](https://chromium.googlesource.com/chromium/src/+/51eeb5e52adee36cec79d1e80613e63838d8260d/chrome/browser/ui/views/tabs/tab_drag_controller.cc)
and [StoppedDraggingTab](https://chromium.googlesource.com/chromium/src/+/ab376d502995fadf0dabd0ff02e50e5d2a53fb7e/chrome/browser/ui/views/tabs/tab_strip.cc).

Pins are 55 DIP wide with the existing 17 DIP overlap, show a native-size favicon
or loading indicator, and have no close button. The full caption remains available
through the existing tooltip. Explicit Close Tab, Ctrl+W and middle click retain
their usual behavior. Close other/left/right skips pins and uses the normal content
close lifecycle, including cancellation and disposal.

Pinned pages with a missing or hidden favicon show Quartz's configured default
favicon. This is a rendering fallback: the content's Icon and ShowIcon properties
are never changed, so unpinning restores the original appearance. Chromium similarly
forces visibility with `IsPinned() || ShouldDisplayFavicon()` in
[TabData](https://chromium.googlesource.com/chromium/src/+/main/chrome/browser/ui/tabs/tab_data.cc).
Its [favicon utilities](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/chrome/browser/favicon/favicon_utils.cc)
provide a default matching the color scheme. A newly received visible favicon
replaces the fallback normally, and loading still uses the spinner.

Pin/unpin uses the shared layout clock: `1 - (1 - t)^2` over 200 ms for position
and width. Reversals snapshot displayed bounds. While pinning, the close target
is disabled immediately; the title remains until the tab is less than 30 DIP wider
than its pinned width. The favicon moves to the pinned center across those last
30 DIP without being scaled. Reduced-motion settings snap to the final layout.

Pin state lives only on `TitleBarTab.IsPinned` for this session. This feature does
not save or restore tabs, leaving session persistence for the planned remember-tabs
feature. Existing strip overflow behavior is retained; it does not add scrolling
or a tab overflow menu.

## Verification

### Active and inactive windows

Window activation colours follow the supplied **Chromium 85.0.4183.121** source:

- `chrome/browser/themes/theme_properties.cc`, `GetDefaultTint`: the inactive
  frame tint `{-1, -1, 0.642}` changes light grey `#DEE1E6` to `#E7EAED`.
  Only Quartz's light theme uses this default tint.
- `chrome/browser/themes/browser_theme_pack.cc`, `BuildFromColors`: generated
  custom themes explicitly disable inactive frame tinting. Quartz's dark, black,
  aqua and Christmas palettes follow this path, preserving their frame and tab
  fills in both states. `SetFrameAndToolbarRelatedColors` propagates custom
  selected-tab text to both states; `GetTabForegroundColor` respects explicit
  custom foregrounds and fades only the unspecified background-tab foreground.
  The selected tab retains its toolbar colour in both window states.
- `chrome/browser/ui/views/tabs/tab_strip.cc`, `GetTabForegroundColor` and
  `UpdateContrastRatioValues`: use 75% foreground blending for inactive windows,
  then enforce Chromium's selected/background text contrast targets; recalculate
  hover highlights and separators against the current frame palette.
- `ui/gfx/color_utils.cc`, `HSLShift`, `AlphaBlend`, `BlendForMinContrast`:
  use Chromium's channel rounding and 8-bit contrast search.

The Windows accent-colour tint `{-1, 0.54, 0.567}` is not a generic dark-theme
transform. Applying its saturation shift to Quartz's neutral `#585858` produced
the incorrect pink `#746868`. It has been removed. Chromium's default dark palette
(`#202124` active / `#3C4043` inactive) is a separate choice; Quartz's custom
palettes keep their existing colours under the generated-theme rules above.
- `chrome/browser/ui/views/frame/windows_10_caption_button.cc`, `PaintSymbol`:
  inactive symbols use alpha `0x66`, except while hovered or pressed. Native
  black/white symbol selection follows `GlassBrowserFrameView::GetReadableFeatureColor`.

`ui/views/widget/widget.cc`, `ShouldPaintAsActive`, separates frame appearance
from focus; `ui/views/bubble/bubble_dialog_delegate_view.cc` keeps the anchor
painted active while an owned bubble is active. Quartz maps this to the foreground
HWND's parent/owner chain, including menus, nested dialogs and the caption overlay.
Another `TitleBarTabs` window terminates that chain and has its own activation
state. Context menus have an explicit source control, including nested submenus.

An out-of-context `EVENT_SYSTEM_FOREGROUND` hook schedules a UI-thread repaint
even when the browser itself has already lost focus to its dialog. This ensures
switching away from an open dialog still dims the browser. Activation messages are
coalesced through the message queue, without a delay timer; a transient null
foreground preserves the preceding appearance. Input cancellation continues to
use actual focus independently of the paint state. Cached title pixels refresh
when their foreground colour changes. The hook is removed on disposal.

Hidden-render verification covers all five Quartz palettes, activation reversal,
theme changes while inactive, selection and geometry preservation, and the rendered
caption-symbol alpha. Ownership checks cover the caption overlay, menus, nested
dialogs, embedded tab forms, unrelated windows and another browser's dialog.
Final visible-window verification is left to the user.

### Existing tab checks

`Tests/Quartz.PinnedTabs.Tests.csproj` exercises the real tab model, renderer,
context menu and close lifecycle using hidden WinForms content and deterministic
timestamps. It produces light/dark frame sheets and checks 100%, 125%, 150% and
200% render scales. It does not launch WebView2 or modify profile data. Visible
browser/compositor smoothness and native tear-out gestures still need a live smoke
check after rebuilding the running browser.
