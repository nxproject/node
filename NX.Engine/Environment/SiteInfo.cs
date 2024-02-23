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

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Information about the site.  Kept as part of
    /// the system settings
    /// 
    /// Defined settings
    /// 
    /// The following settings are used:
    /// 
    /// site_uuid           - The site unique identifier
    /// site_name           - The human readable site name
    /// site_url            - The human readable site name
    /// 
    /// </summary>
    public class SiteInfoClass : ChildOfClass<EnvironmentClass>
    {
        #region Constants
        private const string KeyUUID = "site_uuid";
        private const string KeyName = "site_name";
        private const string KeyURL = "site_url";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public SiteInfoClass(EnvironmentClass env)
            : base(env)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The site unique identifier
        /// 
        /// </summary>
        public string UUID { get { return this.Parent[KeyUUID]; } set { this.Parent[KeyUUID] = value; } }

        /// <summary>
        /// 
        /// The human readable site name
        /// 
        /// </summary>
        public string Name {  get { return this.Parent[KeyName]; } set { this.Parent[KeyName] = value; } }

        /// <summary>
        /// 
        /// The site URL
        /// 
        /// </summary>
        public string URL { get { return this.Parent[KeyURL]; } set { this.Parent[KeyURL] = value; } }
        #endregion
    }
}