# Tab animation comparison — Chromium 85

Compared on 2026-09-16 against the local **85.0.4183.121** source in
`C:\Users\admin\source\repos\Quartz - CHATGPT\Chromium85Reference`.
Quartz's newer card layout, previews, memory footer, theme colours, tab geometry,
and cached renderer remain. Animation behavior uses the older reference, with
one requested exception: the tab close button uses the add button's animated
hover and click feedback instead of Chromium 85's immediate hover highlight.

## Findings and resulting behavior

| Area | Before this change | Result |
| --- | --- | --- |
| Inactive tab hover | Linear steps based on time since the previous paint; long frames capped at 64 ms | 200 ms quadratic ease-out on entry, quadratic ease-in on exit; elapsed-time clock per tab |
| Hover reversal | Linear progress | Start from the displayed opacity; duration is 200 ms times the remaining distance, with Chromium's one-frame minimum |
| Leaving the strip | Relied on global mouse moves; hook-time cursor position could be stale; no explicit native leave repaint | Track both client and non-client mouse leave; wake the idle renderer and sample the current pointer after dispatch |
| Add-button hover | 200 ms smoothstep | 250 ms quadratic ease-in-out, 16% opacity; re-entry recreates the highlight as Chromium does |
| Add-button press | 225 ms cubic ease-out; approximate initial radius | 240 ms `FAST_OUT_SLOW_IN` expansion from one DIP to the farthest host corner, clipped to the button; 14% ripple opacity |
| Add-button release | Immediate 160 ms linear fade | Queue a 300 ms quadratic ease-in-out fade after expansion; a short click takes 540 ms from press through ripple completion |
| Button cancellation | Ripple disappeared immediately outside the button | 200 ms opacity fade and 300 ms shrink; re-entering while held starts another pending ripple |
| Tab close-button feedback | Same approximations as the add button | Same animation engine and timing as the add button, as requested; feedback remains on the closing visual while it exists |
| Tab open/close | Already 200 ms quadratic ease-out, per-tab clocks, overlap-width endpoints, edge rounding | Retained; closing target is now fixed at close initiation so a subsequent close cannot restart its clock |
| Add-button movement | Bounds animation additionally clamped to all displayed tabs every frame | Independent 200 ms bounds animation, as Chromium uses; manual dragging still positions it directly |
| Separator hover transitions | Already used `1 - max(this hover, adjacent inactive hover)` | Retained, now driven by corrected tab-hover curves |
| Mouse-close width hold | Already preserved close targets and waited 300 ms after pointer exit before expansion | Retained; expansion uses the normal 200 ms ease-out bounds animation |
| Hover-card appearance | 200 ms fade in, 150 ms fade out with `FAST_OUT_SLOW_IN` | Retained |
| Hover-card tab switching | 200 ms; preserved a running clock until 80% progress | 75 ms `FAST_OUT_SLOW_IN`, restarting from displayed bounds for each new target; text follows the slide |
| Hover-card initial delay | Newer formula using widest tab and an extra standard-width delay | Chromium 85 default delay group: 300–800 ms logarithmic delay based on the hovered tab's width; 300 ms quick re-entry window retained |
| Preview replacement | Newer 200 ms image crossfade | Immediate replacement when the image arrives, as in Chromium 85; modern preview UI retained |
| Caption-button hover | 200 ms smoothstep | 150 ms ease-out in both directions; distance-scaled reversals and pressed-state resets |
| Title shift when icon visibility changes | Correct 100 ms `FAST_OUT_SLOW_IN`, but rounded position and width separately | Timing retained; interpolate and round each rectangle edge like Chromium |
| Favicon load transition | Newer reversible slide plus a second easing curve | Immediate loading crop, then a fresh 250 ms ease-out reveal on completion; reload cancels the reveal |
| Loading spinner | Same main cycle lengths, newer entry phase and rounded angles | Chromium 85 entry phase and integer angle division; 1320 ms waiting revolution, 1568 ms spinning revolution, 666.666 ms arc keyframe, 900 ms colour transition |

## Source locations

All Chromium paths below are relative to the reference root above.

- `chrome/browser/ui/views/tabs/tab.cc:575–623`: pointer entry, movement and exit.
- `chrome/browser/ui/views/tabs/glow_hover_controller.cc:42–70`: 200 ms hover,
  `EASE_OUT` entry and `EASE_IN` exit.
