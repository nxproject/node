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

using System.Collections.Generic;
using System.Xml;

using NX.Shared;

namespace Fn.USPS
{
    /// <summary>
    /// 
    /// The data area used by all USPS calls
    /// 
    /// </summary>
    public class AddressBlock : ChildOfClass<StoreClass>
    {
        #region Constants
        private const string ArgAddress1 = "address1";
        private const string ArgAddress2 = "address2";
        private const string ArgCity = "city";
        private const string ArgState = "state";
        private const string ArgZIP = "zip";
        private const string ArgPlus4 = "plus4";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        public AddressBlock()
            : base(new StoreClass())
        { }

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="store">The store where the data is ocated</param>
        public AddressBlock(StoreClass store)
            : base(store)
        {
            // Validate
            List<string> c_Ans = new List<string>();

            if (this.Address1.Length > 38) c_Ans.Add("Address1 is is limited to a maximum of 38 characters.");
            if (this.Address2.Length > 38) c_Ans.Add("Address2 is is limited to a maximum of 38 characters.");
            if (this.City.Length > 15) c_Ans.Add("City is is limited to a maximum of 15 characters.");
            if (this.State.Length > 2) c_Ans.Add("State is is limited to a maximum of 2 characters.");
            if (this.ZIP.Length > 5) c_Ans.Add("Zip is is limited to a maximum of 5 characters.");
            if (this.Plus4.Length > 4) c_Ans.Add("ZipPlus4 is is limited to a maximum of 4 characters.");

            this.LastError = c_Ans.Join(", ");
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// First address line
        /// 
        /// </summary>
        public string Address1 { get { return this.Parent[ArgAddress1]; } set { this.Parent[ArgAddress1] = value.MaxLength(38); } }

        /// <summary>
        /// 
        /// The second address line
        /// 
        /// </summary>
        public string Address2 { get { return this.Parent[ArgAddress2]; } set { this.Parent[ArgAddress2] = value.MaxLength(38); } }

        /// <summary>
        /// 
        /// The city
        /// 
        /// </summary>
        public string City { get { return this.Parent[ArgCity]; } set { this.Parent[ArgCity] = value.MaxLength(15); } }

        /// <summary>
        /// 
        /// State abbreviation
        /// 
        /// </summary>
        public string State { get { return this.Parent[ArgState]; } set { this.Parent[ArgState] = value.MaxLength(2); } }

        /// <summary>
        /// 
        /// The ZIP code
        /// 
        /// </summary>
        public string ZIP { get { return this.Parent[ArgZIP]; } set { this.Parent[ArgZIP] = value.MaxLength(5); } }

        /// <summary>
        /// 
        /// The ZIP Plus 4
        /// 
        /// </summary>
        public string Plus4 { get { return this.Parent[ArgPlus4]; } set { this.Parent[ArgPlus4] = value.MaxLength(4); } }

        /// <summary>
        /// 
        /// Identifier
        /// 
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// 
        /// Error return from last call
        /// 
        /// </summary>
        public string LastError { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Load from USPS XML record
        /// 
        /// </summary>
        /// <param name="element">The root element</param>
        public void LoadXml(XmlNode element)
        {
            XmlNode celement = element.SelectSingleNode("Address1");
            if (celement != null)
            {
                this.Address1 = celement.InnerText;
            }

            celement = element.SelectSingleNode("Address2");
            if (celement != null)
            {
                this.Address2 = celement.InnerText;
            }

            celement = element.SelectSingleNode("City");
            if (celement != null)
            {
                this.City = celement.InnerText;
            }

            celement = element.SelectSingleNode("State");
            if (celement != null)
            {
                this.State = celement.InnerText;
            }

            celement = element.SelectSingleNode("Zip5");
            if (celement != null)
            {
                this.ZIP = celement.InnerText;
            }

            celement = element.SelectSingleNode("Zip4");
            if (celement != null)
            {
                this.Plus4 = celement.InnerText;
            }

            if (string.IsNullOrEmpty(this.Address1))
            {
                this.Address1 = this.Address2;
                this.Address2 = "";
            }
            else if (!string.IsNullOrEmpty(this.Address2))
            {
                this.Address1 = this.Address2;
                this.Address2 = "";
            }

        }
        #endregion
    }
}