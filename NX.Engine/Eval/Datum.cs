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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// The datum parser [xxx]
    /// 
    /// </summary>
    public class DatumClass : ChildOfClass<Context>
    {
        #region Constants
        private const char DelimPrefix = ':';
        #endregion

        #region Constructor
        public DatumClass(Context env, string field)
            : base(env)
        {
            // Assume no prefix
            this.Field = field;
            this.Type = Types.Value;
            this.Prefix = "";

            //
            if (this.Field.HasValue())
            {
                if (this.Field.StartsWith(@"[*") && this.Field.EndsWith("]"))
                {
                    this.Type = Types.Store;
                    this.Field = this.Field.Substring(2, this.Field.Length - 3);
                    this.ParsePrefix(this.Parent.Stores.Default);
                }
                else if (this.Field.StartsWith(@"[$") && this.Field.EndsWith("]"))
                {
                    this.Type = Types.Document;
                    this.Field = this.Field.Substring(2, this.Field.Length - 3);
                    this.ParsePrefix(this.Parent.Documents.Default);
                }
                else if (this.Field.StartsWith(@"[") && this.Field.EndsWith("]"))
                {
                    this.Type = Types.Data;
                    this.Field = this.Field.Substring(1, this.Field.Length - 2);
                    this.ParsePrefix("");
                }
                else if (this.Field.StartsWith(@"#") && this.Field.EndsWith("#"))
                {
                    this.Type = Types.Expression;
                    this.Field = this.Field.Substring(1, this.Field.Length - 2);
                }
                else if ((this.Field.StartsWith("\"") && this.Field.EndsWith("\"")) || (this.Field.StartsWith("'") && this.Field.EndsWith("'")))
                {
                    this.Field = this.Field.Substring(1, this.Field.Length - 2);
                }
            }
        }
        #endregion

        #region  Enums
        public enum Types
        {
            Value,
            Data,
            Store,
            Expression,
            Document
        }
        #endregion

        #region Indexer
        public string Value
        {
            get
            {
                string sAns = "";

                switch (this.Type)
                {
                    case Types.Value:
                        sAns = this.Parent.Vars[this.Field];
                        break;

                    case Types.Data:
                        sAns = this.DataGet();
                        break;

                    case Types.Store:
                        sAns = this.StoreGet();
                        break;

                    case Types.Document:
                        sAns = this.DocumentGet();
                        break;

                    case Types.Expression:
                        sAns = this.ExprGet();
                        break;
                }

                return sAns;
            }
            set
            {
                switch (this.Type)
                {
                    case Types.Value:
                        this.Parent.Vars[this.Field] = value;
                        break;

                    case Types.Data:
                        this.DataSet(value);
                        break;

                    case Types.Store:
                        this.StoreSet(value);
                        break;

                    case Types.Document:
                        this.DocumentSet(value);
                        break;

                    case Types.Expression:
                        string sIndirect = this.Value;
                        if (sIndirect.HasValue())
                        {
                            using (DatumClass c_Datum = new DatumClass(this.Parent, sIndirect))
                            {
                                c_Datum.Value = value;
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The field name :xxx or xxx
        /// 
        /// </summary>
        public string Field { get; internal set; }

        /// <summary>
        /// 
        /// The prefix xxx:
        /// 
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// 
        /// The type of datum
        /// 
        /// </summary>
        public Types Type { get; internal set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Splits a prefix:field value
        /// 
        /// </summary>
        /// <param name="defaultvalue"></param>
        private void ParsePrefix(string defaultvalue)
        {
            // Find the delimiter
            int iPos = this.Field.IndexOf(DelimPrefix);
            // Any?
            if (iPos != -1)
            {
                // Split
                this.Prefix = this.Field.Substring(0, iPos);
                this.Field = this.Field.Substring(iPos + 1);
            }

            // Use default
            this.Prefix = this.Prefix.IfEmpty(defaultvalue);
        }

        /// <summary>
        /// 
        /// Dumps contents
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DATUM: Type: {0}, Prefix: {1}, Field: {2}, Value: {3}".FormatString(this.Type   ,
                this.Prefix,
                this.Field,
                this.Value);
        }
        #endregion

        #region Store
        /// <summary>
        /// 
        /// Gets from a store
        /// 
        /// </summary>
        /// <returns>The value</returns>
        private string StoreGet()
        {
            // Assume none
            string sAns = null;

            // Do we have a store name?
            if (this.Prefix.HasValue())
            {
                // Get the store
                StoreClass c_At = this.Parent.Stores[this.Prefix];
                // Any?
                if(c_At != null)
                {
                    // Get
                    sAns = c_At[this.Field];
                }
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Sets to a store
        /// 
        /// </summary>
        /// <param name="value">The value</param>
        private void StoreSet(string value)
        {
            // Do we have a store name?
            if (this.Prefix.HasValue())
            {
                // Get the store
                StoreClass c_At = this.Parent.Stores[this.Prefix];
                // Any?
                if (c_At != null)
                {
                    // Set
                    c_At[this.Field] = value;
                }
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// 
        /// Gets from an object
        /// 
        /// </summary>
        /// <returns></returns>
        private string DataGet()
        {
            // Assume none
            string sAns = null;

            // Do via callback
            if(this.Parent.Callback != null)
            {
                // Make the params
                using (ExprCBParams c_Params = new ExprCBParams(this.Parent, this.Prefix, this.Field, null, ExprCBParams.Modes.Get))
                {
                    sAns = this.Parent.Callback(c_Params);
                }
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Sets to an object
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void DataSet(string value)
        {
            // Do via callback
            if (this.Parent.Callback != null)
            {
                // Make the params
                using (ExprCBParams c_Params = new ExprCBParams(this.Parent, this.Prefix, this.Field, value, ExprCBParams.Modes.Set))
                {
                    var sAns = this.Parent.Callback(c_Params);
                }
            }
        }
        #endregion

        #region Documents
        /// <summary>
        /// 
        /// Gets from a document
        /// 
        /// </summary>
        /// <returns></returns>
        private string DocumentGet()
        {
            // Assume none
            string sAns = null;

            // The prefix is the default
            this.Field = this.Field.IfEmpty(this.Prefix);

            // Do we have a document name?
            if (this.Field.HasValue())
            {
                // Get the store
                Files.DocumentClass c_At = this.Parent.Documents[this.Field];
                // Any?
                if (c_At != null)
                {
                    // Get
                    sAns = c_At.Value;
                }
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Sets a a document
        /// </summary>
        /// <param name="value"></param>
        private void DocumentSet(string value)
        {
            // The prefix is the default
            this.Field = this.Field.IfEmpty(this.Prefix);

            // Do we have a store name?
            if (this.Field.HasValue())
            {
                // Get the store
                Files.DocumentClass c_At = this.Parent.Documents[this.Field];
                // Any?
                if (c_At != null)
                {
                    // Set
                    c_At.Value = value;
                }
            }
        }
        #endregion

        #region Expressions
        /// <summary>
        /// 
        /// Computes an expression
        /// 
        /// </summary>
        /// <returns>The computed value</returns>
        private string ExprGet()
        {
            string sAns = "";

            try
            {
                var c_Ret = Expression.Evaluate(this.Parent, this.Field);

                if (!DatumClass.HideErrors && c_Ret.Error.HasValue())
                {
                    this.Parent.Parent.LogVerbose("ERROR IN EVAL: {0}".FormatString(c_Ret.Error));
                }

                sAns = c_Ret.Value.IfEmpty();
            }
            catch (Exception e)
            {
                this.Parent.Parent.LogException("EVAL: {0}".FormatString(this.Field), e);
            }

            return sAns;
        }
        #endregion

        #region Statics
        public static bool HideErrors { get; set; }
        #endregion
    }
}