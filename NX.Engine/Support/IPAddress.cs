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

using NX.Shared;

namespace NX.Engine
{
    public class IPAddressClass : IDisposable
    {
        #region Constants
        private const string Delimiter = ":";
        private const int DefaultPort = 80;
        #endregion

        #region Constructor
        public IPAddressClass(string ip, int port)
            : this(ip + ":" + port)
        { }

        public IPAddressClass(string ip, string port)
            : this(ip+ ":" + port)
        { }

        public IPAddressClass(string ip)
        {
            //
            string sIP = ip;
            string sPort = "";
            // Split
            int iPos = ip.IndexOf(Delimiter);
            // Any?
            if (iPos != -1)
            {
                sPort = sIP.Substring(iPos + 1);
                sIP = sIP.Substring(0, iPos);
            }

            this.IP = sIP.IfEmpty("localhost");
            this.UsesDefaultPort = !sPort.HasValue();
            this.Port = sPort.ToInteger(DefaultPort);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Properties
        public string IP { get; set; }
        public int Port { get; set; }

        public bool UsesDefaultPort { get; private set; }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.IP + Delimiter + this.Port;
        }
        #endregion
    }
}