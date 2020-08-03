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

namespace NX.Engine.Hive
{
    public class DockerIFFilterClass
    {
        #region Constructor
        public DockerIFFilterClass(params string[] values)
        {
            // Make
            this.Values = new Dictionary<string, IDictionary<string, bool>>();

            // Parse
            for(int i=0;i< values.Length;i+=2)
            {
                // Add a clause
                this.AddClause(values[i], values[i + 1]);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The filters
        /// 
        /// </summary>
        public IDictionary<string, IDictionary<string, bool>> Values { get; private set; }
        #endregion

        #region Methods
        public void AddClause(string field, string value)
        {
            // Make inner dictionary
            IDictionary<string, bool> c_Inner = new Dictionary<string, bool>();
            // Set the value
            c_Inner.Add(value, true);

            // And check the outer
            if(this.Values.ContainsKey(field))
            {
                // Replace
                this.Values[field] = c_Inner;
            }
            else
            {
                // Add
                this.Values.Add(field, c_Inner);
            }
        }
        #endregion
    }
}