- `ui/gfx/animation/slide_animation.cc:45–87`: duration scaling and reversals;
  `linear_animation.cc:21–28,64–78`: minimum frame interval.
- `ui/gfx/animation/tween.cc:31–59,178–187`: the easing equations, cubic Bezier
  `(0.4, 0, 0.2, 1)`, and rectangle edge interpolation.
- `chrome/browser/ui/views/tabs/new_tab_button.cc:84–101,193–195`:
  highlight/ripple opacities and click transition.
- `ui/views/animation/ink_drop_host_view.cc:78–99,141–145`: flood-fill ripple
  and `SHOW_ON_RIPPLE` highlight mode.
- `ui/views/animation/ink_drop_impl.cc:25–46,455–501,728–744,815–824`:
  250 ms hover, highlight lifecycle, and 120 ms fade after a ripple when unhovered.
- `ui/views/animation/ink_drop_highlight.cc:97–140`: highlight restart and easing.
- `ui/views/animation/flood_fill_ink_drop_ripple.cc:82–99,182–233,397–434`:
  press, release, cancellation, queued opacity fade, and ripple radius.
- `chrome/browser/ui/views/tabs/tab_close_button.cc:59–74`: upstream disables
  hover fading; Quartz deliberately uses the add-button fade by user request.
- `ui/views/animation/bounds_animator.h:197–199`,
  `bounds_animator.cc:38–106`: 200 ms ease-out, displayed starting bounds,
  and unchanged-target handling.
- `chrome/browser/ui/views/tabs/tab_strip.cc:2436–2466,2519–2544,2564–2613`:
  opening endpoints, fixed closing targets, and independent tab/button animation.
- `chrome/browser/ui/views/tabs/tab_style_views.cc:410–437,575–690`:
  hover drawing order and separator interpolation.
- `chrome/browser/ui/views/tabs/tab_hover_card_bubble_view.cc:86–113,172–227,
  255–311,535–601,676–705,832–839`: card delay, fading, sliding and preview updates.
- `chrome/browser/ui/views/frame/windows_10_caption_button.cc:18–25` and
  `ui/views/controls/button/button.cc:188–208,539–546`: caption-button transitions.
- `chrome/browser/ui/views/tabs/tab.cc:229,250–255,381–421`: title animation,
  icon visibility changes, and retargeting without restarting the clock.
- `chrome/browser/ui/views/tabs/tab_icon.cc:82–87,306–338,368–378`:
  favicon crop, reveal, and reload cancellation.
- `ui/gfx/paint_throbber.cc`: waiting/spinning timing and integer angle arithmetic.

## Verification and limits

The full Debug solution build passed. The focused suite passed **348 assertions**.

Run `Tests\Run-TabAnimationChecks.ps1`. It builds the full solution to an isolated
output directory and tests the actual EasyTabs assembly using deterministic clocks
and hidden WinForms windows. Checks cover curve values, completion times,
interruption, reduced motion, per-tab bounds clocks, rapid closes, native mouse
leave, stale hook coordinates, card timing, and rendering at 100/125/150/200% scale.

Favicon checks also cover no-icon ↔ icon transitions on active and inactive tabs,
the 100 ms title curve and both endpoints, rapid visibility reversal, reduced
motion, and icon replacement without title movement. Cache updates are exercised
without forced redraw. Waiting hides both real and default icons; spinning shows
only real icons; completion reveals a real icon over 250 ms. A default icon or an
icon arriving after completion does not start that reveal. Pinned no-icon pages
retain their fallback icon and hide their title. These renderer checks do not
exercise live WebView2 favicon downloads or disk-cache persistence.

The native-leave regression was also run against a separate EasyTabs build made
from the original Git `HEAD` sources: the original fails to request a repaint on
leaving an idle strip. The corrected assembly passes that same check.

The existing close-gesture harness passes 17 of 18 scenarios on both the original
and corrected assemblies. Its late `FormClosing` cancellation scenario fails in
both: tab removal is subscribed before the later cancellation handler. This is
an existing close-lifecycle issue, outside the animation changes.

Source and deterministic checks establish the animation formulas and state
transitions, not frame-for-frame visual identity. Quartz still presents a cached
WinForms/Skia layered window through its shared frame scheduler; Chromium has its
own Views/compositor scheduling. No physical mouse or live side-by-side compositor
comparison was performed. OS scheduling and a blocked UI thread can delay a frame
even when its mathematical duration is correct.
