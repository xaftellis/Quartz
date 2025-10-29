using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using Quartz.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Profile;

namespace Quartz.Libs
{
    internal class msg
    {
        public static void Dialog()
        {
            // Grab all profiles
            var profiles = Program.profileService.All().ToList();

            // Prepare a debug list showing how LastActiveProfile would sort them
            var sortedProfiles = profiles
            .OrderByDescending(p => p.lastActive != default(DateTime))                // Active profiles first
            .ThenByDescending(p => p.lastActive != default(DateTime) ? p.lastActive : DateTime.MinValue) // Most recent lastActive first
            .ThenBy(p => p.dateCreated)                                               // For unused profiles, oldest created first
            .ToList();

            // Build the debug string
            string debugStrings = "";
            for (int i = 0; i < sortedProfiles.Count; i++)
            {
                var p = sortedProfiles[i];
                debugStrings +=
                    $"[{i + 1}] {p.Name}{Environment.NewLine}" +
                    $"    Last Active: {GetTimeAgo(p.lastActive)} ({FormatDate(p.lastActive)}){Environment.NewLine}" +
                    $"    Created:     {GetTimeAgo(p.dateCreated)} ({FormatDate(p.dateCreated)}){Environment.NewLine}" +
                    Environment.NewLine;
            }

            // Show current and last active profile
            var currentProfile = Program.profileService.Get(ProfileService.Current);
            var lastActiveProfile = Program.profileService.Get(Program.profileService.LastActiveProfile());

            string message =
                $"Active Profile:{Environment.NewLine}" +
                $"    {currentProfile.Name} - {GetTimeAgo(currentProfile.lastActive)} ({FormatDate(currentProfile.lastActive)}){Environment.NewLine}{Environment.NewLine}" +

                $"Last Active Profile:{Environment.NewLine}" +
                $"    {lastActiveProfile.Name} - {GetTimeAgo(lastActiveProfile.lastActive)} ({FormatDate(lastActiveProfile.lastActive)}){Environment.NewLine}{Environment.NewLine}" +

                $"Full Sorting Debug:{Environment.NewLine}{debugStrings}";

            MessageBox.Show(message, "Profiles Status");
        }

        public static string GetTimeAgo(DateTime? pastDate)
        {
            if (!pastDate.HasValue || pastDate == default)
                return "Never";

            DateTime date = pastDate.Value;
            TimeSpan timeSpan = DateTime.Now - date;

            if (timeSpan.TotalSeconds < 60)
                return (int)timeSpan.TotalSeconds == 1 ? "1 second ago" : $"{(int)timeSpan.TotalSeconds} seconds ago";
            else if (timeSpan.TotalMinutes < 60)
                return (int)timeSpan.TotalMinutes == 1 ? "1 minute ago" : $"{(int)timeSpan.TotalMinutes} minutes ago";
            else if (timeSpan.TotalHours < 24)
                return (int)timeSpan.TotalHours == 1 ? "1 hour ago" : $"{(int)timeSpan.TotalHours} hours ago";
            else if (timeSpan.TotalDays < 7)
                return (int)timeSpan.TotalDays == 1 ? "1 day ago" : $"{(int)timeSpan.TotalDays} days ago";
            else if (timeSpan.TotalDays < 30)
                return (int)(timeSpan.TotalDays / 7) == 1 ? "1 week ago" : $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            else if (timeSpan.TotalDays < 365)
                return (int)(timeSpan.TotalDays / 30) == 1 ? "1 month ago" : $"{(int)(timeSpan.TotalDays / 30)} months ago";
            else
                return (int)(timeSpan.TotalDays / 365) == 1 ? "1 year ago" : $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

        public static string FormatDate(DateTime? date)
        {
            if (!date.HasValue || date == default)
                return "Never";

            // Example: 09/17/2025 10:45 PM
            return date.Value.ToString("MM/dd/yyyy hh:mm tt");
        }
    }
}
