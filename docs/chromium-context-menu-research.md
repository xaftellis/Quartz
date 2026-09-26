# Chromium 154 Windows menus: context menus and browser Zoom row

Research date: 2026-09-25. **Research only; no application implementation or configuration changes.**

Chromium source: `D:/src/chromium154/src`, version **154.0.8037.58**, commit **`a654841425914cbb703a2931e07b70a83aedbafd`** (2026-09-21 17:20:43 -0700, “Incrementing VERSION to 154.0.8037.58”). Version comes from [chrome/VERSION][version]. This checkout was the only Chromium source used. No `out` directory was present; the findings establish the Windows source path and source defaults, not the effective flags of a running Chromium binary. Inspected tracked implementation files had no local diff.

Quartz source: `C:/Users/admin/source/repos/Quartz`, commit **`247f0e21ce8425d49656f6ddc89707a5d39f21ac`**. Paths beginning with `Quartz/` in the inventory identify the application subdirectory inside this repository. Source links below are absolute; line numbers refer to these checkout revisions.

## 1. Scope and implementation boundary

The general visual reference is Chromium's **Views/Aura web-content context menu on Windows**, including its nested menus. **§6A adds the separately traced Windows browser-menu Zoom/fullscreen row as the replacement specification for Quartz's provisional row.** That compound row is not an ordinary web-content context-menu item. Both use Chromium's Skia drawing infrastructure, not the standard Windows `HMENU` appearance. WinForms can provide Quartz's future popup windows, ownership, input and accessibility integration, but all practical menu painting and text layout should use Skia. Do not build the replacement renderer or its model on `ContextMenuStrip`, `ToolStrip`, native menu painting or standard WinForms visual styling.

**WebView2's built-in context-menu path is entirely outside the implementation scope.** Do not replace, disable, intercept or restyle its built-in menu; do not change its menu settings, subscriptions or handlers. Inspection found pre-existing customization in [Browser.TabOpening.cs][q-webview]: `InitializeTabOpening` subscribes to `ContextMenuRequested` at line 33; lines 103–150 insert/replace certain open-link/media/frame commands. [Settings.Updates.cs][q-update-webview]:30 already disables default context menus on its separate loading-progress WebView. These are existing behavior, not integration opportunities: leave them unchanged, including their event wiring. This project must not attempt to “restore” them either. Calling Quartz's existing page-zoom function from an owned menu is a separate command connection (§6A), not a change to WebView2's context menu.

Suitable Quartz-owned targets do exist (§7). Preserve their surrounding items, order and command effects. **The Zoom/fullscreen row is an explicit exception: its control order, geometry, styling, state rules and interaction come from §6A, not Quartz's current implementation.** In particular, the percentage becomes informational; it is not a reset button. Keep Quartz's existing reset-zoom command available through its existing independent shortcut. All proposed work in §8 is future work. Menu animations, fades, ink drops, alert pulses and animated scrolling are excluded.

## 2. Verified creation path and effective Windows configuration

| Stage | Verified call path / relevant condition | Source |
|---|---|---|
| Web-content request | `ChromeWebContentsViewDelegateViews::BuildMenu` creates `RenderViewContextMenuViews`, calls `Init`; `ShowContextMenu` eventually calls `Show`. | [chrome_web_contents_view_delegate_views.cc][entry]:82–109 |
| Model and toolkit | `RenderViewContextMenuViews` installs `ToolkitDelegateViews`. `RenderViewContextMenuBase::Init` builds the context-specific model then initializes the toolkit delegate. Browser `RenderViewContextMenu::InitMenu` selects page/link/image/edit/media commands. | [render_view_context_menu_views.cc][rv-views]:121–152; [render_view_context_menu_base.cc][rv-base]:219–226; [render_view_context_menu.cc][rv-model]:1268 onward |
| Menu view | `ToolkitDelegateViews::Init` uses `MenuModelAdapter::CreateMenu`; `MenuRunner` receives `HAS_MNEMONICS` and `CONTEXT_MENU`. Adapter recursively transfers types, labels, icons, enabled/visible state and submodels. | [toolkit_delegate_views.cc][toolkit]:22–40; [menu_model_adapter.cc][adapter]:87–140, 341–368 |
| Windows presentation | `Show` requires the top-level widget and a nonempty model, suppresses the menu in kiosk mode, converts the renderer point into screen coordinates and calls `RunMenuAt`. Windows uses this Aura implementation (`assert(use_aura)` in its [BUILD.gn][rv-build]:9). | [rv-views]:405–434 |
| Anchor and controller | Mouse/keyboard request `kTopLeft` (the controller mirrors it to `kTopRight` in RTL); touch/touch-edit use `kBottomCenter`. The target does **not** pass `USE_ASH_SYS_UI_LAYOUT`, combobox or special system-menu layout flags. | [toolkit]:31–40; [menu_runner.cc][runner]:76–94; [menu_runner_impl.cc][runner-impl]:121–214; [controller]:2339–2364 |
| Layout and popup | `MenuController::OpenMenuImpl` selects `CalculateBubbleMenuBounds` because the Windows config enables bubble borders. `SubmenuView` is inside `MenuScrollViewContainer`, hosted by `MenuHost`. | [menu_controller.cc][controller]:2689–2730, 3071–3344 |
| Host and painting | `MenuHost` uses a translucent `Widget::TYPE_MENU`, `ShadowType::kNone` for bubble menus, and **forces software compositing on Windows**. It is parented to the owner and shown inactive with menu capture. Bubble background/shadow and menu item painting are Chromium drawing code. | [menu_host.cc][host]:129–174, 213 onward |

`MenuConfig` applies field initializers, then `InitCommon`, then `InitPlatform`. Reading only the header gives obsolete values for several properties. `InitCommon` unconditionally supplies the modern menu spacing; `menu_config_win.cc` supplies the Windows font, keyboard-cue preference, submenu delay and separator variants. Windows selects `NativeThemeWin` through `NativeTheme::NativeUiTheme`, but the relevant background/separator functions draw to `cc::PaintCanvas`, not a Win32 menu renderer. See [menu_config.cc][config]:22–24, 84–95; [menu_config_win.cc][config-win]:15–28; [native_theme.cc][native]:67–74, 161–168.

### Conditions that can change the reference

| Input / flag | Source default / calculation | Consequence |
|---|---|---|
| `kDesktopGlowUp` | Disabled. [ui_base_features.cc][ui-features]:515 | Enables the rounded-icon helper and menu-simplification helper when turned on. |
| `kRoundedIcons` | Disabled; `IsRoundedIconsEnabled = DesktopGlowUp OR RoundedIcons`. [ui-features]:520, 546–548 | Changes the actual arrow/check/radio vector paths, not the 16-DIP slots. |
| `kMenuSimplification` | Disabled; `IsMenuSimplificationEnabled = DesktopGlowUp OR MenuSimplification`. [ui_features.cc][chrome-features]:65, 92–94 | Changes context-dependent item/icon presence. Some model branches also test the raw flag directly. `AddItemWithOptionalIcon` uses the helper and a 16-DIP default icon. [rv-model]:5937–5948; [simple_menu_model.h][simple-model]:76. |
| `kChromeDarkNeutrals26` | Disabled. [ui-features]:524 | Alternate baseline dark neutral palette (§4). |
| `kAppMenuGlowUp` | Disabled; direct feature test, independent of the `DesktopGlowUp` helper. [chrome-features]:68; [browser-app-menu-button]:99–102 | Selects a different browser-menu implementation. The §6A specification requires this flag off; it does not affect the traced web-content path. |
| `kUseRoundedPointConversion` | Enabled. [gfx/switches.cc][gfx-switches]:65 | Rounded rather than floored screen-point conversions (§6). |
| `kUseGammaContrastRegistrySettings` | Enabled on Windows. [ui-features]:433–434 | Text gamma/contrast use Windows registry settings (§3). |
| Profile, colour source, contrast, OS and locale | Runtime inputs, not established by the Git revision | Affect palette, font, glyph rasterization, menu population and scale. Record them with reference captures. |

The table records **C++ feature defaults, not necessarily the effective defaults of a developer build**. The checked-in test assignments resolve the apparent contradiction:

| Configuration layer | Verified assignment / activation rule | Source |
|---|---|---|
| Windows `GlowUp` study | First experiment `Enabled_20260821` enables `MenuSimplification`, `RoundedIcons`, `TabStripDeclutter`, `ToolbarGlowUp`, `WebuiRefresh2026`. It enables neither `DesktopGlowUp` nor `AppMenuGlowUp`; neither name appears as an assigned feature in this JSON. The rounded-icon and simplification helpers nevertheless return true through their individual flags. | [fieldtrial_testing_config.json][fieldtrials]:9862–9883 |
| Windows `ChromeDarkNeutrals26` study | First experiment `Enabled` enables `ChromeDarkNeutrals26`. | [fieldtrials]:3890–3907 |
| Config included in build | `disable_fieldtrial_testing_config=false` and `force_enable_fieldtrial_testing_config=false` by default. `FIELDTRIAL_TESTING_ENABLED = force_enable OR (!disable AND !(Android AND Chrome-branded))`. | [variations/service/BUILD.gn][fieldtrial-build]:8–31 |
| Config used at startup | When compiled in: Chrome-branded builds require `--enable-field-trial-config` or `--enable-benchmarking=enable-field-trial-config`. Other builds use it by default unless disabled with `--disable-field-trial-config` or `--variations-server-url`; an explicit enabling switch takes precedence. | `ShouldUseFieldTrialTestingConfig`, [variations_field_trial_creator.cc][fieldtrial-creator]:132–156 |
| Assignment selection / overrides | First available experiment after platform filtering is the default for eligible Chromium developer and Chrome for Testing builds and listed browser/performance tests, not unit tests. An experiment touching a feature explicitly overridden by `--enable-features` / `--disable-features` is skipped. | [testing/variations/README.md][fieldtrial-readme]:6–29 |

Thus **eligible developer-config baseline** means rounded assets and DarkNeutrals26 enabled, with the established `AppMenu::ZoomView` still active. **Unmodified C++ feature-default baseline** means those features disabled when no testing/Finch/command-line assignment changes them. Test both explicitly; do not call the latter the unconditional Chromium 154 appearance. `MenuSimplification` affects other menu content but does not remove or reorder `CreateZoomMenu` in the ordinary browser model ([app-model]:2376–2378, 2535–2547).

The actual reference executable's branding, GN args, loaded seed and overrides remain unmeasured because this checkout has no build output. The source-backed test assignments above are known; a user's live Finch assignment is not. The conditional `GetLensContextMenuIcon` and other explicitly supplied icons mean an unsimplified web-content menu is not guaranteed to have no icons ([rv-model]:429, 462–478). Column widths must be derived from the actual model.

These item kinds are used by the target, not merely supported by an unrelated Views menu: media loop/controls are check items ([rv-model]:2509–2511); its spelling-options observer creates language radio items ([spelling_options_submenu_observer.cc][spelling]:49–73, installed by [rv-model]:3167–3175); context-dependent commands can set `SetIsNewFeatureAt` ([rv-model]:3052, 3217, 4794). Their visibility depends on the invocation context and feature configuration.

## 3. Geometry and typography specification

Unless stated otherwise, dimensions below are **integer device-independent pixels (DIP)**, before display scaling. `W` is a menu row/body width, excluding the transparent shadow margin; `H` is that row's height. `F`, `A`, `Cap` are the resolved font-list height, ascent/baseline and cap height. Integer division follows C++ truncation; text widths use a ceiling, not truncation. These are source-derived formulas, not measurements from a screenshot.

