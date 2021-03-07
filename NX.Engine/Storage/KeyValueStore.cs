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

using System;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Disk based Key/Value store
    /// 
    /// </summary>
    public class KeyValueStoreClass : SynchronizedStoreClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <param name="path">The folder path to where the key/values are stored</param>
        /// <param name="memonly">If true, the values are stored in memory only</param>
        /// <param name="ext">The extension for each key file (Default: ks)</param>
        public KeyValueStoreClass(EnvironmentClass env, string synch)
            : base(env, synch)
        { }
        #endregion

        #region Events
        /// <summary>
        ///  
        /// The callback for dynamic updates
        /// 
        /// </summary>
        private Action<string, string> IReceivedCallback { get; set; }
        public Action<string, string> ReceivedCallback
        { 
            get { return this.IReceivedCallback; }
            set 
            { 
                // Store
                this.IReceivedCallback = value;  
                // Link up
                base.ChangedCalled += delegate(string key, object value)
                {
                    // Do we have a callback?
                    if(this.IReceivedCallback != null)
                    {
                        this.IReceivedCallback(key, value.ToStringSafe());
                    }
                }; 
            }
        }
        #endregion
    }
}