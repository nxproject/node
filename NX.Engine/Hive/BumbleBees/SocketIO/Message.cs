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
using Newtonsoft.Json.Linq;
using NX.Shared;

namespace NX.Engine.SocketIO
{
    /// <summary>
    /// 
    /// A SOcket.IO message
    /// 
    /// </summary>
    public class MessageClass : ChildOfClass<EventClass>
    {
        #region Constructor
        internal MessageClass(EventClass evt)
            : base(evt)
        {
            //
            this.Payload = new JObject();
        }

        internal MessageClass(EventClass evt, string payload)
            : base(evt)
        {
            //
            this.Payload = payload.ToJObject();
        }
        #endregion

        #region Indexer
        public string this[string key]
        {
            get { return this.Payload.Get(key); }
            set { this.Payload.Set(key, value); }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The data in the message
        /// 
        /// </summary>
        private JObject Payload { get; set; } = new JObject();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Sends message
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Send()
        {
            // Assume failure
            bool bAns = false;

            // Check
            if (this.Parent != null && 
                this.Parent.Parent != null && 
                this.Parent.Parent.Client != null && 
                this.Parent.Parent.Client.Connected)
            {
                // Send
                this.Parent.Parent.Client.EmitAsync(this.Parent.Name, this.Payload.ToSimpleString());
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Payload.ToSimpleString();
        }

        /// <summary>
        /// 
        /// Gets a JSON obkect from the message
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JObject GetJObject(string key)
        {
            return this.Payload.GetJObject(key); 
        }
        #endregion
    }
}