| Visual property | Exact value / formula | Conditions and units | Source / symbol |
|---|---|---|---|
| Outer corner radius | **12** | DIP; root and submenus, Windows bubble path. `kMenuRadius`, `kMenuAuxRadius` and `kMenuTouchRadius` currently map to medium-small = 12. Chrome's layout provider does not replace these metrics. | [layout_provider.cc][layout]:216–238; [config]:29–44 |
| Body top/bottom padding | **12 / 12** | DIP; `rounded_menu_vertical_border_size` is unset, so falls back to radius. A highlighted footnote removes the bottom padding. | [menu_scroll_view_container.cc][scroll]:474–527 |
| Body horizontal inset / outer stroke | **0 / none** | DIP; `menu_horizontal_border_size=0`, `use_outer_border=false`. No painted icon-gutter divider. | [config]:88, 95; [scroll]:496, 510–524 |
| Row leading/trailing content padding | **12 + 8 = 20** | DIP on each side before icon/label/arrow adjustments. | [config]:91; [menu_config.h][config-h]:81; [menu_item_view.cc][item]:732–738 |
| Shared icon/check column `C` | `max(checkOrRadio ? 16 : 0, widest ordinary icon)` | DIP, independently per submenu; no forced empty check column. Additional icon on a check/radio row does not enlarge the shared column. | [submenu_view.cc][submenu]:112–175 |
| Minimum icon content height `K` | **16** if any icon/check/radio exists, otherwise **0** | DIP. A taller supplied icon increases its own row's height; it does not set every row's minimum to that taller value. | [submenu]:139–164; [item]:1600–1605 |
| Shared label start `L` | `20 + C + (C > 0 ? 12 : 0)` | DIP; 20 for text-only menu, 48 for a 16-DIP column. | [submenu]:166–171; [layout]:133–134 |
| Row top/bottom margin | **6 / 6** | DIP; no normal fixed minimum height (`minimum_text_item_height=0`). | [config]:90; [config-h]:74–75 |
| Normal row height | `max(n*F, iconHeight, K) + 12`, `n=1` or `2` | DIP; `n=2` only for secondary title. Container/custom-child rows use their own preferred size/margins. | [item]:1468–1584 |
| Inter-row gap | **0** | DIP; separator slots provide their own spacing. | [config-h]:123–126; [submenu]:301–320 |
| Ordinary icon rectangle | `x=20 + (C-iconWidth)/2`; `y=(H-iconHeight)/2` | Integer DIP, then mirrored in RTL. Model supplies icon dimensions; common context-menu icons are 16×16, not a mandatory size for all images. | [item]:841–918, 1780–1822 |
| Check/radio rectangle | **16×16**, `x=20`, `y=(H-16)/2` | DIP; unchecked checkbox leaves the column empty; radio retains its ring. Extra image on this row begins at `L`, and text begins at `L+imageWidth+12`. | [item]:988–994, 1608–1633, 1780–1822 |
| Submenu arrow rectangle | **16×16**, `x=W-12-8-16 = W-36`, `y=(H-16)/2` | DIP; mirrored in RTL. Arrow-to-edge 8 is in addition to the row border padding 12. | [config]:92; [config-h]:97; [item]:841–918 |
| Actionable submenu exception | Arrow edge gap **14**; click-region width **37**; vertical separator **18×1** | DIP; a distinct item type, not every submenu row. | [config-h]:137–147; [item]:841–918 |
| Label preferred width | `ceil(shapedWidth(rawTitle))` | DIP, using the resolved font list. The sizing call does not pass mnemonic-removal flags. | [item]:1519–1522; [canvas.cc][canvas]:106–108, 146–147 |
| Shared minor/shortcut column | `M = max(visible item minorWidth); R = M>0 ? M+8 : 0` | DIP; accelerators and arrows share this area. There is **no dedicated arrow column**. See width algorithm below. | [config]:87; [submenu]:253–294 |
| Normal preferred body width | `max_i(L_i + titleWidth_i + 20) + R` | DIP; usual text/icon rows, no custom child or extra submenu inset. Full formula below. No 256-DIP minimum. | [item]:1520–1527; [submenu]:247–320 |
| Preferred body height | `12 + sum(visible row/separator heights) + 12` | DIP, ordinary menu with no footnote/overflow. | [submenu]:301–320; [scroll]:332–342, 515–527 |
| Width limit | **800** maximum popup width, including outside shadow insets | DIP; delegate default; root also restricted to monitor width plus outside insets. | [menu_delegate.cc][delegate]:149–152; [controller]:3098–3148 |
| Selection | Entire row rectangle; item radius **0** | DIP; same static selected visual for pointer hover/keyboard selection. Ordinary commands have no separate pressed fill. | [config-h]:176–177; [item]:1225–1324; [native]:388–405 |
| Background | Opaque resolved `kColorMenuBackground` inside rounded body | No acrylic/Mica/blur on this Windows path. Translucency is for shadow/corners. | [scroll]:529–551; [bubble_border.cc][bubble]:741–751 |

### Width, columns and vertical proportions

Compute each submenu independently. `SubmenuView::UpdateMenuPartSizes` calculates `C`, `K`, `L` and trailing padding `T=20`. **It scans menu items without filtering visibility**, whereas the preferred width/height passes skip invisible children. A hidden icon or check item can therefore still affect the common column/minimum icon height. `EmptyMenuMenuItem` and non-menu views are excluded from this metric scan ([submenu]:49–57, 112–175, 265–267).

For each ordinary visible item:

1. Calculate `L_i`. Usually this is `L`; an iconless title uses 20; a check/radio item with an additional image uses `L+imageWidth+12`.
2. `standardWidth_i = L_i + ceil(shapedTitleWidth_i) + T`. For extra child views, add their separately calculated width and a 12-DIP text-to-child gap where appropriate. Badges add `8 + GetBadgeSize(...).width()` ([item]:1523–1534; [badge_painter.h][badge-h]:65).
3. Start `minorWidth_i` with the measured accelerator/minor text. Add minor-image width and 8 DIP between minor text and minor image when both exist. If it has a submenu, add another 8 DIP if the minor area was nonempty, then `16+8=24` DIP for an ordinary submenu arrow (`16+14=30` for actionable submenu).
4. Take the maximum minor width across the submenu, then add 8 DIP if nonzero to produce `R`. An arrow on one row therefore reserves this shared space on every ordinary row; shortcuts do not occupy independently sized per-row columns.
5. The complete preferred-width formula is `max(maxComplexWidth, max(maxSimpleWidth + R + submenuInsets.width, minimumPreferredWidth - 2*submenuInsets.width))`. `maxComplexWidth` includes each row's `standardWidth + childrenWidth`, plus non-menu child preferred widths. The normal target has no extra `SubmenuView` inset or set minimum; the bubble padding belongs to its outer container.

Normal rows have 6 DIP above/below content and no gap to the next row. A text-only row is `F+12`; with any check/icon in that submenu it is at least `16+12=28`. **Do not hardcode a universal 28-, 30- or 36-DIP row.** The font and actual item contents determine it. The 36-DIP height / 256–352-DIP widths in `MenuConfig` are for Ash system UI layout, which this Windows path does not activate even merely because the input was touch.

### Font resolution and actual baseline

Windows overrides the common typography font. The default menu font is `gfx::win::GetSystemFont(kMenu)`, not a hardcoded family, CSS size or Quartz's `Font`. No web-content-menu font override was found in the traced toolkit/model path; `GetLabelFontList` remains an extension point for particular models ([adapter]:242–254; [item]:1057–1071).

Resolution is:

1. `SPI_GETNONCLIENTMETRICS` → `NONCLIENTMETRICS.lfMenuFont` ([system_fonts_win.cc][fonts]:144–216).
2. Apply Chrome's registered `AdjustUiFont` callback. Locale resources may override the family and supply `IDS_UI_FONT_SIZE_SCALER/100`; multiply by `GetAccessibilityFontScale() = 1/UwpTextScaleFactor`; divide by the system DPI scale. The default resource values are family `default`, scaler **100**, minimum font size **5**; translations can differ ([app_locale_settings.grd][locale-settings]:173–187). Callback registration: [chrome_browser_main_win.cc][main-win]:549–554; locale calculation: [l10n_util_win.cc][l10n]:62–81; accessibility inverse: [dpi.cc][dpi]:39–40.
3. Scale signed `lfHeight` with `ClampRound`, preserve a nonzero sign as ±1 if rounding produces zero, then clamp its absolute height to the localized minimum and restore the sign. Use `CreateFontIndirect`/font mapping to resolve the family. The Skia font size is `max(1, tmHeight-tmInternalLeading)` from that mapped font. Italic comes from `lfItalic`; weight is `lfWeight`, or normal if zero. Do not add a selected-item bold weight ([fonts]:54–124).
4. In `PlatformFontSkia`, Windows metrics use `A=ceil(-fAscent)`, `F=ceil(fDescent-fAscent)`, `Cap=ceil(fCapHeight)`; no extra line-gap term. A `FontList` combines maximum baseline and maximum below-baseline extent across its fonts ([platform_font_skia.cc][skia-font]:432–468; [font_list_impl.cc][font-list]:219–227). System fonts and `MenuConfig` are cached; do not assume changing an OS setting immediately recomputes an already-running menu configuration.

For a normal label, `yText = 6 + (H-12-n*F)/2`. Draw the first line in `(L_i, yText, W-T-R-L_i, F)`; its baseline is **`yText+A`**. The secondary title starts at `yText+F` and uses the minor colour. Mirror the text rectangle in RTL. This ties padding and baseline to the same metrics as row sizing; centring glyph bounds or using WinForms `TextRenderer` padding produces a different result ([item]:1179–1222).

Minor text uses a different rectangle: `(W-T-R, 6, R-arrowSpan-rightMinorImageSpan, H-12)`, where `arrowSpan` is 24 if an arrow view exists, and image span is image width plus 8 when text is also present. It is right-aligned in LTR and left-aligned in RTL. `RenderText` vertically centres using this exact baseline function, with display height `D`, rather than centring the ink bounding box:

```text
leading = A - Cap
space = D - (leading != 0 ? Cap : F)
shift = integer_divide(space, 2) - leading
baselineWithinRect = A + clamp(shift, min(0, D-F), abs(D-F))
```

For `D=F` this returns `A`; larger icon rows can make the minor baseline differ from the primary one. See [render_text.cc][render-text]:2076–2094 and [item]:1326–1385. Also preserve a subtle alignment detail: this minor-text `RenderText` leaves `cursor_enabled=true`, unlike the main-label canvas helper. For a fitting single line its alignment width is **`ceil(shapedWidth)+1`**, so the LTR right alignment reserves one additional DIP. No cursor is actually painted by this menu call. See [render_text.h][render-text-h]:991–993; [render-text]:1045–1052, 1906–1917; [canvas_skia.cc][canvas-skia]:60–70.

Text is shaped through `RenderText`/HarfBuzz, with font fallback, Unicode/bidi handling and Windows font rendering parameters. Reusing raw `SKPaint.MeasureText` without shaping/fallback is insufficient. Label measurement uses `NO_ELLIPSIS` on the raw title; painting removes mnemonic ampersands only if `SHOW_PREFIX`/`HIDE_PREFIX` is requested, and shows the underline when OS keyboard cues or the menu's explicit cue state requests it. Windows clips/elides long primary labels at the tail. Keep literal `&&`, combining characters, surrogate pairs and RTL cases in the validation fixtures ([item]:1044–1054; [canvas]:146–147; [canvas-skia]:26–57, 160–229).

`font_render_params_win.cc`:77–109 starts with medium hinting, no autohinter/bitmap glyphs, and disabled antialiasing/subpixel positioning. `SPI_GETFONTSMOOTHING` enables AA and subpixel positioning; ClearType additionally chooses the OS pixel geometry. Gamma/contrast come from the registry when the enabled-by-default feature is active, otherwise Skia's compiled defaults. Match these runtime inputs and verify the actual translucent software-surface rasterization; selecting “Segoe UI, 9pt” alone cannot establish a 1:1 result. [font_render_params_win.cc][font-render]; [render-text]:494–509.

## 4. Chromium colour resolution

### Provider and mixer chain

The menu widget inherits the owner widget's colour-provider key ([widget.cc][widget]:2717–2744). `BrowserWidget::GetColorProviderKey` applies profile `ThemeService` configuration and reapplies widget overrides ([browser_widget.cc][browser-widget]:485–502). The native key supplies preferred colour scheme, contrast/forced-colour information, accent/seed and related settings ([native]:347–365). Windows app light/dark preference comes from `AppsUseLightTheme` ([os_settings_provider_win.cc][os-settings]:205–210).

`ThemeService::GetColorProviderKey` ([theme]:718–791) can override this: incognito selects dark+grayscale; enterprise-isolated profiles light+baseline; browser colour-scheme preferences can override system mode. Policy, autogenerated, user and device themes supply seeds/scheme variants; custom theme data can participate. Consequently there is no single unconditional “Chrome dark colour” independent of profile and OS state.

UI mixer order is **Ref → Sys → CoreDefault → NativeCore → UI → MaterialUI → optional Fluent UI → NativeUI → CSS-system → NativePostprocessing** ([color_mixers.cc][mixers]:24–44). The material mixer overwrites earlier defaults. In particular, the `#292A2D` primary background in `CoreDefault` is **not** the final normal baseline menu background. The traced Chrome-specific menu path adds no replacement menu palette; use the owner provider, not Quartz's theme renderer.

### Normal-contrast baseline palette

These exact sRGB values apply with a baseline colour source, no custom overriding provider, and `ChromeDarkNeutrals26` disabled. “Derived” entries below were calculated from the checkout's colour recipes, not sampled from an image. `#RRGGBB @ alpha` denotes an uncomposited colour/8-bit alpha.

| Property / final colour ID | Light | Dark | Resolution and condition |
|---|---|---|---|
| Body `kColorMenuBackground` | **#FFFFFF** | **#1F1F1F** | MenuBackground → PrimaryBackground → SysSurface → Neutral100 / Neutral10. |
| Ordinary label `kColorMenuItemForeground` | **#1F1F1F** | **#E3E3E3** | SysOnSurface → Neutral10 / Neutral90. |
| Selected label and selected minor text | **#1F1F1F** | **#E3E3E3** | MenuItemForegroundSelected → MenuItemForeground. |
| Unselected accelerator / minor label | **#474747** | **#C7C7C7** | MenuItemForegroundSecondary → SysOnSurfaceSubtle → Neutral30 / Neutral80. |
| Explicit `kColorMenuIcon` | **#474747** | **#C7C7C7** | Same subtle token; applies to models explicitly using this ID. |
| Selection / hover background | **#F2F2F2** | **#363635** | Derived: light Neutral10 @ **0x0F** over white; dark Neutral99 (**#FDFCFB**) @ **0x1A** over Neutral10. Final fill is opaque. |
| Normal separator `kColorMenuSeparator` | **#D3E3FD** | **#5E5E5E** | Separator → SysDivider → Primary90 / Neutral40. Grayscale changes the light value below. |
| Disabled primary and minor text | **#5F6368** | **#9AA0A6** | Derived: MenuItemForegroundDisabled → DisabledForeground → `PickGoogleColor(GoogleGrey600, final PrimaryBackground, 4.5)`. This is not disabled-icon alpha. |
| Disabled supplied vector icon | **#1F1F1F @ 0x60** | **#E3E3E3 @ 0x60** | MenuIconDisabled → SysStateDisabled. Composite over the actual row background. |
| Ordinary check/arrow/default-derived icon | **#626262** | **#A9A9AA** | Derived by `DeriveDefaultIconColor(resolved main text colour)`; this is distinct from explicit MenuIcon. |
| Checked radio | **#0B57D0** | **#A8C7FA** | RadioButtonForegroundChecked → SysPrimary → Primary40 / Primary80. |
| Unchecked radio | **#747775** | **#8E918F** | RadioButtonForegroundUnchecked → SysOutline → NeutralVariant50 / 60. |
| Shadow base RGB | **#3C4043** | **#000000** | `kColorShadowBase`; individual shadow alpha/geometry in §5. |

