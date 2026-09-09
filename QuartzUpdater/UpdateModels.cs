using System;
using System.Collections.Generic;

namespace QuartzUpdater
{
    internal sealed class ReleaseInfo
    {
        public string Tag { get; set; }
        public Version Version { get; set; }
        public string AssetName { get; set; }
        public string DownloadUrl { get; set; }
        public long AssetSize { get; set; }
    }

    internal sealed class UpdateCheckResult
    {
        public string InstallDirectory { get; set; }
        public Version InstalledVersion { get; set; }
        public ReleaseInfo Release { get; set; }

        public bool IsUpdateAvailable
        {
            get { return Release.Version.CompareTo(InstalledVersion) > 0; }
        }

        public bool IsDevelopmentBuild
        {
            get { return InstalledVersion.CompareTo(Release.Version) > 0; }
        }
    }

    internal sealed class UpdateProgress
    {
        public UpdateProgress(string stage, int percent, string detail)
        {
            Stage = stage;
            Percent = Math.Max(0, Math.Min(100, percent));
            Detail = detail;
        }

        public string Stage { get; private set; }
        public int Percent { get; private set; }
        public string Detail { get; private set; }
    }

    internal sealed class UpdateJob
    {
        public string Id { get; set; }
        public string InstallDirectory { get; set; }
        public string OperationDirectory { get; set; }
        public string StagingDirectory { get; set; }
        public string PackagePath { get; set; }
        public string ExpectedVersion { get; set; }
        public string ReleaseTag { get; set; }
        public string JobFilePath { get; set; }
        public string LogFilePath { get; set; }
        public string State { get; set; }
        public int ParentUpdaterProcessId { get; set; }
        public List<int> QuartzProcessIds { get; set; }
        public bool RestartAfterUpdate { get; set; }

        public UpdateJob()
        {
            QuartzProcessIds = new List<int>();
            RestartAfterUpdate = true;
        }
    }

    internal sealed class UpdateApplyResult
    {
        public bool Succeeded { get; set; }
        public bool RolledBack { get; set; }
        public string Message { get; set; }
    }

    internal sealed class UpdateApplyException : Exception
    {
        public UpdateApplyException(string message, Exception innerException, bool rolledBack)
            : base(message, innerException)
        {
            RolledBack = rolledBack;
        }

        public bool RolledBack { get; private set; }
    }
}
