# QuartzUpdater

This project targets .NET Framework 4.8.1 and is designed to run beside
`Quartz.exe` in the installed Quartz directory.

## GitHub release contract

For a release tagged `v2.3.6`, attach this file:

```text
Quartz.v2.3.6.zip
```

The ZIP must contain the deployable Quartz files, including these files at the
ZIP root (or inside one top-level folder):

```text
Quartz.exe
QuartzUpdater.exe
QuartzUpdater.exe.config
Newtonsoft.Json.dll
assets\
other Quartz dependencies...
```

The file version of `Quartz.exe` must match the release tag.

QuartzUpdater does not give an installation-directory `UserData` folder special
treatment. If one exists in the old installation or release package, it is
backed up, replaced, restored, or removed with the rest of the installation.

## Update sequence

1. QuartzUpdater reads the adjacent `Quartz.exe` file version.
2. It requests the latest release from GitHub.
3. If a newer version exists, it downloads to Local AppData as a `.part` file.
4. It checks the GitHub asset size, ZIP paths, and the packaged `Quartz.exe`
   version.
5. It extracts the package without changing the installed version.
6. It runs a temporary copy of QuartzUpdater outside the installation folder.
7. The temporary worker closes Quartz, moves the whole current installation to
   a backup directory, and moves the prepared installation into its place.
8. If the new Quartz process closes during startup, the worker restores the
   previous installation and restarts it.

Update logs are written under:

```text
%LocalAppData%\Xaftellis\Quartz\Updates\Logs
```

The most recent successful version check is written to:

```text
%LocalAppData%\Xaftellis\Quartz\UpdateStatus.json
```

The updater records `updateAvailable` when the GitHub version is newer, and
`upToDate` when Quartz is current or is a newer development build. A successful
installation records `upToDate`; a successful rollback records
`updateAvailable` again. A failed or cancelled version check leaves the previous
status untouched. This file is information for Quartz's UI—the updater never
uses it to decide whether an update should be installed.

The independent `QuartzUpdateStatus.cs` files in the Quartz and QuartzUpdater
projects read and write the same JSON location. They are normal source files in
each project; there is no shared project or linked source dependency.

## Automatic checks without a window

Quartz can run this command when the stored status is missing or more than seven
days old:

```text
QuartzUpdater.exe --check-only
```

This mode displays no updater window, never downloads or installs a package, and
never closes Quartz. It only retrieves the latest release information, writes
`UpdateStatus.json`, and exits. Exit code `0` means the check succeeded; exit
code `1` means it failed. A failure leaves the previous status unchanged and is
recorded in:

```text
%LocalAppData%\Xaftellis\Quartz\Updates\Logs\automatic-check.log
```

The normal updater window remains responsible for downloading and installing
after the user chooses to update.

Failure screens show an `Open update log` link instead of the full path. Hover
over the link to see the complete path, or select it to open the log in the
default Windows application. The updater window and failure message box retain
the useful reason for the failure, but reduce full Windows paths to a filename
such as `Quartz.exe`. Exact paths and technical details remain in the log.

Failed update files are kept while the worker window is open so Retry can use
them. Closing that window removes the failed operation. A later normal updater
launch also removes abandoned completed, rolled-back, or pre-install-failure
operations and temporary worker copies that are no longer running. Logs remain
available for troubleshooting. In-progress jobs, active workers, damaged job
records, and rollback failures are retained instead of being deleted blindly.

Current Quartz user data lives under `%LocalAppData%\Xaftellis\Quartz\UserData`.
Because that directory is outside the Quartz installation folder, the folder
switching process does not affect it.

## Starting the updater from Quartz

Quartz can launch the updater with its normal installed path:

```csharp
Process.Start(new ProcessStartInfo
{
    FileName = Path.Combine(Application.StartupPath, "QuartzUpdater.exe"),
    Arguments = "--parent-pid " + Process.GetCurrentProcess().Id +
        " --owner-hwnd " + Handle.ToInt64(),
    WorkingDirectory = Application.StartupPath,
    UseShellExecute = true
});
```

When the process ID belongs to the adjacent `Quartz.exe` and the window handle
belongs to that process, the normal updater window is owned by the exact Quartz
form that launched it. This works from Settings or any other Quartz form. A
missing, stale, or unrelated window handle falls back to Quartz's main window.
A missing or unrelated process ID uses normal standalone behaviour. Apply
workers deliberately ignore both parent arguments and remain standalone.

The updater owns the version check, download, installation, restart, and error
handling. Quartz does not need to duplicate the GitHub release logic.
