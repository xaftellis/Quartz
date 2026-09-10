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

Right-click a tab and choose **Pin tab** or **Unpin tab**. Pins form a contiguous
prefix. Pinning appends to that prefix; unpinning inserts at the beginning of the
normal tabs. Selection and the live content form are preserved. New ordinary tabs
opened from pinned tabs enter the normal section. Duplicates inherit pinning.
Dragging reorders within the corresponding section, and moving a tab to another
window keeps its pin state.

Pins are 55 DIP wide with the existing 17 DIP overlap, show a native-size favicon
or loading indicator, and have no close button. The full caption remains available
through the existing tooltip. Explicit Close Tab, Ctrl+W and middle click retain
their usual behavior. Close other/left/right skips pins and uses the normal content
close lifecycle, including cancellation and disposal.

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

`Tests/Quartz.PinnedTabs.Tests.csproj` exercises the real tab model, renderer,
context menu and close lifecycle using hidden WinForms content and deterministic
timestamps. It produces light/dark frame sheets and checks 100%, 125%, 150% and
200% render scales. It does not launch WebView2 or modify profile data. Visible
browser/compositor smoothness and native tear-out gestures still need a live smoke
check after rebuilding the running browser.
