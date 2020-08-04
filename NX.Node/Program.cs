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

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

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
                    string sMod = sOut + @"\modules";
                    // Assure
                    sMod.AssurePath();

                    // Get the root
                    string sRoot = sDir.Substring(0, sDir.IndexOf("{0}"));
                    // Now cut back one
                    sDir = sDir.Substring(0, sDir.LastIndexOf(@"\"));
                    // Find all of the directories
                    List<string> c_Dirs = sRoot.GetDirectoriesInPath();
                    // Do each one
                    foreach (string sSDir in c_Dirs)
                    {
                        //
                        string sADir = sDir.FormatString(sSDir.GetDirectoryNameFromPath());
                        // Not were we started

                        // Look at subdirectories
                        foreach (string sXDir in sADir.GetDirectoriesInPath())
                        {
                            if (!sWD.IsSameValue(sXDir))
                            {
                                // Copy Folder
                                CopyFolder(c_Env, sXDir, sMod);
                            }
                        }
                    }

                    // Get the extra folder
                    string sExtras = c_Env.GenomeSourceFolder;
                    // Do we have one?
                    if (sExtras.HasValue())
                    {
                        // Copy folder
                        CopyFolder(c_Env, sExtras, sMod);
                    }

                    // Make into image
                    c_Env.Hive.MakeSelfIntoGenome(sOut);
                    // Delete
                    sOut.DeletePath();

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

                // Otherwise start
                if (!c_Env.InMakeMode)
                {
                    // And recycle
                    c_Env.Start();
                }
            }
            else
            {
               

                // Only thing we are allowed inside a container
                c_Env.Start();
            }
        }

        private static void CopyFolder(EnvironmentClass env, string source, string target)
        {
            if (!source.Contains(";"))
            {
                // Tell user
                env.LogInfo(source + " => " + target);

                // Make path
                target.AssurePath();

                // Do each file
                foreach (string sFile in source.GetFilesInPath())
                {
                    // Get the actual file name
                    string sName = sFile.GetFileNameFromPath();
                    // Copy
                    sFile.CopyFile(target + @"\" + sName);
                }

                // And now each directory
                foreach (string sDir in source.GetDirectoriesInPath())
                {
                    // Copy
                    CopyFolder(env, sDir, target + @"\" + sDir.GetDirectoryNameFromPath());
                }
            }
        }
    }
}