﻿using System;
using System.Diagnostics;
using System.IO;
using Onova.Updater.Internal;

namespace Onova.Updater
{
    // This executable applies the update by copying over new files.
    // It's required because updatee cannot update itself while the files are still in use.

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 4)
                throw new ArgumentException("Not all arguments have been specified.");

            // Extract arguments
            var updateeProcessId = int.Parse(args[0]);
            var updateeFilePath = args[1];
            var packageContentDirPath = args[2];
            var restartUpdatee = bool.Parse(args[3]);

            // Wait until updatee dies
            ProcessEx.WaitForExit(updateeProcessId);

            // Copy over the extracted package
            DirectoryEx.Copy(packageContentDirPath, Path.GetDirectoryName(updateeFilePath));

            // Launch the updatee again if requested
            if (restartUpdatee)
                Process.Start(updateeFilePath);

            // Delete package directory
            Directory.Delete(packageContentDirPath, true);
        }
    }
}