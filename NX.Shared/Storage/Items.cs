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

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text.Json.Serialization;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// A list of items separated by ;
    /// 
    /// </summary>
    public class ItemsClass : List<ItemClass>, IDisposable
    {
        #region Constants
        private const char ItemDelimiter = ';';
        #endregion

        #region Constructor
        public ItemsClass()
        { }

        public ItemsClass(string value, char delim = '\0', bool valuepriority = false)
        {
            //
            this.ValueIsPriority = valuepriority;

            // Assure
            value = value.IfEmpty().RemoveQuotes();

            if (delim == '\0') delim = ItemDelimiter;
            this.Delimiter = delim;

            // Split
            string[] asItems = value.Split(this.Delimiter, StringSplitOptions.RemoveEmptyEntries);

            // Loop thru
            foreach (string sItem in asItems)
            {
                // Add
                this.Add(new ItemClass(sItem, valuepriority));
            }
        }

        public ItemsClass(JArray values, bool valuepriority = false)
        {
            //
            this.ValueIsPriority = valuepriority;

            // Must have value
            if (values != null)
            {
                // Loop thru
                foreach (string sItem in values.ToList())
                {
                    // Add
                    this.Add(new ItemClass(sItem, this.ValueIsPriority));
                }
            }
        }

        public ItemsClass(List<string> values, bool valuepriority = false)
        {
            //
            this.ValueIsPriority = valuepriority;

            // Must have value
            if (values != null)
            {
                // Loop thru
                foreach (string sItem in values)
                {
                    // Add
                    this.Add(new ItemClass(sItem, this.ValueIsPriority));
                }
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Househeeping
        /// 
        /// </summary>
        public void Dispose()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Delimiter between items
        /// 
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// 
        /// Are values given priority?
        /// 
        /// </summary>
        internal bool ValueIsPriority { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Finds the position of the item
        /// 
        /// </summary>
        /// <param name="match">The text to match</param>
        /// <returns>The index or -1 if none</returns>
        public int IndexOf(string match)
        {
            // Assume none 
            int iAns = -1;
            // Loop thru
            for (int i = 0; i < this.Count; i++)
            {
                // Compare
                if (match.IsSameValue(this[i].Priority))
                {
                    // Save
                    iAns = i;
                    // Only one
                    break;
                }
            }

            return iAns;
        }

        /// <summary>
        /// 
        /// Finds the position of the item
        /// 
        /// </summary>
        /// <param name="match">The item to match</param>
        /// <returns>The index or -1 if none</returns>
        public int IndexOf(ItemClass item)
        {
            return this.IndexOf(item.Priority);
        }

        /// <summary>
        /// 
        /// Is the match anywhere?
        /// 
        /// </summary>
        /// <param name="match">The text to match</param>
        /// <returns>True if found</returns>
        public bool Contains(string match)
        {
            return this.IndexOf(match) != -1;
        }

        /// <summary>
        /// 
        /// Is the item anywhere?
        /// 
        /// </summary>
        /// <param name="match">The item to match</param>
        /// <returns>True if found</returns>
        public bool Contains(ItemClass item)
        {
            return this.IndexOf(item.Priority) != -1;
        }

        /// <summary>
        /// 
        /// Returns items in both lists
        /// 
        /// </summary>
        /// <param name="values">The matching list</param>
        /// <returns>A list of items in both lists</returns>
        public ItemsClass In(ItemsClass values)
        {
            // Assume none
            ItemsClass c_Ans = new ItemsClass();
            // Loop thru
            foreach (ItemClass c_Item in this)
            { 
                // Do we have it?
                if(values.Contains(c_Item))
                {
                    // Add
                    c_Ans.Add(c_Item);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns items not found in second
        /// 
        /// </summary>
        /// <param name="values">The matching list</param>
        /// <returns>The list of items not in the matching list</returns>
        public ItemsClass NotIn(ItemsClass values)
        {
            // Assume none
            ItemsClass c_Ans = new ItemsClass();
            // Loop thru
            foreach (ItemClass c_Item in this)
            {
                // Do we have it?
                if (values.Contains(c_Item))
                {
                    // Add
                    c_Ans.Add(c_Item);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns unique items
        /// 
        /// </summary>
        /// <param name="values">The matching list</param>
        /// <returns>The list of unique items</returns>
        public ItemsClass Unique()
        {
            // Assume none
            ItemsClass c_Ans = new ItemsClass();

            // Holds already done
            List<string> c_Done = new List<string>();

            // Loop thru
            foreach (ItemClass c_Item in this)
            {
                // Already seen it?
                if(!c_Done.Contains(c_Item.Priority))
                {
                    // Add
                    c_Ans.Add(c_Item);
                    // And to done
                    c_Done.Add(c_Item.Priority);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns items from both sets
        /// 
        /// </summary>
        /// <param name="values">The second list</param>
        /// <returns>The list of unique from both sets</returns>
        public ItemsClass Merge(params ItemsClass[] values)
        {
            // Assume none
            ItemsClass c_Ans = new ItemsClass();
            // Loop thru
            foreach (ItemClass c_Item in this)
            {
                // Add
                c_Ans.Add(c_Item);
            }

            // Loop thru
            foreach (ItemsClass c_Additional in values)
            {
                // Loop thru
                foreach (ItemClass c_Item in c_Additional)
            {
                    // Add
                    c_Ans.Add(c_Item);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Converts to string, quoting if needed
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Assume none
            string sAns = "";

            foreach (ItemClass c_Item in this)
            {
                sAns += this.Delimiter + c_Item.ToString().RemoveQuotes();
            }

            if (sAns.HasValue())
            {
                sAns = sAns.Substring(1).AddQuotes();
            }

            return sAns;
        }
        #endregion
    }
}