# Tab rendering source review — 16 September 2026

Compared the production Quartz renderer with the supplied local Chromium
85.0.4183.121 reference. This is a source review, not a claim of live visual
parity or of matching today's upstream Chromium. Some existing Quartz geometry
comments name Chromium 86; this review uses the actual local 85 source.

## Applied: tabs overwriting the toolbar join

In `ChromiumTabRenderer.Render`, the toolbar-coloured overlap row was painted
before the tab fills. Each tab path extends through that overlap, so an inactive
tab or its hover highlight could overwrite the row. The supplied screenshot has
exactly that discontinuity: row 34 is tab grey underneath the inactive tab and
white beyond its trailing edge.

The join now paints after all tab and new-tab-button painting. Its top and bottom
edges are rounded separately using `Height` and `ToolbarOverlap`, rather than
rounding its position and thickness separately. Active tabs retain their seamless
connection to the same toolbar colour.

Source: [Quartz paint order](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:753),
[Chromium overlap geometry](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab_style_views.cc:239>),
[Chromium tab-strip child creation](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/frame/browser_view.cc:525>)
and [later toolbar child creation](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/frame/browser_view.cc:566>).

At the user's request, no build or regression checks were run after applying the
fix; visual confirmation is left to the user. The temporary test source and runner
created earlier in this task were removed.

## Applied: crowded tabs entering the add-button area

Quartz retained minimum tab widths when there was insufficient room, but painted
every tab without restricting the canvas to the available tab area. The fix uses
Chromium's trailing-edge visibility rule: a tab that does not fit is hidden and
excluded from mouse targeting, including its close button. Background tabs before
the active tab also reserve the extra width they would need when selected.

The add button is now laid out before tab visibility and mouse targeting are
calculated. Its current animated position bounds tab painting, so open/close
animations cannot paint underneath it. The tab canvas is clipped at that boundary
to contain antialiased edges too. Resizing immediately keeps the add button inside
its newly available space instead of letting its previous animation position
extend toward the caption controls. Closing visuals use the same paint boundary.

Source: `ChromiumTabRenderer.ShouldShowTab`, `FindTab`, `Render`, and
`LayoutAddButton`; [Chromium visibility rule](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab_strip.cc:1404>).
No runtime or build checks were run for this follow-up; visual testing is left to
the user. This adds overflow hiding, not scrolling or an overflow menu.

## Additional findings — not changed

1. **Shared separators can disagree by one physical pixel at 150% scaling.**
   Quartz rounds the overlap to an integer pixel and aligns each path in local
   coordinates. Chromium aligns the original tab bounds in strip coordinates,
   allowing both tabs to round the same shared edge. For example, at 150%, a
   384-pixel tab's trailing separator starts at `round(384 - 13.5) = 371` relative
   to its origin. The next tab's leading separator starts at
   `384 - round(25.5) + round(12) = 370`. Both have a 1.5-pixel width, so their
   strokes do not coincide. This can make the separator look wider or uneven.
   A correction needs coordinated layout and path alignment, rather than a
   constant offset applied to one separator.

   Sources: [Quartz alignment](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabGeometry.cs:86),
   [separator painting](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:845),
   [Chromium alignment](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab_style_views.cc:912>).

2. **Closing titles can shift left as the tab shrinks.** Quartz recalculates
   `roomy`, content padding and icon centring from the current animated width,
   then redraws the closing title using those new bounds. Crossing the 100-DIP
   tab-width threshold drops the left content inset from 20 to 16 DIP, so a title
   can move by 4 DIP during closure. The cached favicon retains its original
   bounds. Chromium deliberately freezes the extra padding and centring decisions
   while closing to prevent this shift.

   Sources: [Quartz content layout](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:855),
   [closing title update](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:910),
   [Chromium closing-state rules](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab.cc:915>).

3. **Remainder pixels skip the active tab even when all tabs have equal sizing
   rules.** Quartz reserves the active tab's floored width, then distributes the
   remaining pixels among inactive tabs. Chromium distributes them left to right
   among all eligible tabs; the active tab is excluded only when its constrained
   sizing rule requires it. With three normal tabs, 600 pixels available and the
   first tab active at 100%, Quartz computes `[211, 212, 211]`; Chromium's rule
   gives `[212, 211, 211]`. Changing selection can consequently move a shared edge
   by one pixel even though the available space did not change.

   Sources: [Quartz width distribution](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabGeometry.cs:39),
   [Chromium extra-space allocation](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab_strip_layout.cc:100>).

These are source-derived cases; their appearance in a running Quartz window has
not been tested in this review.

## Close-tab button sizing

The desktop close-button dimensions agree with the local Chromium 85 code.
No close-button sizing change was made.

| Measurement, before DPI scaling | Quartz | Chromium 85 |
| --- | --- | --- |
| Mouse target | 16 x 16 DIP | 16 x 16 DIP |
| Hover circle diameter | 16 DIP | 16 DIP |
| Stroke width | 1.5 DIP | 1.5 DIP |
| Diagonal endpoint coordinates within the button | 4.75 to 11.25 DIP | 4.75 to 11.25 DIP |
| Visible cross width and height, including round caps | 8 x 8 DIP | 8 x 8 DIP |

Chromium computes the diagonal bounds as `(16 - 8) - 1.5 = 6.5` DIP,
centred within 16 DIP. That gives `(16 - 6.5) / 2 = 4.75` and
`4.75 + 6.5 = 11.25`, exactly Quartz's endpoint constants. Both use
antialiasing and rounded stroke caps. Chromium ignores its surrounding layout
padding for mouse targeting, so the larger padded view is not a larger mouse
close target. Its touch mode uses a separate 24-DIP size; that is not the desktop
mouse sizing used here.

Sources: [Quartz hit box and hover circle](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:875),
[Quartz cross](C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs:1069),
[Chromium cross and mouse targeting](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/chrome/browser/ui/views/tabs/tab_close_button.cc:152>),
[Chromium circle radius](<C:/Users/admin/source/repos/Quartz - CHATGPT/Chromium85Reference/ui/views/layout/layout_provider.cc:145>).

This confirms the source dimensions. It does not establish identical on-screen
rasterisation, monitor scaling, or agreement with a different Chromium version.
The reported smaller appearance remains unconfirmed by a live comparison.

## Geometry and ordering that agree with the local reference

The basic non-touch dimensions agree: 35-DIP tab height including the 1-DIP
toolbar overlap, 8-DIP corner radius, 17-DIP tab overlap, 256-DIP standard width,
55-DIP pinned width, 48/32-DIP active/inactive minimum widths, and 20-DIP separator
height. The shrinking corner-radius formula also agrees.

Quartz paints the active tab last and orders inactive tabs by hover state and
animation value, matching Chromium's approach for Quartz's supported selection
model. The 16-DIP favicon, 68-DIP content threshold for inactive close buttons,
and 30-DIP pinned-icon transition range are consistent with the local source.

References: Chromium `tab_style.cc`, `layout_constants.cc`, `tab_style_views.cc`,
`tab.cc`, and `TabStrip::PaintChildren`; Quartz `ChromiumTabGeometry.cs` and
`ChromiumTabRenderer.cs`.

Quartz's custom palettes, GDI+ title rasterisation, and WinForms layered overlay
remain its own implementation. Matching dimensions and paint-order rules does
not establish identical font rasterisation or native window composition.
