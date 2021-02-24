///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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

using NX.Engine;
using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// A Traefik processor command
    /// 
    /// </summary>
    public class CommandClass : ChildOfClass<InterfaceClass>
    {
        #region Constructor
        internal CommandClass(InterfaceClass itf)
            : base(itf)
        {
            //
            this.Values = new JObject();
        }

        internal CommandClass(InterfaceClass itf, string value)
            : base(itf)
        {
            // Cheap way to "secure" the call
            this.Values = value.DecodeFromBase64(this.Parent.Parent.Parent.TraefikHive).ToJObject();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Internal store
        /// 
        /// </summary>
        private JObject Values { get; set; }

        /// <summary>
        /// 
        /// The hive that originated the command
        /// 
        /// </summary>
        public string From
        {
            get { return this.Values.Get("from"); }
            internal set { this.Values.Set("from", value); }
        }

        /// <summary>
        /// 
        /// The command
        /// 
        /// </summary>
        public string Command
        {
            get { return this.Values.Get("cmd"); }
            set { this.Values.Set("cmd", value); }
        }

        /// <summary>
        /// 
        /// The IP address
        /// 
        /// </summary>
        public string IP
        {
            get { return this.Values.Get("ip"); }
            set { this.Values.Set("ip", value); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Sends a command to the Traefik site
        /// 
        /// </summary>
        public JObject Send()
        {
            // Assume no call
            JObject c_Ans = null;

            if(this.Parent.Parent.Parent.TraefikHive.HasValue())
            {
                // Encode
                string sMsg = this.Values.ToSimpleString().EncodeToBase64(this.Parent.Parent.Parent.TraefikHive);

                // Mke URL
                string sURL = (this.Parent.Parent.Parent.TraefikHive + "." + this.Parent.ENVDomain).URLMake().CombinePath(Do.Route);
                // Call
                c_Ans = sURL.URLPost(sMsg.ToBytes()).FromBytes().ToJObject();
            }

            return c_Ans;
        }
        #endregion

        #region Statics
        ///// <summary>
        ///// 
        ///// The name of ythe hive that runs Traefik
        ///// 
        ///// </summary>
        //public static string TraefikHive { get; set; }
        #endregion
    }
}