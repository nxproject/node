///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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
/// Install-Package Docker.DotNet -Version 3.125.2
/// 

using System;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A message from one bee to another
    /// 
    /// </summary>
    public class BeeMessageClass : IDisposable
    {
        #region Constructor
        public BeeMessageClass(string field, string id, string message)
        {
            //
            this.Values = new JObject();
        }

        public BeeMessageClass(string payload)
        {
            //
            this.Values = payload.ToJObject();
        }

        public BeeMessageClass(JObject payload)
        {
            //
            this.Values = payload;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The underlying message
        /// 
        /// </summary>
        public JObject Values { get; private set; }

        /// <summary>
        /// 
        /// The bee's field
        /// 
        /// </summary>
        public string Field
        {
            get { return this.Values.Get("field"); }
            set { this.Values.Set("field", value); }
        }

        /// <summary>
        /// 
        /// The bee's ID
        /// 
        /// </summary>
        public string Id
        {
            get { return this.Values.Get("Id"); }
            set { this.Values.Set("Id", value); }
        }

        /// <summary>
        /// 
        /// The message
        /// 
        /// </summary>
        public string Message
        {
            get { return this.Values.Get("msg"); }
            set { this.Values.Set("msg", value); }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.Values.ToSimpleString();
        }
        #endregion
    }
}