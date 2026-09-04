using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzUpdater
{
    internal static class UpdaterClass
    {
        private const string LatestReleaseUrl =
            "https://api.github.com/repos/xaftellis/Quartz/releases/latest";

        private static readonly HttpClient Client = CreateHttpClient();

        // Used by the isolated test harness; production leaves this null.
        internal static string UpdatesRootOverride { get; set; }
        internal static Func<string, bool> ProcessRunningOverride { get; set; }
        internal static Func<CancellationToken, Task<ReleaseInfo>> LatestReleaseOverride { get; set; }

        public static async Task<UpdateCheckResult> CheckForUpdatesAsync(
            CancellationToken cancellationToken)
        {
            CleanupAbandonedOperations();
            CleanupOldWorkerDirectories();

            string installDirectory = Path.GetFullPath(AppContext.BaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            Version installedVersion = GetExecutableVersion(
                Path.Combine(installDirectory, "Quartz.exe"));

            ReleaseInfo release = LatestReleaseOverride == null
                ? await GetLatestReleaseAsync(cancellationToken)
                : await LatestReleaseOverride(cancellationToken);

            var result = new UpdateCheckResult
            {
                InstallDirectory = installDirectory,
                InstalledVersion = installedVersion,
                Release = release
            };

            RecordUpdateCheckStatus(result);
            return result;
        }

        internal static void RecordUpdateCheckStatus(UpdateCheckResult result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            QuartzUpdateStatus status = result.IsUpdateAvailable
                ? QuartzUpdateStatus.CreateUpdateAvailable(
                    result.InstalledVersion,
                    result.Release.Version)
                : QuartzUpdateStatus.CreateUpToDate(
                    result.InstalledVersion,
                    result.Release.Version);

            TryRecordUpdateStatus(status);
        }

        internal static void TryRecordUpdateStatus(QuartzUpdateStatus status)
        {
            if (!QuartzUpdateStatusStore.TryWrite(status))
            {
                UpdateLog.Write(
                    "Update-status warning: UpdateStatus.json could not be written.");
            }
        }

        public static async Task<UpdateJob> PrepareUpdateAsync(
            UpdateCheckResult checkResult,
            IProgress<UpdateProgress> progress,
            CancellationToken cancellationToken)
        {
            if (checkResult == null)
                throw new ArgumentNullException("checkResult");

            if (!checkResult.IsUpdateAvailable)
                throw new InvalidOperationException("There is no newer Quartz version to install.");

            EnsureInstallLocationCanBeUpdated(checkResult.InstallDirectory);

            string operationId = Guid.NewGuid().ToString("N");
            string operationDirectory = Path.Combine(
                GetUpdatesRoot(),
                checkResult.Release.Version.ToString(),
                operationId);

            string logFilePath = Path.Combine(
                GetUpdatesRoot(),
                "Logs",
                string.Format("update-{0:yyyyMMdd-HHmmss}-{1}.log", DateTime.Now, operationId));

            Directory.CreateDirectory(operationDirectory);
            UpdateLog.Initialize(logFilePath, "Updater session started.");
            UpdateLog.Write("Preparing update " + checkResult.Release.Tag + ".");

            string partialPackagePath = Path.Combine(operationDirectory, "Quartz.zip.part");
            string packagePath = Path.Combine(operationDirectory, "Quartz.zip");
            string extractionDirectory = Path.Combine(operationDirectory, "staging");

            try
            {
                Report(progress, "Downloading update", 0, "Connecting to GitHub...");

                await DownloadFileAsync(
                    checkResult.Release.DownloadUrl,
                    partialPackagePath,
                    checkResult.Release.AssetSize,
                    progress,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(packagePath))
                    File.Delete(packagePath);

                File.Move(partialPackagePath, packagePath);
                ValidateDownloadedSize(packagePath, checkResult.Release.AssetSize);

                Report(progress, "Extracting update", 65, "Opening the downloaded package...");

                string payloadDirectory = await ExtractPackageAsync(
                    packagePath,
                    extractionDirectory,
                    progress,
                    cancellationToken);

                ValidatePayload(payloadDirectory, checkResult.Release.Version);

                var job = new UpdateJob
                {
                    Id = operationId,
                    InstallDirectory = checkResult.InstallDirectory,
                    OperationDirectory = operationDirectory,
                    StagingDirectory = payloadDirectory,
                    PackagePath = packagePath,
                    ExpectedVersion = checkResult.Release.Version.ToString(),
                    ReleaseTag = checkResult.Release.Tag,
                    LogFilePath = logFilePath,
                    State = "Prepared",
                    RestartAfterUpdate = true
                };

                job.JobFilePath = Path.Combine(operationDirectory, "update-job.json");
                SaveUpdateJob(job);

                Report(progress, "Ready to install", 100, "The update has been verified.");
                UpdateLog.Write("Update package prepared successfully.");
                return job;
            }
            catch (Exception exception)
            {
                UpdateLog.Write(exception);
                BestEffortDeleteDirectory(operationDirectory);
                throw;
            }
        }

        public static void StartApplyWorker(UpdateJob job)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            job.ParentUpdaterProcessId = Process.GetCurrentProcess().Id;
            job.QuartzProcessIds = FindRunningQuartzProcesses(job.InstallDirectory);
            job.State = "LaunchingWorker";
            SaveUpdateJob(job);

            string workerDirectory = Path.Combine(GetWorkerRoot(), job.Id);
            Directory.CreateDirectory(workerDirectory);

            string sourceExecutable = Assembly.GetExecutingAssembly().Location;
            string targetExecutable = Path.Combine(
                workerDirectory,
                Path.GetFileName(sourceExecutable));

            File.Copy(sourceExecutable, targetExecutable, true);

            string sourceConfig = sourceExecutable + ".config";
            if (File.Exists(sourceConfig))
            {
                File.Copy(
                    sourceConfig,
                    Path.Combine(workerDirectory, Path.GetFileName(sourceConfig)),
                    true);
            }

            string jsonAssembly = typeof(JObject).Assembly.Location;
            File.Copy(
                jsonAssembly,
                Path.Combine(workerDirectory, Path.GetFileName(jsonAssembly)),
                true);

            var startInfo = new ProcessStartInfo
            {
                FileName = targetExecutable,
                Arguments = "--apply-job " + QuoteArgument(job.JobFilePath),
                WorkingDirectory = workerDirectory,
                UseShellExecute = false
            };

            Process worker = Process.Start(startInfo);
            if (worker == null)
                throw new InvalidOperationException("Windows could not start the update worker.");

            UpdateLog.Write("Temporary update worker started with process ID " + worker.Id + ".");
        }

        public static UpdateJob LoadUpdateJob(string jobFilePath)
        {
            if (string.IsNullOrWhiteSpace(jobFilePath))
                throw new ArgumentException("The update job path was not supplied.", "jobFilePath");

            string fullPath = Path.GetFullPath(jobFilePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("The update job could not be found.", fullPath);

            UpdateJob job = JsonConvert.DeserializeObject<UpdateJob>(File.ReadAllText(fullPath));
            if (job == null)
                throw new InvalidDataException("The update job is empty or invalid.");

            job.JobFilePath = fullPath;
            if (job.QuartzProcessIds == null)
                job.QuartzProcessIds = new List<int>();

            return job;
        }

        public static void SaveUpdateJob(UpdateJob job)
        {
            if (job == null || string.IsNullOrWhiteSpace(job.JobFilePath))
                throw new InvalidOperationException("The update job has no journal path.");

            string directory = Path.GetDirectoryName(job.JobFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = job.JobFilePath + ".tmp";
            string backupPath = job.JobFilePath + ".bak";
            File.WriteAllText(
                temporaryPath,
                JsonConvert.SerializeObject(job, Formatting.Indented));

            if (File.Exists(job.JobFilePath))
            {
                File.Replace(temporaryPath, job.JobFilePath, backupPath, true);

                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            else
            {
                File.Move(temporaryPath, job.JobFilePath);
            }
        }

        internal static Version GetExecutableVersion(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            {
                throw new FileNotFoundException(
                    "Quartz.exe was not found. QuartzUpdater.exe must be placed beside Quartz.exe.",
                    executablePath);
            }

            FileVersionInfo information = FileVersionInfo.GetVersionInfo(executablePath);
            string value = information.FileVersion;

            if (string.IsNullOrWhiteSpace(value))
                value = information.ProductVersion;

            Version version;
            if (!TryParseVersion(value, out version))
            {
                throw new InvalidDataException(
                    "Quartz.exe has an invalid file version: " + (value ?? "(missing)"));
            }

            return NormalizeVersion(version);
        }

        internal static Version NormalizeVersion(Version version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            int minor = version.Minor < 0 ? 0 : version.Minor;
            int build = version.Build < 0 ? 0 : version.Build;
            return new Version(version.Major, minor, build);
        }

        internal static void ValidatePayload(string payloadDirectory, Version expectedVersion)
        {
            string quartzExecutable = Path.Combine(payloadDirectory, "Quartz.exe");
            string updaterExecutable = Path.Combine(payloadDirectory, "QuartzUpdater.exe");

            if (!File.Exists(quartzExecutable))
                throw new InvalidDataException("The update package does not contain Quartz.exe.");

            if (!File.Exists(updaterExecutable))
            {
                throw new InvalidDataException(
                    "The update package does not contain QuartzUpdater.exe. " +
                    "It must be included so future updates continue to work.");
            }

            Version packageVersion = GetExecutableVersion(quartzExecutable);
            if (packageVersion.CompareTo(NormalizeVersion(expectedVersion)) != 0)
            {
                throw new InvalidDataException(
                    "The package contains Quartz " + packageVersion +
                    ", but GitHub says the release is " + NormalizeVersion(expectedVersion) + ".");
            }
        }

        internal static List<int> FindRunningQuartzProcesses(string installDirectory)
        {
            var processIds = new List<int>();
            string expectedPath = Path.GetFullPath(
                Path.Combine(installDirectory, "Quartz.exe"));

            foreach (Process process in Process.GetProcessesByName("Quartz"))
            {
                try
                {
                    string processPath = process.MainModule == null
                        ? null
                        : process.MainModule.FileName;

                    if (!string.IsNullOrEmpty(processPath) &&
                        string.Equals(
                            Path.GetFullPath(processPath),
                            expectedPath,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        processIds.Add(process.Id);
                    }
                }
                catch
                {
                    // A process that cannot be inspected is not safe to close automatically.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return processIds.Distinct().ToList();
        }

        internal static string GetUpdatesRoot()
        {
            if (!string.IsNullOrWhiteSpace(UpdatesRootOverride))
                return Path.GetFullPath(UpdatesRootOverride);

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Xaftellis",
                "Quartz",
                "Updates");
        }

        internal static void CleanupTerminalOperation(UpdateJob job)
        {
            if (job == null || !IsTerminalCleanupState(job.State))
                return;

            try
            {
                string updatesRoot = Path.GetFullPath(GetUpdatesRoot())
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string operationDirectory = Path.GetFullPath(job.OperationDirectory)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (!IsPathInside(operationDirectory, updatesRoot))
                    return;

                if (!string.Equals(
                    Path.GetFileName(operationDirectory),
                    job.Id,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                BestEffortDeleteDirectory(operationDirectory);
                TryDeleteDirectoryIfEmpty(Path.GetDirectoryName(operationDirectory));
            }
            catch
            {
                // Cleanup is retried on a future normal updater launch.
            }
        }

        private static async Task<ReleaseInfo> GetLatestReleaseAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                using (HttpResponseMessage response = await Client.GetAsync(
                    LatestReleaseUrl,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "GitHub returned " + (int)response.StatusCode + " " +
                            response.ReasonPhrase + " while checking for updates.");
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    JObject release = JObject.Parse(json);

                    string tag = (string)release["tag_name"];
                    if (string.IsNullOrWhiteSpace(tag))
                        throw new InvalidDataException("The latest GitHub release has no version tag.");

                    string versionText = tag.TrimStart('v', 'V');
                    Version parsedVersion;
                    if (!Version.TryParse(versionText, out parsedVersion))
                    {
                        throw new InvalidDataException(
                            "The GitHub release tag is not a valid version: " + tag);
                    }

                    Version version = NormalizeVersion(parsedVersion);
                    string expectedAssetName = "Quartz." + tag + ".zip";

                    JArray assets = release["assets"] as JArray;
                    if (assets == null)
                        throw new InvalidDataException("The latest GitHub release has no asset list.");

                    List<JObject> matchingAssets = assets
                        .OfType<JObject>()
                        .Where(asset => string.Equals(
                            (string)asset["name"],
                            expectedAssetName,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matchingAssets.Count != 1)
                    {
                        throw new InvalidDataException(
                            "Expected exactly one release asset named " + expectedAssetName +
                            ", but found " + matchingAssets.Count + ".");
                    }

                    JObject packageAsset = matchingAssets[0];
                    string downloadUrl = (string)packageAsset["browser_download_url"];
                    if (string.IsNullOrWhiteSpace(downloadUrl))
                        throw new InvalidDataException("The release asset has no download URL.");

                    return new ReleaseInfo
                    {
                        Tag = tag,
                        Version = version,
                        AssetName = expectedAssetName,
                        DownloadUrl = downloadUrl,
                        AssetSize = (long?)packageAsset["size"] ?? 0
                    };
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                UpdateLog.Write(exception);
                throw new InvalidOperationException(
                    "Quartz could not retrieve the latest release information. " +
                    exception.Message,
                    exception);
            }
        }

        private static async Task DownloadFileAsync(
            string downloadUrl,
            string destinationPath,
            long expectedSize,
            IProgress<UpdateProgress> progress,
            CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await Client.GetAsync(
                downloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                long totalSize = expectedSize > 0
                    ? expectedSize
                    : (response.Content.Headers.ContentLength ?? 0);

                using (Stream input = await response.Content.ReadAsStreamAsync())
                using (var output = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    true))
                {
                    var buffer = new byte[81920];
                    long downloaded = 0;
                    int bytesRead;

                    while ((bytesRead = await input.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        cancellationToken)) > 0)
                    {
                        await output.WriteAsync(
                            buffer,
                            0,
                            bytesRead,
                            cancellationToken);

                        downloaded += bytesRead;
                        int percent = totalSize > 0
                            ? (int)Math.Min(60, downloaded * 60L / totalSize)
                            : 0;

                        Report(
                            progress,
                            "Downloading update",
                            percent,
                            totalSize > 0
                                ? FormatBytes(downloaded) + " of " + FormatBytes(totalSize)
                                : FormatBytes(downloaded));
                    }
                }
            }
        }

        private static void ValidateDownloadedSize(string packagePath, long expectedSize)
        {
            if (expectedSize <= 0)
                return;

            long actualSize = new FileInfo(packagePath).Length;
            if (actualSize != expectedSize)
            {
                throw new InvalidDataException(
                    "The downloaded ZIP is incomplete. Expected " + expectedSize +
                    " bytes but received " + actualSize + ".");
            }
        }

        private static async Task<string> ExtractPackageAsync(
            string packagePath,
            string extractionDirectory,
            IProgress<UpdateProgress> progress,
            CancellationToken cancellationToken)
        {
            if (Directory.Exists(extractionDirectory))
                Directory.Delete(extractionDirectory, true);

            Directory.CreateDirectory(extractionDirectory);
            string extractionRoot = Path.GetFullPath(extractionDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string extractionRootWithSeparator = extractionRoot + Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(packagePath))
            {
                long totalLength = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    checked { totalLength += entry.Length; }
                }

                string driveRoot = Path.GetPathRoot(extractionRoot);
                if (!string.IsNullOrEmpty(driveRoot))
                {
                    long available = new DriveInfo(driveRoot).AvailableFreeSpace;
                    if (totalLength > available)
                        throw new IOException("There is not enough disk space to extract the update.");
                }

                long extractedLength = 0;
                int entryIndex = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    entryIndex++;

                    string destinationPath = Path.GetFullPath(
                        Path.Combine(extractionRoot, entry.FullName));

                    if (!destinationPath.StartsWith(
                        extractionRootWithSeparator,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException(
                            "The ZIP contains an unsafe path: " + entry.FullName);
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string parentDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(parentDirectory))
                        Directory.CreateDirectory(parentDirectory);

                    using (Stream input = entry.Open())
                    using (var output = new FileStream(
                        destinationPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        true))
                    {
                        await input.CopyToAsync(output, 81920, cancellationToken);
                    }

                    extractedLength += entry.Length;
                    int percent = totalLength > 0
                        ? 68 + (int)Math.Min(22, extractedLength * 22L / totalLength)
                        : 68 + (int)(entryIndex * 22L / Math.Max(1, archive.Entries.Count));

                    Report(progress, "Extracting update", percent, entry.FullName);
                }
            }

            string rootQuartz = Path.Combine(extractionRoot, "Quartz.exe");
            if (File.Exists(rootQuartz))
                return extractionRoot;

            string[] topLevelFiles = Directory.GetFiles(extractionRoot);
            string[] topLevelDirectories = Directory.GetDirectories(extractionRoot);
            if (topLevelFiles.Length == 0 &&
                topLevelDirectories.Length == 1 &&
                File.Exists(Path.Combine(topLevelDirectories[0], "Quartz.exe")))
            {
                return topLevelDirectories[0];
            }

            throw new InvalidDataException(
                "The ZIP must contain Quartz.exe at its root, or inside one top-level folder.");
        }

        private static void EnsureInstallLocationCanBeUpdated(string installDirectory)
        {
            string fullDirectory = Path.GetFullPath(installDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string root = Path.GetPathRoot(fullDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.Equals(fullDirectory, root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Quartz cannot be updated directly in a drive root.");

            string parent = Path.GetDirectoryName(fullDirectory);
            if (string.IsNullOrWhiteSpace(parent))
                throw new InvalidOperationException("The Quartz installation directory is invalid.");

            string probePath = Path.Combine(
                parent,
                ".quartz-update-permission-" + Guid.NewGuid().ToString("N") + ".tmp");

            try
            {
                using (File.Create(probePath))
                {
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new UnauthorizedAccessException(
                    "QuartzUpdater does not have permission to update this installation. " +
                    "Run it as administrator and try again.",
                    exception);
            }
            finally
            {
                try
                {
                    if (File.Exists(probePath))
                        File.Delete(probePath);
                }
                catch
                {
                }
            }
        }

        private static bool TryParseVersion(string value, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (Version.TryParse(value.Trim(), out version))
                return true;

            Match match = Regex.Match(value, @"\d+(?:\.\d+){1,3}");
            return match.Success && Version.TryParse(match.Value, out version);
        }

        private static HttpClient CreateHttpClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("QuartzUpdater/1.2.9");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            return client;
        }

        private static string GetWorkerRoot()
        {
            return Path.Combine(GetUpdatesRoot(), "Workers");
        }

        private static void CleanupAbandonedOperations()
        {
            string updatesRoot = GetUpdatesRoot();
            if (!Directory.Exists(updatesRoot))
                return;

            foreach (string versionDirectory in Directory.GetDirectories(updatesRoot))
            {
                Version ignoredVersion;
                if (!Version.TryParse(Path.GetFileName(versionDirectory), out ignoredVersion))
                    continue;

                foreach (string operationDirectory in Directory.GetDirectories(versionDirectory))
                {
                    string operationId = Path.GetFileName(operationDirectory);
                    Guid ignoredId;
                    if (!Guid.TryParseExact(operationId, "N", out ignoredId))
                        continue;

                    string workerDirectory = Path.Combine(GetWorkerRoot(), operationId);
                    if (IsProcessRunningFromDirectory(workerDirectory))
                        continue;

                    string jobFilePath = Path.Combine(operationDirectory, "update-job.json");
                    if (!File.Exists(jobFilePath))
                        continue;

                    try
                    {
                        UpdateJob job = LoadUpdateJob(jobFilePath);
                        if (string.Equals(
                            job.Id,
                            operationId,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            CleanupTerminalOperation(job);
                        }
                    }
                    catch
                    {
                        // Unknown or damaged jobs are retained for manual inspection.
                    }
                }

                TryDeleteDirectoryIfEmpty(versionDirectory);
            }
        }

        private static void CleanupOldWorkerDirectories()
        {
            string workerRoot = GetWorkerRoot();
            if (!Directory.Exists(workerRoot))
                return;

            string currentBase = Path.GetFullPath(AppContext.BaseDirectory);
            foreach (string directory in Directory.GetDirectories(workerRoot))
            {
                try
                {
                    string fullDirectory = Path.GetFullPath(directory)
                        .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                    if (currentBase.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (IsProcessRunningFromDirectory(directory))
                        continue;

                    Directory.Delete(directory, true);
                }
                catch
                {
                }
            }
        }

        private static bool IsTerminalCleanupState(string state)
        {
            return string.Equals(state, "FailedBeforeInstall", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(state, "RolledBack", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(state, "Complete", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPathInside(string candidatePath, string parentDirectory)
        {
            string candidate = Path.GetFullPath(candidatePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = Path.GetFullPath(parentDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return string.Equals(candidate, parent, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(
                    parent + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsProcessRunningFromDirectory(string directory)
        {
            if (ProcessRunningOverride != null)
                return ProcessRunningOverride(directory);

            string directoryPrefix = Path.GetFullPath(directory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;

            foreach (Process process in Process.GetProcessesByName("QuartzUpdater"))
            {
                try
                {
                    string processPath = process.MainModule == null
                        ? null
                        : process.MainModule.FileName;

                    if (!string.IsNullOrEmpty(processPath) &&
                        Path.GetFullPath(processPath).StartsWith(
                            directoryPrefix,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    // If a QuartzUpdater process cannot be inspected, keep files conservatively.
                    return true;
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        private static void TryDeleteDirectoryIfEmpty(string directory)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(directory) &&
                    Directory.Exists(directory) &&
                    Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch
            {
            }
        }

        private static void BestEffortDeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
            }
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }

        private static string FormatBytes(long value)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double size = value;
            int unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return size.ToString(unit == 0 ? "0" : "0.0") + " " + units[unit];
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
