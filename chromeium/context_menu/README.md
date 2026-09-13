# Chromium Windows menu reference

Quartz's existing WinForms context menus use the desktop Views menu geometry
from Chromium main revision **589f82dce2d0ccf2235aee0bf334630960947b8f**,
fetched 13 September 2026 (commit timestamp 12 September 2026, 09:59:30 UTC).
This is the current development source, not a claim about a released Chrome version.

Source files at that revision:

- [menu_config.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/controls/menu/menu_config.cc): 6 DIP item vertical margins, 12 DIP horizontal border padding plus 8 DIP content padding, 16 DIP arrows, 17 DIP separator rows, no outer border stroke.
- [menu_config_win.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/controls/menu/menu_config_win.cc): Windows system menu font and system submenu delay.
- [layout_provider.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/layout/layout_provider.cc): 12 DIP menu corner radius.
- [submenu_view.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/controls/menu/submenu_view.cc): optional icon/check column and shared right-aligned accelerator column.
- [menu_scroll_view_container.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/controls/menu/menu_scroll_view_container.cc): corner-radius top/bottom padding; shadow elevation 12 for root menus and 16 for submenus.
- [shadow_value.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/gfx/shadow_value.cc): key and ambient shadow layers (alpha 61 and 31).
- [skia_paint_util.cc](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/gfx/skia_paint_util.cc): conversion of shadow blur radius to Gaussian sigma.
- [check.icon](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/ui/views/vector_icons/check.icon) and [keyboard_arrow_right_flippable.icon](https://github.com/chromium/chromium/blob/589f82dce2d0ccf2235aee0bf334630960947b8f/components/vector_icons/keyboard_arrow_right_flippable.icon): downloaded originals in `icons/`. Chromium stores these as vector command files. `convert-icons.ps1` mechanically converts their commands to the embedded SVGs in `Quartz/Resources/ChromiumMenus`, preserving every coordinate and the fill rule. The renderer loads these assets using Quartz's existing SVG library; it does not redraw their shapes.

The default light appearance is independent of Quartz's optional themes.
Windows high-contrast colors remain supported. WebView2 menus are unchanged.
The existing ToolStrip items, click handlers, opening/closing handlers, shortcuts,
accessibility providers and scrolling behaviour remain in use. Generated submenus
are styled in place, so their event handlers and component ownership survive.

Menu opening uses a shared 120 ms linear opacity fade, including generated
submenus. The UI thread remains available for normal input; a short-lived
WinForms timer advances by elapsed time, so missed ticks do not lengthen the fade.
The layered shadow reuses its native bitmap/DC throughout the animation and has
no opaque interior beneath the real menu (which would double-blend the opacity).
Closing, hiding, disposal and keyboard/mouse commands stop the fade immediately.
Quartz's existing Animation option and Windows menu-animation/UI-effects settings
are respected, and remote desktop sessions skip the effect. The old blocking
`AnimateWindow` calls have been removed from menu handlers; calendar slide effects
are outside this change.

Rendering differs from Chromium internally: WinForms uses GDI text and the
existing SVG library rather than Skia. The non-activating layered popup surface
uses a three-box approximation of the Gaussian shadow. Pixel-for-pixel matching
of font rasterization and blur across these rendering engines is not guaranteed.

Chromium glyphs are Copyright 2026 The Chromium Authors and use its BSD license;
see [LICENSE](LICENSE). The Chromium license is already included in
Quartz's distributed assets (`assets/quartz.com/newtab/chromium-LICENSE`).
