using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuartzUpdater
{
    internal static class UpdateApplier
    {
        public static async Task<UpdateApplyResult> ApplyAsync(
            UpdateJob job,
            IProgress<UpdateProgress> progress)
        {
            ValidateJob(job);
            UpdateLog.Initialize(job.LogFilePath, "Apply worker session started.");
            UpdateLog.Write("Apply worker loaded job " + job.Id + ".");

            Version expectedVersion = UpdaterClass.NormalizeVersion(
                Version.Parse(job.ExpectedVersion));

            string installDirectory = NormalizeDirectory(job.InstallDirectory);
            string installParent = Path.GetDirectoryName(installDirectory);
            string installName = Path.GetFileName(installDirectory);
            string pendingDirectory = Path.Combine(
                installParent,
                "." + installName + ".pending-" + job.Id);
            string backupDirectory = Path.Combine(
                installParent,
                "." + installName + ".backup-" + job.Id);
            string failedDirectory = Path.Combine(
                installParent,
                "." + installName + ".failed-" + job.Id);

            bool backupCreated = false;
            bool replacementInstalled = false;

            try
            {
                Report(progress, "Preparing installation", 0, "Waiting for QuartzUpdater to close...");
                bool parentExited = await WaitForProcessToExitAsync(
                    job.ParentUpdaterProcessId,
                    TimeSpan.FromSeconds(30));

                if (!parentExited)
                {
                    throw new InvalidOperationException(
                        "The original QuartzUpdater process did not close.");
                }

                Report(progress, "Closing Quartz", 5, "Closing Quartz before files are replaced...");
                await CloseQuartzProcessesAsync(job, progress);
                await Task.Delay(1200);

                List<int> remainingProcesses = UpdaterClass.FindRunningQuartzProcesses(installDirectory);
                if (remainingProcesses.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Quartz is still running. Close every Quartz window, then select Retry.");
                }

                BestEffortDeleteDirectory(pendingDirectory);
                BestEffortDeleteDirectory(failedDirectory);

                Report(progress, "Preparing installation", 10, "Preparing the new version...");
                await CopyDirectoryAsync(job.StagingDirectory, pendingDirectory, progress);
                UpdaterClass.ValidatePayload(pendingDirectory, expectedVersion);

                job.State = "PendingPrepared";
                UpdaterClass.SaveUpdateJob(job);
                UpdateLog.Write("Pending installation prepared at " + pendingDirectory + ".");

                if (Directory.Exists(backupDirectory))
                {
                    UpdateLog.Write(
                        "A previous update backup already exists at " +
                        backupDirectory + ".");
                    throw new IOException(
                        "A previous update backup already exists. Close the updater and try again.");
                }

                Report(progress, "Installing update", 58, "Backing up the current version...");
                await MoveDirectoryWithRetryAsync(installDirectory, backupDirectory);
                backupCreated = true;

                job.State = "BackupCreated";
                UpdaterClass.SaveUpdateJob(job);
                UpdateLog.Write("Current installation moved to backup.");

                Report(progress, "Installing update", 72, "Activating the new version...");
                await MoveDirectoryWithRetryAsync(pendingDirectory, installDirectory);
                replacementInstalled = true;

                job.State = "Installed";
                UpdaterClass.SaveUpdateJob(job);

                string installedExecutable = Path.Combine(installDirectory, "Quartz.exe");
                Version installedVersion = UpdaterClass.GetExecutableVersion(installedExecutable);
                if (installedVersion.CompareTo(expectedVersion) != 0)
                {
                    throw new InvalidDataException(
                        "The installed Quartz version is " + installedVersion +
                        ", but " + expectedVersion + " was expected.");
                }

                Process quartzProcess = null;
                if (job.RestartAfterUpdate)
                {
                    Report(progress, "Restarting Quartz", 88, "Starting Quartz " + expectedVersion + "...");
                    quartzProcess = StartQuartz(installDirectory);

                    await Task.Delay(6000);
                    quartzProcess.Refresh();
                    if (quartzProcess.HasExited)
                    {
                        throw new InvalidOperationException(
                            "The new Quartz version closed during startup.");
                    }
                }

                UpdaterClass.TryRecordUpdateStatus(
                    QuartzUpdateStatus.CreateUpToDate(
                        installedVersion,
                        expectedVersion));

                job.State = "Healthy";
                BestEffortSaveJob(job);
                UpdateLog.Write("Quartz " + expectedVersion + " started successfully.");

                Report(progress, "Finishing update", 96, "Removing the previous version...");
                BestEffortDeleteDirectory(backupDirectory);
                backupCreated = false;

                job.State = "Complete";
                BestEffortSaveJob(job);
                BestEffortDeleteDirectory(job.OperationDirectory);

                Report(progress, "Update complete", 100, "Quartz " + expectedVersion + " is ready.");
                return new UpdateApplyResult
                {
                    Succeeded = true,
                    RolledBack = false,
                    Message = "Quartz was updated to " + expectedVersion + "."
                };
            }
            catch (Exception exception)
            {
                UpdateLog.Write(exception);
                bool rolledBack = false;
                Exception rollbackException = null;

                if (backupCreated)
                {
                    try
                    {
                        Report(progress, "Restoring Quartz", 90, "The update failed; restoring the previous version...");

                        if (replacementInstalled && Directory.Exists(installDirectory))
                            await MoveDirectoryWithRetryAsync(installDirectory, failedDirectory);

                        if (Directory.Exists(backupDirectory))
                            await MoveDirectoryWithRetryAsync(backupDirectory, installDirectory);

                        rolledBack = Directory.Exists(installDirectory) &&
                            File.Exists(Path.Combine(installDirectory, "Quartz.exe"));

                        if (rolledBack)
                        {
                            Version restoredVersion = UpdaterClass.GetExecutableVersion(
                                Path.Combine(installDirectory, "Quartz.exe"));

                            UpdaterClass.TryRecordUpdateStatus(
                                QuartzUpdateStatus.CreateUpdateAvailable(
                                    restoredVersion,
                                    expectedVersion));

                            if (job.RestartAfterUpdate)
                                StartQuartz(installDirectory);
                        }

                        BestEffortDeleteDirectory(failedDirectory);
                        job.State = rolledBack ? "RolledBack" : "RollbackFailed";
                        BestEffortSaveJob(job);
                        UpdateLog.Write(rolledBack
                            ? "Rollback completed successfully."
                            : "Rollback did not restore a valid Quartz installation.");
                    }
                    catch (Exception rollbackFailure)
                    {
                        rollbackException = rollbackFailure;
                        UpdateLog.Write(rollbackFailure);
                    }
                }

                BestEffortDeleteDirectory(pendingDirectory);

                string message;
                if (rolledBack)
                {
                    message = "The update failed, but the previous Quartz version was restored. " +
                        GetUserFacingError(exception);
                }
                else if (backupCreated)
                {
                    UpdateLog.Write(
                        "Automatic rollback failed. The backup remains at " +
                        backupDirectory + ".");
                    message = "The update failed and Quartz could not be restored automatically. " +
                        "A backup of the previous version was kept. " +
                        GetUserFacingError(rollbackException ?? exception);
                }
                else
                {
                    job.State = "FailedBeforeInstall";
                    BestEffortSaveJob(job);
                    message = "The update stopped before any installed files were changed. " +
                        GetUserFacingError(exception);
                }

                throw new UpdateApplyException(message, exception, rolledBack);
            }
        }

        internal static string GetUserFacingError(Exception exception)
        {
            if (exception == null)
                return "An unknown error occurred.";

            if (exception is PathTooLongException)
                return "A Quartz file or folder path is too long for Windows.";

            string message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
                return "An unknown error occurred.";

            if (!Regex.IsMatch(message, @"(?:[A-Za-z]:\\|\\\\)"))
                return message.Trim();

            string leafName = null;
            var fileNotFound = exception as FileNotFoundException;
            if (fileNotFound != null)
                leafName = GetPathLeaf(fileNotFound.FileName);

            if (string.IsNullOrWhiteSpace(leafName))
            {
                Match match = Regex.Match(
                    message,
                    @"'(?<path>(?:[A-Za-z]:\\|\\\\)[^'\r\n]+)'");
                if (match.Success)
                    leafName = GetPathLeaf(match.Groups["path"].Value);
            }

            string item = string.IsNullOrWhiteSpace(leafName)
                ? "a required Quartz file or folder"
                : leafName;

            if (exception is FileNotFoundException)
                return item + " could not be found.";

            if (exception is DirectoryNotFoundException)
                return item + " could not be found.";

            if (exception is UnauthorizedAccessException)
                return "Windows denied access to " + item +
                    ". Close Quartz and try again, or run the updater as administrator.";

            if (exception is IOException &&
                message.IndexOf(
                    "being used by another process",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return item +
                    " is being used by another process. Close Quartz and try again.";
            }

            return "Windows could not access " + item +
                ". See the update log for the complete path and technical details.";
        }

        private static string GetPathLeaf(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                string trimmed = path.Trim().TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
                return Path.GetFileName(trimmed);
            }
            catch
            {
                return null;
            }
        }

        private static void ValidateJob(UpdateJob job)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            Guid parsedId;
            if (!Guid.TryParseExact(job.Id, "N", out parsedId))
                throw new InvalidDataException("The update job ID is invalid.");

            string updatesRoot = NormalizeDirectory(UpdaterClass.GetUpdatesRoot());
            string operationDirectory = NormalizeDirectory(job.OperationDirectory);
            string stagingDirectory = NormalizeDirectory(job.StagingDirectory);
            string jobFilePath = Path.GetFullPath(job.JobFilePath);
            string installDirectory = NormalizeDirectory(job.InstallDirectory);
            string currentWorkerDirectory = NormalizeDirectory(AppContext.BaseDirectory);

            if (!IsPathInside(operationDirectory, updatesRoot))
                throw new InvalidDataException("The update operation directory is outside Quartz update storage.");

            if (!IsPathInside(stagingDirectory, operationDirectory))
                throw new InvalidDataException("The staging directory is outside the update operation.");

            if (!IsPathInside(jobFilePath, operationDirectory))
                throw new InvalidDataException("The update journal is outside the update operation.");

            if (IsPathInside(currentWorkerDirectory, installDirectory))
            {
                throw new InvalidOperationException(
                    "The apply worker is still running from inside the Quartz installation.");
            }

            string installRoot = Path.GetPathRoot(installDirectory);
            if (string.Equals(
                installDirectory.TrimEnd(Path.DirectorySeparatorChar),
                installRoot.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("A drive root cannot be used as the Quartz installation.");
            }

            if (!Directory.Exists(installDirectory) ||
                !File.Exists(Path.Combine(installDirectory, "Quartz.exe")))
            {
                throw new InvalidDataException("The Quartz installation directory is missing or invalid.");
            }

            if (!Directory.Exists(stagingDirectory))
                throw new InvalidDataException("The staged update is missing.");

            Version expectedVersion;
            if (!Version.TryParse(job.ExpectedVersion, out expectedVersion))
                throw new InvalidDataException("The expected update version is invalid.");

            UpdaterClass.ValidatePayload(stagingDirectory, expectedVersion);
        }

        private static async Task CloseQuartzProcessesAsync(
            UpdateJob job,
            IProgress<UpdateProgress> progress)
        {
            var processIds = new HashSet<int>(job.QuartzProcessIds ?? new List<int>());
            foreach (int processId in UpdaterClass.FindRunningQuartzProcesses(job.InstallDirectory))
                processIds.Add(processId);

            foreach (int processId in processIds)
            {
                Process process;
                try
                {
                    process = Process.GetProcessById(processId);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                using (process)
                {
                    if (process.HasExited)
                        continue;

                    Report(progress, "Closing Quartz", 6, "Waiting for Quartz to close...");
                    try
                    {
                        process.CloseMainWindow();
                    }
                    catch
                    {
                    }

                    bool exited = await WaitForProcessToExitAsync(
                        processId,
                        TimeSpan.FromSeconds(25));

                    if (!exited)
                    {
                        throw new InvalidOperationException(
                            "Quartz is still running. Close every Quartz window, then select Retry.");
                    }
                }
            }
        }

        private static async Task<bool> WaitForProcessToExitAsync(
            int processId,
            TimeSpan timeout)
        {
            if (processId <= 0)
                return true;

            DateTime deadline = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using (Process process = Process.GetProcessById(processId))
                    {
                        if (process.HasExited)
                            return true;
                    }
                }
                catch (ArgumentException)
                {
                    return true;
                }

                await Task.Delay(250);
            }

            try
            {
                using (Process process = Process.GetProcessById(processId))
                    return process.HasExited;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        private static async Task CopyDirectoryAsync(
            string sourceDirectory,
            string destinationDirectory,
            IProgress<UpdateProgress> progress)
        {
            string sourceRoot = NormalizeDirectory(sourceDirectory);
            Directory.CreateDirectory(destinationDirectory);

            string[] directories = Directory.GetDirectories(
                sourceRoot,
                "*",
                SearchOption.AllDirectories);

            foreach (string directory in directories)
            {
                string relative = directory.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
            }

            string[] files = Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
                throw new InvalidDataException("The staged update contains no files.");

            long totalBytes = files.Sum(file => new FileInfo(file).Length);
            long copiedBytes = 0;

            foreach (string sourceFile in files)
            {
                string relative = sourceFile.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destinationFile = Path.Combine(destinationDirectory, relative);
                string destinationParent = Path.GetDirectoryName(destinationFile);

                if (!string.IsNullOrEmpty(destinationParent))
                    Directory.CreateDirectory(destinationParent);

                using (var input = new FileStream(
                    sourceFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    true))
                using (var output = new FileStream(
                    destinationFile,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    true))
                {
                    await input.CopyToAsync(output, 81920);
                }

                File.SetLastWriteTimeUtc(
                    destinationFile,
                    File.GetLastWriteTimeUtc(sourceFile));

                copiedBytes += new FileInfo(sourceFile).Length;
                int percent = totalBytes > 0
                    ? 10 + (int)Math.Min(45, copiedBytes * 45L / totalBytes)
                    : 55;

                Report(progress, "Preparing installation", percent, relative);
            }
        }

        private static async Task MoveDirectoryWithRetryAsync(
            string sourceDirectory,
            string destinationDirectory)
        {
            Exception lastException = null;

            for (int attempt = 1; attempt <= 8; attempt++)
            {
                try
                {
                    Directory.Move(sourceDirectory, destinationDirectory);
                    return;
                }
                catch (IOException exception)
                {
                    lastException = exception;
                }
                catch (UnauthorizedAccessException exception)
                {
                    lastException = exception;
                }

                await Task.Delay(500);
            }

            throw new IOException(
                "Windows could not move " + sourceDirectory + " to " + destinationDirectory + ".",
                lastException);
        }

        private static Process StartQuartz(string installDirectory)
        {
            string executablePath = Path.Combine(installDirectory, "Quartz.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = installDirectory,
                UseShellExecute = true
            };

            Process process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException("Windows could not start Quartz.exe.");

            return process;
        }

        private static string NormalizeDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidDataException("An update path is missing.");

            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool IsPathInside(string candidatePath, string parentDirectory)
        {
            string candidate = Path.GetFullPath(candidatePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = NormalizeDirectory(parentDirectory);

            return string.Equals(candidate, parent, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(
                    parent + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static void BestEffortDeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch (Exception exception)
            {
                UpdateLog.Write("Cleanup warning for " + directory + ": " + exception.Message);
            }
        }

        private static void BestEffortSaveJob(UpdateJob job)
        {
            try
            {
                UpdaterClass.SaveUpdateJob(job);
            }
            catch (Exception exception)
            {
                UpdateLog.Write("Journal warning: " + exception.Message);
            }
        }

        private static void Report(
            IProgress<UpdateProgress> progress,
            string stage,
            int percent,
            string detail)
        {
            if (progress != null)
                progress.Report(new UpdateProgress(stage, percent, detail));
        }
    }
}
