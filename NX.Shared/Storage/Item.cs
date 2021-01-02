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
using System.Linq;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// A Key/Value pair with options in the
    /// format of:  key[=value][:opt1[:opt2]...]
    /// 
    /// </summary>
    public class ItemClass : ChildOfClass<ItemDefinitionClass>
    {
        #region Constructor
        public ItemClass(string value)
            : this(new ItemDefinitionClass(), value)
        { }

        public ItemClass(ItemDefinitionClass defs)
            : base(defs)
        { }

        public ItemClass(ItemDefinitionClass defs, string value)
            : this(defs)
        {
            // Assure
            value = value.IfEmpty().RemoveQuotes();

            // Modifiers?
            value = this.ParseOptions(value);

            // Value?
            int iPos = value.IndexOf(this.Parent.KeyValueDelimiter);
            if (iPos != -1)
            {
                // Split
                this.Value = value.Substring(iPos + 1);
                // Remove
                value = value.Substring(0, iPos);
            }
            else
            {
                this.Value = "";
            }

            // Set
            this.Key = value;

            // Empty value?
            if (!this.Value.HasValue() && this.ValueIsPriority)
            {
                this.Value = this.Key;
                this.Key = "";
            }
        }

        /// <summary>
        /// 
        /// Parses for modifiers
        /// 
        /// </summary>
        /// <param name="value">The value to be parsed</param>
        /// <returns>The value without modifiers</returns>
        private string ParseOptions(string value)
        {
            // Until no more
            while (true)
            {
                // Assume none
                int iIndex = -1;
                string sDelim = null;

                // Loop
                foreach (string sPoss in this.Parent.OptionDelimiters)
                {
                    // Do we find?
                    int iPoss = value.LastIndexOf(sPoss);
                    // After the one already found?
                    if (iPoss != -1 && iPoss > iIndex)
                    {
                        // Save
                        iIndex = iPoss;
                        sDelim = sPoss;
                    }
                }

                // Found one?
                if (iIndex > -1)
                {
                    // Add it
                    this.AddOption(value.Substring(iIndex + sDelim.Length), sDelim);
                    // Remove
                    value = value.Substring(0, iIndex);
                }
                else
                {
                    break;
                }

            }

            return value;
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
        public List<ItemOptionClass> Options { get; set; } = new List<ItemOptionClass>();

        /// <summary>
        /// 
        /// A shortcut for the times where only one option is allowed
        /// 
        /// </summary>
        public string Option
        {
            get
            {
                // Assume none
                string sAns = null;

                // Any options defined?
                if (this.Options.Count > 0)
                {
                    // Get first
                    sAns = this.Options.First().Value;
                }

                return sAns;
            }
        }

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
                if (this.ValueIsPriority)
                {
                    this.Value = value;
                }
                else
                {
                    this.Key = value;
                }
            }
        }

        private bool ValueIsPriority
        {
            get
            {
                // Assume not
                bool bAns = false;

                // Do we have a parent?
                if(this.Parent != null)
                {
                    bAns = this.Parent.ValueIsPriority;
                }

                return bAns;
            }
        }

        /// <summary>
        /// 
        /// User defined fields
        /// 
        /// </summary>
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Converts item to string
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Start with the key
            string sAns = this.Key.IfEmpty();
            // Value?
            if (this.Value.HasValue()) sAns += this.Parent.KeyValueDelimiter + this.Value;
            // Options?
            foreach (ItemOptionClass c_Option in this.Options)
            {
                // Add
                sAns += c_Option.ToString();
            }

            return sAns.AddQuotes();
        }

        /// <summary>
        /// 
        /// Adds an optional value
        /// 
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="option">The option</param>
        public void AddOption(string value, string option = null)
        {
            // Do we have a value?
            if (value.HasValue())
            {
                // Assure option
                option = option.IfEmpty(this.Parent.OptionDelimiters.First());
                // Create
                ItemOptionClass c_Option = new ItemOptionClass(option, value);
                // None?
                if (this.Options.Count == 0)
                {
                    // Add
                    this.Options.Add(c_Option);
                }
                else
                {
                    // Insert
                    this.Options.Insert(0, c_Option);
                }
            }
        }
        #endregion
    }
}