///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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

/// Packet Manager Requirements
/// 
/// Install-Package Octokit -Version 0.48.0
/// 

using System;
using System.Collections.Generic;

using Octokit;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// This class interfaces with a Git site and uses it
    /// to fetch files.  If the file being fetched is a .cs
    /// and the target folder is the dynamic folder, the
    /// file is compiled and stored as a .dll
    /// 
    /// </summary>
    public class GitClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The environment object</param>
        public GitClass(EnvironmentClass env)
            : base(env)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Fetches a single file from a repo
        /// 
        /// </summary>
        /// <param name="repo">Name of the repo (Owner/Name)</param>
        /// <param name="file">The file to be fetched (Sample.cs)</param>
        /// <param name="folder">The target folder (/code) [Defaults to dynamic folder]</param>
        /// <return>The file retrieved if any</return>
        public string FetchFile(string repo, string file, string folder = null)
        {
            // Assume none retrieved
            string sAns = null;

            // Get the token
            string sToken = this.Parent.GitToken;
            // See if it is in repo string
            int iPos = repo.IndexOf("@");
            // Any?
            if(iPos != -1)
            {
                // Parse
                sToken = repo.Substring(0, iPos);
                repo = repo.Substring(iPos + 1);
            }

            // Only works if a token is supplied
            if (sToken.HasValue())
            {
                // The client
                GitHubClient c_Client = null;
                // Do we have an enterprise system?
                if (this.Parent.GitURL.HasValue())
                {
                    c_Client = new GitHubClient(new ProductHeaderValue(this.Parent.GitProduct), new Uri(this.Parent.GitURL));
                }
                else
                {
                    c_Client = new GitHubClient(new ProductHeaderValue(this.Parent.GitProduct));
                }

                // And use token
                c_Client.Credentials = new Credentials(sToken);

                // Build he query
                SearchCodeRequest c_Req = new SearchCodeRequest(file)
                {
                    In = new CodeInQualifier[] { CodeInQualifier.Path },
                    Repos = new RepositoryCollection { repo }
                };
                SearchCodeResult c_List = c_Client.Search.SearchCode(c_Req).Result;
                // Loop thru
                foreach (SearchCode c_Entry in c_List.Items)
                {
                    sAns = this.GetCode(file, c_Entry.Sha, c_Entry.Url, folder);
                    // Only do the latest
                    break;
                }
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Fetches all the files in a repo.
        /// 
        /// </summary>
        /// <param name="repo">Name of the repo (Owner/Name)</param>
        /// <param name="folder">The target folder (/code) [Defaults to dynamic folder]</param>
        /// <returns>The list of files retrieved</returns>
        public List<string> FetchRepo(string repo, string folder = null)
        {
            // Assume none retrieved
            List<string> c_Ans = new List<string>();

            // Get the token
            string sToken = this.Parent.GitToken;
            // See if it is in repo string
            int iPos = repo.IndexOf("@");
            // Any?
            if (iPos != -1)
            {
                // Parse
                sToken = repo.Substring(0, iPos);
                repo = repo.Substring(iPos + 1);
            }

            // Only works if a token is supplied
            if (sToken.HasValue())
            {
                // The client
                GitHubClient c_Client = null;
                // Do we have an enterprise system?
                if (this.Parent.GitURL.HasValue())
                {
                    c_Client = new GitHubClient(new ProductHeaderValue(this.Parent.GitProduct), new Uri(this.Parent.GitURL));
                }
                else
                {
                    c_Client = new GitHubClient(new ProductHeaderValue(this.Parent.GitProduct));
                }

                // And use token
                c_Client.Credentials = new Credentials(sToken);

                // Parse
                string[] asPieces = repo.Split('/');

                // Get the repo
                var c_List = c_Client.Repository.Content.GetAllContents(asPieces[0], asPieces[1]).Result;

                // Loop thru
                foreach (RepositoryContent c_Entry in c_List)
                {
                    // Fetch
                    string sFile =  this.GetCode(c_Entry.Name, c_Entry.Sha, c_Entry.Url, folder);
                    // If retrieved, add to list
                    if (sFile.HasValue()) c_Ans.Add(sFile);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Fetches one file and processes it.
        /// 
        /// </summary>
        /// <param name="name">The name of the file (Program.cs)</param>
        /// <param name="sha">The SHA for the file,  Obtained from Git</param>
        /// <param name="url">The Git URL to be used to retrieve the file</param>
        /// <param name="folder">The target folder (/code) [Defaults to dynamic folder]</param>
        /// <returns>The file retrieved if any</returns>
        private string GetCode(string name, string sha, string url, string folder = null)
        {
            // Assume nothing got loaded
            string sAns = null;

            // Set the target folder
            string sTargetFolder = folder.IfEmpty(this.Parent.DynamicFolder);
            // Make sure that it exists
            sTargetFolder.AssurePath();
            // The version folder
            string sVersionFolder = sTargetFolder.CombinePath("_version");
            // Make sure
            sVersionFolder.AssurePath();
            // Get the file name
            string sFileName = name.GetFileNameFromPath();
            // Is it new?
            if (!sha.IsSameValue(sVersionFolder.CombinePath(sFileName).ReadFile()))
            {
                // Get the code
                byte[] abPayload = url.URLGet("token", this.Parent.GitToken,
                    "User-Agent", "nx.server",
                    "Accept", "application/vnd.github.v3+json"
                    );
                // Did we get anything?
                if (abPayload != null)
                {
                    // Must be in the dynamic folder
                    if (sTargetFolder.IsSameValue(this.Parent.DynamicFolder))
                    {
                        // According to extension
                        switch (name.GetExtensionFromPath().ToLower())
                        {
                            case "cs":
                                // Make a new name
                                sFileName = name.GetFileNameOnlyFromPath() + ".dll";
                                // Compile the body
                                abPayload = Compilers.CSharp(sFileName, abPayload);
                                break;

                            case "vb":
                                // Make a new name
                                sFileName = name.GetFileNameOnlyFromPath() + ".dll";
                                // Compile the body
                                abPayload = Compilers.VB(sFileName, abPayload);
                                break;
                        }
                    }

                    // Do we have a payload?
                    if (abPayload != null && name.GetExtensionFromPath().IsSameValue("dll"))
                    {
                        // Got a file
                        sAns = sTargetFolder.CombinePath(sFileName);
                        // Save
                        sAns.WriteFileAsBytes(abPayload);
                        // Save the new version
                        sVersionFolder.CombinePath(sFileName).WriteFile(sha);
                    }
                }
            }

            return sAns;
        }
        #endregion
    }
}