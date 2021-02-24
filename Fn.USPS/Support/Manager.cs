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

using NX.Engine;
using NX.Shared;

namespace Fn.USPS
{
    /// <summary>
    /// 
    /// USPS interface
    /// 
    /// Defined settings
    /// 
    /// The following settings are used:
    /// 
    /// usps_key             - The API key provided by USPS.
    /// 
    /// </summary>
    public class ManagerClass : ChildOfClass<EnvironmentClass>
    {
        #region Constants
        private const string ProductionUrl = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string TestingUrl = "http://testing.shippingapis.com/ShippingAPITest.dll";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <param name="testmode">True if we are only testing</param>
        public ManagerClass(EnvironmentClass env)
            : base(env)
        {
            this.UserID = env["usps_key"];
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// re we testing?
        /// 
        /// </summary>
        private bool TestMode { get; set; }

        /// <summary>
        /// 
        /// The USPS key
        /// 
        /// </summary>
        public string UserID { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Makes the required URL
        /// 
        /// </summary>
        /// <param name="url">The raw URL</param>
        /// <param name="addr">Address block to use as data</param>
        /// <returns>The properly formatted URL</returns>
        public string MakeURL(string url, AddressBlock addr)
        {
            // Prefix with Production or Testing
            string sAns = (this.TestMode ? TestingUrl : ProductionUrl).CombineURL(url);

            // And add address
            sAns = sAns.FormatString(this.UserID,
                                                        addr.ID,
                                                        addr.Address1,
                                                        addr.Address2,
                                                        addr.City,
                                                        addr.State,
                                                        addr.ZIP,
                                                        addr.Plus4);

            return sAns;
        }
        #endregion
    }
}