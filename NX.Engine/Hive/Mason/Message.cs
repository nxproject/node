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

using NX.Shared;
using System;

namespace NX.Engine.Hive.Mason
{
    /// <summary>
    /// 
    /// A mason message
    /// 
    /// </summary>
    public class MessageClass : StoreClass
    {
        #region Constants
        private const string KeyID = "id";
        private const string KeyQueue = "queue";
        internal const string KeyRequestorBee = "from";
        internal const string KeyMasonBee = "mason";
        private const string KeyRequest = "request";
        private const string KeyResponse = "response";
        internal const string KeyDoNotRespond = "dnr";

        internal const string KeyRequestTS = "requestts";
        internal const string KeyResponseTS = "responsets";

        private const string KeyRequestTTL = "requestttl";
        private const string KeyResponseTTL = "responsettl";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor (plain)
        /// 
        /// </summary>
        public MessageClass()
            : base()
        {
            this.ID = "".GUID();
        }

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="values">The values as a JObject</param>
        public MessageClass(JObject values)
            : base(values)
        { }

        /// <summary>
        /// 
        /// Constructor (basic)
        /// 
        /// </summary>
        /// <param name="value">The value to use as the stating contents</param>
        /// <param name="indeserialize">If true, we are in a desisialization loop</param>
        public MessageClass(string value)
            : base(value)
        { }

        /// <summary>
        /// 
        /// Constuctor
        /// 
        /// </summary>
        /// <param name="values">The string array of key/values pair</param>
        public MessageClass(params string[] values)
            : base()
        {
            // Loop thru
            for (int i = 0; i < values.Length; i += 2)
            {
                // Set
                this[values[i]] = values[i + 1];
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The ID of the message
        /// 
        /// </summary>
        public string ID
        {
            get { return this[KeyID]; }
            private set { this[KeyID] = value; }

        }

        /// <summary>
        /// 
        /// The queue being used 
        /// 
        /// </summary>
        public string Queue
        {
            get { return this[KeyQueue]; }
            set { this[KeyQueue] = value; }
        }

        /// <summary>
        /// 
        /// The ID of the bee that requested 
        /// 
        /// </summary>
        public string RequestorBee
        {
            get { return this[KeyRequestorBee]; }
            set { this[KeyRequestorBee] = value; }
        }

        /// <summary>
        /// 
        /// The ID of the bee that did the work 
        /// 
        /// </summary>
        public string MasonBee
        {
            get { return this[KeyMasonBee]; }
            set { this[KeyMasonBee] = value; }
        }

        /// <summary>
        /// 
        /// The request 
        /// 
        /// </summary>
        public StoreClass Request
        {
            get { return this.GetAsStore(KeyRequest); }
            set { this.Set(KeyRequest, value); }
        }

        /// <summary>
        /// 
        /// The response 
        /// 
        /// </summary>
        public StoreClass Response
        {
            get { return this.GetAsStore(KeyResponse); }
            set { this.Set(KeyResponse, value); }
        }

        /// <summary>
        /// 
        /// Number of milliseconds that the request is valid
        /// 
        /// </summary>
        public TimeSpan RequestTTL
        {
            get { return this[KeyRequestTTL].ToDouble(0).MillisecondsAsTimeSpan(); }
            set { this[KeyRequestTTL] = value.TotalMilliseconds.ToString(); }
        }

        /// <summary>
        /// 
        /// Number of milliseconds that the response is valid
        /// 
        /// </summary>
        public TimeSpan ResponseTTL
        {
            get { return this[KeyResponseTTL].ToDouble(0).MillisecondsAsTimeSpan(); }
            set { this[KeyResponseTTL] = value.TotalMilliseconds.ToString(); }
        }

        /// <summary>
        /// 
        /// The timestamp of when the request was sent
        /// 
        /// </summary>
        public DateTime? RequestTimestamp
        {
            get
            {
                // Assume none
                DateTime? c_Ans = null;

                // A value?
                if (this[KeyRequestTS].HasValue()) c_Ans = DateTime.Parse(this[KeyRequestTS]);

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The timestampe of when the response was sent
        /// 
        /// </summary>
        public DateTime? ResponseTimestamp
        {
            get
            {
                // Assume none
                DateTime? c_Ans = null;

                // A value?
                if (this[KeyResponseTS].HasValue()) c_Ans = DateTime.Parse(this[KeyResponseTS]);

                return c_Ans;
            }
        }

        public bool DoNotRespond
        {
            get { return this[KeyDoNotRespond].ToBoolean(); }
            set { this[KeyDoNotRespond] = value.ToString(); }
        }
        #endregion
    }
}