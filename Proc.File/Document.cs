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
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package itextSharp -Version 5.5.13.1
/// 

using System;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace Proc.File
{
    /// <summary>
    /// 
    /// A document in the document tree
    /// 
    /// </summary>
    public class DocumentClass : ChildOfClass<StorageClass>
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
        public DocumentClass(StorageClass stg, string path)
            : base(stg)
        {
            // Save
            this.Path = this.Parent.Collapse(path);
        }

        public DocumentClass(StorageClass stg, FolderClass folder, string name)
            : this(stg, folder.Path.CombinePath(name))
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
            if(this.IMergeMap != null)
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
            get { return this.Location.ReadFile(); }
            set { this.Location.WriteFile(value); }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a byte array
        /// 
        /// </summary>
        public byte[] ValueAsBytes
        {
            get { return this.Location.ReadFileAsBytes(); }
            set { this.Location.WriteFileAsBytes(value); }
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

                // Is it there?
                if(this.Location.FileExists())
                {
                    // Get it
                    c_Ans = this.Location.GetLastWriteFromPath();
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
                using(DocumentClass c_Doc = new DocumentClass(this.Parent, this.MetadataFolder, ReversePointerFile))
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
                if(this.IMetadataFolder == null)
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
            this.Location.DeleteFile();
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
        private static byte[] MiniMerge(StorageClass stg, DocumentClass source, StoreClass data)
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
                                DocumentClass c_Wkg = new DocumentClass(stg, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(stg, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, stg.Parent));
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
                                DocumentClass c_Wkg = new DocumentClass(stg, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(stg, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, stg.Parent)); ;
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