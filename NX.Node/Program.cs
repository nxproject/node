///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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
        static void Main(string[] args)
        {
            // Create working environment
            EnvironmentClass c_Env = new EnvironmentClass(args);

            // Only outside a container
            if (!"".InContainer())
            {
                // Do we need to make into container?
                if (c_Env.MakeGenome)
                {
                    // Get the root folder
                    string sRootFolder = "".WorkingDirectory();
                    // Do we have a bin?
                    int iPos = sRootFolder.IndexOf(@"\bin\");
                    if (iPos != -1)
                    {
                        // Remove tail
                        sRootFolder = sRootFolder.Substring(0, iPos);
                    }
                    // Get the name
                    sRootFolder = sRootFolder.GetDirectoryNameFromPath();

                    // Get the working directory
                    string sWD = "".WorkingDirectory();
                    string sDir = sWD;
                    // Find ourselves and extract the root
                    sDir = sDir.Replace(sRootFolder, "{0}");
                    // And make the output directory
                    // A folder inside ourselves
                    string sOut = "/build/container";
                    // And it is empty
                    sOut.DeletePath();
                    // Make sure it exists
                    sOut.AssurePath();

                    // Copy
                    CopyFolder(c_Env, sWD, sOut);

                    // The modules folder
                    string sMod = sOut.CombinePath("modules");
                    // Assure
                    sMod.AssurePath();
                    // And the UI folder
                    string sUI = sOut.CombinePath("modulesui");
                    // Assure
                    sUI.AssurePath();

                    // Get the root
                    string sRoot = sDir.Substring(0, sDir.IndexOf("{0}") - 1);
                    // Now cut back one
                    sDir = sDir.Substring(sDir.IndexOf("{0}") + 3);

                    //
                    CopyTree(c_Env, sRoot, sDir, sMod, sUI, sRootFolder);

                    // Get the extra folders
                    List<string> c_Folders = c_Env.GetAsJArray(EnvironmentClass.KeyCodeFolder).ToList();
                    // Loop thru
                    foreach (string sFolder in c_Folders)
                    {
                        // Get the directory
                        string sSource = sFolder;
                        // Add extra delim
                        if (!sSource.EndsWith(@"\")) sSource += @"\";

                        // Root of many projects?
                        if (!CopyTree(c_Env, sSource, sDir, sMod, sUI))
                        {
                            // If not, see if it is a single project
                            if(!CopyProject(c_Env, sSource, sDir, sMod, sUI))
                            {
                                CopyFolder(c_Env, sSource, sMod);
                            }
                        }
                    }

                    // Make into image
                    c_Env.Hive.MakeSelfIntoGenome(sOut);
                    // Delete
                    sOut.DeletePath();

                    // Kill all
                    c_Env.Hive.KillProcessorBees();

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
                    c_Env.Start();
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
                c_Env.Start();
            }
        }

        #region Support
        private static bool CopyTree(EnvironmentClass env,
                                            string rootdir,
                                            string template,
                                            string moddir,
                                            string uidir,
                                            params string[] skip)
        {
            // Assume failue
            bool bAns = false;

            // Make skip list
            List<string> c_Skip = new List<string>(skip);

            // Find all of the directories
            List<string> c_Dirs = rootdir.GetDirectoriesInPath();
            // Do each one
            foreach (string sSDir in c_Dirs)
            {
                // If not in skip
                if (!c_Skip.Contains(sSDir.GetDirectoryNameFromPath()))
                {
                    //
                    if (CopyProject(env, sSDir, template, moddir, uidir)) bAns = true;
                }
            }

            return bAns;
        }

        private static bool CopyProject(EnvironmentClass env,
                                            string sourcedir,
                                            string template,
                                            string moddir,
                                            string uidir)
        {
            // Assume failure
            bool bAns = false;

            // Get the name
            string sAName = sourcedir.GetDirectoryNameFromPath();

            // According to source
            if (sAName.IsSameValue("docs"))
            {
                // Do not copy
            }
            else if (sAName.StartsWith("UI."))
            {
                // Make sub folder
                string sSAUI = uidir.CombinePath(sAName.Substring(1 + sAName.IndexOf(".")).ToLower());
                sSAUI.AssurePath();

                // UI
                bAns = CopyFolder(env, sourcedir, sSAUI);
            }
            else if (!sAName.StartsWith("."))
            {
                // Make source
                string sADir = sourcedir + template;

                // Copy all the files to modules
                bAns = CopyFolder(env, sADir, moddir);
            }

            return bAns;
        }

        private static bool CopyFolder(EnvironmentClass env, string source, string target)
        {
            // Assume failure
            bool bAns = false;

            if (!source.Contains(";"))
            {
                // Tell user
                env.LogVerbose(source + " => " + target);

                // Make path
                target.AssurePath();

                // The list
                List<string> c_Files = source.GetFilesInPath();
                // Do each file
                foreach (string sFile in c_Files)
                {
                    // Get the actual file name
                    string sName = sFile.GetFileNameFromPath();
                    // Copy
                    sFile.CopyFile(target + @"\" + sName);
                    //
                    bAns = true;
                }

                // And now each directory
                foreach (string sDir in source.GetDirectoriesInPath())
                {
                    // Copy
                    if (CopyFolder(env, sDir, target + @"\" + sDir.GetDirectoryNameFromPath())) bAns = true;
                }
            }

            return bAns;
        }
        #endregion
    }
}