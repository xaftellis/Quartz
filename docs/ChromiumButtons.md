# Quartz buttons and Chromium 85

Reference checkout: `C:\src\chromium85\src`, version **85.0.4183.121** from `chrome/VERSION`.

This is a WinForms port of the desktop toolbar ink-drop behavior, with a shared Skia image renderer. It is not a claim of complete pixel or interaction parity with a running Chromium build. The final audit and live checks are deferred at the user's request while the reported icon, favourites-spacing and click regressions are addressed.

## Architecture and API

`Quartz/Controls/ChromiumButton.cs` derives from `Button` to preserve existing Click/MouseUp handlers, validation, keyboard activation, accessibility and `IButtonControl`/DialogResult integration. Its paint override bypasses the ButtonBase paint adapter. The surface, ink layers and images are drawn by Quartz; existing Paint subscribers still run afterwards.

`ChromiumButtonAnimation.cs` models separate highlight, ripple radius and ripple opacity timelines. A shared UI-thread timer schedules only controls that are animating; samples use monotonic time. A held active control has no repeating animation work once its transitions finish.

`ChromiumButtonImage.cs` caches device-scale image representations. Supplied bitmaps retain their colours and aspect ratio. Missing raster sizes use the Lanczos3 filter and 14-bit fixed-point convolution derived from Chromium. Optional `VectorIcon` values use copies of Chromium's `.icon` path data and its representation-selection rules. The browser toolbar deliberately uses **Quartz's theme-selected Image resources**: setting a forced vector there had overridden those resources and used old white/transparent designer ForeColor values, making icons disappear on the light toolbar.

```csharp
button.IsActive = true;   // remain active through pointer leave and capture loss
button.IsActive = false;  // run the deactivation fade
bool pressed = button.IsPressed; // effective physical/keyboard/menu/explicit state

button.IconSize = 16;     // logical-DIP icon bounding box
button.Image = artwork;  // caller retains ownership; custom renderer draws it
button.VectorIcon = ChromiumIcon.None; // select Image rather than an optional vector
button.FocusOnPress = false; // browser toolbar: preserve focus in the page
```

Assigned ContextMenuStrip instances are observed through Opened/Closed/Disposed to hold the source button active. Menu classes, renderers, content and opening behavior are unchanged. Explicit activation and observed menu activation are independent; clearing one does not clear the other.

## Source-to-behavior map

All source paths below are relative to the reference checkout.

