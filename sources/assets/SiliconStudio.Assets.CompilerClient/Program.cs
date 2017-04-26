// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SiliconStudio.ExecServer;

namespace SiliconStudio.Assets.CompilerClient
{
    /// <summary>
    /// Small wrapper to communicate through ExecServer to launch Assets.CompilerApp.exe.
    /// The purpose of this small exe is to have the process name called "CompilerClient" instead
    /// of a generic name "ExecServer".
    /// </summary>
    public class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            const string CompilerAppExeName = "SiliconStudio.Assets.CompilerApp.exe";

            var serverApp = new ExecServerApp();
            // The first two parameters are the executable path and the current directory
            var newArgs = new List<string>()
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CompilerAppExeName),
                Environment.CurrentDirectory
            };

            // Set the SiliconStudioXenkoDir environment variable
            var installDir = DirectoryHelper.GetInstallationDirectory("Xenko");
            Environment.SetEnvironmentVariable("SiliconStudioXenkoDir", installDir);

            // Use shadow caching only in dev environment
            if (DirectoryHelper.IsRootDevDirectory(installDir))
            {
                newArgs.Insert(0, "/shadow");
            }

            newArgs.AddRange(args);
            var result = serverApp.Run(newArgs.ToArray());

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            return result;
        }
    }
}
