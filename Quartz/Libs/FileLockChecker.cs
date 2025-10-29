using System;
using System.IO;

namespace Quartz.Libs
{
    public class FileLockChecker
    {
        // Main method to check if any file or folder is locked in a directory and its subdirectories
        public static bool IsLocked(string path)
        {
            if (Directory.Exists(path))
            {
                // Check if any file or folder in the directory is locked (including subdirectories)
                return CheckDirectory(path);
            }
            else if (File.Exists(path))
            {
                // If it's a single file, check if it's locked
                return IsFileLocked(path);
            }
            return false; // Path does not exist or is not a file/folder
        }

        // Method to check if a file is locked
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // If we can open the file with exclusive access, it's not locked
                    return false;
                }
            }
            catch (IOException)
            {
                // If an IOException occurs, it indicates the file is locked
                return true;
            }
        }

        // Method to recursively check a directory and all its subdirectories and files
        private static bool CheckDirectory(string directoryPath)
        {
            try
            {
                // Check all files in the directory
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    if (IsFileLocked(file))
                    {
                        return true; // Return true if any file is locked
                    }
                }

                // Recursively check all subdirectories
                foreach (var subDirectory in Directory.GetDirectories(directoryPath))
                {
                    if (CheckDirectory(subDirectory))
                    {
                        return true; // Return true if any subdirectory or file is locked
                    }
                }

                return false; // No locked files or directories found
            }
            catch (UnauthorizedAccessException)
            {
                // If access is denied to any part of the directory, consider it as "locked"
                return true;
            }
            catch (Exception)
            {
                // Catch any other exceptions that may occur during directory traversal
                return true;
            }
        }
    }
}