# Launching QuartzUpdater from Quartz

The Check For Updates button should launch the adjacent updater and pass the
current Quartz process ID:

```csharp
private void checkforupdates()
{
    string updaterPath = Path.Combine(
        Application.StartupPath,
        "QuartzUpdater.exe");

    if (!File.Exists(updaterPath))
    {
        MessageBox.Show(
            "QuartzUpdater.exe was not found beside Quartz.exe.",
            "Check for updates",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        return;
    }

    try
    {
        Process updaterProcess = Process.Start(new ProcessStartInfo
        {
            FileName = updaterPath,
            Arguments = "--parent-pid " + Process.GetCurrentProcess().Id +
                " --owner-hwnd " + Handle.ToInt64(),
            WorkingDirectory = Application.StartupPath,
            UseShellExecute = true
        });

        if (updaterProcess == null)
            throw new InvalidOperationException("Windows did not return the updater process.");

        updaterProcess.Exited += QuartzUpdaterProcess_Exited;
        updaterProcess.EnableRaisingEvents = true;
    }
    catch (Exception exception)
    {
        MessageBox.Show(
            "QuartzUpdater could not be opened. " + exception.Message,
            "Check for updates",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

private void QuartzUpdaterProcess_Exited(object sender, EventArgs e)
{
    Process updaterProcess = sender as Process;

    try
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        BeginInvoke(new Action(() =>
        {
            if (!IsDisposed && !Disposing)
                OnQuartzUpdaterClosed();
        }));
    }
    catch (InvalidOperationException)
    {
    }
    finally
    {
        if (updaterProcess != null)
        {
            updaterProcess.Exited -= QuartzUpdaterProcess_Exited;
            updaterProcess.Dispose();
        }
    }
}

public event EventHandler QuartzUpdaterClosed;

protected virtual void OnQuartzUpdaterClosed()
{
    QuartzUpdaterClosed?.Invoke(this, EventArgs.Empty);
}
```

The file needs `using System.Diagnostics;` and `using System.IO;`, which the
reviewed Quartz `Settings.cs` already has. The updater verifies that the process
ID belongs to the adjacent `Quartz.exe` and that the form handle belongs to that
process. Any Quartz form can use the same method. A bad form handle falls back
to the main Quartz window; an invalid or absent process ID uses standalone mode.
`QuartzUpdaterClosed` is raised on Quartz's UI thread whenever that launched
updater process exits. Subscribe to it wherever Quartz needs to react:

```csharp
QuartzUpdaterClosed += (sender, eventArgs) =>
{
    // The updater window has closed.
};
```

## Reading the most recent update status

`QuartzUpdateStatus.cs` is a normal source file inside the Quartz project. It is
independent from the similarly named updater file; there is no shared project to
configure. After a successful updater check, Quartz can read the result with:

```csharp
QuartzUpdateStatus status;
if (QuartzUpdateStatusStore.TryRead(out status))
{
    if (status.IsUpdateAvailable)
    {
        // Show your "update available" UI.
    }

    bool shouldCheckAgain = status.IsOlderThan(TimeSpan.FromDays(7));
}
```

The file is `%LocalAppData%\Xaftellis\Quartz\UpdateStatus.json`. Its stored data
is limited to the schema version, state, check time, installed version, and
latest version. It deliberately has no GitHub release-tag field.

## Seven-day automatic checking

`QuartzAutomaticUpdateChecker.cs` contains Quartz's automatic-check behaviour.
Quartz calls `QuartzAutomaticUpdateChecker.TryStartIfDue()` during startup. The
method starts `QuartzUpdater.exe --check-only` when automatic checking is enabled
and the status is missing or older than seven days.

The About page checkbox changes the global `AutomaticallyCheckForUpdates`
setting. Missing settings are treated as enabled, so existing installations get
the recommended default. The automatic path is non-blocking, shows no window,
and never installs an update.
