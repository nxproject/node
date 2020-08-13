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
            this.Path = this.Parent.Collapse(path);

            // And make sure that it exists
            if (!this.Parent.IsAvailable)
            {
                // If we have a disk based system, create the path
                this.AssurePath();
            }
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
                    // Get list of children
                    List<string> c_Files = this.Parent.ListObjectvalues(this.Parent.MakeAttributeName(ManagerClass.Types.Child, this.Path));
                    // Loop thru
                    foreach(string sEntry in c_Files)
                    {
                        // Add
                        c_Ans.Add(new DocumentClass(this.Parent, sEntry));
                    }
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
                    // Get list of children
                    List<string> c_Dirs = this.Parent.ListObjectvalues(this.Parent.MakeAttributeName(ManagerClass.Types.ChildFolder, this.Path));
                    // Loop thru
                    foreach (string sEntry in c_Dirs)
                    {
                        // Add
                        c_Ans.Add(new FolderClass(this.Parent, sEntry));
                    }
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
                // Delete
                this.Parent.DeleteObject(this.Path, true);
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
                this.AssureFolder();
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
        /// Make sure that the folder entry
        /// 
        /// </summary>
        private void AssureFolder()
        {
            // Do we have Minio
            if (this.Parent.IsAvailable)
            {
                // Asssure
                // Does it exist?
                if (!this.Parent.GetAttribute(ManagerClass.Types.Path, this.Path).HasValue())
                {
                    // Create
                    this.Parent.SetAttribute(ManagerClass.Types.Path, this.Path, this.Path);
                    // And set the creation time
                    this.Parent.SetAttribute(ManagerClass.Types.LastWrite, this.Path, DateTime.Now.ToUniversalTime().ToDBDate());

                    // Get the  parent path
                    string sPath = this.Path.GetParentDirectoryFromPath();
                    // Anything left?
                    if (sPath.HasValue())
                    {
                        // Assure
                        using (FolderClass c_Parent = new FolderClass(this.Parent, sPath))
                        {
                            // make if needed
                            c_Parent.AssureFolder();

                            // Set us as a child folder
                            this.Parent.SetAttribute(ManagerClass.Types.ChildFolder, c_Parent.Path, this.Path);
                        }
                    }
                }
            }
        }
        #endregion
    }
}