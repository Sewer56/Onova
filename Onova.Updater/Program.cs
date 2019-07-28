﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Onova.Updater.Internal;

namespace Onova.Updater
{
    // This executable applies the update by copying over new files.
    // It's required because updatee cannot update itself while the files are still in use.

    public static class Program
    {
        private static TextWriter _log;

        private static string AssemblyDirPath => AppDomain.CurrentDomain.BaseDirectory;
        private static string LogFilePath => Path.Combine(AssemblyDirPath, "Log.txt");
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static void WriteLog(string value)
        {
            var date = DateTimeOffset.Now;
            _log.WriteLine($"{date:dd-MMM-yyyy HH:mm:ss.fff}> {value}");
        }

        public static void Main(string[] args)
        {
            // Write log
            using (_log = File.CreateText(LogFilePath))
            {
                // Launch info
                WriteLog($"Onova Updater v{Version} started with args: [{args.JoinToString(", ")}].");

                try
                {
                    // Extract arguments
                    var updateeFilePath = args[0];
                    var packageContentDirPath = args[1];
                    var restartUpdatee = bool.Parse(args[2]);

                    // Execute update
                    Update(updateeFilePath, packageContentDirPath, restartUpdatee);
                    WriteLog("Update completed successfully.");
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
            }
        }

        private static void Update(string updateeFilePath, string packageContentDirPath, bool restartUpdatee)
        {
            // Wait until updatee is writable to ensure all running instances have exited
            WriteLog("Waiting for all running updatee instances to exit...");
            while (!FileEx.CheckWriteAccess(updateeFilePath))
                Thread.Sleep(100);

            // Copy over the package contents
            WriteLog("Copying package contents from storage to updatee's directory...");
            var updateeDirPath = Path.GetDirectoryName(updateeFilePath);
            DirectoryEx.Copy(packageContentDirPath, updateeDirPath);

            // Launch the updatee again if requested
            if (restartUpdatee)
            {
                WriteLog("Restarting updatee...");

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = updateeFilePath; // Specify exe name.
                start.UseShellExecute = false;
                start.WorkingDirectory = Path.GetDirectoryName(updateeFilePath);

                using (var restartedUpdateeProcess = Process.Start(start))
                    WriteLog($"Restarted as pid:{restartedUpdateeProcess?.Id}.");
            }

            // Delete package content directory
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(packageContentDirPath, true);
        }
    }
}