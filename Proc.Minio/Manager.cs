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

/// Packet Manager Requirements
/// 
/// Install-Package Minio -Version 3.1.13
/// 

using System;
using System.IO;
using System.Collections.Generic;

using Minio;
using Minio.DataModel;

using NX.Engine;
using NX.Shared;
using NX.Engine.Files;

namespace Proc.Minio
{
    public class ManagerClass : BumbleBeeClass
    {
        #region Constants
        private const string MinioPrefix = "__";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public ManagerClass(EnvironmentClass env)
            : base(env, "minio")
        {
            // Handle the event
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Kill current
                if (this.Client != null)
                {
                    this.Client = null;
                }
                if (this.FileInterface != null)
                {
                    this.FileInterface = null;
                }

                // Accordingly
                if (isavailable)
                {
                    // Create the physical file interface
                    this.FileInterface = env.Globals.Get<NX.Engine.Files.ManagerClass>();
                    // And handle changes
                    this.FileInterface.FileSystemChanged += delegate (FileSystemParamClass param)
                    {
                            // Only if not done already
                            if (!param.Handled)
                        {
                                // Flag
                                param.Handled = true;

                                // File based?
                                if (param.Document != null)
                            {
                                    // According to what we need to do
                                    switch (param.Action)
                                {
                                    case FileSystemParamClass.Actions.Read:
                                            // Get
                                            param.Document.Value = this.GetObject(param.Document.Path);
                                        break;

                                    case FileSystemParamClass.Actions.Write:
                                            // Set
                                            this.SetObject(param.Document.Path, param.Document.Value);
                                            // Delete
                                            param.Document.Location.DeleteFile();
                                        break;

                                    case FileSystemParamClass.Actions.GetLastWrite:
                                            // Get
                                            param.On = this.GetAttribute(ManagerClass.Types.LastWrite, param.Document.Path).FromDBDate();
                                        break;

                                    case FileSystemParamClass.Actions.Delete:
                                            // Delete
                                            this.DeleteObject(param.Path);
                                        break;

                                    case FileSystemParamClass.Actions.GetStream:
                                            // Get
                                            this.GetStream(param.Path, param.StreamCallback);
                                        break;

                                    case FileSystemParamClass.Actions.SetStream:
                                            // Get
                                            this.SetStream(param.Path, param.Stream);
                                        break;
                                }
                            }
                            else
                            {
                                    // According to what we need to do
                                    switch (param.Action)
                                {
                                    case FileSystemParamClass.Actions.Delete:
                                            // Delete
                                            this.DeleteObject(param.Path);
                                        break;

                                    case FileSystemParamClass.Actions.CreatePath:
                                            //
                                            this.AssureFolder(param.Path);
                                        break;

                                    case FileSystemParamClass.Actions.ListFiles:
                                            // Get list of children
                                            param.List = this.ListObjectvalues(this.MakeAttributeName(ManagerClass.Types.Child, param.Path));
                                        break;

                                    case FileSystemParamClass.Actions.ListFolders:
                                            // Get list of children
                                            param.List = this.ListObjectvalues(this.MakeAttributeName(ManagerClass.Types.ChildFolder, param.Path));
                                        break;
                                }
                            }
                        }
                    };

                    // Get the settings
                    string sAccessKey = this.Parent["minio_access"].IfEmpty(this.Parent.Hive.Name.MD5HashString());
                    string sSecret = this.Parent["minio_secret"].IfEmpty(this.Parent.Hive.Name.MD5HashString());

                    // Get the url
                    string sURL = this.Location;

                    // Must have all three
                    if (sURL.HasValue() && sAccessKey.HasValue() && sSecret.HasValue())
                    {
                        // Make the client
                        this.Client = new MinioClient(sURL, sAccessKey, sSecret);

                        // Get the root directory
                        string sRoot = NX.Engine.Files.ManagerClass.MappedFolder;
                        // Get all of the files
                        List<string> c_Files = sRoot.GetTreeInPath();
                        // Loop thru
                        foreach (string sFile in c_Files)
                        {
                            // Skip our own files
                            if (sFile.IndexOf("_minio") == -1)
                            {
                                // Make the document
                                using (DocumentClass c_Doc = new DocumentClass(this.FileInterface, sFile.Substring(sRoot.Length)))
                                {
                                    // Copy from local to Minio
                                    c_Doc.ValueAsBytes = sFile.ReadFileAsBytes();
                                    // Delete local copy
                                    sFile.DeleteFile();
                                }
                            }
                        }
                    }
                }
            };

