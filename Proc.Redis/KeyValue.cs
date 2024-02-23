
using NX.Shared;
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
///

namespace Proc.Redis
{
    /// <summary>
    /// 
    /// A Redis based volatile key/value store.
    /// Setting the TTL property to a positive value will
    /// cause the keys to expire if not accessed within
    /// the time frame set.
    /// 
    /// </summary>
    public class KeyValueClass : BaseClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="mgr">The Redis manager</param>
        /// <param name="name">The name</param>
        public KeyValueClass(ManagerClass mgr, string name = "")
            : base(mgr, name)
        { }
        #endregion

        #region Indexer
        /// <summary>
        /// 
        /// Get/Sets a value for a given key
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public string this[string key]
        {
            get
            {
                // Assume none
                string sAns = null;

                // Are we connected?
                if (this.Parent.Client != null)
                {
                    // Make the full name
                    string sKey = this.Name.AsDelimitedName(key);

                    // Get it
                    sAns = this.Get(sKey);
                    // Did we get a value
                    if (sAns != null && this.TTL > 0)
                    {
                        // Set the expiration, giving it a lease on life
                        this.Expire(sKey, this.TTL.SecondsAsTimeSpan());
                    }
                }

                return sAns;
            }
            set
            {
                // Are we connected?
                if (this.Parent.Client != null)
                {
                    // Make the full name
                    string sKey = this.Name.AsDelimitedName(key);

                    // Save it
                    this.Set(sKey, value);
                    // And set the expiration
                    if (this.TTL > 0) this.Expire(sKey, this.TTL.SecondsAsTimeSpan());
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// How long to keep the value alive until it is obtained 
        /// 
        /// </summary>
        public int TTL { get; set; } = 0;
        #endregion
    }
}