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
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

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

        public const string MergeMapFile = "merge.map";
        #endregion

        #region Constructor
        public DocumentClass(ManagerClass mgr, string path)
            : base(mgr)
        {
            // Save
            this.Path = this.Parent.Collapse(path);
        }

        public DocumentClass(ManagerClass mgr, FolderClass folder, string name)
            : this(mgr, folder.Path.CombinePath(name))
        { }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            if (this.IMergeMap != null)
            {
                this.IMergeMap.Dispose();
                this.IMergeMap = null;
            }

            base.Dispose();
        }
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
        /// Gets/sets the value of the file as a string
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                // Assume noting
                string sAns = null;

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Get
                    sAns = this.Parent.GetObject(this.Path);
                }
                else
                {
                    // Read physical
                    sAns = this.Location.ReadFile();
                }

                return sAns;
            }
            set
            {
                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Set
                    this.Parent.SetObject(this.Path, value);
                }
                else
                {
                    // Write physical
                    this.Location.WriteFile(value);
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
                // Assume noting
                byte[] abAns = null;

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    abAns = this.Value.ToBytes();
                }
                else
                {
                    // Read physical
                    abAns = this.Location.ReadFileAsBytes();
                }

                return abAns;
            }
            set
            {
                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    this.Value = value.FromBytes();
                }
                else
                {
                    // Write physical
                    this.Location.WriteFileAsBytes(value);
                }
            }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a PDF string
        /// 
        /// </summary>
        public byte[] ValueAsPDF
        {
            get
            {
                // Assume noting
                byte[] abAns = null;

                // According to type
                switch (this.Extension.ToLower())
                {
                    case "pdf":
                        abAns = this.ValueAsBytes;
                        break;

                    case "docx":
                        abAns = ConversionClass.DOCX2PDF(this.ValueAsBytes, this.Name);
                        break;
                }

                return abAns;
            }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as an HTML string
        /// 
        /// </summary>
        public byte[] ValueAsHTML
        {
            get
            {
                // Assume noting
                byte[] abAns = null;

                // According to type
                switch (this.Extension.ToLower())
                {
                   case "docx":
                        abAns = ConversionClass.DOCX2HTML(this.ValueAsBytes, this.Name);
                        break;
                }

                return abAns;
            }
            set
            {
                // According to type
                switch (this.Extension.ToLower())
                {
                    case "docx":
                        this.ValueAsBytes = ConversionClass.HTML2DOCX(value, this.Name);
                        break;
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

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Via folder
                    using (FolderClass c_Folder = this.Folder)
                    {
                        // Get
                        c_Ans = this.Parent.GetAttribute(ManagerClass.Types.LastWrite, this.Path).FromDBDate();
                    }
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

        /// <summary>
        /// 
        /// The metadata folder for the document
        /// 
        /// </summary>
        private FolderClass IMetadataFolder { get; set; }
        public FolderClass MetadataFolder
        {
            get
            {
                if (this.IMetadataFolder == null)
                {
                    // Make the path
                    string sPath = DocumentClass.MetadataFolderRoot(this.Parent.Parent).CombinePath(this.Path.SHA1HashString());
                    // Create
                    this.IMetadataFolder = new FolderClass(this.Parent, sPath);
                }

                return this.IMetadataFolder;
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
            // Delete the file itself
            if (this.Parent.IsAvailable)
            {
                // Delete
                this.Parent.DeleteObject(this.Path);
            }
            else
            {
                this.Location.DeleteFile();
            }
            // And any metadata
            this.MetadataFolder.Delete();
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
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                // Get
                this.Parent.GetStream(this.Path, cb);
            }
            else
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
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                // Get
                this.Parent.SetStream(this.Path, stream);
            }
            else
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

        #region Merge
        /// <summary>
        /// 
        /// The path to the merge map
        /// 
        /// </summary>
        public DocumentClass MergeMapDocument
        {
            get { return new DocumentClass(this.Parent, this.MetadataFolder, MergeMapFile); }
        }

        /// <summary>
        /// 
        /// The merge map
        /// 
        /// </summary>
        private MergeMapClass IMergeMap { get; set; }
        private MergeMapClass MergeMap
        {
            get
            {
                // Is it cached?
                if (this.IMergeMap == null)
                {
                    // No, create it
                    using (DocumentClass c_Map = this.MergeMapDocument)
                    {
                        // New?
                        bool bNew = c_Map.WrittenOn < this.WrittenOn;

                        // Load
                        this.IMergeMap = new MergeMapClass(c_Map);

                        // New?
                        if (bNew)
                        {
                            this.IMergeMap.MakeFields(this);
                        }
                    }
                }
                return this.IMergeMap;
            }
        }

        /// <summary>
        /// 
        /// Merges the document with a given store of data
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        public void Merge(DocumentClass result, StoreClass data)
        {
            // According to type
            switch (this.Extension)
            {
                case "docx":
                    // Create support object for MS Word
                    using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                    {
                        // And merge
                        result.ValueAsBytes = c_Filler.Merge(DocumentClass.MiniMerge(this.Parent, this, data), data);
                    }
                    break;

                case "pdf":
                case "fdf":
                    // Create support object for Adobe
                    using (Vendors.PDFClass c_Filler = new Vendors.PDFClass())
                    {
                        // And merge
                        result.ValueAsBytes = c_Filler.Merge(this.ValueAsBytes, data);
                    }
                    break;

                default:
                    // Otherwise an empty file
                    result.Value = "";
                    break;
            }
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
        public static string MetadataFolderRoot(EnvironmentClass env)
        {
            return env.DocumentFolder.CombinePath("_metadata");
        }

        /// <summary>
        /// 
        /// Does the work of merging a document.  Note that 
        /// this is a recursive operation
        /// 
        /// </summary>
        /// <param name="stg">The current storage manager</param>
        /// <param name="source">The source document</param>
        /// <param name="data">The merge data</param>
        /// <returns></returns>
        private static byte[] MiniMerge(ManagerClass mgr, DocumentClass source, StoreClass data)
        {
            byte[] abAns = source.ValueAsBytes; ;

            using (Vendors.DocXClass c_Merge = new Vendors.DocXClass())
            {
                for (int iIndex = 10; iIndex > 0; iIndex--)
                {
                    JArray c_Docs = source.MergeMap.GetDocs(MergeMapClass.PPDocTypes.PreDoc, iIndex);
                    if (c_Docs != null)
                    {
                        for (int iDoc = 0; iDoc < c_Docs.Count; iDoc++)
                        {
                            string sMDoc = c_Docs.Get(iDoc);
                            if (sMDoc.HasValue())
                            {
                                DocumentClass c_Wkg = new DocumentClass(mgr, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(mgr, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, mgr.Parent));
                                }

                                abAns = c_Merge.Append(abAns, abExtra, false);
                            }
                        }
                    }
                }

                for (int iIndex = 1; iIndex <= 10; iIndex++)
                {
                    JArray c_Docs = source.MergeMap.GetDocs(MergeMapClass.PPDocTypes.PostDoc, iIndex);
                    if (c_Docs != null)
                    {
                        for (int iDoc = 0; iDoc < c_Docs.Count; iDoc++)
                        {
                            string sMDoc = c_Docs.Get(iDoc);
                            if (sMDoc.HasValue())
                            {
                                DocumentClass c_Wkg = new DocumentClass(mgr, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(mgr, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, mgr.Parent)); ;
                                }

                                abAns = c_Merge.Append(abAns, abExtra, true);
                            }
                        }
                    }
                }
            }

            return abAns;
        }
        #endregion
    }
}