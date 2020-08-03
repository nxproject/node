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

using NX.Engine;
using NX.Shared;

namespace Proc.File
{
    public class StorageClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        internal StorageClass(EnvironmentClass env)
            : base(env)
        {
            //
            this.Minio = this.Parent.Globals.Get<ManagerClass>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The Minio interface
        /// 
        /// </summary>
        internal ManagerClass Minio { get; set; }

        /// <summary>
        /// 
        /// Do we have access to Minio?
        /// 
        /// </summary>
        public bool MinIOEnabled
        {
            get { return this.Minio.IsAvailable; }
        }
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
            if (path.StartsWith(this.Parent.DocumentFolder))
            {
                path = path.Substring(this.Parent.DocumentFolder.Length);
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
            return this.Parent.DocumentFolder.CombinePath(path);
        }
        #endregion
    }
}