| Chromium reference | Behavior carried into Quartz |
| --- | --- |
| `chrome/browser/ui/views/toolbar/toolbar_button.cc`, `toolbar_button.h` | 16-DIP normal toolbar icon, 6-DIP insets; action ink drop; persistent menu activation; toolbar press does not request focus. |
| `chrome/browser/ui/layout_constants.cc` | Desktop toolbar button height 28; related element gap 4; toolbar standard spacing 8. Touch-UI dimensions are a different mode and are not implemented. |
| `chrome/browser/ui/views/toolbar/toolbar_ink_drop_util.cc`, `.h`; `ui/views/layout/layout_provider.cc` | Toolbar highlight opacity .08, ripple opacity .06; maximum-emphasis capsule; larger targets inset on all sides by half the excess over location-bar height. |
| `ui/views/animation/ink_drop_host_view.cc` | Default flood-fill ripple; SHOW_ON_RIPPLE highlight behavior; pointer-origin ripple or centered keyboard/programmatic origin; separate rounded mask. |
| `ui/views/animation/flood_fill_ink_drop_ripple.cc` | Minimum radius 1 DIP, expansion to farthest **host** corner; pending/click/active/cancel timelines listed below. |
| `ui/views/animation/ink_drop_impl.cc` | 250-ms hover transitions, highlight retained while a non-hidden ripple is active, 120-ms unhovered highlight fade when the ripple hides. Hidden-targeted, triggered and deactivated ripples are discarded before a new ripple state starts. |
| `ui/views/animation/ink_drop_highlight.cc` | Quadratic EASE_IN_OUT highlight fades. Fade-out starts at the current opacity; re-entering recreates a highlight whose FadeIn starts at zero. |
| `ui/gfx/animation/tween.cc` | EASE_IN is t²; EASE_IN_OUT is piecewise quadratic; FAST_OUT_SLOW_IN is cubic-bezier(.4, 0, .2, 1). |
| `ui/views/controls/button/button_controller.cc`, `button.cc` | Press -> ACTION_PENDING, valid release -> ACTION_TRIGGERED; drag out cancels, drag back in restarts; Space activates on release; keyboard/programmatic origin is centered. |
| `ui/views/controls/button/menu_button_controller.cc`; `chrome/browser/ui/views/frame/app_menu_button.cc` | A menu owns a persistent pressed lock until it closes. Quartz exposes IsActive and observes existing menus rather than porting their controllers. |
| `ui/gfx/animation/animation_win.cc`; `ui/views/animation/ink_drop_event_handler.cc`; `ui/events/win/events_win_utils.cc` | Windows client-area animation preference; remote-session fallback; promoted touch input does not initiate a hidden ripple. |
| `chrome/browser/themes/theme_helper.cc`; `ui/gfx/color_utils.cc` | Same artwork for normal/hover/pressed, disabled icon alpha 0x6E, toolbar ink chosen between white and Google Grey 900 using the source luminance threshold. Quartz's palettes remain its own. |
| `ui/views/controls/focus_ring.cc`; `ui/views/style/platform_style.cc`; `ui/native_theme/common_theme.cc` | 2-DIP focus stroke; Google Blue 600/300 with alpha 0x4D for ordinary light/dark focus; Windows high-contrast foreground. |
| `ui/views/controls/button/image_button.cc`; `label_button.cc`; `ui/views/controls/image_view.cc` | Independent image/label layout, centered image origin, preferred-size measurement and device-scale representations; no native pressed image offset or Windows disabled-image effect. |
| `ui/gfx/paint_vector_icon.cc`; `skia/ext/image_operations.cc`; `skia/ext/convolver.cc`, `.h` | Even-odd vector fill and representation selection; pixel-center Lanczos3 filter, normalized fixed-point weights, two-pass convolution and premultiplied-alpha correction. |
| `chrome/browser/ui/views/bookmarks/bookmark_bar_view.cc`; `chrome/browser/ui/views/chrome_layout_provider.cc` | Favourites: 28-DIP height, 150-DIP maximum width, 6-DIP padding, 16-DIP favicon, **8-DIP icon/label gap**, 4-DIP inter-button gap. Width derives from icon + gap + full title + insets, then clamps; the renderer elides text. |
| `chrome/browser/ui/views/location_bar/icon_label_bubble_view.cc`; `chrome/browser/ui/omnibox/omnibox_theme.cc` | Site-info variant uses surrounding foreground as ink, .10 highlight and .16 ripple rather than the toolbar values; rounded local-bounds geometry. |

### Ripple timelines

| Transition | Radius | Opacity |
| --- | --- | --- |
| Mouse/key down: ACTION_PENDING | 240 ms FAST_OUT_SLOW_IN to maximum | Immediately visible; opacity timeline pauses until expansion completes. |
| Accepted release: ACTION_TRIGGERED | Finish the pending expansion | Queue 300 ms EASE_IN_OUT fade **after** the pending pause. A quick click therefore continues after mouse-up; a long hold begins fading on release. |
| Cancel: HIDDEN | 300 ms EASE_IN_OUT back to minimum | 200 ms EASE_IN_OUT to zero. |
| Persistent ACTIVATED from pending | Retain the pending expansion | Retain pending opacity. |
| Persistent ACTIVATED from other states | New ripple, 200 ms EASE_IN_OUT | 150 ms EASE_IN to visible. |
| DEACTIVATED | Finish any existing expansion | Queue 300 ms EASE_IN_OUT after outstanding opacity work. |

