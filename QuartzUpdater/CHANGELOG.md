# QuartzUpdater 1.2.9

- Added `--check-only` for a completely hidden version check with no install.
- Recorded the result in `UpdateStatus.json` and returned an exit code.
- Kept the previous status unchanged when an automatic check fails.
- Added one `automatic-check.log` for silent-check troubleshooting.
- Added Quartz's optional seven-day automatic-check setting and About-page checkbox.

# QuartzUpdater 1.2.8

- Added `UpdateStatus.json` with the states `updateAvailable` and `upToDate`.
- Recorded status after successful checks, successful installs, and rollbacks.
- Added independent copy-paste read/write classes to Quartz and QuartzUpdater.
- Removed the unused GitHub release tag from the status file.

# QuartzUpdater 1.2.7

- Restored the useful failure reason in the error message box.
- Reduced full Windows paths in visible errors to a filename such as `Quartz.exe`.
- Kept exact paths and complete technical errors in the update log.

# QuartzUpdater 1.2.6

- Replaced displayed log paths with a concise `Open update log` link.
- Opened the log through its default Windows application when the link is selected.
- Kept the full path in the link tooltip and removed it from failure message boxes.

# QuartzUpdater 1.2.5

- Shortened long log paths in the error details so they no longer wrap below the label.
- Added a tooltip containing the complete log path.
- Cleared stale log tooltips whenever the updater changes state.

# QuartzUpdater 1.2.4

- Added optional `--owner-hwnd` support for the exact Quartz form that launched the updater.
- Verified that the supplied form belongs to the adjacent Quartz process before using it.
- Fell back to Quartz's main window when the form handle is missing, stale, or unrelated.
- Continued using standalone mode when the Quartz process itself cannot be verified.

# QuartzUpdater 1.2.3

- Added optional `--parent-pid` support for launches from Quartz.
- Attached the normal update window to the verified adjacent Quartz process.
- Centred owned updater windows over Quartz and hid their separate taskbar button.
- Kept direct launches and installer-worker launches standalone.

# QuartzUpdater 1.2.2

- Removed special handling for legacy `UserData` directories.
- Restored the original whole-installation-folder backup, activation, and rollback process.
- Release-package `UserData` content is handled like any other installation content.

# QuartzUpdater 1.2.1

- Removed failed or rolled-back operation files when the worker window closes.
- Added startup cleanup for abandoned terminal jobs from older updater versions.
- Removed exited temporary worker copies on the next normal updater launch.
- Continued retaining update logs for troubleshooting.

# QuartzUpdater 1.2.0

- Removed SHA-256 release-asset discovery and verification.
- Protected legacy top-level `UserData` directories during install and rollback.
- Ignored `UserData` content accidentally included in an update package.
- Prevented late progress callbacks from hiding the real error message.
- Replaced duplicate log session messages with distinct updater and worker entries.

# QuartzUpdater 1.1.0

- Added current-versus-latest comparison and clear UI states.
- Added cancellable, streamed downloads with byte progress.
- Added GitHub asset-size and optional SHA-256 verification.
- Added protected ZIP extraction and packaged executable version validation.
- Added a temporary apply worker so the updater can safely replace itself.
- Added graceful Quartz shutdown and process waiting.
- Added complete installation backup, replacement, restart checking, and rollback.
- Added persistent update logs and a JSON update journal.
- Added development-build protection to prevent accidental downgrades.
