///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2024 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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

using System;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    public class KeyJObjectStoreClass : KeyValueStoreClass
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
        public KeyJObjectStoreClass(EnvironmentClass env, string synch)
            : base(env, synch)
        { }

        #endregion

        #region Indexer
        /// <summary>
        /// 
        /// Gte/Set as an JSON object
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The JSON object</returns>
        public new JObject this[string key]
        {
            get { return base[key].ToJObject(); }
            set { base[key] = value.ToString(); }
        }
        #endregion

        #region Properties
        /// <summary>
        ///  
        /// The callback for dynamic updates
        /// 
        /// </summary>
        private Action<string, JObject> IReceivedCallback { get; set; }
        public new Action<string, JObject> ReceivedCallback
        {
            get { return this.IReceivedCallback; }
            set
            {// Store
                this.IReceivedCallback = value;
                // Link up
                base.ReceivedCallback = delegate (string key, string value)
                {
                    // Do we have a callback?
                    if (this.IReceivedCallback != null)
                    {
                        this.IReceivedCallback(key, value.ToJObject());
                    }
                };
            }
        }
        #endregion
    }
}