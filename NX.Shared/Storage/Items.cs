///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
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
using System.Linq;
using System.Collections.Generic;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// A list of items separated by ;
    /// 
    /// </summary>
    public class ItemsClass : List<ItemClass>, IDisposable
    {
        #region Constructor
        public ItemsClass()
        { }

        public ItemsClass(string value, ItemDefinitionClass def = null)
        {
            //
            this.Definitions = def;
            if (this.Definitions == null) this.Definitions = new ItemDefinitionClass();

            // Assure
            value = value.IfEmpty().RemoveQuotes();

            // Split
            string[] asItems = value.Split(this.Definitions.ItemDelimiter, StringSplitOptions.RemoveEmptyEntries);

            // Loop thru
            foreach (string sItem in asItems)
            {
                // Add
                this.Add(new ItemClass(this.Definitions, sItem));
            }
        }

        public ItemsClass(JArray values, ItemDefinitionClass def = null)
        {
            //
            this.Definitions = def;
            if (this.Definitions == null) this.Definitions = new ItemDefinitionClass();

            // Must have value
            if (values != null)
            {
                // Loop thru
                foreach (string sItem in values.ToList())
                {
                    // Add
                    this.Add(new ItemClass(this.Definitions, sItem));
                }
            }
        }

        public ItemsClass(List<string> values, ItemDefinitionClass def = null)
        {
            //
            this.Definitions = def;
            if (this.Definitions == null) this.Definitions = new ItemDefinitionClass();

            // Must have value
            if (values != null)
            {
                // Loop thru
                foreach (string sItem in values)
                {
                    // Add
                    this.Add(new ItemClass(this.Definitions, sItem));
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
        /// The definitions to be used
        /// 
        /// </summary>
        public ItemDefinitionClass Definitions { get; private set; }
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
        public new int IndexOf(ItemClass item)
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
        public new bool Contains(ItemClass item)
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
                if (!c_Done.Contains(c_Item.Priority))
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
            // Add
            c_Ans.AddRange(this);

            // Loop thru
            foreach (ItemsClass c_Additional in values)
            {
                // Add
                c_Ans.AddRange(c_Additional);
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
                sAns += this.Definitions.ItemDelimiter + c_Item.ToString().RemoveQuotes();
            }

            if (sAns.HasValue())
            {
                sAns = sAns.Substring(1).AddQuotes();
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Pops an item from the stack
        /// 
        /// </summary>
        /// <returns></returns>
        public ItemClass Pop()
        {
            // Assume none
            ItemClass c_Ans = null;

            // Any?
            if (this.Count > 0)
            {
                // Get first
                c_Ans = this.First();
                // remove
                this.RemoveAt(0);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Pushes an item into the stack
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ItemsClass Push(ItemClass item)
        {
            // Append
            this.Add(item);

            return this;
        }

        /// <summary>
        /// 
        /// Pushes an item into the stack
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ItemsClass Push(ItemsClass items)
        {
            // Append
            this.AddRange(items);

            return this;
        }
        #endregion
    }
}