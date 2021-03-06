﻿///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
///--------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using NX.Engine;
using NX.Shared;

namespace NXNode
{
    class Program
    {
        #region Entry point
        static void Main(string[] args)
        {
            // Create working environment
            EnvironmentClass c_Env = new EnvironmentClass(args);

            // Only outside a container
            if (!"".InContainer())
            {
                // The working code folders
                List<string> c_CodeFolders = c_Env.GetAsJArray("code_folder").ToList(); ;
                // Get the root folder
                string sBaseFolder = "".WorkingDirectory();
                // Do we have a bin?
                int iPos = sBaseFolder.IndexOf(@"\bin\");
                if (iPos != -1)
                {
                    // Remove tail
                    sBaseFolder = sBaseFolder.Substring(0, iPos);
                }
                // Add parent
                //c_CodeFolders.Add(sBaseFolder.Substring(0, sBaseFolder.LastIndexOf(@"\")));

                // Loop thru
                foreach (string sStartFolder in c_CodeFolders)
                {
                    // Loop thru children
                    foreach (string sAtDir in sStartFolder.GetDirectoriesInPath())
                    {
                        // Skip if working directory
                        if (!sBaseFolder.IsSameValue(sAtDir))
                        {
                            // Get the folder name
                            string sFolder = sAtDir.GetDirectoryNameFromPath();
                            // UI?
                            if (sFolder.StartsWith("UI.", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Set target
                                string sTargetDir = "".WorkingDirectory().CombinePath(sFolder.ToLower()).AdjustPathToOS();
                                // Assure
                                sTargetDir.AssurePath();

                                // UI
                                CopyFolder(sAtDir, sTargetDir);
                            }
                            else
                            {
                                // Move to bin
                                string sBinFolder = sAtDir.CombinePath("bin").CombinePath("".InDebug() ? "Debug" : "Release").AdjustPathToOS();
                                // Loop thru code
                                foreach (string sCodeFolder in sBinFolder.GetDirectoriesInPath())
                                {
                                    // Do the folder
                                    CopyFolder(sCodeFolder, "".WorkingDirectory());
                                }
                            }
                        }
                    }
                }

                // Do we need to recycle?
                if(c_Env["destroy_hive"].FromDBBoolean())
                {
                    // Bye
                    c_Env.Hive.DestroyHive();
                }

                // Make into image
                if (c_Env.MakeGenome)
                {
                    // Make
                    c_Env.Hive.MakeSelfIntoGenome("".WorkingDirectory());

                    // Kill processors
                    c_Env.Hive.KillProcessorBees();
                    // And Nginx
                    c_Env.Hive.KillDNA("nginx");

                    //
                    c_Env.LogInfo("Container has been created");
                }

                // Do we make a bee?
                if (c_Env.MakeBee)
                {
                    // Make a bee in the hive
                    NX.Engine.Hive.BeeClass c_Bee = c_Env.Hive.MakeWorkerBee(c_Env.Process);
                    //
                    c_Env.LogInfo(@"Bee of proc=""{0}"" has {1} been created".FormatString(c_Env.Process, c_Bee == null ? "not " : ""));
                }

                // Normal mode?
                if (!c_Env.InMakeMode)
                {
                    // And recycle
                    Boot(c_Env);
                }
                else
                {
                    // Bye
                    Environment.Exit(0);
                }
            }
            else
            {
                // Only thing we are allowed inside a container
                Boot(c_Env);
            }
        }
        #endregion

        #region Support
        /// <summary>
        /// 
        /// Starts the process with the built in items
        /// 
        /// </summary>
        /// <param name="env"></param>
        private static void Boot(EnvironmentClass env)
        {
            // Load basic
            env.Start(true, "Fn.System", "Proc.Default", "Route.System", "Route.UI");

            // Kick the bee
            if (!env.InMakeMode)
            {
                env.Hive.Roster.Refresh();
            }
        }

        /// <summary>
        /// 
        /// Copies a code/data folder into the working directory
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void CopyFolder(string source, string target)
        {
            // Assure target
            target.AssurePath();

            // Console.WriteLine("Loading " + source);

            // The list
            List<string> c_Files = source.GetFilesInPath();
            // Do each file
            foreach (string sFile in c_Files)
            {
                // Copy
                sFile.CopyFile(target.CombinePath(sFile.GetFileNameFromPath().AdjustPathToOS()));
            }

            // And now each directory
            foreach (string sDir in source.GetDirectoriesInPath())
            {
                // Get the name
                string sName = sDir.GetDirectoryNameFromPath();
                // Skip system
                if (!sName.StartsWith("."))
                {
                    // Copy
                    CopyFolder(source.CombinePath(sName).AdjustPathToOS(),
                                target.CombinePath(sName).AdjustPathToOS());
                }
            }
        }
        #endregion
    }
}