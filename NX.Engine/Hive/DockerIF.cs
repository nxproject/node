///--------------------------------------------------------------------------------
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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package Docker.DotNet -Version 3.125.2
/// 

using System;
using System.Collections.Generic;
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
                    this.Parent.Parent.Parent.LogInfo("URL for field {0} set to {1}".FormatString(this.Parent.Name, this.URL));
                }
                else
                {
                    // Error
                    this.Parent.Parent.Parent.LogError("Unable to set URL for field {0}".FormatString(this.Parent.Name));
                }
            }
            else
            {
                // U
                this.Parent.Parent.Parent.LogError("Unable to ping field {0} at {1}".FormatString(this.Parent.Name, this.URL));
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
                    string sTagName = "";

                    if (c_Entry.RepoTags != null && c_Entry.RepoTags.Count > 0)
                    {
                        sTagName = c_Entry.RepoTags[0];
                    }
                    // Does the repo tag match?
                    bAns = sName.IsSameValue(sTagName);
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
                string sValue2 = this.Parent.Parent.Parent.HiveName;

                // Loop thru
                foreach (var c_Entry in c_List)
                {
                    // Must match 
                    int iMatched = 2;

                    // Do we have any?
                    if (c_Entry.Labels != null)
                    {
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
        public void BuildImage(DockerIFNameClass name, string dir, EnvironmentClass env = null)
        {
            // In case of long build
            this.Parent.Parent.Parent.Hive.AllowAutoRefresh = false;

            // Make room
            string sFile = null;

            this.Parent.Parent.Parent.LogInfo("Building image {0}".FormatString(name.LocalNameWithTag));

            // Protect
            try
            {
                // Make a temp file
                sFile = this.CreateTarballForDockerfile(name, dir, env);

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

                // In case of long build
                this.Parent.Parent.Parent.Hive.AllowAutoRefresh = true;
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
                // Kill any containers
                foreach (string sCtxID in this.ListContainers(name))
                {
                    // Kill
                    this.RemoveContainer(sCtxID);
                }

                // Get the list
                List<string> c_IDs = this.ListImages(name);
                // Loop thru
                foreach (string sID in c_IDs)
                {
                    // Do
                    this.Client.Images.DeleteImageAsync(sID, new ImageDeleteParameters()
                    {
                        Force = true
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
        private string CreateTarballForDockerfile(DockerIFNameClass name,
                                                    string directory,
                                                    EnvironmentClass env = null)
        {
            // Make the temp file
            string sOut = "".WorkingDirectory().CombinePath("__temp");
            // Delete it
            sOut.DeletePath();

            //
            string sRoot = "".WorkingDirectory();

            // Assume nothing to copy
            List<DockerIFEntryClass> c_Files = new List<DockerIFEntryClass>();
            // Append dockerfile
            this.AddEntries(c_Files, this.Parent.Parent.GenomeDirectory(name.Name), true);
            // Assure
            directory = directory.IfEmpty();
            // Do we have a directory?
            if (directory.HasValue() && !directory.IsSameValue("-"))
            {
                // Get all the files
                this.AddEntries(c_Files, directory, false);
            }

            // Assure
            sOut.AssurePath();
            // Add file name
            sOut = sOut.CombinePath("I".GUID() + ".tar").AdjustPathToOS();

            // Make the stream
            FileStream c_Wkg = new FileStream(sOut, FileMode.CreateNew);

            // Open the tarball
            TarOutputStream c_Tarball = new TarOutputStream(c_Wkg);

            // Loop thru.
            foreach (DockerIFEntryClass c_In in c_Files)
            {
                //Replacing slashes as KyleGobel suggested and removing leading /
                string tarName = c_In.TarName.Replace('\\', '/').TrimStart('/');

                //Let's create the entry header
                TarEntry c_Entry = TarEntry.CreateTarEntry(tarName);
                c_Entry.TarHeader.Mode = Convert.ToInt32("100755", 8); //chmod 755

                // Is this a Dockerfile?
                if (c_In.Path.GetFileNameFromPath().IsSameValue("Dockerfile") && c_In.IsGenome)
                {
                    // Read
                    string sDF = c_In.Path.ReadFile();

                    // Labels
                    string sLabels = @"""{0}""=""{1}""".FormatString(this.Parent.Parent.LabelGenome, name.Name);
                    sLabels += @" ""{0}""=""{1}""".FormatString(this.Parent.Parent.LabelHive, this.Parent.Parent.Parent.HiveName);
                    // Format
                    sDF = this.Parent.Parent.Parent.Format(sDF, true, "proj_label", sLabels);

                    // Set the size
                    c_Entry.Size = sDF.Length;
                    // And write entry
                    c_Tarball.PutNextEntry(c_Entry);

                    // Now the contents
                    c_Tarball.Write(sDF.ToBytes(), 0, sDF.Length);
                }
                else
                {
                    // Assue normal processing
                    bool bDo = true;

                    if (c_In.Path.GetFileNameFromPath().IsSameValue("config.json") && env != null)
                    {
                        // Convert to bytes
                        byte[] abBuffer = env.SynchObject.ToSimpleString().ToBytes();

                        // Set the size
                        c_Entry.Size = abBuffer.Length;
                        // And write entry
                        c_Tarball.PutNextEntry(c_Entry);

                        //Now write the bytes of data                            
                        c_Tarball.Write(abBuffer, 0, abBuffer.Length);
                    }

                    // Process
                    if (bDo)
                    {
                        using (FileStream c_Stream = File.OpenRead(c_In.Path))
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
                }

                //Nothing more to do with this entry
                c_Tarball.CloseEntry();
            }

            // Close the tarball
            c_Tarball.Close();

            // Close the output stream
            c_Wkg.Close();

            // 
            return sOut;
        }

        /// <summary>
        /// 
        /// Add fiels to the TBD queue
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dir"></param>
        /// <param name="isgenome"></param>
        private void AddEntries(List<DockerIFEntryClass> list, string dir, bool isgenome)
        {
            // Protect
            try
            {
                // Get the files
                List<string> c_Files = dir.GetTreeInPath();
                // Loop thru
                foreach (string sFile in c_Files)
                {
                    list.Add(new DockerIFEntryClass(sFile, dir, isgenome));
                }
            }
            catch { }
        }
        #endregion

        #region Containers
        /// <summary>
        /// 
        /// Lists all containers
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <returns>True if it exists</returns>
        public List<string> ListContainersAll()
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Protect
            try
            {
                // Do we have it?
                var c_List = this.Client.Containers.ListContainersAsync(new ContainersListParameters()
                {
                }).Result;

                // Get the name
                string sKey2 = this.Parent.Parent.LabelHive;
                string sValue2 = this.Parent.Parent.Parent.HiveName;

                foreach (var c_Entry in c_List)
                {
                    // Must match 
                    int iMatched = 1;

                    // Do we have any?
                    if (c_Entry.Labels != null)
                    {
                        // Loop thru
                        foreach (var c_KV in c_Entry.Labels)
                        {
                            if (c_KV.Value.IsSameValue(sValue2) && c_KV.Key.IsSameValue(sKey2))
                            {
                                iMatched--;
                            }
                            if (iMatched <= 0)
                            {
                                break;
                            }
                        }
                    }

                    if (iMatched <= 0) c_Ans.Add(c_Entry.ID);
                }
            }
            catch (Exception e)
            {
                this.HandleException("ListContainersAll", e);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Lists all containers
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <returns>True if it exists</returns>
        public List<ContainerListResponse> ListContainersInfo()
        {
            // Assume none
            List<ContainerListResponse> c_Ans = new List<ContainerListResponse>();

            // Protect
            try
            {
                // Do we have it?
                var c_List = this.Client.Containers.ListContainersAsync(new ContainersListParameters()
                {
                }).Result;

                // Get the name
                string sKey2 = this.Parent.Parent.LabelHive;
                string sValue2 = this.Parent.Parent.Parent.HiveName;

                foreach (var c_Entry in c_List)
                {
                    // Must match 
                    int iMatched = 1;

                    // Do we have any?
                    if (c_Entry.Labels != null)
                    {
                        // Loop thru
                        foreach (var c_KV in c_Entry.Labels)
                        {
                            if (c_KV.Value.IsSameValue(sValue2) && c_KV.Key.IsSameValue(sKey2))
                            {
                                iMatched--;
                            }
                            if (iMatched <= 0)
                            {
                                break;
                            }
                        }
                    }

                    if (iMatched <= 0) c_Ans.Add(c_Entry);
                }
            }
            catch (Exception e)
            {
                this.HandleException("ListContainersInfo", e);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Checks to see if container is in local repo
        /// 
        /// </summary>
        /// <param name="name">The handy dandy name</param>
        /// <returns>True if it exists</returns>
        public List<string> ListContainers(DockerIFNameClass name)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Protect
            try
            {
                // Do we have it?
                var c_List = this.Client.Containers.ListContainersAsync(new ContainersListParameters()
                {
                }).Result;

                // Get the name
                string sKey1 = this.Parent.Parent.LabelGenome;
                string sValue1 = name.Name;
                string sKey2 = this.Parent.Parent.LabelHive;
                string sValue2 = this.Parent.Parent.Parent.HiveName;

                // Loop thru
                foreach (var c_Entry in c_List)
                {
                    // Must match 
                    int iMatched = 2;

                    // Do we have any?
                    if (c_Entry.Labels != null)
                    {
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
                    }

                    if (iMatched <= 0) c_Ans.Add(c_Entry.ID);
                }
            }
            catch (Exception e)
            {
                this.HandleException("ListContainers", e);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Lists the containers with a given id
        /// 
        /// </summary>
        /// <param name="filter">The filter</param>
        /// <returns></returns>
        public IList<ContainerListResponse> ListContainersByID(string id)
        {
            // Make the filter
            DockerIFFilterClass c_Filter = new DockerIFFilterClass("id", id);

            // Call
            return this.ListContainersByFilter(c_Filter);
        }

        /// <summary>
        /// 
        /// Lists the containers matching a filter
        /// 
        /// </summary>
        /// <param name="filter">The filter</param>
        /// <returns></returns>
        public IList<ContainerListResponse> ListContainersByFilter(DockerIFFilterClass filter)
        {
            // Assume none
            IList<ContainerListResponse> c_Ans = new List<ContainerListResponse>();

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
                this.HandleException("ListContainersByFilter", e);
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
                if (c_Resp != null && c_Resp.Warnings != null)
                {
                    foreach (string sMsg in c_Resp.Warnings)
                    {
                        this.Parent.Parent.Parent.LogError("Warnings while creating bee: {0}".FormatString(sMsg));
                    }

                    // The id
                    sAns = c_Resp.ID;
                }
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
        public void RestartContainer(string id, string name = null, string task = null)
        {
            // Protect
            try
            {
                if (name.HasValue())
                {
                    this.Parent.Parent.Parent.LogVerbose("RESTARTING {0}:{1}".FormatString(task, name));
                }

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


        /// <summary>
        /// 
        /// Gets the Stdout and StdErr logs
        /// 
        /// </summary>
        /// <param name="id"></param>
        public string GetLogsAsString(string id)
        {
            //
            string sAns = "";

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
                    sAns += this.DumpStreamAsString(c_Result, "--> STDOUT ".RPad(80, "-"), "".RPad(80, "-"));
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
                    sAns += this.DumpStreamAsString(c_Result, "--> STDERR ".RPad(80, "-"), "".RPad(80, "-"));
                }
            }
            catch (Exception e)
            {
                this.HandleException("GetContainerLogsAsync;StdErr", e);
            }

            return sAns;
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
            // Get the exceptions
            string sExc = e.GetAllExceptions();

            // Skip unwanted
            if (sExc.IndexOf("NotFound") == -1 &&
                sExc.IndexOf("reuse that name") == -1)
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

        /// <summary>
        /// 
        /// Copies a stream to the logs
        /// 
        /// </summary>
        /// <param name="stream">The source stream</param>
        /// <param name="header">The header</param>
        private string DumpStreamAsString(Stream stream, string header = null, string footer = null)
        {
            StringBuilder c_Buffer = new StringBuilder();

            //
            if (header.HasValue()) c_Buffer.AppendLine(header);

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
                    c_Buffer.AppendLine(sLine.Substring(0, iEOL).ASCIIOnly());
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
                c_Buffer.AppendLine(sLine);
            }
            //
            if (footer.HasValue()) c_Buffer.AppendLine(footer);

            return c_Buffer.ToString();
        }
        #endregion
    }

    public class DockerIFEntryClass
    {
        #region Constructor
        public DockerIFEntryClass(string file, string dir, bool isgenome)
        {
            //
            this.Path = file;
            this.TarName = file.Substring(dir.Length - (dir.EndsWith("/") ? 1 : 0));
            this.IsGenome = isgenome;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The full path
        /// 
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 
        /// The name to use in tarball
        /// 
        /// </summary>
        public string TarName { get; set; }

        /// <summary>
        /// 
        /// Is this file part of the genome?
        /// 
        /// </summary>
        public bool IsGenome { get; private set; }
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
            c_Ans.Tag = env.HiveName;

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