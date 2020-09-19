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
using System.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A URl that can be connect to
    /// 
    /// </summary>
    public class TickleAreaClass : ChildOfClass<BeeCVClass>
    {
        #region Constructor
        public TickleAreaClass(BeeCVClass cv, string publicport, string privateport, string url = null)
            : base(cv)
        {
            // From parameters
            this.PublicPort = publicport;
            this.PrivatePort = privateport;

            // Get the field location
            string sURL = url;
            // If none, get from field
            if (!sURL.HasValue())
            {
                sURL = this.Parent.Parent.Parent.GetFieldURL(this.Field);
            }
            // One available?
            if (sURL.HasValue())
            {
                // Does it have a port
                int iPos = sURL.LastIndexOf(":");
                // Remove it
                if (iPos != -1) sURL = sURL.Substring(0, iPos);
                // Add public port
                this.Location = sURL + ":" + this.PublicPort;
            }

            // Make the DNA
            string sDNA = this.Parent.DNA;
            // Process?
            if (sDNA.IsSameValue(HiveClass.ProcessorDNAName))
            {
                // Add the process
                sDNA += "." + this.Parent.Proc;
            }

            // Save
            this.DNA = sDNA;
        }
        #endregion

        #region Properties
        public string BeeId { get { return this.Parent.Id; } }
        public string Field { get { return this.Parent.Field; } }
        public string DNA { get; private set; }

        public string Location { get; private set; }
        public string PublicPort { get; private set; }
        public string PrivatePort { get; private set; }

        public bool IsAvailable { get { return this.Location.HasValue(); } }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.DNA + ":" + this.PublicPort + ":" + this.PrivatePort;
        }
        #endregion
    }
}