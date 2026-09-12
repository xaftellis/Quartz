# WebView2 focus regression checks

Build with Visual Studio MSBuild on Windows (.NET Framework 4.8.1 and the
WebView2 Runtime installed):

```powershell
$focusOutput = Join-Path (Get-Location) 'Tests\bin\FocusValidation\'
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' Tests\Quartz.Focus.Tests.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU "/p:OutputPath=$focusOutput" /v:minimal
& .\Tests\bin\FocusValidation\Quartz.Focus.Tests.exe
```

The harness uses the real Browser controls and a real WebView2 renderer with a
unique test profile. It redirects settings to a fixture file and serves its page
locally through WebResourceRequested. It does not load the user's browser profile
or write browsing history. Allow WebView2's child processes to run; a sandbox
that terminates those processes cannot run these integration checks.

Tests explicitly focus Chromium's native child HWND instead of calling only
WebView2.Focus(), reproducing the focus transition that bypasses WinForms'
ActiveControl updates. They cover repeated URL expansion/selection and shortening,
unsaved edits, stale focus callbacks, all eight Browser component menus, tab and
title-bar menus, detached menus, nested submenus, menus opened while the page
already has focus, partial address selections, HTML input focus, and hook cleanup
when tabs are hidden, shown or disposed.

The native focus bridge also handles the WinForms menu dismissal issue reported
in [WebView2Feedback #3288](https://github.com/MicrosoftEdge/WebView2Feedback/issues/3288).
Menus take focus when opened, ensuring that returning to an already-focused page
creates a focus transition. The bridge observes native focus events and dismisses
the UI thread's open managed dropdowns after checking the current focus. It does
not depend on page scripts or a polling timer.