Recipe sources: [ui_color_mixer.cc][ui-colors]:136–156, 192–208; [material_ui_color_mixer.cc][material-colors]:114–140; [sys_color_mixer.cc][sys-colors]:155–160, 179–180, 198–220, 252–277; [ref_color_mixer.cc][ref-colors]:30–43, 137–175; disabled text [core_default_color_mixer.cc][core-colors]:43–45 and [color_utils.cc][color-utils]:141 onward. Google grey constants are in [color_palette.h][palette].

For an opaque background, the hover recipe's channel calculation is `ClampRound((alpha*foreground + (255-alpha)*background)/255)`, with opaque output alpha ([color-utils]:581–614). Default icon derivation blends the text colour toward the maximum-contrast endpoint by **0x4C**; the endpoints are white or GoogleGrey900 **#202124**, selected by relative luminance ([color-utils]:35–40, 617–633, 687–690). Recompute this after resolving disabled/selected/custom text colour; do not apply a global icon opacity.

`MenuItemView::GetTextColor` prioritizes custom overrides, then highlighted type, disabled, selected, minor and ordinary styles in that order ([item]:1390–1433). Disabled style wins over selected style for ordinary disabled items. Supplied vector icons use their explicit colour in normal state; disabled switches to MenuIconDisabled; selection switches an unset/default MenuIcon colour to the derived icon colour. Other explicitly coloured icons remain explicit. Bitmap icons do not pass through this vector recolouring branch. Radio glyphs select checked/unchecked IDs without a separate disabled branch in this implementation ([item]:1696–1763).

### Other resolved configurations

| Configuration | Exact change / calculation | Source |
|---|---|---|
| Grayscale | Fixed baseline reference palette; light divider becomes **Neutral90 #E3E3E3**, dark remains Neutral40. Other menu tokens above, including the checked-radio primary, remain unchanged by `AddGrayscaleSysColorOverrides`. | [sys-colors]:75–110; [ref-colors]:302–305 |
| `ChromeDarkNeutrals26` enabled, baseline/grayscale dark | Background **#191B1F**; primary **#DADBE5**; minor/explicit MenuIcon **#C2C3CC**; separator **#4E5059**; hover uses **#F9FAFC @ 0x1A**, producing **#303236**; derived ordinary check/arrow **#A3A4AB**; disabled text recipe now selects **#80868B**. | [ref-colors]:93–136; same recipes as above |
| Seeded/accent theme | If seed exists and source is neither baseline nor grayscale, call `GeneratePalette(seed, schemeVariant or TonalSpot)`, then each reference colour is its tonal palette's `.get(tone)`. A black seed is changed to **#010101**. TonalSpot uses primary chroma40, secondary16, tertiary hue+60/chroma24, neutral6, neutral-variant8. Use the checkout's HCT/palette implementation and selected variant, not an RGB tint approximation. | [ref-colors]:296–321; [palette_factory.cc][palette-factory], `GeneratePalette` |
| Themed system-token overrides | Nonbaseline theme surface is Neutral99 in light / Neutral10 in dark; divider is Primary90 in light / Secondary35 in dark. Hover and disabled recipes evaluate against the resulting final background. | [sys-colors]:20–39, final override dispatch |
| Windows high contrast | NativeUI runs when contrast mode is not normal. Menu background → **COLOR_BTNFACE**; ordinary/minor text and separator → **COLOR_BTNTEXT**; disabled text → **COLOR_GRAYTEXT**; selected background → **COLOR_HIGHLIGHT**; selected text → **COLOR_HIGHLIGHTTEXT**; highlighted text → **COLOR_HOTLIGHT**. Other icon/radio IDs must also resolve through native mixers. No fixed hex values are valid across contrast themes. | [native_color_mixers_win.cc][native-colors]:136–233 |

The palette is a resolved input to the future renderer. Quartz may select a light/dark mode, but must not supply its White/Dark/Black/Aqua/Xmas menu colours as Chromium equivalents. For a reproducible first comparison, explicitly use a normal-contrast baseline Chromium profile and record all feature flags; test grayscale, seeded and high-contrast variants separately.

## 5. Separators, glyphs, shadows and interaction states

### Separators

`MenuSeparator` establishes a slot and a paint rectangle. **On Windows, `NativeThemeWin::PaintMenuSeparator` draws a line through the rectangle's centre using default stroke width zero**, i.e. a Skia device-pixel hairline. The configured “thickness” is not necessarily the resulting device-pixel thickness. The paint flags do not enable antialiasing. See [menu_separator.cc][separator]:33–111; [native_theme_win.cc][native-win]:384–402; [paint_flags.h][paint-flags]:193 and `CorePaintFlags` defaults.

| Type | Slot height (DIP) | Paint rectangle / line position within slot | Horizontal extent on target path |
|---|---:|---|---|
| Normal | **17** | Rectangle `y=8,h=1`; line centre **8.5 DIP** | `x=0` to `W` |
| Upper | **5** | Rectangle `y=0,h=1`; centre **0.5 DIP** | `0` to `W` |
| Lower | **7** | Rectangle `y=6,h=1`; centre **6.5 DIP** | `0` to `W` |
| Double | **18** | Rectangle `y=8,h=2`; centre **9 DIP**; Windows still draws one hairline | `0` to `W` |
| Spacing | **4** | No paint | None |
| Padded | **1** | Rectangle `y=0,h=1`; centre **0.5 DIP** | `64` to `W`; source explicitly insets left, without RTL mirroring here |

The normal separator occupies 17 DIP between adjacent row slots; do not add another 8-DIP gap around a 17-DIP separator. It spans the body, including the icon column, because horizontal padding and outer stroke are disabled. Do not replace it with a filled 1-DIP rectangle that becomes two physical pixels at 200%.

### Vector glyphs

These are vector paths, not text glyphs from a symbol font. The source-default (rounded-icon feature off) arrow is [submenu_arrow_chrome_refresh_old.icon][arrow-old], a 16-DIP canvas with `FLIPS_IN_RTL`. The checkmark is [menu_check_old.icon][check-old], with a 16-DIP representation as well as a larger representation. Preserve the source path and fill behavior when translating to Skia.

Old radio assets are [menu_radio_empty_old.icon][radio-empty] and [menu_radio_selected_old.icon][radio-selected]: 32-unit source canvas, centre `(16,16)`, ring radii 14/11 and selected inner radius 7. They render in a 16-DIP slot: centre `(8,8)`, ring radii 7/5.5 and inner dot 3.5. The unchecked checkbox renders no mark but keeps the reserved column. With rounded icons enabled, select `kKeyboardArrowRightFlippableIcon`, `kCheckIcon`, `kRadioButtonCheckedIcon` and `kCircleIcon` instead; their selection is explicit in [item]:1702–1726. Colour behavior is in §4. Application icons/favicons remain image content, not substitutions with those control glyphs.

### Optional “New” badges

Only render a badge when the model requests it; do not add one to Quartz commands by default. The Views implementation uses `TypographyProvider(CONTEXT_BADGE, STYLE_SECONDARY)`, **not** the menu font reduced by the unused `kBadgeFontSizeAdjustment=-1` constant. Chrome requests target size **9** through `PlatformFont::GetFontSizeDelta(9)` and **bold** weight; the resource-bundle/base font and OS/locale adjustments still determine the resulting font. See [badge_painter.cc][badge]:40–85 and [chrome_typography_provider.cc][chrome-typography]:64–85.

The badge begins 8 DIP after measured primary text. Its text is inset horizontally by 4 DIP; radius is **4 DIP** (`kBadgeRadius`). With badge metrics `Ab,Fb,Capb`, visual padding above is `max(0,4-(Ab-Capb))`, below `max(0,4-(Fb-Ab))`, and left/right 4. Badge text top relative to primary text top is `(A-Cap) + ClampRound((Cap-Capb)/2.0) - (Ab-Capb)`. Thus badge width is `ceil(shapedBadgeTextWidth)+8`, and height is `Fb+topPadding+bottomPadding`; it does not independently enlarge the ordinary row-height formula. These helpers are in [text_utils.cc][text-utils]:187–212; paint and size calculations in [badge]:29–85. Body/foreground resolve through `kColorBadgeBackground → SysTonalContainer` and `kColorBadgeForeground → SysOnTonalContainer`: baseline light **#D3E3FD / #041E49**, dark **#004A77 / #C2E7FF**, subject to the same themed/native token overrides. Sources: [material-colors]:40–41; [sys-colors]:205–208; [ref-colors]:31, 40, 51, 58.

### Body and shadow geometry

The Windows bubble border supplies **both** the rounded body and the shadow; `MenuHost` disables the native window shadow. `use_outer_border=false` removes the border stroke even though a MenuBorder colour ID exists. Shadow RGB is light **#3C4043** / dark **#000000** under the baseline normal-contrast provider.

| Popup | Shadow component | Offset x,y (DIP) | `ShadowValue.blur` (DIP) | Alpha |
|---|---|---|---:|---:|
| Root, elevation **12** | Key | `(0,12)` | **48** | **0x3D** |
| Root, elevation **12** | Ambient | `(0,0)` | **24** | **0x1F** |
| Submenu, elevation **16** | Key | `(0,0)` | **32** | **0x1A** |
| Submenu, elevation **16** | Ambient | `(0,12)` | **32** | **0x3D** |

The root's elevation 12 is **not** the named elevation-twelve colour recipe elsewhere in the UI mixer: `BubbleBorder::ShadowElevationToColorsMap` only specializes standard elevations 3 and 16. Elevation 12 falls back to `MakeMdShadowValues`, which sets the two alphas above. This distinction materially changes the result. Sources: [scroll]:491–495; [bubble]:54–87, 136–209; [shadow_value.cc][shadow]:74–128; [ui-colors]:192–208.

For each component, convert `radius=blur/2` to Skia sigma using **`sigma = radius>0 ? 0.288675f*radius + 0.5f : 0`**. Do not pass the table's blur directly as sigma. `CreateShadowDrawLooper` inserts the source content and both shadow layers with alpha override; preserve its layer composition. `BubbleBorder` applies an antialiased difference clip excluding the rounded client body before drawing its shadow, and `BubbleBackground` fills that body separately. Sources: [skia_paint_util.cc][skia-paint]:75–97; [bubble]:446–458, 741–751.

Shadow outside margins derive from the maximum component spread and offset (`round(blur/2)` plus/minus x/y), not the Gaussian's infinite tail. The resulting **top,left,bottom,right** margins are:

- Root: **(12,24,36,24)** DIP. Preferred host size is `bodyWidth+48` by `bodyHeight+48`.
- Submenu: **(16,16,28,16)** DIP. Preferred host size is `bodyWidth+32` by `bodyHeight+44`.

See `ShadowValue::GetMargin` ([shadow]:27–65) and `BubbleBorder::GetInsets`. Host size, visible body, row content and hit-testing must be distinct rectangles. Adding DWM's shadow or clipping to the body before allocating the host would invalidate both appearance and placement.

### Interaction and state painting

Ordinary rows repaint a static full-width selected background; pointer-down does not introduce a separate pressed colour. Keyboard selection uses the same visual. Parent submenu items retain the selected relationship while navigating their children. Check/radio state comes from the menu model/delegate rather than local paint state. A disabled row does not execute and normal pointer hit testing excludes it; accessibility traversal can still expose disabled rows, whose disabled paint style has priority ([item]:958–961, 1225–1433; [controller]:2506–2515).

Preserve these controller behaviors when implementing the isolated menu session:

- Select on pointer movement; open submenus after Windows `SPI_GETMENUSHOWDELAY` (config fallback **400 ms**). This timer is an interaction delay, not an animation.
- Execute an enabled leaf on the accepted mouse release, then dismiss as appropriate. Chromium suppresses an opening release when elapsed time is under **200 ms** and movement is below **4 DIP**; port the corresponding distance/condition rather than triggering the row underneath the initial click ([controller]:192, 208, 1072–1133).
- Up/Down navigate with wrap; Home/End go to endpoints; Enter activates/opens; forward/back arrow semantics mirror in RTL; Escape closes the current level or session. Windows Alt/F10 cancel. Space is not ordinary menu activation on this path (it is handled for a hot-tracked embedded button). Mnemonics honor enabled items and the Windows cue setting. See [controller]:2033–2160 and [config-h]:128–132, 203–210.
- Handle capture loss, outside clicks, owner destruction and focus restoration for the whole root/submenu chain. Keep pointer and keyboard selection synchronized; do not make an ordinary menu item a tab-stop WinForms button.

Chromium also supports highlighted footnotes, badges, custom child views and actionable submenu split regions. They are exceptions to the ordinary formulas, not styles to apply universally. A highlighted footnote has vertical item margin **11 DIP** and removes the container's bottom padding ([item]:422; [config-h]:213; [scroll]:525–526). A container uses the child's preferred margins, with top/bottom at least the normal vertical margin ([item]:1657–1668). The separate browser-menu Zoom row has the compound-view specification in §6A. Implement no fade, pulse, slide or fling animation, even where the Chromium/general Views code or Quartz's old menu classes offers one.

## 6. Nested placement, overflow and display scaling

### Screen placement

The active routine is `CalculateBubbleMenuBounds`, not the older nonbubble routine. The monitor is selected near the invocation anchor; use its work area unless the invocation point is outside the work area but inside that display's full bounds ([controller]:2316–2334). All calculations here are screen **DIP**, including possibly negative monitor origins.

