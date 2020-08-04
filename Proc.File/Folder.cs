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
using System.IO;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace Proc.File
{
    /// <summary>
    /// 
    /// A folder in the documents tree
    /// 
    /// </summary>
    public class FolderClass : ChildOfClass<ManagerClass>
    {
        #region Constructor
        public FolderClass(ManagerClass mgr, string path)
            : base(mgr)
        {
            // Save
            this.Path = this.Parent.Collapse(path);

            // And make sure that it exists
            if (!this.Parent.IsAvailable)
            {
                // If we have a disk based system, create the path
                this.AssurePath();
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            //
            if (this.IMinioExtra != null)
            {
                this.IMinioExtra.Dispose();
                this.IMinioExtra = null;
            }

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The path
        /// 
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 
        /// The full (physical) path
        /// 
        /// </summary>
        public string Location
        {
            get { return this.Parent.Expand(this.Path); }
        }

        /// <summary>
        /// 
        /// A list of files in the folder
        /// 
        /// </summary>
        public List<DocumentClass> Files
        {
            get
            {
                List<DocumentClass> c_Ans = new List<DocumentClass>();

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // TBD
                }
                else
                {
                    foreach (string sEntry in this.Location.GetFilesInPath())
                    {
                        c_Ans.Add(new DocumentClass(this.Parent, sEntry));
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// A list of folders in the folder
        /// 
        /// </summary>
        public List<FolderClass> Folders
        {
            get
            {
                // Assume none
                List<FolderClass> c_Ans = new List<FolderClass>();

                // Using MinIO?
                if (this.Parent.IsAvailable)
                {
                    // TBD
                }
                else
                {
                    // Loop thru
                    foreach (string sEntry in this.Location.GetDirectoriesInPath())
                    {
                        c_Ans.Add(new FolderClass(this.Parent, sEntry));
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The folder name in Minio
        /// 
        /// </summary>
        private string MinioName
        {
            get { return this.Path.ToLower().MD5HashString(); }
        }

        /// <summary>
        /// 
        /// The Minio extra information for this folder
        /// 
        /// </summary>
        private MinioExtraInfo IMinioExtra { get; set; }
        public MinioExtraInfo MinioExtra
        {
            get
            {
                if (this.IMinioExtra == null)
                {
                    this.IMinioExtra = new MinioExtraInfo(this);
                }

                return this.IMinioExtra;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Deletes the folder 
        /// 
        /// </summary>
        public void Delete()
        {
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                // TBD
            }
            else
            {
                // Physical
                this.Location.DeletePath();
            }
        }

        /// <summary>
        ///  Makes sure that the path exists
        ///  
        /// </summary>
        public void AssurePath()
        {
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                //
                this.AssureBucket(false);
            }
            else
            {
                // Physical
                this.Location.AssurePath();
            }
        }
        #endregion

        #region MinIO
        /// <summary>
        /// 
        /// Gets an object from Minio
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <returns>The object value</returns>
        public string GetObject(string name, bool isprivate = false)
        {
            // Assume none
            string sAns = null;

            // Do we have Minio
            if (this.Parent.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket(isprivate);

                // Read
                this.Parent.Client.GetObjectAsync(this.MinioName, name, delegate (Stream stream)
                {
                    // Make a buffer
                    byte[] abBuffer = new byte[32 * 1024];

                    // Read a chunk
                    int iSize = stream.Read(abBuffer, 0, abBuffer.Length);
                    // Until no more
                    while (iSize > 0)
                    {
                        // Append
                        sAns += abBuffer.SubArray(0, iSize).FromBytes();
                        // Read again
                        iSize = stream.Read(abBuffer, 0, abBuffer.Length);
                    }

                }).Wait();
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Writes an object to Minio
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="value">The object value</param>
        /// <param name="isprivate">Trues is what is being written is part of the info block</param>
        public void SetObject(string name, string value, bool isprivate = false)
        {
            // Do we have Minio
            if (this.Parent.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket(isprivate);

                // Make into stream
                using (MemoryStream c_Stream = new MemoryStream(value.ToBytes()))
                {
                    // Write
                    this.Parent.Client.PutObjectAsync(this.MinioName, name, c_Stream, (long)value.Length).Wait();
                    // Private?
                    if (!isprivate)
                    {
                        //
                        this.MinioExtra.SetLastWrittenOn(name, DateTime.Now);
                    }
                }
            }
        }

        public void DeleteObject(string name, bool isprivate = false)
        {
            // Do we have Minio
            if (this.Parent.IsAvailable)
            {
                // Make sure bucket exixts
                this.AssureBucket(isprivate);

                // Do
                this.Parent.Client.RemoveObjectAsync(this.MinioName, name).Wait();

                // Remove all keys
                this.MinioExtra.Delete(MinioExtraInfo.Types.File, this.MinioName, name);
            }
        }

        /// <summary>
        /// 
        /// Make sure that the Minio bucket exists
        /// 
        /// </summary>
        private void AssureBucket(bool isprivate)
        {
            // Do we have Minio
            if (this.Parent.IsAvailable)
            {
                // Does it exist?
                if (!this.Parent.Client.BucketExistsAsync(this.MinioName).Result)
                {
                    // Create
                    this.Parent.Client.MakeBucketAsync(this.MinioName).Wait();

                    // Get the timestamp
                    DateTime c_Now = DateTime.Now;

                    // Set the base info
                    this.MinioExtra.Name = this.Path.GetDirectoryNameFromPath();
                    this.MinioExtra.Path = this.Path;
                    this.MinioExtra.CreatedOn = c_Now;
                    this.MinioExtra.LastAccessOn = c_Now;

                    // Get the path
                    string sPath = this.Path;
                    // Look for last piece
                    int iPos = this.Path.LastIndexOf("/");
                    //  Any?
                    if (iPos != -1)
                    {
                        // Remove
                        sPath = sPath.Substring(0, iPos);
                        // ANything left?
                        if (sPath.HasValue())
                        {
                            // Assure
                            using (FolderClass c_Parent = new FolderClass(this.Parent, sPath))
                            {
                                // Set us as a child folder
                                this.MinioExtra.SetFolder(this.MinioName, "", this.Path);
                            }
                        }
                    }
                }
                else
                {
                    // Unless a private call
                    if (!isprivate)
                    {
                        // Set
                        this.MinioExtra.LastAccessOn = DateTime.Now;
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// Keeps all of the extra info to make the S3 bucket system
    /// look and behave like folders
    /// 
    /// </summary>
    public class MinioExtraInfo : ChildOfClass<FolderClass>
    {
        #region Constructor
        internal MinioExtraInfo(FolderClass folder)
        : base(folder)
        { }
        #endregion

        #region Enums
        public enum Types
        {
            Setting,
            Folder,
            File
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The folder name
        /// 
        /// </summary>
        public string Name
        {
            get { return this.Get(Types.Setting, "name", null); }
            set { this.Set(Types.Setting, "name", null, value); }
        }

        /// <summary>
        /// 
        /// The path
        /// 
        /// </summary>
        public string Path
        {
            get { return this.Get(Types.Setting, "path", null); }
            set { this.Set(Types.Setting, "path", null, value); }
        }

        /// <summary>
        /// 
        /// Date folder was created
        /// 
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.Get(Types.Setting, "created", null).FromDBDate(); }
            set { this.Set(Types.Setting, "created", null, value.ToUniversalTime().ToDBDate()); }
        }


        /// <summary>
        /// 
        /// Date folder was last accessed
        /// 
        /// </summary>
        public DateTime LastAccessOn
        {
            get { return this.Get(Types.Setting, "last", null).FromDBDate(); }
            set { this.Set(Types.Setting, "last", null, value.ToUniversalTime().ToDBDate()); }
        }
        #endregion

        #region File
        /// <summary>
        /// 
        /// Get the time that the file was last written
        /// 
        /// </summary>
        /// <param name="file">The file name</param>
        /// <returns>Date and time last written</returns>
        public DateTime GetLastWrittenOn(string file)
        {
            return this.Get(Types.File, file, "lw").FromDBDate();
        }

        /// <summary>
        /// 
        /// Set the last time the file was written
        /// 
        /// </summary>
        /// <param name="file">The file name</param>
        /// <param name="value">Date and time of last write</param>
        public void SetLastWrittenOn(string file, DateTime value)
        {
            this.Set(Types.File, file, "lw", value.ToUniversalTime().ToDBDate());
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Creates the name for the private
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <returns>The FQ value</returns>
        private string MakeName(Types type, string name, string op)
        {
            return ("__" + type + "_" + name + "_" + op.IfEmpty()).ToLower();
        }

        /// <summary>
        /// 
        /// Gets a private from the folder
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <returns>The value</returns>
        public string Get(Types type, string name, string op)
        {
            // Get
            return this.Parent.GetObject(this.MakeName(type, name, op), true);
        }

        /// <summary>
        /// 
        /// Sets a private from the folder
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <param name="value">The value to store</param>
        public void Set(Types type, string name, string op, string value)
        {
            // Set
            this.Parent.SetObject(this.MakeName(type, name, op), value, true);
        }

        /// <summary>
        /// 
        /// Gets a private from the folder
        /// 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name</param>
        /// <returns>The value</returns>
        public string Delete(Types type, string name, string op = null)
        {
            // Get
            return this.Parent.GetObject(this.MakeName(type, name, op), true);
        }

        public void SetFolder(string folder, string op, string value)
        {
            // TBD
        }
        #endregion
    }
}