using Microsoft.Win32;

namespace Quartz.Libs
{
    public static class ThemeHelper
    {
        public static string GetTheme()
        {
            // Registry key for Windows 11 theme
            const string themeKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string themeValue = "AppsUseLightTheme";

            // Get the value from the registry
            object value = Registry.GetValue(themeKey, themeValue, null);

            // Check if the value exists and return the appropriate string
            if (value != null && value is int intValue)
            {
                return intValue == 0 ? "dark" : "light"; // 0 means dark theme, 1 means light theme
            }

            // Default to light theme if the value is not found
            return "light";
        }
    }
}
