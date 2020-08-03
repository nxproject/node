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

using System.Collections.Generic;

using NX.Engine;
using NX.Shared;

namespace Proc.File
{
    /// <summary>
    /// 
    /// A folder in the documents tree
    /// 
    /// </summary>
    public class FolderClass : ChildOfClass<StorageClass>
    {
        #region Constructor
        public FolderClass(StorageClass stg, string path)
            : base(stg)
        {
            // Save
            this.Path = this.Parent.Collapse(path);

            // And make sure that it exists
            if (!this.Parent.MinIOEnabled)
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

                foreach (string sEntry in this.Location.GetFilesInPath())
                {
                    c_Ans.Add(new DocumentClass(this.Parent, sEntry));
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
                if (this.Parent.MinIOEnabled)
                {

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
            this.Location.DeletePath();
        }

        /// <summary>
        /// 
        /// Copies all entries in the folder to another folder
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(FolderClass target)
        {
            // Do
            this.Location.CopyDirectoryTree(target.Location);
        }

        public void AssurePath()
        {

        }
        #endregion

        #region MinIO
        #endregion
    }

    public class FolderInfoClass : ChildOfClass<FolderClass>
    {
        #region Constructor
        internal FolderInfoClass(FolderClass folder)
            : base(folder)
        { }
        #endregion
    }
}