Let `B` be the outside shadow margins above and `SW,SH` the host dimensions including them. The root first limits `SW` to the delegate's 800 DIP and the monitor width plus `B.left+B.right`, and `SH` to monitor height plus `B.top+B.bottom`. For a mouse invocation at `(ax,ay)`, the candidates are:

```text
rightCandidate = ax - B.left
leftCandidate  = ax - SW + B.right
belowCandidate = ay - B.top
aboveCandidate = ay - SH + B.bottom
```

For the LTR `kTopLeft` anchor, prefer the right candidate if the visible body fits to the monitor's right; otherwise use the left. RTL converts it to `kTopRight`, preferring the left candidate if that body's left edge fits, otherwise the right ([controller]:2339–2364, 3198–3217). Prefer below if it fits, except that a previously chosen above placement is retained when it still fits during resizing. Otherwise choose above; if neither fits, bottom-align the available menu. Clamp host origin to:

```text
x: [monitor.left - B.left, monitor.right - SW + B.right]
y: [monitor.top  - B.top,  monitor.bottom - SH + B.bottom]
```

The visible body fits the monitor; transparent shadow may extend outside it. Touch `kBottomCenter` uses the corresponding half-width candidate in the same routine; it does not turn on Ash row metrics. See [controller]:3071–3280.

A child submenu anchors to the parent item's screen rectangle with anchor height forcibly set to **1 DIP**. Normally it opens right in LTR, left in RTL, continuing the preferred menu-open direction:

```text
rightCandidate = parentRow.right - B.left       // overlap is 0
leftCandidate  = parentRow.left - (SW-B.right)
yCandidate     = parentRow.top - B.top - 12
```

Thus the **first child row**, after its top body padding, aligns with the parent row's top. Visible parent/child body edges abut; shadows overlap. Try the preferred side, then the other, otherwise flush to the applicable screen edge; clamp the y origin and cap child height to monitor height plus shadow margins. The child branch has no additional root-style width cap beyond the delegate limit, which matters on an extremely narrow display. Port the source's direction state, not merely a symmetric guess: line 3304 retains the preferred direction on the right-to-left fallback, while the opposite branch changes it. See [controller]:3281–3344.

### Overflow

