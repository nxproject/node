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

namespace NX.Engine.NginX
{
    /// <summary>
    /// 
    /// NginX information about a route
    /// 
    /// </summary>
    public class InformationClass : ChildOfClass<ServicesClass>
    {
        #region Constructor
        public InformationClass(ServicesClass svcs, string location, bool rewrite)
            : base(svcs)
        {
            //
            this.Location = location;
            this.Rewrite = false; // rewrite;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The route name
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// The location name, if any
        /// 
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// 
        /// Do we remove the route?
        /// 
        /// </summary>
        public bool Rewrite { get; private set; }
        #endregion

        #region Methods
        public void Apply(ItemClass item)
        {
            // Set the name
            this.Name = item.Key;

            // Set the location
            this.Location = item.Value.IfEmpty(this.Location).IfEmpty(this.Name);
        }
        #endregion
    }
}