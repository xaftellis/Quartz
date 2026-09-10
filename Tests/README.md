# Favourites regression checks

Build with Visual Studio MSBuild on Windows (.NET Framework 4.8.1):

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' Tests\Quartz.Favourites.Tests.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal
& .\Tests\bin\Debug\Quartz.Favourites.Tests.exe
```

The harness references the real Quartz assembly, creates hidden WinForms controls,
and uses unique temporary JSON files. It does not launch Quartz or alter a user profile.
It checks layout compatibility, drag thresholds, mixed widths and grab offsets,
intermediate/final animation positions, stationary-pointer stability, native mouse
release, click suppression and recovery, middle-click delivery, keyboard tab order,
Escape, outside drops, capture loss, resizing, scrolling, reduced motion, disposal,
profile isolation, stale data, add/paste/edit/bulk-update ordering, alphabetical
sorting, failed writes, and order/icon validation.
Duplicate-favourite checks cover matching names, matching URLs, exact copies,
independent editing/deletion, stable sorting and drag order after reopening,
and distinct button identities. Legacy files receive repeatable IDs without a
write on read; the next normal save persists those IDs.
Alphabetical-sort checks cover intermediate movement, reusing buttons, scroll
position, retargeting during animation, dragging during sorting, reduced motion,
and keeping programmatic sorting separate from custom-order saves.
Icon and membership checks cover native first/intermediate/final frames, rapid
icon reversals, adding/removing during transitions, the last-item contraction,
handler retention, scroll extent, and reduced motion. Pixel checks compare every
intermediate label with a single native label in Flat, Popup and Standard styles
on light and dark backgrounds. Add/close checks verify the EasyTabs timing curves,
opaque unscaled content, and clipping at the changing right edge. Native renders
are saved as `bin/Debug/favourites-icon-transition.png` and
`bin/Debug/favourites-membership-*.png` for inspecting intermediate frames.

Live computer-control smoke check (2026-09-10, rebuilt Debug browser): hide and
restore favourite icons, save a temporary favourite, drag it from last to first,
then delete it. The original three favourites and visible icons were restored.
Intermediate-frame assertions above complement the live UI checks; desktop
screenshots taken after an action do not capture every frame of a 200 ms animation.

Before release, check the visible browser at the display scales and themes you use:
drag both ways, hold at each scroll edge, cancel with Escape or Alt+Tab, switch tabs,
and restart to confirm persistence. Hidden-window tests cannot assess perceived
smoothness or every interaction with WebView2 and the desktop compositor.
