# Pinned-tab regression checks

Build and run on Windows with Visual Studio MSBuild and .NET Framework 4.8.1:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' Tests\Quartz.PinnedTabs.Tests.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal
& .\Tests\bin\Debug\Quartz.PinnedTabs.Tests.exe
```

If Quartz is running from the normal Debug output directory, add a separate
absolute `/p:OutputPath=...` directory to the build command and run the test
executable from that directory. The September 10 validation build used
`Tests/bin/PinnedValidation` to avoid interrupting the running browser.

Checks cover pinned ordering, active/background selection, clamped insertion and
mixed ranges, fixed widths, pin/unpin timing, rapid reversal, reduced motion,
loading, multiple DPI scales and light/dark themes, drag boundaries and stationary
pointer stability, transfer to another window, menu labels and enabled states,
bulk-close protection, explicit close, cancellation, disposal and session-only
state. Drag checks include movement across the pinned boundary, valid model order,
intermediate return frames and reduced-motion release. Pixel checks verify the
temporary default favicon, exact restoration after unpinning, loading completion
and a real favicon arriving later. `pinned-fallback-light.png` and
`pinned-fallback-dark.png` show before/pinned/unpinned appearances.
Generated `pinned-tabs-light.png` and `pinned-tabs-dark.png` show six frames
of a pin transition, 40 ms apart, in the test output directory.

The tests use hidden forms and no WebView2 instances. Before release, smoke-test
the rebuilt browser: pin/unpin active and background tabs; open a link and favourite
from a pin; duplicate a pin; drag both sections; tear out/merge a pin; close other
tabs; explicitly close a pin; and restart to confirm that pins are not restored.
Test the themes and display scales used for the release.

The implementation notes and Chromium source references are in
[ChromiumTabRendering.md](../EasyTabs/ChromiumTabRendering.md).

The same executable also runs `TabWindowTests.cs`, covering context-menu window
moves, live content identity, selection and pinned ordering, repeated transfers,
close cancellation, last-tab window closure, destination ordering and stale
menu entries. See [TabWindowMoves.md](../EasyTabs/TabWindowMoves.md) for the
current Chromium references and live-browser smoke checks.