Over-tall menus use a clipped scrolling viewport with up/down **menu scroll rows**, not a standard vertical scrollbar. Each scroll row's preferred height comes from the submenu's ordinary preferred item height. The painted triangle uses `scroll_arrow_height=3` DIP, with x coordinates centre±3 (the control's preferred arrow width is `2*3-1=5`). Up is initially hidden; down is visible when content exceeds the viewport. Wheel scrolling advances by menu item boundaries; continuous scroll offsets are clamped and fractional residue is accumulated with `ClampRound`. Sources: [scroll]:72–142, 380–399; [submenu]:102–105, 389–443, 779–805. Preserve reachability of all Quartz entries without importing animated fling behavior.

### DPI and rounding contract

| Layer | Source calculation | Implementation consequence |
|---|---|---|
| Windows display scale | Effective per-monitor DPI / **96**, multiplied by the UWP accessibility text scale when `include_accessibility=true`. A forced device scale overrides this and uses text multiplier 1; failed monitor query falls back to global scale. | Text scaling here scales the Views UI geometry as well. Do not scale the font a second time after normalizing `lfMenuFont`. [screen_win.cc][screen]:61–105; [dpi]:39–40. |
| Logical screen space | Translate relative to the chosen monitor's pixel/DIP origins, then scale. | Do not multiply a global screen point by a single scale across differently scaled monitors. [screen]:590–619. |
| Point conversion | Rounded point conversion enabled by default; floored path remains behind its feature condition. | Reproduce the enabled path and record overrides. [screen]:733–765; [gfx-switches]:65. |
| Rectangle/window extents | `ScaleToEnclosingRect`; integer sizes use ceiling as appropriate. Windows desktop host uses `DIPToScreenRect`. | Preserve origin conversion separately from extent conversion; do not round width and screen origin with one blanket rule. [screen]:733–787; [desktop_window_tree_host_win.cc][desktop-host]:280. |
| Layout and font metrics | Integer-DIP layout; shaped advance ceiling; metric ceilings; integer division in centring; `ClampRound` for font normalization. | Retain calculations in DIP until the correct conversion boundary. C# midpoint-to-even rounding is not a blanket substitute for C++ rounding. |
| Views paint recording | Pixel-canvas child bounds round **edges** (`round(x*s)`, `round((x+w)*s)`) and share the parent's last edge. Root uses enclosing bounds. `kUniformScaling` and `kScaleWithEdgeSnapping` choose uniform scale versus snapped extent/size. | Avoid cumulative rounding of each row's height. Check the actual paint-context/scale mode during pixel comparison. [paint_info.cc][paint-info]:20–45, 100–144. |
| Hairlines and text clipping | Separator stroke zero stays a device-pixel hairline; text paint allows a 0.5-DIP clip outset at fractional scale. | Do not treat every visible stroke as a 1-DIP filled shape or clip glyph fringes to rounded integer ink bounds. [native-win]:384–402; [canvas-skia]:160–169. |

Quartz targets .NET Framework **4.8.1** and already references SkiaSharp (application package **4.152.1**, EasyTabs **4.152.0**; no package change is needed for this research). The checked-in [app.manifest][q-manifest], [App.config][q-app-config] and startup code do not declare per-monitor DPI awareness. [ChromiumTabRenderer.cs][q-tab-renderer]:244–246 explicitly accounts for the possibility of Windows DPI virtualization (`DeviceDpi=96`). This is evidence of a compatibility constraint, **not a measurement of the running process's awareness**. Verify the actual process/thread/window DPI context before claiming a match at non-100% scales. Do not silently change application-wide DPI awareness as part of an isolated menu replacement.

## 6A. Browser-menu Zoom/fullscreen row — replacement specification

This section supersedes the provisional row in `Quartz/Browser.ZoomMenu.cs`. Its measurements and behavior are **not** requirements. The reference here is the ordinary Windows browser's three-dot **app menu**, with `AppMenuGlowUp` **off**. It is not `RenderViewContextMenuViews`, the location-bar zoom bubble, a PWA menu, or WebView2's built-in context menu. Keep the row in Quartz's existing owned menu position; do not import Chrome's surrounding command list.

### Creation and applicable implementation

| Stage | Verified path and consequence | Source / symbol |
|---|---|---|
| Open browser menu | `BrowserAppMenuButton::ShowMenu` constructs `AppMenuModel`; `AppMenuButton::RunMenu` initializes it, constructs `AppMenu`, then runs it. | [browser_app_menu_button.cc][browser-app-menu-button]:83–112; [app_menu_button.cc][app-menu-button]:109–125 |
| Populate row | `Init → Build → CreateZoomMenu`. Normal separators immediately precede/follow the row. `ButtonMenuItemModel` entries are index **0 minus**, **1 plus**, **2 fullscreen**; the percentage is not a command entry. `AddButtonItem(kZoomMenuPlaceholder, ...)` and `SetCommandIcon` add the row and its magnifier icon. | [app_menu_model.cc][app-model]:1366–1367, 2376–2378, 2535–2547 |
| Build Views hierarchy | Adapter maps `TYPE_BUTTON_ITEM` to `MenuItemView::kNormal` and transfers its icon. `AppMenu::PopulateMenu` changes the title to `IDS_ZOOM_MENU2`, attaches one `ZoomView`, and sets `children_use_full_width=true`. `ZoomView` constructs minus, percentage `Label`, plus, fullscreen in that order. | [adapter]:87–137; [app_menu.cc][app-menu]:736–838, 1686–1694 |
| Host | Root `MenuRunner` uses `HAS_MNEMONICS`, optional keyboard flags, **no `CONTEXT_MENU` or Ash-layout flag**. Anchor is the app-menu button's screen bounds, `kTopRight` (mirrored for RTL). It shares the Windows popup/body/shadow infrastructure in §§2–6, but uses this button anchor instead of the web-content click point. | [app-menu]:1094–1120, 1134–1159 |
| Excluded alternate implementation | A raw `kAppMenuGlowUp` test selects `RunActionMenu → ActionAppMenu → ActionAppMenuZoomView`. The alternative uses horizontal `BoxLayout`, action relationships and a separate **16-DIP-long** vertical `views::Separator`; it does not establish the geometry below. Neither the C++ default nor the eligible `GlowUp` test assignment enables this flag (§2). If a reference binary explicitly enables it, use that implementation as a separate research target rather than mixing its constants into this row. | [chrome-features]:68; [browser-app-menu-button]:99–102; [app-menu-button]:128–139; [action_app_menu.cc][action-menu]:168–174; [action_app_menu_zoom_view.cc][action-zoom]:33–88, 118–141 |

`MenuSimplification` does not gate this row in `AppMenuModel::Build`; `RoundedIcons` selects its vector family, and `ChromeDarkNeutrals26` changes its dark neutral colours. The Windows constants are in the **non-ChromeOS** branch: fullscreen padding 38 and percentage padding 2, not ChromeOS's 74 and 15 ([app-menu]:140–158). Model comments mentioning “reset” are stale descriptions: the constructor and command entries above contain no reset button.

### Layout and text specification

All geometry below is integer **DIP**, in LTR before Views mirroring. `W,H,F,A,Cap,C,L,T` have the meanings in §3; `T=20`. Define `P(s)=ceil(shaped width of s in MenuConfig.font_list)`, `Q` as the percentage-column width, and `G` as the whole trailing control group's width. Do not measure with WinForms `TextRenderer`, a fixed string-width estimate, or Quartz's existing button sizes.

| Visual property | Exact value / formula | Conditions / units | Source / symbol |
|---|---|---|---|
| Visible order | Leading magnifier; **Zoom** title; **minus → percentage label → plus → vertical divider/fullscreen** | LTR; mirror layout in RTL. Percentage has no click action. | `CreateZoomMenu`, [app-model]:2535–2547; `ZoomView`, [app-menu]:759–832 |
| Minus/plus glyph and preferred size | Glyph **16×16**; empty border `(top=0,left=15,bottom=0,right=15)`; preferred size **46×16** | DIP; both original asset families have 16-unit canvases. Width is max of the two preferred widths. | [app-menu]:141, 384–403, 845–849; `ImageButton::CalculatePreferredSize`, [image-button]:140–151 |
| Fullscreen preferred/allocated size | Preferred **16×16**; allocated width **16+38=54** | DIP; this subclass has no padding border. Its size override adds its zero insets; do not give it the minus/plus 15-DIP border. | `FullscreenButton`, [app-menu]:597–646, 850–857 |
| Percentage column `Q` | `4 + max(P(FormatPercent(round(f*100))))` over `f` returned by `PageZoom::PresetZoomFactors(ZoomController::GetZoomPercent())` | DIP; two-DIP inset on each side. No active contents: `4+P(FormatPercent(100))`. Cached, invalidated on zoom change. | [app-menu]:775–780, 920–921, 942–968 |
| Group preferred width `G` | `46 + Q + 46 + 54 = 146+Q`; reported preferred height **0** | DIP; actual height comes from the parent row. | `ZoomView::CalculatePreferredSize`, [app-menu]:845–857 |
| Parent width contribution | `complexWidth = L + P(Zoom title) + T + 12 + G` | DIP; extra 12 is `DISTANCE_RELATED_LABEL_HORIZONTAL`. Global menu width is §3's max of simple/complex rows, not a fixed Zoom-row width. | [item]:1519–1527; [layout]:133–134; [submenu]:247–320 |
| Group placement | Parent-local `(W-G, 0, G, H)` | Right-aligned to the body edge because `children_use_full_width=true`; **no additional trailing 20** on this child. The width calculation still reserves `T+12` between title's measured end and group when this row determines width. | [item]:841–885; [app-menu]:1691–1693 |
| Child bounds within group | Minus `(0,0,46,H)`; percentage `(46,0,Q,H)`; plus `(46+Q,0,46,H)`; fullscreen `(92+Q,0,54,H)` | DIP; all occupy the full row height. These are not four equally sized buttons. | `ZoomView::Layout`, [app-menu]:860–883 |
| Row height | `H=max(F,16,K)+12`, with `K=16` on this icon-bearing menu | DIP; no Zoom-specific vertical-margin override in `PopulateMenu`; at least 28, not universally 28. `GetChildPreferredSize` takes height from the row's 16-DIP magnifier, not the group's preferred height 0. | [item]:1468–1506, 1553–1583; [app-menu]:1686–1694 |
| Leading icon/title placement | Icon `(20+(C-16)/2, (H-16)/2,16,16)`; title starts at `L=20+C+12` | DIP; `C` is shared across this **entire app menu**, so a larger profile/other icon can move this row's title too. `L=48` only when `C=16`. | [item]:898–904, 1780–1822; [submenu]:112–175 |
| Button glyph positioning | Minus/plus local x **15**; fullscreen local x **19**; all y `(H-16)/2` | DIP; integer division, unchanged between normal/hover/pressed. No pressed translation. | `ComputeImagePaintPosition`, [image-button]:289–317 |
| Painted button discs | Size **28×28**, radius **14**, clamped and integer-centred in available bounds | DIP; minus/plus local x **9**. Fullscreen first removes 1 DIP at its leading edge, so disc x `1+(53-28)/2 = 13`. y `(H-28)/2`. | `InMenuButtonBackground::Paint/DrawBackground`, [app-menu]:258–314; `Rect::ToCenteredSize/ClampToCenteredSize`, [rect]:261–270 |
| Fullscreen divider | Local rect `(0,0,1,H)`; Windows line at **x=0.5**, y 0 through H | Coordinates DIP; stroke width 0 yields a **device-pixel hairline**, not a filled 1-DIP strip. Retained when fullscreen button is disabled. | [app-menu]:261–278; `NativeThemeWin::PaintMenuSeparator`, [native-win]:384–402 |
| Surrounding separators | Ordinary **17-DIP** slots, hairline at slot y 8.5 | Same Windows source as §5; do not add extra vertical margins to this row. | [app-model]:2376–2378; [separator]:33–111 |

**Width calculation detail:** the call at [app-menu]:955–956 literally supplies an integer percentage to an API named `PresetZoomFactors`, whose argument is a factor. Its implementation only inserts a custom value strictly between 0.25 and 5, so normal percentages 25–500 add nothing; the measured set is the preset list below. Preserve this checkout's calculation in the reference specification, rather than silently changing the argument to `percent/100`. This is a verified source inconsistency, not an unresolved dimension. The result still depends on font and locale, so no fixed `Q` is justified ([page-zoom]:28–63).

**Typography:** both the title and percentage use the Windows menu font resolved in §3, with its actual family, size, weight and fallback. No extra bolding or digit font is applied. The title's first baseline is `6+(H-12-F)/2+A`. The percentage `Label` instead centres text in its **full-height** content rectangle `(2,0,Q-4,H)` using `RenderText::DetermineBaselineCenteringText(H,font_list)`, i.e. the §3 baseline function with `D=H`. Do not force it onto the title baseline. In LTR its text origin is `Q-2-ceil(shapedWidth)`; add the label/group origins above. `Label::SetHorizontalAlignment(ALIGN_RIGHT)` flips to left alignment in RTL. Its cursor is disabled, so it has **no extra one-DIP caret reservation** (unlike the ordinary accelerator column). Auto colour readability is explicitly disabled and text colour is `kColorMenuItemForeground`. Sources: [app-menu]:772–780; [label]:361–367, 863–876, 1348–1350, 1376–1390; [render-text]:1045–1063, 1906–1933, 2076–2094; [render-text-h]:970.

Display percent is `int(ZoomLevelToZoomFactor(level)*100+0.5)`, formatted with ICU `FormatPercent`, including locale-specific digits, spacing, percent-sign placement and bidi behavior. It is not a concatenated ASCII `"%"` string ([zoom-controller]:194–198; [number-format]:99–102). The width pass uses `round(f*100)` on positive preset factors; that produces the same displayed integer percentages for those factors.

### Original vector assets and painting

**Use the original paths from this checkout in the final implementation.** Port their path commands, coordinates, contour order and fill rules to Skia, retaining Chromium attribution. Do not substitute Unicode minus/plus/fullscreen characters, icon-font glyphs, hand-redrawn approximations, or Quartz's provisional fullscreen bitmap.

| Role | Rounded-icon helper false | Helper true (eligible developer configuration) | Rendered size |
|---|---|---|---|
| Row magnifier | `kZoomInOldIcon`, [zoom_in_old.icon][zoom-old]:5–43 | `kZoomInIcon`, [zoom_in.icon][zoom-icon]:5–58 | **16 DIP** explicitly passed by `SetCommandIcon`, although the rounded asset canvas is 20. Scale that path by 16/20. |
| Minus | `kZoomMinusMenuRefreshOldIcon`, [zoom_minus_menu_refresh_old.icon][minus-old]:5–10 | `kRemoveIcon`, [remove.icon][remove-icon]:5–17 | **16 DIP**, intrinsic 16-unit canvas |
| Plus | `kZoomPlusMenuRefreshOldIcon`, [zoom_plus_menu_refresh_old.icon][plus-old]:5–18 | `kAddIcon`, [add.icon][add-icon]:5–31 | **16 DIP**, intrinsic 16-unit canvas |
| Fullscreen | `kFullscreenRefreshOldIcon`, [fullscreen_refresh_old.icon][fullscreen-old]:5–34 | `kFullscreenIcon`, [fullscreen.icon][fullscreen-icon]:5–66 | **16 DIP**, intrinsic 16-unit canvas; **same glyph when entering/exiting fullscreen** |

All eight files are under `chrome/app/vector_icons`. Asset selection is [app-menu]:768–770, 804–827 and [app-model]:1094–1101, 2544–2546. The vector painter defaults to antialiased even-odd paths, handles `FILL_RULE_NONZERO` as winding, and scales by requested DIP size divided by source canvas size ([vector-paint]:139–166, 350–365, 467–469). Retain those differences: the old minus/plus files omit a nonzero-fill command; the other listed files include it.

Paint the menu body, row title/magnifier, each button background, then its image, and the percentage text through their respective geometry. The button background's `cc::PaintFlags` leaves **antialiasing off** and fill/SrcOver at defaults; `Canvas::DrawRoundRect` forwards those flags unchanged ([app-menu]:308–314; [paint-flags-cc]:35–43; [canvas]:326–336). This differs from the antialiased control glyph paths. There is no button outline or extra shadow. The ordinary full-row selected fill is suppressed because this item has non-icon children and `highlight_when_selected_with_child_views=false` ([item]:1766–1771; [item-h]:777). Only the active control disc changes state; the title, magnifier and percentage retain their ordinary colours.

### Exact colours and states

These are normal-contrast **baseline** colour-provider results. Dark26 columns distinguish the two configurations in §2. The button discs are a different token from both the menu body and ordinary row selection.

| Painted element / state | Light | Dark, Dark26 off | Dark, Dark26 on | Resolution |
|---|---|---|---|---|
| Row/body | #FFFFFF | #1F1F1F | #191B1F | `kColorMenuBackground`, §4 |
| Title / percentage | #1F1F1F | #E3E3E3 | #DADBE5 | `kColorMenuItemForeground → SysOnSurface` |
| Leading magnifier | #474747 | #C7C7C7 | #C2C3CC | Explicit `kColorMenuIcon → SysOnSurfaceSubtle` |
| Normal button disc | **#F2F2F2** | **#282828** | **#22242A** | `kColorMenuButtonBackground → SysNeutralContainer → Neutral95 / Neutral15` |
| Hover, keyboard hot-track, pressed disc | **#E6E6E6** | **#3E3E3E** | **#383A3F** | Derived opaque `kColorMenuButtonBackgroundSelected`: `SysStateHoverOnSubtle` composited over normal disc |
| Normal button glyph | #1F1F1F | #E3E3E3 | #DADBE5 | Explicit `kColorMenuItemForeground` |
| Hover/pressed glyph | #1F1F1F | #E3E3E3 | #DADBE5 | `kColorMenuItemForegroundSelected`; same as foreground for this provider, separate ID for contrast themes |
| Divider | #D3E3FD | #5E5E5E | #4E5059 | `kColorMenuSeparator`; grayscale light changes to #E3E3E3 |
| Disabled button | **No disc; normal glyph colour remains** | Same rule | Same rule | Background returns before fill; no disabled image model is supplied, so `ImageButton::GetImageToPaint` falls back to the normal image. Fullscreen divider is painted before this return. |

Recipes: [material-colors]:114–125; [sys-colors]:217–220, 252–254; [ref-colors]:93–164. Hover composites use light `#1F1F1F @0x0F`, dark-off `#FDFCFB @0x1A`, dark-on `#F9FAFC @0x1A` and §4's channel-rounding formula. Disabled behavior: [app-menu]:258–292, 395–403, 818–827; [image-button]:234–273. **Do not apply the ordinary menu item's disabled-icon alpha or disabled-text colour to these child-button glyphs.** The always-enabled parent and informational percentage also do not turn gray when one zoom limit is reached.

Seeded themes use the resolved `SysNeutralContainer` (**Neutral94 in light, NeutralVariant15 in dark** after the themed override), and the same composite formula; do not bake the baseline table into all themes ([sys-colors]:20–39). Windows high contrast replaces normal disc/body with `COLOR_BTNFACE`, normal foreground and divider with `COLOR_BTNTEXT`, selected disc with `COLOR_HIGHLIGHT`, selected glyph with `COLOR_HIGHLIGHTTEXT`; the magnifier's subtle token resolves through the native mixer to `COLOR_WINDOWTEXT`. Disabled still follows the absence-of-disc/normal-image rule. Resolve current OS colours rather than guessing hex values ([native-colors]:69–94, 136–233).

### Hit regions, input, enabled state and menu lifetime

| Case | Verified behavior | Source |
|---|---|---|
| Hit regions | Entire **46×H**, **46×H**, **54×H** button rectangles, including blank padding outside the 28-DIP discs. Percentage/title are not reset targets. Background shape does not install a circular hit mask. | [app-menu]:860–883; [button-controller]:23–60 |
| Pointer activation | Primary-button release inside an enabled button invokes once; press shows pressed state. Release outside cancels that click. Hover and pressed share the selected disc/image recipe. Menu capture/drag forwarding can synthesize a button press as the pointer enters a child during a menu drag, then forward release. | [button-h]:386; [button-controller-h]:73; [button-controller]:23–83; [controller]:1072–1088, 1118–1122, 3723–3785 |
| Minus / plus | Call `ButtonMenuItemModel::ActivatedAt` directly, **keeping the menu open**. `DoesCommandIdDismissMenu` independently returns false for these two commands. Percentage and enabled state update in place. | [app-menu]:759–806; [button-model]:110–112; [app-model]:1382–1383 |
| Fullscreen | `CancelAndEvaluate` stores model/index and cancels root. `AppMenu::OnMenuClosed` then calls `ActivatedAt`; thus **close first, execute afterward**, for both entry and exit. | [app-menu]:808–814, 1465–1497, 1776–1779 |
| Percentage/title click | No command; the compound-row mouse-release guard does not accept the parent. Its delegate also ignores `kZoomMenuPlaceholder`. No reset, popup or percentage editor. | [controller]:1136–1146; [item-h]:755; [app-menu]:1366–1371; [label]:855–856, 1005–1008 |
| Zoom limit state | Display `z=GetZoomPercent()`, then minus enabled iff `z>GetMinimumZoomPercent()`, plus iff `z<GetMaximumZoomPercent()`. Windows contents initialize these limits from 0.25×100 and 5×100, i.e. **25 and 500**. No active contents: display 100; the constructor's model-enabled states remain. | [app-menu]:544–574, 908–928; [web-contents]:1364–1367, 7452–7457; [blink-zoom]:19–26 |
| Fullscreen state | Enabled iff `canEnterFullscreen OR isAlreadyFullscreen`; always allow exit. `CanUserEnterFullscreen → BrowserView::CanFullscreen → WidgetDelegate::CanFullscreen AND GetWebApiWindowResizable().value_or(true)`. Window creation capabilities, picture-in-picture/forced app mode and fullscreen policy feed the capability. | [app-menu]:617–638, 930–939; [browser-view]:683–684, 897–917, 3627–3629, 6104–6111 |
| Live refresh | Subscribe to profile `ZoomEventManager`; read active contents again on each change, update text/states and invalidate `Q`. Observe widget size-constraint changes for fullscreen state and release that observation on destruction. | [app-menu]:746–757, 885–938 |
| Keyboard traversal | **Down** (also PageDown) walks minus → plus → fullscreen → next menu row. **Up** (also PageUp) reverses this and enters from fullscreen. Percentage is skipped; disabled or undrawn controls are skipped through `View::IsFocusable`. Leaving the group resumes ordinary menu navigation. | [controller]:313–363, 2051–2058, 3352–3383, 3971–4013; [view]:2087–2089 |
| Keyboard activation | **Enter or Space on a hot-tracked button** sends a synthetic Return accelerator to it, invoking its same callback. Surviving zoom buttons are hot-tracked again; fullscreen follows close-before-execute. Hot tracking sets `STATE_HOVERED`, with accessibility popup-focus/active-descendant signals. | [controller]:2085–2134, 2300–2313, 4017–4047; [button]:376–389, 564–567 |
| Other navigation | Left/Right operate the menu's submenu hierarchy (RTL reverses directions); they do not adjust zoom or traverse this group. Home/End select first/last menu items. There is no row-specific Tab traversal or reset key. If Enter finds no hot child, normal parent acceptance may close the menu but the placeholder executes no command. | [controller]:2037–2083, 2121–2129, 3506–3516; [app-menu]:1366–1371 |
| Browser shortcuts vs row keys | Normal browser mappings include Ctrl-minus/plus, Ctrl-0 and F11. They are not additional row-button bindings. In the Windows open-menu Aura route these keys are absent from `ShouldCancelMenuForEvent` and from the row-navigation cases; `OnWillDispatchKeyEvent` finishes with `SetSkipped`, consuming ordinary dispatch. The Windows default Views delegate leaves the menu open. Do not assume pressing F11 while this menu is open invokes its fullscreen button. | [accelerators]:133–138, 159, 257–259; [menu-pre-target]:61–122; [controller]:1587–1683; [views-delegate]:72–79; [chrome-views-delegate]:40–46; [events]:305–311 |
| Dismissal | Escape closes the current submenu/root normally; Alt or unmodified F10 cancels on Windows. Outside click, activation change or capture loss use the ordinary menu-session cancellation. Disabled controls invoke nothing. | [controller]:2136–2170, 3610–3685; [menu-pre-target]:44–58; [views-delegate]:78–79 |

The row wrapper exposes accessibility role **menu**, and the three buttons **menu item**, always focusable when enabled. The percentage has role **alert** on Windows and emits an alert after zoom changes, excluding initial construction. Its text is informational, not editable. English minus/plus accessible names **and tooltips** are “Make Text Smaller” / “Make Text Larger”; they deliberately omit accelerator text. Fullscreen tooltip is “Full screen”, “Exit full screen” or “Full screen disabled”; its accessible name additionally includes the current accelerator (F11 on Windows). No shortcut column is painted for the compound row. Sources: [app-menu]:322–339, 387–403, 525–527, 561–568, 603–638, 764–806, 922–926, 1404–1407; [menu-resources]:1591–1593, 1747–1752, 10937–10944. The checkout's own tests assert the wrapper role and fullscreen enable/exit behavior ([app-menu-tests]:320–393); they were read, not run.

Ordinary menu keyboard selection uses hot tracking and accessibility focus override, not actual `View::RequestFocus`. Mouse press also leaves `request_focus_on_press=false`. Although `ImageButton` installs a focus-ring helper, its default predicate is actual `HasFocus`; do not add a separate outline to the usual hot-tracked disc. Sources: [controller]:4017–4047; [button-h]:389; [button]:695–699; [focus-ring]:411–417.

**No animations in Quartz:** Chromium's `ImageButton` enables its 150-ms hover-image transition when a hover image exists; this is outside the requested implementation. Render the final normal/hover/pressed/disabled states immediately, omit image interpolation and ink-drop effects, and do not add transitions for percentages, discs or window dismissal ([image-button]:70–74, 263–273; [button]:231–249, 674).

### DPI, RTL and enclosing-menu placement

Apply §6's Windows screen-coordinate conversion and integer-DIP layout before rasterization. Button images specifically request `PaintInfo::kUniformScaling`: raster at the actual device scale, preserving aspect ratio, rather than stretching a cached 1× image into independently rounded x/y dimensions ([image-button]:154–165). Vector representation selection uses `ceil(deviceScale*requestedDipSize)` and then source-canvas scaling ([vector-paint]:467–469, 350–365). Keep the integer-centre asymmetry of the 53-DIP fullscreen background area; do not replace it with arbitrary half-DIP centring. Keep hit regions in the same transformed coordinate space as painting. Re-measure localized percentage/title text for the resolved font inputs and compare fractional display scales separately from page zoom.

Views mirrors child placement for RTL; `Label` flips its right alignment, and `ImageButton` flips its paint canvas. The fullscreen background code explicitly ensures its leading divider is mirrored **once** ([label]:361–367; [image-button]:42–48; [app-menu]:261–283). Avoid applying both an extra host mirror and an independent control mirror.

The root app menu still uses the bubble's radius 12, padding 12/12 and shadow allocation from §5. At its `kTopRight` button anchor, the leftward candidate is `anchor.right-hostWidth+shadowRight`, and the below candidate is `anchor.bottom-shadowTop`; flip/fallback, clamp and overflow rules are in `CalculateBubbleMenuBounds` ([controller]:3071–3344, especially 3151–3179). The row itself creates no submenu or separate popup; it participates in the enclosing menu's width/scrolling calculation. Quartz can preserve its existing owned menu invocation point, including tray invocation; transplanting the row does not authorize changing the settings button's primary-click behavior.

### Command implementation and Quartz adapter boundary

Chromium's child model calls `AppMenuModel::ExecuteCommand → chrome::ExecuteCommand`. `BrowserCommandController` maps minus/plus to `chrome::Zoom → PageZoom::Zoom(active contents, direction)` and fullscreen to `ToggleFullscreenMode(user_initiated=true)` ([button-model]:110–112; [app-model]:1386–1422; [browser-command-controller]:840–842, 1081–1089; [browser-commands]:2624–2626).

The source preset factors are **0.25, 1/3, 0.5, 2/3, 0.75, 0.8, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5**; keep 1/3 and 2/3 as ratios, not rounded 0.33/0.67. Chromium operates in zoom levels `log(factor)/log(1.2)`, inserts the browser's default level if strictly inside the range and not already equivalent, then finds the next smaller/larger level, skipping candidates within **0.001 in level space**. At an endpoint it makes no change. Reset is a separate command that restores the default level and page scale 1; this row does not call it. Sources: [blink-zoom]:13–55; [page-zoom]:28–53, 71–129.

| Chromium role | Future Quartz connection, preserving browser functions | Exact existing seam |
|---|---|---|
| Minus/plus callbacks | Dispatch `BrowserCommand.ZoomOut` / `ZoomIn` once through `ShortcutManager.ExecuteCommand(browser,command)` as **menu activations**, after checking the current browser/window. Keep the custom menu session open, re-read actual zoom and repaint/re-measure as needed. Enter/Space invocation has the same menu-command semantics; it is not a WebView accelerator event. | [q-shortcuts]:109–123; [q-browser-shortcuts]:49–74, 143–145; [q-zoom]:304–375 |
| Percentage and limits | Read `wvWebView1.ZoomFactor`, calculate Chromium-rounded localized percent, and expose command availability plus 25/500 limit state to the renderer. Refresh on opening and after application zoom notifications; the existing `ZoomFactorChanged` handler only persists settings, so it does not by itself refresh a future custom view. Add a narrowly scoped application-menu refresh signal when implementing; do not touch WebView2 context-menu handlers/settings. | [q-browser]:1606–1609, 1688–1699; [q-zoom]:361–375 |
| Fullscreen callback/state | Close the owned custom popup and release capture first, then dispatch `BrowserCommand.Fullscreen`. Read the owning `AppContainer.FullScreen` to name entry/exit. Existing function saves/restores bounds/state and applies chrome to all tabs; retain it. Do not confuse window fullscreen with a web page's `ContainsFullScreenElement`. | [q-browser-shortcuts]:131; [AppContainer.FullScreen.cs][q-fullscreen]:13–56; [q-browser]:108–114, 830–833 |
| Global shortcuts | Preserve existing Ctrl-plus/minus, Ctrl-0 and F11 command registrations and settings guards outside custom menus. The new owned menu controller must get its navigation keys before `HandleKeyDown`'s general close-and-dispatch path. Within it, follow the open-menu behavior above instead of inheriting WinForms button tab order or Quartz's current shortcut-driven dismissal. | [q-shortcuts]:56, 71–73, 151–169; [q-browser-shortcuts]:49–61 |

`StepMenuZoom` already uses the same preset ratios and `SetMenuZoom` clamps 0.25–5, assigns WebView zoom and persists settings. It currently compares **factor-space** epsilon 0.001 and does not insert a separate browser default level ([q-zoom]:15–34, 304–375). Those details are **not** requirements for this replacement. For exact Chromium stepping near arbitrary factors/defaults, a small zoom-command adapter should use the level-space calculation above and funnel the resulting factor through the existing setter; avoid changing page navigation, storage semantics or WebView settings. Quartz has no separate default-level input in this helper: use its existing reset/default 100% until such an input actually exists, rather than importing Chrome profile storage.

Quartz's `CanExecuteShortcutCommand(Fullscreen)` validates the live browser/window but does not expose Chromium's separate can-enter policy. Expose that capability as menu-model state if Quartz has a real window restriction; in the existing normal resizable window it is available. Always keep exit available when already fullscreen. The Chromium capability calculation is established above; any Quartz-specific restriction must come from an actual host capability, not invented policy.

Only the Zoom-row adapter/presentation in [q-zoom], its initialization/opening refresh in [q-browser]:134, 1688–1699, and the narrow custom-menu shortcut/session seam need future integration. Existing action functions can be reused. Do not retain `FlowLayoutPanel`, WinForms `Button`, `ToolStripControlHost`, reset-percentage hit target, current dimensions/palette, or the provisional fullscreen glyph as rendering requirements. No such replacement or command change is made in this research stage.

## 7. Exact Quartz-owned menu targets

The inventory covers the explicit application context menus found in source, including their dropdown submenus. “—” denotes an existing separator; “→” a submenu. Labels below reflect current default English resources/runtime text. Preserve this surrounding menu inventory; **the internal Zoom/fullscreen row is replaced by §6A**, including its non-clickable percentage. Existing row visuals and interaction are recorded only to locate the replacement seam.

| Surface and menu | Existing item order / dynamic content | Creation, invocation and command ownership |
|---|---|---|
| Tab strip: `TabContextMenu` | New tab to the left; New tab to the right; Move tab to new/another window; —; Reload; Duplicate; Pin/Unpin; Mute/Unmute tab; —; Close Tab; Close other tabs; Close tabs to the left; Close tabs to the right. | [TabContextMenu.cs][q-tab-menu]:38–99, opening 236–265. Created in [AppContainer.cs][q-container]:126–127; shown by [TitleBarTabsOverlay.cs][q-overlay]:1480–1493 using clicked tab/parent from [ContextMenuProvider.cs][q-menu-provider]. |
| Tab move submenu | If no other window: leaf “Move tab to new window”. Otherwise “Move tab to another window” → New window; —; eligible windows in `OpenWindowsByActivation` order. New-window availability depends on tab count. | [TabContextMenu.WindowMenu.cs][q-window-menu]:18–47. Preserve titles, activation order, stale-target checks and transfer actions. Window titles use name/selected caption, newline removal, count suffix and width truncation (50–77); preserve semantics while replacing GDI measurement. |
| Empty title/tab-strip space: `DefaultContextMenu` | Restore; Move; Size; Minimize; Maximize; —; Reload all tabs...; Favourite all tabs...; Name window...; —; Task manager; —; Close. | [DefaultContextMenu.cs][q-default-menu]:46–98, opening/enabling 158 onward; [q-overlay]:1494–1504. Keep existing window commands and state checks; using window-management APIs for these actions does not imply native menu rendering. |
| Address edit: `mnuSearch` | Emoji; —; Undo; Redo; —; Cut; Copy; Paste; Paste and go; Delete; —; Select All; —; Always show full URLs (check). | [Browser.Designer.cs][q-browser-designer]:374–393, assigned to `txtWebAddress` at 541. [Browser.cs][q-browser]:2409–2454 actions, 2499–2528 opening: focus/selection/clipboard-dependent enabled state, URL toggle, dynamic “Paste and search for …” label. |
| Favourites bar/button: `mnuMenu` | Open; Open in new tab; Open in new window; —; Edit; —; Cut; Copy; Paste; —; Delete; Delete all; —; Show icons (check); Sort by alphabetically (check); Show favourites bar (check). | [q-browser-designer]:150–170, panel assignment 529; generated favourite buttons [q-browser]:435. Opening 1650–1680 distinguishes bar from favourite and updates enabled/check/count state. Actions use `SourceControl`; preserve the specific favourite context. |
| Settings-button **context** menu and tray: `SettingsMenuStrip` | New tab; New window; Profiles; —; History →; Favourites →; Downloads →; —; Zoom custom row; —; Find; Print; Open File In Browser; More tools →; —; UserData →; Settings; [conditional —; Update available]; —; Restart; Exit. | [q-browser-designer]:554, 570–594; tray assignment [q-browser]:942; opening 1688–1699 updates capabilities/zoom/update. The settings button's primary click at 2102–2109 opens the Settings dialog; do not replace that behavior with menu opening. |
| `mnuHistory` | History command; —; up to **11** most recent entries in descending `When`, or disabled “No recent pages”; —; Clear browsing data... | Active rebuild [q-browser]:1926–1991 (`count <= 10`, deliberately document the existing 11-item behavior). Entries preserve title shortening, favicon, URL tooltip/tag and left/middle/modifier navigation. Parent History also has a command binding. |
| `mnuFavourites` | Add this tab to favourites...; Add all tabs to favourites; —; Show favourites bar (check); [if favourites exist: —]; favourites ordered by `Index`, optional icons. | Active rebuild [q-browser]:2595–2660. Preserve captured URL, left/middle/modifier navigation, checked state and existing shortcut commands. |
| `mnuDownloadsDropDown` | Location: current download path; Change Location. | [q-browser-designer]:285–307; opening/actions [q-browser]:1838–1854. Location row is informational (no action handler); preserve the actual enabled/resource state. Downloads parent also has its own action. |
| `mnuExperts` (“More tools”) | Name window...; —; Task manager; —; Developer Tools. | [q-browser-designer]:718 onward; actions and capability checks in [q-browser], [Browser.Shortcuts.cs][q-browser-shortcuts]. |
| `mnuUserData` | Open Folder in File Explorer; —; Reset. | [q-browser-designer]:769 onward; existing browser action handlers. |
| History window: `contextMenuStrip1` | Open; Open in new tab; Open in new window; —; Copy link; Delete. Multi-selection changes open/copy labels to plural forms. | [History.Designer.cs][q-history-designer]:170–179; [History.cs][q-history]:472–482 selects the row and shows at mouse position, 496–500 closes on scroll, 514–528 updates labels. Preserve selected rows and `rowIndex`, not just displayed text. |
| Profiles window: `ContextMenuStripProfiles` | Set default/Unset default; Edit; —; Delete. | [Profiles.Designer.cs][q-profiles-designer]:150–157; [Profiles.cs][q-profiles]:147 attaches to profile buttons; 395–430 opening captures profile, default state and delete constraints. Existing handlers 293 onward use `SourceControl`. |

The update entry is inserted immediately after Settings by [Browser.Updates.cs][q-browser-updates]:15–46 and shown only for an available update with a version. Keep this insertion/visibility rule and its updater command. [Browser.ZoomMenu.cs][q-zoom]:36–160 currently replaces the designer zoom item with a `ToolStripControlHost`/`FlowLayoutPanel` and buttons, including a clickable reset percentage. **Replace that provisional presentation and its interaction with §6A.** Reuse the existing browser action functions through its documented adapter; do not preserve the current dimensions, styling, glyph approximations, reset target or WinForms control behavior.

[Browser.Background.cs][q-background] also contains asynchronous history/favourite population helpers, but no calls to those helpers were found; the synchronous opening handlers above are the active paths. Avoid implementing a different list from an unused helper.

### Shared integration dependencies and narrow change boundary

- [AnimatedContextMenuStrip.cs][q-animated] and [FocusAwareContextMenuStrip.cs][q-focus] are legacy behavior references only. They provide a 100-ms fade and focus-on-open; do not subclass them for the new renderer.
- [NewControlThemeChanger.cs][q-theme]:215–234 and [ThemeService.cs][q-theme-service]:79–80 assign existing palettes/renderers. The new popup should consume its own Chromium colour resolver and a light/dark/contrast input. No global replacement of existing theme services or other controls is needed.
- [Browser.Shortcuts.cs][q-browser-shortcuts]:11–47 binds commands and discovers/closes menus via `ContextMenuStrip`. [ShortcutManager.cs][q-shortcuts]:180–206 recognizes a `ToolStripDropDown` input surface. Add only the necessary recognition/closure of owned custom popup sessions, preserve `CanExecuteShortcutCommand` and execute each command exactly once. **Do not change WebView key-event registration/handling or settings checks.**
- Existing favourites/profile handlers cast the sender to `ToolStripMenuItem` and obtain `Owner.SourceControl`. Replace these menu-only dependencies with explicit captured invocation context (tab/window, favourite/profile ID, address control or selected history rows). Reuse the underlying command actions rather than fabricating ToolStrip senders.
- [ChromiumButton.cs][q-button]:750–791 observes context-menu open/close/dispose to maintain toolbar pressed state. A migrated menu needs an equivalent session-state signal; do not redesign the button or its drawing system.
- [WindowActivation.cs][q-activation] already recognizes owned windows through owner relationships, plus legacy ToolStrip handling. Give popup hosts the correct owner and first verify existing activation logic; do not pre-emptively rewrite it.
- Source/declaration wiring changes should remain in the listed menu-owning classes/designers, `AppContainer`, the two EasyTabs menu call sites/provider, and narrowly necessary menu-session integration. Do not change WebView2, navigation/profile storage, updater behavior, unrelated form layouts, tab rendering or application-wide DPI settings.

## 8. Practical isolated implementation plan (not started)

1. **Build an isolated preview first.** Proposed new code may live under `C:/Users/admin/source/repos/Quartz/Quartz/Controls/ChromiumMenus/`, with a separate explicitly opened preview form. This is a proposal, not an existing path. The preview uses synthetic menu models and logs command invocations; it does not contain or subscribe to WebView2. Include ordinary context-item fixtures and a separate browser-menu fixture for §6A with adjustable zoom, locale, font metrics, fullscreen/capability state and both feature profiles from §2. If the application's runtime is DPI-virtualized, use a separate appropriately DPI-aware preview process for pixel comparisons instead of silently changing Quartz's process mode.
2. **Separate model, layout, painter and host.** Use plain menu data and explicit invocation context. Ordinary rows use §3; add a distinct compound Zoom row with three command targets, one informational percentage, separate text-baseline calculation, original vector assets and state/lifetime rules from §6A. Calculate all layout in DIP and resolve Chromium colours before painting. Preserve surrounding dynamic snapshots and stale-target guards. Do not use hidden `ToolStrip` objects as the new model or measurement engine.
3. **Render the complete popup using Skia.** A borderless owned WinForms window supplies lifecycle/input/accessibility only; an alpha-capable premultiplied surface supplies body, shadow, text, control vectors and image content. A layered-window presenter is one viable host. Ensure no extra Win32/DWM shadow or border. Existing premultiplied BGRA Skia allocation in [q-tab-renderer]:782–827 is a useful local pattern, not a file to modify. Use shaping/font fallback compatible with Chromium; retain Chromium asset attribution when translating vector paths. Do not add animations.
4. **Add one menu-session controller.** Manage root/submenu windows, hit regions, capture, keyboard/mnemonics, opening delay, monitor placement/scrolling, cancellation and focus restoration. Implement the compound row's full rectangular hit regions, Up/Down child traversal, Enter/Space invocation, live percentage announcements, zoom-stays-open and fullscreen-close-before-command behavior. Route owned menu input before the existing general shortcut dismissal path; keep outside-menu shortcut registrations and WebView key wiring unchanged. Expose accessible roles, names, active descendant and disabled/checked states as specified. Custom painting must not remove keyboard or screen-reader access.
5. **Integrate the tab strip first.** The two calls in `TitleBarTabsOverlay` and construction in `AppContainer` are the cleanest seam. Replace the menu-only `ContextMenuProvider` contract with a small presenter callback/interface carrying owner/tab/point so EasyTabs need not reference Quartz. Port the tab/default menu construction and command context while retaining pin/mute state, protected pinned tabs in bulk-close operations, transfer behavior and window commands. Validate these before touching additional surfaces.
6. **Migrate the remaining menu families through local adapters.** The inventory in §7 is the target list. Preserve dynamic update/history/favourite lists, primary button actions, tray invocation and parent-command-plus-submenu semantics. For Zoom/fullscreen, replace `Browser.ZoomMenu.cs`'s provisional control host with the §6A Skia compound row. Import the actual selected Chromium vector paths, use its order/geometry/state and input rules, and connect the three commands to the existing browser functions through the documented adapter. Keep Ctrl-0 as an independent command, not a percentage click. Check factor-versus-level epsilon/default handling when connecting the stepper. These are established Chromium requirements; no new row-proportion design decision is needed.
7. **Validate and finish within this boundary.** Compare the preview and migrated menu families using §10. Keep the old general theme/WinForms menu classes until unrelated callers no longer need them; broad cleanup is unnecessary. Do not alter `CoreWebView2.ContextMenuRequested`, `AreDefaultContextMenusEnabled`, menu-item replacement, initialization, handler subscriptions or any built-in WebView2 menu behavior.

## 9. Verified findings versus remaining platform dependencies

**Established from the source:** the web-content Views/Aura route and the separate browser-menu `AppMenu::ZoomView` route; the `AppMenuGlowUp` boundary; feature defaults versus eligible developer test assignments; Windows geometry, font/baseline formulas, colour recipes, original vector assets, button-state painting, keyboard/input and menu lifetime rules; submenu placement/DPI conversion; Quartz-owned targets and command seams. Numeric composites and shadow margins are derived from those formulas. The literal percentage-as-factor width call is recorded in §6A rather than silently corrected. No menu renderer, application code/configuration change or screenshot calibration was produced during this research.

| Remaining input / question | Why source alone cannot give one universal value | Required resolution during implementation validation |
|---|---|---|
| Effective Chromium build/feature configuration | No executable/build output was found. The C++ defaults and developer test assignments are known (§2), but loaded seeds/overrides and actual GN args are not. | Record the reference binary's revision/configuration and effective feature states. Use `AppMenuGlowUp=off` for §6A; compare rounded icons/Dark26 off and the eligible developer-config on profile separately. |
| Actual family, size, weight, baseline and rasterization | Windows menu LOGFONT, locale translations, installed fallback fonts, text scale, ClearType geometry/gamma/contrast and Skia backend settings are runtime inputs. | Capture those inputs; log normalized font metrics and shaped advances alongside image comparisons. Never fill the gap with a guessed “Chrome font”. |
| Effective colour-provider key | Profile scheme, grayscale/accent seed, policy/custom theme and high contrast change token resolution. | Fix a baseline profile for the initial comparison; record and separately validate other provider keys. Source formulas above define the variants. |
| Pixel-identical text/shadow rasterization | Quartz's SkiaSharp packages and Chromium's Skia/rendering pipeline are not established as identical binaries. Translucent-surface LCD eligibility and paint-recording scale mode also matter. | Compare actual output; isolate geometry errors from rasterization differences. A mathematically matching layout alone is not proof of pixel identity. |
| Quartz DPI awareness | Checked-in configuration lacks per-monitor awareness declarations; compatibility settings/host/runtime can still affect the process. | Inspect actual process/thread/window awareness and physical-versus-virtualized coordinates. Keep any broader application DPI work separate. |
| Quartz-specific action/submenu combinations | Application-specific actions are not Chromium web-content context commands. Zoom geometry is now resolved by the browser-menu specification in §6A. | Preserve the remaining application-specific commands/context. Do not use this open question to retain the provisional Zoom row. |
| Zoom default and host capabilities | Chromium has a profile default zoom level and fullscreen capability/policy inputs; Quartz's current helpers expose a factor and window fullscreen state without equivalent independent inputs. | Use the existing action/setter seams and explicit current capability state. Validate nonpreset/default stepping; do not invent profile/policy state or change WebView2 menu behavior. |
| Screen colour management / HDR | Source colours are sRGB recipes; display profiles and compositor output can alter captured/displayed pixels. | Start with controlled SDR captures, then test the relevant display configuration if required. |

## 10. Validation checklist

Use captures from the **same Chromium checkout/build**, not an older Chrome screenshot or WebView2's potentially different runtime. Match content, Windows settings, locale, profile and flags. Compare **visible body edges separately from host/shadow bounds**, then rows, baselines, columns and painted pixels.

- [ ] Record revision, build/branding, feature overrides, theme-provider inputs, Windows menu font/metrics, smoothing settings, text scale, DPI awareness and display work area for each comparison.
- [ ] Confirm whether the test-config assignments apply; record individual `RoundedIcons`, `MenuSimplification`, `ChromeDarkNeutrals26`, `DesktopGlowUp` and `AppMenuGlowUp` states. Do not equate the study name `GlowUp` with either umbrella or alternate-menu flag.
- [ ] Representative web-content references: plain page, link, image/media, selected text and editable text; at least one menu with icons, disabled commands, checks and a nested submenu. Populate equivalent preview fixtures without replacing the real web menu.
- [ ] Text-only versus 16-DIP icon/check columns; no forced empty gutter; mixed icons, wide/tall image, check/radio plus extra icon, hidden-icon metric effect, secondary label and iconless title.
- [ ] Short/long labels, long accelerators, arrow with/without minor text, RTL, literal ampersands/mnemonic underlines, CJK/Arabic/Indic text, emoji and fallback glyphs. Check primary/shortcut baselines and the extra minor alignment unit.
- [ ] Normal, pointer-hover, keyboard-selected, pressed/released and disabled states; checked/unchecked checkbox and radio; selected minor colour; vector recolouring versus bitmap content; no item animation.
- [ ] Normal separator's 17-DIP slot and physical hairline; upper/lower/spacing/padded/double variants in the preview; no unwanted icon-column inset or additional separator margins.
- [ ] Radius12, body padding12/12, no outer stroke; distinct root/submenu shadows, correct halo allocation and no duplicate OS shadow. Verify light and dark against a nonuniform desktop background.
- [ ] Baseline light/dark, grayscale, DarkNeutrals26 off/on, seeded theme and Windows high contrast as separate cases. Check disabled-text contrast recipe rather than reusing disabled-icon alpha.
- [ ] Root and at least two nested levels; first-child-row alignment, RTL, left/right fallback, top/bottom work-area limits, invocation in taskbar/full-display area, negative monitor origins and oversized menu scrolling.
- [ ] **100%, 125%, 150%, 175%, 200%** display scales; mixed-DPI monitors; accessibility text scaling separately and together with display scaling. Check edge rounding, hairlines, glyph fringes and transitions between monitors.
- [ ] Mouse release guard, submenu delay, disabled hit testing, outside click/capture loss, Home/End/arrows/Enter/Escape/Alt/F10/mnemonics, focus restoration and accessibility names/roles/states. Commands fire exactly once.
- [ ] Each surrounding Quartz menu retains its item order, dynamic labels/visibility, command functions and action context. Include pinned tabs, stale destination windows, clipboard-dependent address/favourite menus, multi-row History, active/default profiles, update visibility and tray invocation. The Zoom row follows the replacement specification, not its provisional implementation.
- [ ] Browser-menu row fixture: magnifier/title, minus, informational percentage, plus, divider/fullscreen; no clickable percentage, reset button, arrow or shortcut text column. Use every original glyph from the selected feature family, including rounded 20-to-16 magnifier scaling and unchanged enter/exit fullscreen glyph.
- [ ] Verify `Q` from all localized preset strings, `G=146+Q`, button widths 46/46/54, row height from font metrics, body-edge alignment and shared icon column. Compare title and percentage baselines separately; include odd row heights and larger profile icons elsewhere in the menu.
- [ ] Compare 28-DIP discs with full button hit rectangles, including clicks in blank padding and beside the fullscreen divider. Test pressed/release-inside, release-outside, drag across child controls, disabled clicks, no parent hover fill, and immediate state changes with no animations.
- [ ] Verify normal/hover/pressed/disabled discs and original glyph colours in light, dark-off, Dark26-on, grayscale/seeded and Windows contrast themes. Disabled buttons lose the disc but retain the normal glyph; fullscreen divider remains.
- [ ] Zoom values 25%, 33%, 67%, 100%, 125%, 500%, arbitrary nonpreset values and a custom default fixture; compare steps near the 0.001 level-space tolerance. Check endpoint enable state, localized readout, live updates, stable measured percentage column and repeated zoom without dismissal.
- [ ] Down/Up (and PageDown/PageUp) traverse enabled child buttons then adjacent rows, skipping the percentage and disabled controls; Enter/Space invoke once. Left/Right remain submenu navigation; no invented Tab/reset behavior. Check Ctrl-plus/minus, Ctrl-0 and F11 separately with menu open and closed, without duplicate dispatch.
- [ ] Fullscreen entry and exit close the menu/release capture before toggling the owning window. Validate normal, forbidden-entry and already-fullscreen states; already-fullscreen permits exit. Confirm saved window state/restoration and behavior after switching tabs. Percentage alert, menu-item names/tooltips and active-descendant accessibility signals match §6A.
- [ ] Repeat the browser-row fixtures at 100%, 125%, 150%, 175%, 200% display scales and in RTL; inspect original vector scaling, circle centring, the physical-pixel vertical divider and hit/paint alignment across monitors. Page zoom must not scale the browser menu itself.
- [ ] Application code diff contains only the future approved menu integration boundary. **All WebView2 menu settings, menu handlers, event subscriptions and default-menu behavior remain unchanged.** Right-click page/link/image/edit fields still use the existing built-in WebView2 menu path.

Research stops here. The only artifact of this stage is this handover document.

<!-- Absolute source references, pinned by the revisions in the introduction. -->
[version]: D:/src/chromium154/src/chrome/VERSION
[entry]: D:/src/chromium154/src/chrome/browser/ui/views/tab_contents/chrome_web_contents_view_delegate_views.cc
[rv-views]: D:/src/chromium154/src/chrome/browser/ui/views/renderer_context_menu/render_view_context_menu_views.cc
[rv-base]: D:/src/chromium154/src/components/renderer_context_menu/render_view_context_menu_base.cc
[rv-model]: D:/src/chromium154/src/chrome/browser/renderer_context_menu/render_view_context_menu.cc
[rv-build]: D:/src/chromium154/src/chrome/browser/ui/views/renderer_context_menu/BUILD.gn
[spelling]: D:/src/chromium154/src/chrome/browser/renderer_context_menu/spelling_options_submenu_observer.cc
[toolkit]: D:/src/chromium154/src/components/renderer_context_menu/views/toolkit_delegate_views.cc
[adapter]: D:/src/chromium154/src/ui/views/controls/menu/menu_model_adapter.cc
[runner]: D:/src/chromium154/src/ui/views/controls/menu/menu_runner.cc
[runner-impl]: D:/src/chromium154/src/ui/views/controls/menu/menu_runner_impl.cc
[controller]: D:/src/chromium154/src/ui/views/controls/menu/menu_controller.cc
[host]: D:/src/chromium154/src/ui/views/controls/menu/menu_host.cc
[config]: D:/src/chromium154/src/ui/views/controls/menu/menu_config.cc
[config-h]: D:/src/chromium154/src/ui/views/controls/menu/menu_config.h
[config-win]: D:/src/chromium154/src/ui/views/controls/menu/menu_config_win.cc
[native]: D:/src/chromium154/src/ui/native_theme/native_theme.cc
[native-win]: D:/src/chromium154/src/ui/native_theme/native_theme_win.cc
[ui-features]: D:/src/chromium154/src/ui/base/ui_base_features.cc
[chrome-features]: D:/src/chromium154/src/chrome/browser/ui/ui_features.cc
[simple-model]: D:/src/chromium154/src/ui/menus/simple_menu_model.h
[gfx-switches]: D:/src/chromium154/src/ui/gfx/switches.cc
[layout]: D:/src/chromium154/src/ui/views/layout/layout_provider.cc
[scroll]: D:/src/chromium154/src/ui/views/controls/menu/menu_scroll_view_container.cc
[item]: D:/src/chromium154/src/ui/views/controls/menu/menu_item_view.cc
[submenu]: D:/src/chromium154/src/ui/views/controls/menu/submenu_view.cc
[canvas]: D:/src/chromium154/src/ui/gfx/canvas.cc
[canvas-skia]: D:/src/chromium154/src/ui/gfx/canvas_skia.cc
[text-utils]: D:/src/chromium154/src/ui/gfx/text_utils.cc
[badge]: D:/src/chromium154/src/ui/views/badge_painter.cc
[badge-h]: D:/src/chromium154/src/ui/views/badge_painter.h
[chrome-typography]: D:/src/chromium154/src/chrome/browser/ui/views/chrome_typography_provider.cc
[delegate]: D:/src/chromium154/src/ui/views/controls/menu/menu_delegate.cc
[bubble]: D:/src/chromium154/src/ui/views/bubble/bubble_border.cc
[fonts]: D:/src/chromium154/src/ui/gfx/system_fonts_win.cc
[locale-settings]: D:/src/chromium154/src/ui/strings/app_locale_settings.grd
[main-win]: D:/src/chromium154/src/chrome/browser/chrome_browser_main_win.cc
[l10n]: D:/src/chromium154/src/ui/base/l10n/l10n_util_win.cc
[dpi]: D:/src/chromium154/src/ui/display/win/dpi.cc
[skia-font]: D:/src/chromium154/src/ui/gfx/platform_font_skia.cc
[font-list]: D:/src/chromium154/src/ui/gfx/font_list_impl.cc
[render-text]: D:/src/chromium154/src/ui/gfx/render_text.cc
[render-text-h]: D:/src/chromium154/src/ui/gfx/render_text.h
[font-render]: D:/src/chromium154/src/ui/gfx/font_render_params_win.cc
[widget]: D:/src/chromium154/src/ui/views/widget/widget.cc
[browser-widget]: D:/src/chromium154/src/chrome/browser/ui/views/frame/browser_widget.cc
[os-settings]: D:/src/chromium154/src/ui/native_theme/os_settings_provider_win.cc
[theme]: D:/src/chromium154/src/chrome/browser/themes/theme_service.cc
[mixers]: D:/src/chromium154/src/ui/color/color_mixers.cc
[ui-colors]: D:/src/chromium154/src/ui/color/ui_color_mixer.cc
[material-colors]: D:/src/chromium154/src/ui/color/material_ui_color_mixer.cc
[sys-colors]: D:/src/chromium154/src/ui/color/sys_color_mixer.cc
[ref-colors]: D:/src/chromium154/src/ui/color/ref_color_mixer.cc
[core-colors]: D:/src/chromium154/src/ui/color/core_default_color_mixer.cc
[color-utils]: D:/src/chromium154/src/ui/gfx/color_utils.cc
[palette]: D:/src/chromium154/src/ui/gfx/color_palette.h
[palette-factory]: D:/src/chromium154/src/ui/color/dynamic_color/palette_factory.cc
[native-colors]: D:/src/chromium154/src/ui/color/win/native_color_mixers_win.cc
[separator]: D:/src/chromium154/src/ui/views/controls/menu/menu_separator.cc
[paint-flags]: D:/src/chromium154/src/cc/paint/paint_flags.h
[arrow-old]: D:/src/chromium154/src/components/vector_icons/submenu_arrow_chrome_refresh_old.icon
[check-old]: D:/src/chromium154/src/ui/views/vector_icons/menu_check_old.icon
[radio-empty]: D:/src/chromium154/src/ui/views/vector_icons/menu_radio_empty_old.icon
[radio-selected]: D:/src/chromium154/src/ui/views/vector_icons/menu_radio_selected_old.icon
[shadow]: D:/src/chromium154/src/ui/gfx/shadow_value.cc
[skia-paint]: D:/src/chromium154/src/ui/gfx/skia_paint_util.cc
[screen]: D:/src/chromium154/src/ui/display/win/screen_win.cc
[desktop-host]: D:/src/chromium154/src/ui/views/widget/desktop_aura/desktop_window_tree_host_win.cc
[paint-info]: D:/src/chromium154/src/ui/views/paint_info.cc
[q-webview]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.TabOpening.cs
[q-update-webview]: C:/Users/admin/source/repos/Quartz/Quartz/Settings.Updates.cs
[q-manifest]: C:/Users/admin/source/repos/Quartz/Quartz/app.manifest
[q-app-config]: C:/Users/admin/source/repos/Quartz/Quartz/App.config
[q-tab-renderer]: C:/Users/admin/source/repos/Quartz/EasyTabs/ChromiumTabRenderer.cs
[q-tab-menu]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/TabContextMenu.cs
[q-window-menu]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/TabContextMenu.WindowMenu.cs
[q-default-menu]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/DefaultContextMenu.cs
[q-container]: C:/Users/admin/source/repos/Quartz/Quartz/AppContainer.cs
[q-overlay]: C:/Users/admin/source/repos/Quartz/EasyTabs/TitleBarTabsOverlay.cs
[q-menu-provider]: C:/Users/admin/source/repos/Quartz/EasyTabs/ContextMenuProvider.cs
[q-browser-designer]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.Designer.cs
[q-browser]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.cs
[q-browser-shortcuts]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.Shortcuts.cs
[q-history-designer]: C:/Users/admin/source/repos/Quartz/Quartz/History.Designer.cs
[q-history]: C:/Users/admin/source/repos/Quartz/Quartz/History.cs
[q-profiles-designer]: C:/Users/admin/source/repos/Quartz/Quartz/Profiles.Designer.cs
[q-profiles]: C:/Users/admin/source/repos/Quartz/Quartz/Profiles.cs
[q-settings-designer]: C:/Users/admin/source/repos/Quartz/Quartz/Settings.Designer.cs
[q-settings]: C:/Users/admin/source/repos/Quartz/Quartz/Settings.cs
[q-browser-updates]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.Updates.cs
[q-zoom]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.ZoomMenu.cs
[q-background]: C:/Users/admin/source/repos/Quartz/Quartz/Browser.Background.cs
[q-animated]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/AnimatedContextMenuStrip.cs
[q-focus]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/FocusAwareContextMenuStrip.cs
[q-theme]: C:/Users/admin/source/repos/Quartz/Quartz/Services/NewControlThemeChanger.cs
[q-theme-service]: C:/Users/admin/source/repos/Quartz/Quartz/Services/ThemeService.cs
[q-shortcuts]: C:/Users/admin/source/repos/Quartz/Quartz/ShortcutManager.cs
[q-button]: C:/Users/admin/source/repos/Quartz/Quartz/Controls/ChromiumButton.cs
[q-activation]: C:/Users/admin/source/repos/Quartz/EasyTabs/WindowActivation.cs
[fieldtrials]: D:/src/chromium154/src/testing/variations/fieldtrial_testing_config.json
[fieldtrial-readme]: D:/src/chromium154/src/testing/variations/README.md
[fieldtrial-build]: D:/src/chromium154/src/components/variations/service/BUILD.gn
[fieldtrial-creator]: D:/src/chromium154/src/components/variations/service/variations_field_trial_creator.cc
[browser-app-menu-button]: D:/src/chromium154/src/chrome/browser/ui/views/toolbar/browser_app_menu_button.cc
[app-menu-button]: D:/src/chromium154/src/chrome/browser/ui/views/frame/app_menu_button.cc
[app-menu]: D:/src/chromium154/src/chrome/browser/ui/views/toolbar/app_menu.cc
[app-model]: D:/src/chromium154/src/chrome/browser/ui/toolbar/app_menu_model.cc
[action-menu]: D:/src/chromium154/src/chrome/browser/ui/views/app_menu/action_app_menu.cc
[action-zoom]: D:/src/chromium154/src/chrome/browser/ui/views/app_menu/action_app_menu_zoom_view.cc
[image-button]: D:/src/chromium154/src/ui/views/controls/button/image_button.cc
[button]: D:/src/chromium154/src/ui/views/controls/button/button.cc
[button-h]: D:/src/chromium154/src/ui/views/controls/button/button.h
[button-controller]: D:/src/chromium154/src/ui/views/controls/button/button_controller.cc
[button-controller-h]: D:/src/chromium154/src/ui/views/controls/button/button_controller.h
[label]: D:/src/chromium154/src/ui/views/controls/label.cc
[item-h]: D:/src/chromium154/src/ui/views/controls/menu/menu_item_view.h
[rect]: D:/src/chromium154/src/ui/gfx/geometry/rect.cc
[view]: D:/src/chromium154/src/ui/views/view.cc
[views-delegate]: D:/src/chromium154/src/ui/views/views_delegate.cc
[chrome-views-delegate]: D:/src/chromium154/src/chrome/browser/ui/views/chrome_views_delegate.h
[menu-pre-target]: D:/src/chromium154/src/ui/views/controls/menu/menu_pre_target_handler_aura.cc
[events]: D:/src/chromium154/src/ui/events/event.h
[button-model]: D:/src/chromium154/src/ui/base/models/button_menu_item_model.cc
[page-zoom]: D:/src/chromium154/src/components/zoom/page_zoom.cc
[blink-zoom]: D:/src/chromium154/src/third_party/blink/common/page/page_zoom.cc
[zoom-controller]: D:/src/chromium154/src/components/zoom/zoom_controller.cc
[web-contents]: D:/src/chromium154/src/content/browser/web_contents/web_contents_impl.cc
[browser-commands]: D:/src/chromium154/src/chrome/browser/ui/browser_commands.cc
[browser-command-controller]: D:/src/chromium154/src/chrome/browser/ui/browser_command_controller.cc
[browser-view]: D:/src/chromium154/src/chrome/browser/ui/views/frame/browser_view.cc
[accelerators]: D:/src/chromium154/src/chrome/browser/ui/accelerator_table.cc
[menu-resources]: D:/src/chromium154/src/chrome/app/generated_resources.grd
[number-format]: D:/src/chromium154/src/base/i18n/number_formatting.cc
[vector-paint]: D:/src/chromium154/src/ui/gfx/paint_vector_icon.cc
[paint-flags-cc]: D:/src/chromium154/src/cc/paint/paint_flags.cc
[zoom-old]: D:/src/chromium154/src/chrome/app/vector_icons/zoom_in_old.icon
[zoom-icon]: D:/src/chromium154/src/chrome/app/vector_icons/zoom_in.icon
[minus-old]: D:/src/chromium154/src/chrome/app/vector_icons/zoom_minus_menu_refresh_old.icon
[remove-icon]: D:/src/chromium154/src/chrome/app/vector_icons/remove.icon
[plus-old]: D:/src/chromium154/src/chrome/app/vector_icons/zoom_plus_menu_refresh_old.icon
[add-icon]: D:/src/chromium154/src/chrome/app/vector_icons/add.icon
[fullscreen-old]: D:/src/chromium154/src/chrome/app/vector_icons/fullscreen_refresh_old.icon
[fullscreen-icon]: D:/src/chromium154/src/chrome/app/vector_icons/fullscreen.icon
[app-menu-tests]: D:/src/chromium154/src/chrome/browser/ui/views/toolbar/app_menu_browsertest.cc
[q-fullscreen]: C:/Users/admin/source/repos/Quartz/Quartz/AppContainer.FullScreen.cs
[focus-ring]: D:/src/chromium154/src/ui/views/controls/focus_ring.cc
