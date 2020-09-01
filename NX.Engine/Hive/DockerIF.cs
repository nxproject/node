﻿///--------------------------------------------------------------------------------
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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package Docker.DotNet -Version 3.125.2
/// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;

using NX.Shared;

namespace NX.Engine.Hive
{
    public class DockerIFClass : ChildOfClass<FieldClass>
    {
        #region Constructor
        public DockerIFClass(FieldClass field, string ip)
            : base(field)
        {
            // The URL
            this.URL = ip.URLMake();

            // Can we reach it?
            if (this.IsAlive)
            {
                // Make
                try
                {
                    this.Client = new DockerClientConfiguration(new System.Uri(this.URL)).CreateClient();
                }
                catch { }

                // Do we have a client?
                if (this.Client != null)
                {
                    // Tell user
                    this.Parent.Parent.Parent.LogInfo("URL for field {0} set to {1}", this.Parent.Name, this.URL);
                }
                else
                {
                    // Error
                    this.Parent.Parent.Parent.LogError("Unable to set URL for field {0}", this.Parent.Name);
                }
            }
            else
            {
                // U
                this.Parent.Parent.Parent.LogError("Unable to ping field {0} at {1}", this.Parent.Name, this.URL);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The URL to the Docker deamon
        /// 
        /// </summary>
        public string URL { get; private set; }

        /// <summary>
        /// 
        /// Docker.DotNet client
        /// 
        /// </summary>
        private DockerClient Client { get; set; }

        /// <summary>
        /// 
        /// Is the field reachable?
        /// 
        /// </summary>
        public bool IsAlive
        {
            get { return this.URL.CombineURL("_ping").URLGet().FromBytes().IsSameValue("ok"); }
        }
        #endregion

        #region Images
        /// <summary>
        /// 
        /// Checks to see if image is in local repo
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <returns>True if it exists</returns>
        public bool CheckForImage(DockerIFNameClass name)
        {
            // Assume image not found
            bool bAns = false;

            // Protect
            try
            {
                // Do we have it?
                var c_List = this.Client.Images.ListImagesAsync(new ImagesListParameters()
                {
                }).Result;

                // Get the name
                string sName = name.LocalNameWithTag;

                foreach (var c_Entry in c_List)
                {
                    // Does the repo tag match?
                    bAns = sName.IsSameValue(c_Entry.RepoTags[0]);
                    // Only one
                    if (bAns) break;
                }
            }
            catch (Exception e)
            {
                this.HandleException("ListImagesAsync", e);
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Checks to see if image is in local repo
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <returns>True if it exists</returns>
        public List<string> ListImages(DockerIFNameClass name)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Protect
            try
            {
                // Do we have it?
                var c_List = this.Client.Images.ListImagesAsync(new ImagesListParameters()
                {
                }).Result;

                // Get the name
                string sKey1 = this.Parent.Parent.LabelGenome;
                string sValue1 = name.Name;
                string sKey2 = this.Parent.Parent.LabelHive;
                string sValue2 = this.Parent.Parent.Name;

                foreach (var c_Entry in c_List)
                {
                    // Must match 2
                    int iMatched = 2;

                    // Loop thru
                    foreach (var c_KV in c_Entry.Labels)
                    {
                        if ((c_KV.Value.IsSameValue(sValue1) && c_KV.Key.IsSameValue(sKey1) ||
                            c_KV.Value.IsSameValue(sValue2) && c_KV.Key.IsSameValue(sKey2)))
                        {
                            iMatched--;
                        }
                        if (iMatched <= 0)
                        {
                            break;
                        }
                    }
                    if (iMatched <= 0) c_Ans.Add(c_Entry.ID);
                }
            }
            catch (Exception e)
            {
                this.HandleException("ListImagesAsync", e);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets an image from a repo
        /// 
        /// </summary>
        /// <param name="name">The image name</param>
        /// <param name="cb">The callback when the image is pulled</param>
        public void PullImage(DockerIFNameClass name, Action<bool> cb)
        {
            // Protect
            try
            {
                // Do we have a repo?
                if (!name.Repo.HasValue())
                {
                    // Callback
                    if (cb != null) cb(false);
                }
                else
                {
                    // Make auth
                    AuthConfig c_Auth = this.MakeAuth();

                    // Create image
                    this.Client.Images.CreateImageAsync(new ImagesCreateParameters()
                    {
                        FromImage = name.RepoNameNoTag,
                        Repo = name.LocalNameNoTag,
                        Tag = name.Tag
                    }, c_Auth, new Progress<JSONMessage>(delegate (JSONMessage msg)
                    {
                        switch (msg.Status)
                        {
                            case "completed":
                                // Callback
                                if (cb != null) cb(true);
                                break;

                            case "error":
                                // Callback
                                if (cb != null) cb(false);
                                break;
                        }
                    }));
                }
            }
            catch (Exception e)
            {
                this.HandleException("CreateImageAsync", e);
            }
        }

        /// <summary>
        /// 
        /// Builds an image locally
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <param name="df">The definition (Dockerfile contents)</param>
        public void BuildImage(DockerIFNameClass name, string dir)
        {
            // Make room
            string sFile = null;

            // Protect
            try
            {
                // Make a temp file
                sFile = this.CreateTarballForDockerfile(name, dir);

                // Using a stream so we can handle bigly
                using (FileStream c_Stream = new FileStream(sFile, FileMode.Open))
                {
                    using (Stream c_Result = this.Client.Images.BuildImageFromDockerfileAsync(c_Stream, new ImageBuildParameters()
                    {
                        Dockerfile = "Dockerfile",
                        Tags = new List<string> { name.LocalNameWithTag }
                    }).Result)
                    {
                        //
                        this.DumpStream(c_Result);
                    }
                }
            }
            catch (Exception e)
            {
                this.HandleException("BuildImageFromDockerfileAsync", e);
            }
            finally
            {
                // Did we make a temp?
                if (sFile.HasValue())
                {
                    // Delete temp
                    sFile.DeleteFile();
                }
            }
        }

        ///// <summary>
        ///// 
        ///// 
        ///// </summary>
        ///// <param name="name">The handy dandy name</param>
        //public void PushImage(DockerIFNameClass name)
        //{
        //    if (name.Repo.HasValue())
        //    {
        //        // Protect
        //        try
        //        {
        //            this.Client.Images.PushImageAsync(name.LocalNameNoTag + ":" + name.Version, new ImagePushParameters()
        //            {
        //                Tag = name.Tag

        //            }, this.MakeAuth(), new Progress<JSONMessage>()
        //            {
        //            }).Wait();
        //        }
        //        catch (Exception e)
        //        {
        //            this.HandleException("PushImageAsync", e);
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// Deletes an image from local
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        public void DeleteImage(DockerIFNameClass name)
        {
            // Protect
            try
            {
                // Get the list
                List<string> c_IDs = this.ListImages(name);
                // Loop thru
                foreach (string sID in c_IDs)
                {
                    // Do
                    this.Client.Images.DeleteImageAsync(sID, new ImageDeleteParameters()
                    {
                        Force = true,
                        PruneChildren = true
                    });
                }
            }
            catch (Exception e)
            {
                this.HandleException("DeleteImageAsync", e);
            }
        }

        /// <summary>
        /// 
        /// Deletes all dangling images
        /// 
        /// </summary>
        public void CleanupImages()
        {
            // Protect
            try
            {
                // Delete
                this.Client.Images.PruneImagesAsync(new ImagesPruneParameters()
                {
                });
            }
            catch (Exception e)
            {
                this.HandleException("PruneImagesAsync", e);
            }
        }

        /// <summary>
        ///  Creates a .tar file for use in the build
        ///  
        /// Original found at:
        /// https://github.com/dotnet/Docker.DotNet/issues/309
        /// 
        /// </summary>
        /// <param name="dockerfile">The Dockerfile contents</param>
        /// <returns>A byte array of the .tar file</returns>
        private string CreateTarballForDockerfile(DockerIFNameClass name, string directory)
        {
            // Make the temp file
            string sOut = "".WorkingDirectory().CombinePath("__temp");
            // Delete it
            sOut.DeletePath();

            // Do we have a directory?
            if (directory.HasValue())
            {
                // Get all the files
                var c_Files = directory.GetTreeInPath();

                // Assure
                sOut.AssurePath();
                // Add file name
                sOut = sOut.CombinePath("image.tar").AdjustPathToOS();

                // Make the stream
                FileStream c_Wkg = new FileStream(sOut, FileMode.CreateNew);

                // Open the tarball
                TarOutputStream c_Tarball = new TarOutputStream(c_Wkg);

                // Only do the Dockerfile once (root dir)
                bool bDoDockerfile = true;

                // Loop thru.
                foreach (string sIn in c_Files)
                {
                    //Replacing slashes as KyleGobel suggested and removing leading /
                    string tarName = sIn.Substring(directory.Length).Replace('\\', '/').TrimStart('/');

                    //Let's create the entry header
                    TarEntry c_Entry = TarEntry.CreateTarEntry(tarName);
                    c_Entry.TarHeader.Mode = Convert.ToInt32("100755", 8); //chmod 755

                    // Is this a Dockerfile?
                    if (sIn.GetFileNameFromPath().IsSameValue("Dockerfile") && bDoDockerfile)
                    {
                        // No more
                        bDoDockerfile = false;

                        // Read
                        string sDF = sIn.ReadFile();
                        // Format
                        sDF = this.Parent.Parent.Parent.Format(sDF, true,
                                "proj_label", @"""{0}""=""{1}""".FormatString(this.Parent.Parent.LabelGenome, name.Name));

                        // Set the size
                        c_Entry.Size = sDF.Length;
                        // And write entry
                        c_Tarball.PutNextEntry(c_Entry);

                        // Now the contents
                        c_Tarball.Write(sDF.ToBytes(), 0, sDF.Length);
                    }
                    else
                    {
                        using (FileStream c_Stream = File.OpenRead(sIn))
                        {
                            // Set the size
                            c_Entry.Size = c_Stream.Length;
                            // And write entry
                            c_Tarball.PutNextEntry(c_Entry);

                            //Now write the bytes of data
                            byte[] abBuffer = new byte[32 * 1024];
                            while (true)
                            {
                                int iRead = c_Stream.Read(abBuffer, 0, abBuffer.Length);
                                if (iRead <= 0)
                                    break;

                                c_Tarball.Write(abBuffer, 0, iRead);
                            }
                        }
                    }

                    //Nothing more to do with this entry
                    c_Tarball.CloseEntry();
                }

                // Close the tarball
                c_Tarball.Close();

                // Close the output stream
                c_Wkg.Close();
            }

            // 
            return sOut;
        }
        #endregion

        #region Containers
        /// <summary>
        /// 
        /// Lists the containers matching a filter
        /// 
        /// </summary>
        /// <param name="filter">The filter</param>
        /// <returns></returns>
        public IList<ContainerListResponse> ListContainers(DockerIFFilterClass filter = null)
        {
            // Assume none
            IList<ContainerListResponse> c_Ans = new List<ContainerListResponse>();

            // Handle missing filter
            if (filter == null) filter = new DockerIFFilterClass();

            // Protect
            try
            {
                c_Ans = this.Client.Containers.ListContainersAsync(new ContainersListParameters()
                {
                    All = true,
                    Filters = filter.Values as IDictionary<string, IDictionary<string, bool>>
                }).Result;
            }
            catch (Exception e)
            {
                this.HandleException("ListContainersAsync", e);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Creates a container
        /// 
        /// </summary>
        /// <param name="def">The Docker.DotNet config</param>
        /// <returns></returns>
        public string CreateContainer(DDNConfigClass def)
        {
            // Assume none
            string sAns = null;

            // Protect
            try
            {
                // Make
                CreateContainerResponse c_Resp = this.Client.Containers.CreateContainerAsync(def.Target).Result;

                // Dump warning
                foreach (string sMsg in c_Resp.Warnings)
                {
                    this.Parent.Parent.Parent.LogError("Warnings while creating bee: {0}".FormatString(sMsg));
                }

                // The id
                sAns = c_Resp.ID;
            }
            catch (Exception e)
            {
                this.HandleException("CreateContainerAsync", e);
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Starts a container
        /// 
        /// </summary>
        /// <param name="id">The container id</param>
        public void StartContainer(string id)
        {
            // Protect
            try
            {
                // Start
                this.Client.Containers.StartContainerAsync(id, new ContainerStartParameters()
                {
                }).Wait();
            }
            catch (Exception e)
            {
                this.HandleException("StartContainerAsync", e);
            }
        }

        /// <summary>
        /// 
        /// Stops a container
        /// 
        /// </summary>
        /// <param name="id">The container id</param>
        public void StopContainer(string id)
        {
            // Protect
            try
            {
                // Stop it
                this.Client.Containers.StopContainerAsync(id,
                    new ContainerStopParameters()
                    {
                    }).Wait();
            }
            catch (Exception e)
            {
                this.HandleException("StopContainerAsync", e);
            }
        }

        /// <summary>
        /// 
        /// Restarts a container
        /// 
        /// </summary>
        /// <param name="id">The container id</param>
        public void RestartContainer(string id, string name, string task)
        {
            // Protect
            try
            {
                this.Parent.Parent.Parent.LogInfo("RESTARTING {0}:{1}", task, name);

                // Stop it
                this.Client.Containers.RestartContainerAsync(id,
                    new ContainerRestartParameters()
                    { 
                    }).Wait();
            }
            catch (Exception e)
            {
                this.HandleException("RestartContainerAsync", e);
            }
        }

        /// <summary>

        /// <summary>
        /// 
        /// Removes a container
        /// 
        /// </summary>
        /// <param name="id">The container id</param>
        public void RemoveContainer(string id)
        {
            // Protect
            try
            {
                // Stop it
                this.Client.Containers.RemoveContainerAsync(id,
                    new ContainerRemoveParameters()
                    {
                        Force = true
                    }).Wait();
            }
            catch (Exception e)
            {
                this.HandleException("RemoveContainerAsync", e);
            }
        }

        /// <summary>
        /// 
        /// Gets the Stdout and StdErr logs
        /// 
        /// </summary>
        /// <param name="id"></param>

        public void GetLogs(string id)
        {
            // Protect
            try
            {
                // Stop it
                using (Stream c_Result = this.Client.Containers.GetContainerLogsAsync(id,
                    new ContainerLogsParameters()
                    {
                        ShowStdout = true,
                        ShowStderr = false
                    }).Result)
                {
                    this.DumpStream(c_Result, "--> STDOUT ".RPad(80, "-"), "".RPad(80, "-"));
                }
            }
            catch (Exception e)
            {
                this.HandleException("GetContainerLogsAsync;StdOut", e);
            }

            // Protect
            try
            {
                // Stop it
                using (Stream c_Result = this.Client.Containers.GetContainerLogsAsync(id,
                    new ContainerLogsParameters()
                    {
                        ShowStdout = false,
                        ShowStderr = true
                    }).Result)
                {
                    this.DumpStream(c_Result, "--> STDERR ".RPad(80, "-"), "".RPad(80, "-"));
                }
            }
            catch (Exception e)
            {
                this.HandleException("GetContainerLogsAsync;StdErr", e);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Handles a DockerIF exception
        /// 
        /// </summary>
        /// <param name="fn"Docker call being made></param>
        /// <param name="e">The exception</param>
        private void HandleException(string fn, Exception e)
        {
            if (e.GetAllExceptions().IndexOf("NotFound") == -1)
            {
                this.Parent.Parent.Parent.LogException("DockerIF: " + fn, e);
            }
        }

        /// <summary>
        /// 
        /// Makes the X-Registr-Auth string
        /// 
        /// </summary>
        /// <returns></returns>
        private AuthConfig MakeAuth()
        {
            // Assume none
            AuthConfig c_Ans = null;

            if (this.Parent.Parent.Parent["repo_username"].HasValue())
            {
                // Create
                c_Ans = new AuthConfig()
                {
                    Username = this.Parent.Parent.Parent["repo_username"],
                    Password = this.Parent.Parent.Parent["repo_userpwd"],
                    Email = this.Parent.Parent.Parent["repo_useremail"]
                };
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Copies a stream to the logs
        /// 
        /// </summary>
        /// <param name="stream">The source stream</param>
        /// <param name="header">The header</param>
        private void DumpStream(Stream stream, string header = null, string footer = null)
        {
            //
            if (header.HasValue()) this.Parent.Parent.Parent.LogVerbose(header);

            //
            string sLine = "";

            // The buffer
            byte[] abBuffer = new byte[32 * 1024];
            // Read line
            int iSize = stream.Read(abBuffer, 0, abBuffer.Length);

            // Loop thru
            while (iSize > 0)
            {
                // Append
                sLine += abBuffer.SubArray(0, iSize).FromBytes();
                // Dump any lines
                int iEOL = sLine.IndexOf("\x0A");
                // Till no more
                while (iEOL != -1)
                {
                    // Log
                    this.Parent.Parent.Parent.LogVerbose(sLine.Substring(0, iEOL).ASCIIOnly());
                    // Remove
                    sLine = sLine.Substring(iEOL + 1);
                    // And  repeat
                    iEOL = sLine.IndexOf("\x0A");
                }

                // From stream
                iSize = stream.Read(abBuffer, 0, abBuffer.Length);
            }

            // Turn what is left into ASCII
            sLine = sLine.ASCIIOnly();
            // Any
            if (sLine.HasValue())
            {
                // Log
                this.Parent.Parent.Parent.LogVerbose(sLine);
            }
            //
            if (footer.HasValue()) this.Parent.Parent.Parent.LogVerbose(footer);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// Handy dandy image name tool
    /// 
    /// </summary>
    public class DockerIFNameClass : IDisposable
    {
        #region Constructor
        private DockerIFNameClass(string qname)
        {
            //
            this.Parse(qname);
        }

        private DockerIFNameClass(DockerIFNameClass name, string newname = null)
        {
            //
            this.Repo = name.Repo;
            this.Project = name.Project;
            this.Name = newname.IfEmpty(name.Name);
            this.Tag = name.Tag;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Properties
        public string Repo { get; internal set; }
        public string Project { get; internal set; }
        public string Name { get; internal set; }
        public string Tag { get; internal set; }

        public string LocalNameNoTag
        {
            get { return this.Project + "/" + this.Name; }
        }

        public string LocalNameWithTag
        {
            get { return this.LocalNameNoTag + ":" + this.Tag; }
        }

        public string RepoNameNoTag
        {
            get { return this.Repo + "/" + this.LocalNameNoTag; }
        }

        public string RepoNameWithTag
        {
            get { return this.RepoNameNoTag + ":" + this.Tag; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Parses a fully qualified name
        /// 
        /// </summary>
        /// <param name="qname"></param>
        private void Parse(string qname)
        {
            // Does it have a tag?
            int iPos = qname.IndexOf(":");
            if (iPos != -1)
            {
                // Save
                this.Tag = qname.Substring(iPos + 1);
                // Remove
                qname = qname.Substring(0, iPos);
            }

            // Split
            string[] asPieces = qname.Split('/');
            // According to count
            switch (asPieces.Length)
            {
                case 0:
                    break;

                case 1:
                    this.Name = asPieces[0];
                    break;

                case 2:
                    this.Project = asPieces[0];
                    this.Name = asPieces[1];
                    break;

                default:
                    this.Repo = asPieces[0];
                    this.Project = asPieces[1];
                    this.Name = asPieces[2];
                    break;
            }
        }
        #endregion

        #region Statics
        /// <summary>
        /// 
        /// Makes a name object from a string in theform of:
        /// 
        /// [repo/][project/]name]:tag]
        /// 
        /// </summary>
        /// <param name="env">The current environemnt</param>
        /// <param name="qname">The qualified name</param>
        /// <returns>The parsed name object</returns>
        public static DockerIFNameClass Make(EnvironmentClass env, string qname)
        {
            DockerIFNameClass c_Ans = new DockerIFNameClass(qname);

            if (!c_Ans.Project.HasValue()) c_Ans.Project = env[EnvironmentClass.KeyRepoProject];
            if (!c_Ans.Tag.HasValue()) c_Ans.Tag = env.Tier;

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Makes a name object from anothe name object
        /// with the chance to change the names
        /// 
        /// </summary>
        /// <param name="name">The original name object</param>
        /// <param name="newname">The new name</param>
        /// <returns>The parsed name object</returns>
        public static DockerIFNameClass Make(DockerIFNameClass name, string newname = null)
        {
            return new DockerIFNameClass(name, newname);
        }
        #endregion
    }
}