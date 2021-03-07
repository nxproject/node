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
/// 

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using NX.Shared;

namespace NX.Engine.Files
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
            this.Path = this.Parent.Collapse(path).StandarizePath();

            // And make sure that it exists
            if (!this.Parent.IsAvailable)
            {
                // If we have a disk based system, create the path
                this.AssurePath();
            }
        }
        #endregion

        #region Indexer
        public DocumentClass this[string name]
        {
            get { return new DocumentClass(this.Parent, this.Path.CombinePath(name)); }
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
        /// Returns the folder name only
        /// 
        /// </summary>
        public string Name { get { return this.Path.GetFileNameFromPath(); } }

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
        /// Does the file exist?
        /// 
        /// </summary>
        public bool Exists
        {
            get { return this.Location.DirectoryExists(); }
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

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.ListFiles, this);

                // Get from cloud
                this.Parent.SignalChange(c_P);

                // DOne?
                if (c_P.Handled)
                {
                    // Get list
                    List<string> c_List = c_P.List;
                    // Sort
                    c_List.Sort();
                    // Loop thru
                    foreach (string sEntry in c_List)
                    {
                        // Add
                        c_Ans.Add(new DocumentClass(this.Parent, sEntry));
                    }
                }
                else
                {
                    // Get list
                    List<string> c_List = this.Location.GetFilesInPath();
                    // Sort
                    c_List.Sort();
                    c_List = c_List.Unique();
                    // Loop thru
                    foreach (string sEntry in c_List)
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

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.ListFolders, this);

                // Get from cloud
                this.Parent.SignalChange(c_P);

                // DOne?
                if (c_P.Handled)
                {
                    // Get list
                    List<string> c_List = c_P.List;
                    // Sort
                    c_List.Sort();
                    // Loop thru
                    foreach (string sEntry in c_List)
                    {
                        // Add
                        c_Ans.Add(new FolderClass(this.Parent, sEntry));
                    }
                }
                else
                {
                    // Get list
                    List<string> c_List = this.Location.GetDirectoriesInPath();
                    // Sort
                    c_List.Sort();
                    // Loop thru
                    foreach (string sEntry in c_List)
                    {
                        c_Ans.Add(new FolderClass(this.Parent, sEntry));
                    }
                }

                return c_Ans;
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
            // Physical
            this.Location.DeletePath();

            // Make the parameter
            FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Delete, this);

            // Get from cloud
            this.Parent.SignalChange(c_P);
        }

        /// <summary>
        ///  Makes sure that the path exists
        ///  
        /// </summary>
        public void AssurePath()
        {
            // Make the parameter
            FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.CreatePath, this);

            // Get from cloud
            this.Parent.SignalChange(c_P);

            // Is MinIO there?
            if (!c_P.Handled)
            {
                //
                this.Location.AssurePath();
            }
            else
            {

            }
        }

        /// <summary>
        /// 
        /// Returns a sub folder (if any)
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderClass SubFolder(string path)
        {
            // Assume same
            FolderClass c_Ans = this;
            // Do we have a value?
            if (path.HasValue())
            {
                c_Ans = new FolderClass(this.Parent, this.Path.CombinePath(path));

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.CreatePath, this);

                // Get from cloud
                this.Parent.SignalChange(c_P);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Get the tree of folders and files
        /// 
        /// </summary>
        public JArray Tree(bool showhidden = false, bool filesonly = false)
        {
            // Assume none
            JArray c_Ans = new JArray();

            // Loop thru
            foreach (FolderClass c_Folder in this.Folders)
            {
                // Must be valid
                if (c_Folder.Exists)
                {
                    // Get the name
                    string sName = c_Folder.Path.GetDirectoryNameFromPath();

                    if (showhidden || !sName.StartsWith("_"))
                    {
                        // 
                        if (filesonly)
                        {
                            JArray c_Files = c_Folder.Tree(showhidden, filesonly);
                            c_Ans.AddElements(c_Files);
                        }
                        else
                        {
                            // Make entry
                            JObject c_Entry = new JObject();

                            c_Entry.Set("name", sName);
                            c_Entry.Set("path", c_Folder.Path);
                            c_Entry.Set("items", c_Folder.Tree(showhidden, filesonly));

                            // Save
                            c_Ans.Add(c_Entry);
                        }
                    }
                }
            }

            // Loop thru
            foreach (DocumentClass c_File in this.Files)
            {
                // Must be valid
                if (c_File.Exists)
                {
                    // Make entry
                    JObject c_Entry = new JObject();

                    c_Entry.Set("name", c_File.Path.GetFileNameFromPath());
                    c_Entry.Set("path", c_File.Path);

                    // Save
                    c_Ans.Add(c_Entry);
                }
            }

            return c_Ans;
        }
        #endregion

        #region Backups
        /// <summary>
        /// 
        /// Make sure that a backup exists
        /// 
        /// </summary>
        public int AssureBackup()
        {
            int iAns = 0;

            // Open
            foreach (DocumentClass c_File in this.Files)
            {
                iAns += c_File.AssureBackup();
            }

            foreach(FolderClass c_Folder in this.Folders)
            {
                iAns += c_Folder.AssureBackup();
            }

            return iAns;
        }

        /// <summary>
        /// 
        /// Restores from backup, if any
        /// 
        /// </summary>
        public int RestoreBackup()
        {
            int iAns = 0;

            // Open
            foreach (DocumentClass c_File in this.Files)
            {
                iAns += c_File.RestoreBackup();
            }

            foreach (FolderClass c_Folder in this.Folders)
            {
                iAns += c_Folder.RestoreBackup();
            }

            return iAns;
        }
        #endregion
    }
}