            // Bootstap
            this.CheckForAvailability();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The Minio client
        /// 
        /// </summary>
        public MinioClient Client { get; set; }

        /// <summary>
        /// 
        /// Is the manager available?
        /// 
        /// </summary>
        public override bool IsAvailable => this.Client != null;

        /// <summary>
        /// 
        /// The physical file interface
        /// 
        /// </summary>
        private NX.Engine.Files.ManagerClass FileInterface { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Collapses a path, removing known base
        /// 
        /// </summary>
        /// <param name="path">The path to collapse</param>
        /// <returns>The collapsed path</returns>
        public string Collapse(string path)
        {
            // Does it start with the base?
            if (path.StartsWith(NX.Engine.Files.ManagerClass.MappedFolder))
            {
                path = path.Substring(NX.Engine.Files.ManagerClass.MappedFolder.Length);
            }

            return path;
        }

        /// <summary>
        /// 
        /// Expands the path, adding known baase
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string Expand(string path)
        {
            return NX.Engine.Files.ManagerClass.MappedFolder.CombinePath(path);
        }

        private void AssureFolder(string path)
        {
            if (!this.GetAttribute(ManagerClass.Types.Path, path).HasValue())
            {
                // Create
                this.SetAttribute(ManagerClass.Types.Path, path, path);
                // And set the creation time
                this.SetAttribute(ManagerClass.Types.LastWrite, path, DateTime.Now.ToUniversalTime().ToDBDate());

                // Get the  parent path
                string sPath = path.GetParentDirectoryFromPath();
                // Anything left?
                if (sPath.HasValue())
                {
                    // Assure
                    this.AssureFolder(sPath);
                    // Set us as a child folder
                    this.SetAttribute(ManagerClass.Types.ChildFolder, sPath, path);
                }
            }

        }
        #endregion

        #region Enums
        public enum Types
        {
            Name,

            All,

            Path,
            LastWrite,

            Child,
            ChildFolder
        }
        #endregion

        #region MinIO
        /// <summary>
        /// 
        /// The bucket that we use
        /// 
        /// </summary>
        private string MinioBucket
        {
            get { return this.Parent["minio_bucket"].IfEmpty("nxproject"); }
        }

        /// <summary>
        /// 
        /// Have we checked it?
        /// 
        /// </summary>
        private bool BucketAssured { get; set; }

        /// <summary>
        /// 
        /// Make sure that the Minio bucket exists
        /// 
        /// </summary>
        private void AssureBucket()
        {
            // Done already?
            if (!this.BucketAssured)
            {
                // Do we have Minio
                if (this.IsAvailable)
                {
                    // Does it exist?
                    if (!this.Client.BucketExistsAsync(this.MinioBucket).Result)
                    {
                        // Create
                        this.Client.MakeBucketAsync(this.MinioBucket).Wait();
                    }

                    // And done
                    this.BucketAssured = true;
                }
            }
        }

        /// <summary>
        /// 
        /// Makes an object name
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MakeObjectName(string name)
        {
            // Assume plain
            string sAns = name.IfEmpty();

            // Not an attribute?
            if (!sAns.StartsWith(MinioPrefix))
            {
                // Convert
                sAns = name.IfEmpty().ToLower().MD5HashString();
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Gets an object from Minio
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <returns>The object value</returns>
        public string GetObject(string name, DocumentClass doc = null)
        {
            // Assume none
            string sAns = null;

            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Read
                this.Client.GetObjectAsync(this.MinioBucket, this.MakeObjectName(name), delegate (Stream stream)
                {
                    // Assume in-memory
                    FileStream c_Local = null;
                    // Do we have a file?
                    if (doc != null)
                    {
                        // Open it
                        c_Local = new FileStream(doc.Location, FileMode.Create);
                    }

                    // Make a buffer
                    byte[] abBuffer = new byte[32 * 1024];

                    // Read a chunk
                    int iSize = stream.Read(abBuffer, 0, abBuffer.Length);
                    // Until no more
                    while (iSize > 0)
                    {
                        // In-memory
                        if (c_Local == null)
                        {
                            // Append
                            sAns += abBuffer.SubArray(0, iSize).FromBytes();
                        }
                        else
                        {
                            // Copy
                            c_Local.Write(abBuffer, 0, iSize);
                        }

                        // Read again
                        iSize = stream.Read(abBuffer, 0, abBuffer.Length);
                    }

                    // Close
                    stream.Close();
                    if (c_Local != null) c_Local.Close();

                }).Wait();
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Reads an object via a stream
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="cb">The callback</param>
        public void GetStream(string name, Action<Stream> cb)
        {
            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Read
                this.Client.GetObjectAsync(this.MinioBucket, this.MakeObjectName(name), delegate (Stream stream)
                {
                    // Call
                    if (cb != null) cb(stream);

                    // Close
                    try
                    {
                        stream.Close();
                    }
                    catch { }

                }).Wait();
            }
        }

        /// <summary>
        /// 
        /// Writes an object to Minio
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="value">The object value</param>
        /// <param name="isprivate">Trues is what is being written is part of the info block</param>
        public void SetObject(string name, string value, DocumentClass doc = null)
        {
            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Placeholder
                Stream c_Stream = null;
                long lSize = 0;

                // In-memory?
                if (doc == null)
                {
                    //
                    c_Stream = new MemoryStream(value.ToBytes());
                    lSize = value.Length;
                }
                else
                {
                    // Open
                    c_Stream = new FileStream(doc.Location, FileMode.Open);
                    lSize = c_Stream.Length;
                }

                // Write
                this.Client.PutObjectAsync(this.MinioBucket, this.MakeObjectName(name), c_Stream, lSize).Wait();

                // to disk?
                if (doc == null)
                {
                    // Dispose
                    c_Stream.Dispose();
                }
                else
                {
                    // Close the stream
                    c_Stream.Flush();
                    c_Stream.Close();
                }

                // Private?
                if (!name.StartsWith(MinioPrefix))
                {
                    // Now is the time that we wrote
                    this.SetAttribute(Types.LastWrite, name, DateTime.Now.ToDBDate());

                    // And make child
                    this.SetAttribute(Types.Child, name.GetDirectoryFromPath(), name);
                }
            }
        }

        /// <summary>
        /// 
        /// Writes an object via a stream
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="stream">The stream</param>
        public void SetStream(string name, Stream stream)
        {
            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Write
                this.Client.PutObjectAsync(this.MinioBucket, this.MakeObjectName(name), stream, stream.Length).Wait();

                // Private?
                if (!name.StartsWith(MinioPrefix))
                {
                    // Now is the time that we wrote
                    this.SetAttribute(Types.LastWrite, name, DateTime.Now.ToDBDate());

                    // And make child
                    this.SetAttribute(Types.Child, name.GetDirectoryFromPath(), name);
                }
            }
        }

        /// <summary>
        /// 
        /// Deletes an object
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="isprivate">Trues is what is being written is part of the info block</param>
        public void DeleteObject(string name, bool isfolder = false)
        {
            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Do
                this.Client.RemoveObjectAsync(this.MinioBucket, this.MakeObjectName(name)).Wait();


                // Get parent name
                string sPath = name.GetParentDirectoryFromPath();

                // Folder?
                if (isfolder)
                {
                    // Delete link
                    this.DeleteAttribute(Types.ChildFolder, sPath, name);
                }

                // Remove child link
                this.DeleteAttribute(Types.Child, sPath, name);

                // Get all attributes
                List<string> c_Attr = this.ListAttributes(name);

                // Loop thru
                foreach (string sAttr in c_Attr)
                {
                    // Delete
                    this.DeleteAttribute(Types.Name, sAttr);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns true if the object exists
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <returns>True if it exists</returns>
        public bool ObjectExists(string name)
        {
            // Assume not
            bool bAns = false;

            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Get
                bAns = this.Client.BucketExistsAsync(this.MakeObjectName(name)).Result;
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// List all object names with the given prefix
        /// 
        /// </summary>
        /// <param name="prefix">The prefix to match</param>
        /// <returns>The list of keys</returns>
        public List<string> ListObjects(string prefix)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Do
                var c_Result = this.Client.ListObjectsAsync(this.MinioBucket, this.MakeObjectName(prefix));
                c_Result.Subscribe(delegate (Item item)
                {
                    // Add
                    c_Ans.Add(item.Key);
                }, delegate ()
                {
                });
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// List all object values with the given prefix
        /// 
        /// </summary>
        /// <param name="prefix">The prefix to match</param>
        /// <returns>The list of keys</returns>
        public List<string> ListObjectvalues(string prefix)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Do we have Minio
            if (this.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket();

                // Do
                var c_Result = this.Client.ListObjectsAsync(this.MinioBucket, this.MakeObjectName(prefix));
                c_Result.Subscribe(delegate (Item item)
                {
                    // Add
                    c_Ans.Add(this.GetObject(item.Key));
                }, delegate ()
                {
                });
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Creates the name for the attribute
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <param name="extra">The second name</param>
        /// <returns>The FQ value</returns>
        public string MakeAttributeName(Types type, string name, string extra = null)
        {
            // Assume plain
            string sAns = name;

            // Non-data
            if (type != Types.Name)
            {
                // Do extra if there
                if (extra.HasValue())
                {
                    extra = "_" + extra.ToLower().MD5HashString();
                }

                sAns = MinioPrefix + name.IfEmpty().ToLower().MD5HashString() + "_" + type + extra.IfEmpty();
            }
            else if (type == Types.All)
            {
                sAns = MinioPrefix + name.MD5HashString() + "_";
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Gets an attribute
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <param name="extra">The second name</param>
        /// <returns>The value</returns>
        public string GetAttribute(Types type, string name, string extra = null)
        {
            // Get
            return this.GetObject(this.MakeAttributeName(type, name, extra));
        }

        /// <summary>
        /// 
        /// Sets a attribute
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <param name="extra">The second name</param>
        /// <param name="value">The value to store</param>
        public void SetAttribute(Types type, string name, string value, string extra = null)
        {
            // Set
            this.SetObject(this.MakeAttributeName(type, name, extra), value);
        }

        /// <summary>
        /// 
        /// Deletes an attribute
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <param name="extra">The second name</param>
        public void DeleteAttribute(Types type, string name, string extra = null)
        {
            // Delete
            this.DeleteObject(this.MakeAttributeName(type, name, extra));
        }

        /// <summary>
        ///  
        /// Lists all attributes
        ///  
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <returns>The list of keys</returns>
        public List<string> ListAttributes(string name, bool removeprefix = true)
        {
            // Make the pattern
            string sPatt = this.MakeAttributeName(Types.All, name);

            // Get the list
            List<string> c_Ans = this.ListObjects(sPatt);

            // remove prefix
            if (removeprefix)
            {
                // Loop thru
                for (int i = 0; i < c_Ans.Count; i++)
                {
                    // Remove prefix
                    c_Ans[i] = c_Ans[i].Substring(sPatt.Length);
                }
            }

            return c_Ans;
        }
        #endregion

        #region Events
        public void SignalChange(string path, bool deleted)
        {
            //
            this.ContentsChanged?.Invoke(path, deleted);
        }

        /// <summary>
        /// 
        /// The delegate for the AvailabilityChanged event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnChangedHandler(string path, bool deleted);

        /// <summary>
        /// 
        /// Defines the event to be raised when a DNA is added/deleted
        /// 
        /// </summary>
        public event OnChangedHandler ContentsChanged;
        #endregion
    }
}