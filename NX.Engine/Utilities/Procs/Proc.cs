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

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Base code for a process call
    /// 
    /// </summary>
    public class ProcClass : IPlugIn
    {
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        #region Constructor
        public ProcClass()
        { }
        #endregion

        #region IPlugIn
        /// <summary>
        /// 
        /// The name of the fuction.  Note that the system generates
        /// the name from the assembly and instance.  If the assembly
        /// is called Proc.Sample and the instance is called CallX
        /// the name would be Sample.CallX
        /// 
        /// </summary>
        public string Name 
        {
            get { return this.ObjectFullName(); }
        }

        /// <summary>
        /// 
        /// Code to be run when the function is loaded, once per
        /// session.
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public virtual void Initialize(EnvironmentClass env)
        { }

        /// <summary>
        /// 
        /// Code to run when the code is disposed
        /// 
        /// </summary>
        public virtual void Dispose()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Code called when the process is called
        /// 
        /// </summary>
        /// <param name="call">The call object that received the call</param>
        /// <param name="store">The store where the params are stored</param>
        public virtual StoreClass Do(HTTPCallClass call, StoreClass values)
        {
            return null;
        }
        #endregion
    }
}