The WinForms ordering is critical: `Button.OnMouseUp` calls `OnClick` before the public MouseUp event. Quartz must retain its press flag through `base.OnMouseUp` and cancel only if the state is still pending afterwards. Clearing it before that call rejected native clicks and took the cancellation path instead of the click fade.

## Migration inventory

| Quartz surface | Controls |
| --- | --- |
| Browser toolbar | Back, Forward, Reload, Stop, Downloads, Add favourite, Settings; SiteInfoButton uses the shared base. |
| Favourites bar | Dynamic FavouriteButton instances; existing ordering, drag/drop, middle-click, preview, entrance and content transitions retained. Removed the six-space icon prefix and fixed 23-pixel height. Snapshot layers now hide painting without removing Image or Text, preserving the layout during animations. |
| Profiles and ModifyProfile | Rewritten CircularImageButton with cached circular avatar, caption and separate child action button; profile events retained. Profiles disposes old tiles/images and reuses one tooltip component. |
| AddBirthday, ClearHistory, CustomMessageBox, Favorite, History, NameWindow, Password, Settings, TryOut | Designer Button instances use the shared control. Sub-28-pixel action buttons and calendar arrows were increased to 28 pixels at design scale. Larger established dialog targets remain. |
| SiteInfoPopup | Header icon buttons and action buttons. This is a Form, not a context menu. |
| QuartzUpdater | Four actions use the same linked control/animation/image source files and Skia dependency. |

Excluded by instruction: **EasyTabs and every context-menu implementation**, including embedded zoom controls. Checkboxes, hyperlinks, native Windows dialogs and WebView HTML controls are not toolbar push buttons and were not migrated.

## Practical differences and unfinished validation

- WinForms paints on its UI thread, rather than Chromium's compositor; time curves match, but a blocked UI thread can skip frames.
- Focus rings fit inside each child HWND; Chromium can paint its ring beyond a View's bounds. WinForms dialog tab/validation/default-button handling remains in place.
- Browser artwork and palettes remain Quartz's. Optional Chromium vectors are available, but do not override the toolbar's selected theme images. Site-info pictograms and profile-tile composition retain Quartz-specific drawing; they are not a port of Chromium's full page-info/profile UI.
- The port targets normal desktop geometry, not Chromium's separate touch layout. Existing address-bar layout, larger dialog layouts, scrolling favourites overflow and profile tile dimensions remain Quartz-specific. Favourites bar width/height constants are DPI-scaled; physical mixed-monitor behavior still needs checking.
- A resize resets transient feedback and preserves explicitly activated feedback. Chromium rebuilds its layers on bounds changes; the full observer/layer lifecycle is not reproduced. Invisible post-fade ripple cleanup is collapsed.
- Chromium's menu opening policy, long-press menu delay, gestures and alternate-action states are not ported into the existing Quartz context menus.
- Modern SkiaSharp is newer than Chromium 85's Skia. Text uses Windows TextRenderer instead of Chromium RenderText, and platform font metrics/raster details can differ.
- Preliminary isolated builds and an offscreen harness passed before the latest reported regressions. Those assertions exercised deterministic timelines, layout, images and direct control methods; they did **not** establish native mouse dispatch or live Quartz parity. In particular, the old release helper manually invoked Click before MouseUp and missed the ordering defect.
- At the user's request, the latest icon-selection, native-release-order and favourite snapshot fixes have **not** been rebuilt or run through the broader audit. Native mouse clicks, quick release/long hold, modal/menu close, favourite transitions, theme switching and mixed-DPI rendering remain to be verified together before an accuracy sign-off.

Chromium-derived code and copied icon data retain the BSD notice in `Chromium-LICENSE`, also copied to application build output.
