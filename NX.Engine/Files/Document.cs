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
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package itextSharp -Version 5.5.13.1
/// Install-Package itextSharp.xmlworker -Version 5.5.13.1
/// Install-Package OpenXmlPowerTools -Version 4.5.3.2
/// Install-Package DocumentFormat.OpenXML -Version 2.11.3
/// 

using System;
using System.IO;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Linq;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace NX.Engine.Files
{
    /// <summary>
    /// 
    /// A document in the document tree
    /// 
    /// </summary>
    public class DocumentClass : ChildOfClass<ManagerClass>
    {
        #region Constants
        /// <summary>
        /// 
        /// A file that holds the path to the file
        /// 
        /// </summary>
        public const string ReversePointerFile = "orig.path";
        #endregion

        #region Constructor
        public DocumentClass(ManagerClass mgr, string path)
            : base(mgr)
        {
            // Save
            this.Path = this.Parent.Collapse(path).StandarizePath();
        }

        public DocumentClass(ManagerClass mgr, FolderClass folder, string name)
            : this(mgr, folder.Path.CombinePath(name))
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Path to the file
        /// 
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// 
        /// The holding folder
        /// 
        /// </summary>
        public FolderClass Folder
        {
            get { return new FolderClass(this.Parent, this.Path.GetDirectoryFromPath()); }
        }

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
        /// The name of the file with extension
        /// 
        /// </summary>
        public string Name
        {
            get { return this.Path.GetFileNameFromPath(); }
        }

        /// <summary>
        /// 
        /// The name of the file without extension
        /// 
        /// </summary>
        public string NameOnly
        {
            get { return this.Path.GetFileNameOnlyFromPath(); }
        }

        /// <summary>
        /// 
        /// The extension
        /// 
        /// </summary>
        public string Extension
        {
            get { return this.Path.GetExtensionFromPath(); }
        }

        /// <summary>
        /// 
        /// Does the file exist?
        /// 
        /// </summary>
        public bool Exists
        {
            get { return this.Location.FileExists(); }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a string
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Read, this);

                // Read from cloud
                this.Parent.SignalChange(c_P);

                // Read physical
                string sAns = this.Location.ReadFile();

                // Was it handled?
                if (c_P.Handled)
                {
                    // Delete so not to call cloud
                    this.Location.DeleteFile();
                }

                return sAns;
            }
            set
            {
                // Write physical
                this.Location.WriteFile(value);

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Write, this);

                // Write to cloud
                this.Parent.SignalChange(c_P);

                // Was it handled?
                if (c_P.Handled)
                {
                    // Delete so not to call cloud
                    this.Location.DeleteFile();
                }
            }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a byte array
        /// 
        /// </summary>
        public byte[] ValueAsBytes
        {
            get
            {
                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Read, this);

                // Read from cloud
                this.Parent.SignalChange(c_P);

                // Read physical
                byte[] abAns = this.Location.ReadFileAsBytes();

                // Was it handled?
                if (c_P.Handled)
                {
                    // Delete so not to call cloud
                    this.Location.DeleteFile();
                }

                return abAns;
            }
            set
            {
                // Write physical
                this.Location.WriteFileAsBytes(value);

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Write, this);

                // Write to cloud
                this.Parent.SignalChange(c_P);

                // Was it handled?
                if (c_P.Handled)
                {
                    // Delete so not to call cloud
                    this.Location.DeleteFile();
                }
            }
        }

        /// <summary>
        /// 
        /// The date and time that the file was updated
        /// if new, then DateTime.MaxValue
        /// 
        /// </summary>
        public DateTime WrittenOn
        {
            get
            {
                // Assume never
                DateTime c_Ans = DateTime.MaxValue;

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.GetLastWrite, this);

                // Get from cloud
                this.Parent.SignalChange(c_P);

                // Handled?
                if (c_P.Handled)
                {
                    // Get
                    c_Ans = (DateTime)c_P.On;
                }
                else
                {
                    // Is it there?
                    if (this.Location.FileExists())
                    {
                        // Get it
                        c_Ans = this.Location.GetLastWriteFromPath();
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The URL at which the file can be obtained.
        /// Useful for sending a file reference without
        /// exposing path information
        /// 
        /// </summary>
        public string URL
        {
            get
            {
                // Make the ID
                string sAns = this.Path.SHA1HashString();

                // Save a pointer back to itself
                using (DocumentClass c_Doc = new DocumentClass(this.Parent, this.MetadataFolder, ReversePointerFile))
                {
                    // Write
                    c_Doc.Value = this.Path;
                }

                // Make it.
                // NB:  Must use the route defined in Routes.Files.Support
                return this.Parent.Parent.SiteInfo.URL.CombineURL("f", sAns);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Delete the file (correctly)
        /// 
        /// </summary>
        public void Delete()
        {
            // Delete local
            this.Location.DeleteFile();
            // And any metadata
            this.MetadataFolder.Delete();

            // Make the parameter
            FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Delete, this);

            // Get from cloud
            this.Parent.SignalChange(c_P);
        }

        /// <summary>
        /// 
        /// Returns a file reference in a sub folder
        /// 
        /// </summary>
        /// <param name="folder">The sub folder name</param>
        /// <returns></returns>
        public DocumentClass InSubfolder(string folder)
        {
            // Make the path
            string sPath = this.Path.GetDirectoryFromPath().CombinePath(folder).CombinePath(this.Name);

            // And return the file
            return new DocumentClass(this.Parent, sPath);
        }

        /// <summary>
        ///   
        /// Copies document to another document
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void CopyTo(DocumentClass doc, bool includemetadata = true)
        {
            // Copy
            doc.ValueAsBytes = this.ValueAsBytes;
            // Copy metadata
            if (includemetadata)
            {
                foreach (DocumentClass c_Child in this.MetadataFolder.Files)
                {
                    // Copy
                    using (DocumentClass c_Target = new DocumentClass(this.Parent, doc.MetadataFolder, c_Child.Name))
                    {
                        c_Target.ValueAsBytes = c_Child.ValueAsBytes;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// Move a document
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void MoveTo(DocumentClass doc, bool includemetadata = true)
        {
            // Copy
            this.CopyTo(doc, includemetadata);
            // Delete
            this.Delete();
        }
        #endregion

        #region Streams
        /// <summary>
        /// 
        /// Reads from file as a stream
        /// 
        /// </summary>
        /// <param name="cb">The callback using a stream</param>
        public void AsReadStream(Action<Stream> cb)
        {
            // Make the parameter
            FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.GetStream, this);
            c_P.StreamCallback = cb;

            // Get from cloud
            this.Parent.SignalChange(c_P);

            // Is MinIO there?
            if (!c_P.Handled)
            {
                // Open
                using (FileStream c_Stream = new FileStream(this.Path, FileMode.Open, FileAccess.Read))
                {
                    // Call
                    cb(c_Stream);

                    // Close
                    try
                    {
                        c_Stream.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 
        /// Writes to the file as a stream
        /// 
        /// </summary>
        /// <param name="stream">The stream</param>
        public void AsWriteStream(Stream stream)
        {
            // Make the parameter
            FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.SetStream, this);
            c_P.Stream = stream;

            // Get from cloud
            this.Parent.SignalChange(c_P);

            // Is MinIO there?
            if (!c_P.Handled)
            {
                // Open
                using (FileStream c_Stream = new FileStream(this.Path, FileMode.Create, FileAccess.Write))
                {
                    //
                    stream.CopyTo(c_Stream);

                    // Close
                    try
                    {
                        c_Stream.Close();
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region Backups
        /// <summary>
        /// 
        /// Make sure that a backup exists
        /// 
        /// </summary>
        public void AssureBackup()
        {
            // Open
            using (DocumentClass c_Bkp = this.MetadataDocument("backup"))
            {
                // Newer?
                if (!c_Bkp.Exists || this.WrittenOn > c_Bkp.WrittenOn)
                {
                    // Copy
                    this.CopyTo(c_Bkp, false);
                }
            }
        }

        /// <summary>
        /// 
        /// Restores from backup, if any
        /// 
        /// </summary>
        public void RestoreBackup()
        {
            // Open
            using (DocumentClass c_Bkp = this.MetadataDocument("backup"))
            {
                // Newer?
                if (c_Bkp.Exists)
                {
                    // Copy
                    c_Bkp.CopyTo(this, false);
                }
            }
        }
        #endregion

        #region Metadata
        /// <summary>
        /// 
        /// The metadata folder for the document
        /// 
        /// </summary>
        private FolderClass IMetadataFolder { get; set; }
        private FolderClass MetadataFolder
        {
            get
            {
                if (this.IMetadataFolder == null)
                {
                    // Create
                    this.IMetadataFolder = this.Folder.SubFolder("_metadata").SubFolder(this.Name.MD5HashString());
                }

                return this.IMetadataFolder;
            }
        }

        /// <summary>
        /// 
        /// Returns document in the metadata folder
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public DocumentClass MetadataDocument(string bucket, string ext = null)
        {
            // Get the name
            string sName = bucket.IfEmpty(this.NameOnly);
            // And set the extension
            if (ext.HasValue()) sName += "." + ext;
            // Get from metadata folder
            return this.MetadataFolder[sName];
        }
        #endregion

        #region Statics
        /// <summary>
        /// 
        /// Returns the folder that holds the metadata
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <returns></returns>
        //public static string MetadataFolderRoot(EnvironmentClass env)
        //{
        //    return ManagerClass.MappedFolder.CombinePath("_metadata");
        //}
        #endregion
    }
}