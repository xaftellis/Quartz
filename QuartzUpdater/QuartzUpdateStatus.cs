using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace QuartzUpdater
{
    public sealed class QuartzUpdateStatus
    {
        public const int CurrentSchemaVersion = 1;
        public const string UpdateAvailableState = "updateAvailable";
        public const string UpToDateState = "upToDate";

        [JsonProperty("schemaVersion", Order = 1)]
        public int SchemaVersion { get; set; }

        [JsonProperty("state", Order = 2)]
        public string State { get; set; }

        [JsonProperty("checkedAtUtc", Order = 3)]
        public DateTime CheckedAtUtc { get; set; }

        [JsonProperty("installedVersion", Order = 4)]
        public string InstalledVersion { get; set; }

        [JsonProperty("latestVersion", Order = 5)]
        public string LatestVersion { get; set; }

        [JsonIgnore]
        public bool IsUpdateAvailable
        {
            get
            {
                return string.Equals(
                    State,
                    UpdateAvailableState,
                    StringComparison.Ordinal);
            }
        }

        [JsonIgnore]
        public bool IsUpToDate
        {
            get
            {
                return string.Equals(
                    State,
                    UpToDateState,
                    StringComparison.Ordinal);
            }
        }

        public bool IsOlderThan(TimeSpan maximumAge)
        {
            if (maximumAge < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("maximumAge");

            DateTime checkedUtc = CheckedAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(CheckedAtUtc, DateTimeKind.Utc)
                : CheckedAtUtc.ToUniversalTime();

            return checkedUtc < DateTime.UtcNow.Subtract(maximumAge);
        }

        public static QuartzUpdateStatus CreateUpdateAvailable(
            Version installedVersion,
            Version latestVersion)
        {
            return Create(
                UpdateAvailableState,
                installedVersion,
                latestVersion);
        }

        public static QuartzUpdateStatus CreateUpToDate(
            Version installedVersion,
            Version latestVersion)
        {
            return Create(
                UpToDateState,
                installedVersion,
                latestVersion);
        }

        private static QuartzUpdateStatus Create(
            string state,
            Version installedVersion,
            Version latestVersion)
        {
            if (installedVersion == null)
                throw new ArgumentNullException("installedVersion");
            if (latestVersion == null)
                throw new ArgumentNullException("latestVersion");

            return new QuartzUpdateStatus
            {
                SchemaVersion = CurrentSchemaVersion,
                State = state,
                CheckedAtUtc = DateTime.UtcNow,
                InstalledVersion = installedVersion.ToString(),
                LatestVersion = latestVersion.ToString()
            };
        }
    }

    public static class QuartzUpdateStatusStore
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

        // Tests can redirect this without changing the production default.
        internal static string DefaultFilePathOverride { get; set; }

        public static string DefaultFilePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DefaultFilePathOverride))
                    return Path.GetFullPath(DefaultFilePathOverride);

                return Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "Xaftellis",
                    "Quartz",
                    "UpdateStatus.json");
            }
        }

        public static bool TryRead(out QuartzUpdateStatus status)
        {
            return TryRead(DefaultFilePath, out status);
        }

        public static bool TryRead(
            string filePath,
            out QuartzUpdateStatus status)
        {
            status = null;

            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return false;

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                QuartzUpdateStatus candidate =
                    JsonConvert.DeserializeObject<QuartzUpdateStatus>(
                        json,
                        SerializerSettings);

                if (!IsValid(candidate))
                    return false;

                status = candidate;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Write(QuartzUpdateStatus status)
        {
            Write(DefaultFilePath, status);
        }

        public static void Write(
            string filePath,
            QuartzUpdateStatus status)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "The update-status file path is missing.",
                    "filePath");
            if (!IsValid(status))
                throw new InvalidDataException("The update status is invalid.");

            string fullPath = Path.GetFullPath(filePath);
            string directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidDataException(
                    "The update-status directory is invalid.");

            Directory.CreateDirectory(directory);

            string temporaryPath = fullPath + ".tmp";
            string backupPath = fullPath + ".bak";

            try
            {
                DeleteIfPresent(temporaryPath);
                DeleteIfPresent(backupPath);

                string json = JsonConvert.SerializeObject(
                    status,
                    Formatting.Indented,
                    SerializerSettings);
                File.WriteAllText(
                    temporaryPath,
                    json + Environment.NewLine,
                    new UTF8Encoding(false));

                if (File.Exists(fullPath))
                {
                    File.Replace(
                        temporaryPath,
                        fullPath,
                        backupPath,
                        true);
                    DeleteIfPresent(backupPath);
                }
                else
                {
                    File.Move(temporaryPath, fullPath);
                }
            }
            finally
            {
                DeleteIfPresent(temporaryPath);
            }
        }

        public static bool TryWrite(QuartzUpdateStatus status)
        {
            return TryWrite(DefaultFilePath, status);
        }

        public static bool TryWrite(
            string filePath,
            QuartzUpdateStatus status)
        {
            try
            {
                Write(filePath, status);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValid(QuartzUpdateStatus status)
        {
            if (status == null ||
                status.SchemaVersion != QuartzUpdateStatus.CurrentSchemaVersion ||
                status.CheckedAtUtc == default(DateTime))
            {
                return false;
            }

            bool knownState = string.Equals(
                    status.State,
                    QuartzUpdateStatus.UpdateAvailableState,
                    StringComparison.Ordinal) ||
                string.Equals(
                    status.State,
                    QuartzUpdateStatus.UpToDateState,
                    StringComparison.Ordinal);

            Version ignoredVersion;
            return knownState &&
                Version.TryParse(status.InstalledVersion, out ignoredVersion) &&
                Version.TryParse(status.LatestVersion, out ignoredVersion);
        }

        private static void DeleteIfPresent(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
            }
        }
    }
}
