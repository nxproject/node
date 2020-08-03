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
using System.Collections.Generic;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// A Key/Value pair with options in the
    /// format of:  key[=value][:opt1[:opt2]...]
    /// 
    /// </summary>
    public class ItemClass : IDisposable
    {
        #region Constants
        private const string KVDelimiter = "=";
        private const string ModifierDelimiter = ":";
        #endregion

        #region Constructor
        public ItemClass()
        { }

        public ItemClass(string value, bool valuepriority = false)
        {
            //
            this.ValueIsPriority = valuepriority;

            // Assure
            value = value.IfEmpty().RemoveQuotes();

            // Options?
            int iPos = value.IndexOf(ModifierDelimiter);
            if (iPos != -1)
            {
                // Split
                this.Modifiers = new List<string>(value.Substring(iPos + 1).Split(ModifierDelimiter, StringSplitOptions.RemoveEmptyEntries));
                // Remove
                value = value.Substring(0, iPos);
            }

            // Value?
            iPos = value.IndexOf(KVDelimiter);
            if (iPos != -1)
            {
                // Split
                this.Value = value.Substring(iPos + 1);
                // Remove
                value = value.Substring(0, iPos);
            }

            this.Key = value;

            // Empty value?
            if(!this.Value.HasValue() && this.ValueIsPriority)
            {
                this.Value = this.Key;
                this.Key = "";
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
        /// The key or value before =
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// The value or value after =
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// The values after : and delimited by :
        /// 
        /// </summary>
        public List<string> Modifiers { get; set; }

        /// <summary>
        /// 
        /// Are values given priority?
        /// 
        /// </summary>
        internal bool ValueIsPriority { get; set; }

        /// <summary>
        /// 
        /// The priority value
        /// 
        /// </summary>
        public string Priority
        {
            get { return this.ValueIsPriority ? this.Value : this.Key; }
            set
            {
                if(this.ValueIsPriority)
                {
                    this.Value = value;
                }
                else
                {
                    this.Key = value;
                }
            }
        }

        /// <summary>
        /// 
        /// Are any options available?
        /// 
        /// </summary>
        public int ModifierCount
        {
            get
            {
                // Assume none
                int iAns = 0;

                // Any?
                if (this.Modifiers != null)
                {
                    // Get count
                    iAns = this.Modifiers.Count;
                }

                return iAns;
            }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            // Start with the key
            string sAns = this.Key.IfEmpty();
            // Value?
            if (this.Value.HasValue()) sAns += KVDelimiter + this.Value;
            // Options?
            if (this.Modifiers != null && this.Modifiers.Count > 0)
            {
                // Add
                sAns += ModifierDelimiter + this.Modifiers.Join(ModifierDelimiter);
            }

            return sAns.AddQuotes();
        }
        #endregion
    }

    public static class ExtensionsItem
    {
        public static ItemClass AsItem(this string value)
        {
            return new ItemClass(value);
        }
    }
}