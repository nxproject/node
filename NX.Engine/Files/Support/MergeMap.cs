///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace NX.Engine.Files
{
    /// <summary>
    /// 
    /// A document merge map.
    /// 
    /// </summary>
    public class MergeMapClass : BasedObjectClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="doc">The document where thae map is kept</param>
        internal MergeMapClass(DocumentClass doc)
            : base(doc)
        {
            //
            this.Values = new StoreClass(this.Document.Value);
        }
        #endregion

        #region Enums
        public enum PPDocTypes
        {
            PreDoc,
            PostDoc
        }
        #endregion

        #region Indexer
        /// <summary>
        /// 
        /// The merge fields
        /// 
        /// </summary>
        /// <param name="key">The field name</param>
        /// <returns>The merge expression</returns>
        public string this[string key]
        {
            get { return this.Values[key]; }
            set { this.Values[key] = value; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The map merge document
        /// 
        /// </summary>
        public DocumentClass Document { get { return this.Root as DocumentClass; } }

        /// <summary>
        /// The contents
        /// </summary>
        private StoreClass Values { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns the string representation of the map merge document
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// 
        /// Save the map merge document
        /// 
        /// </summary>
        public void Save()
        {
            this.Document.Value = this.Values.ToString();
        }

        /// <summary>
        /// 
        /// Get an array of pre/post document names
        /// 
        /// </summary>
        /// <param name="type">The type of document array (PreDoc/PostDoc)</param>
        /// <param name="instance">The instance (a number)</param>
        /// <returns>A JSON array of document names</returns>
        public JArray GetDocs(PPDocTypes type, int instance)
        {
            return this.Values.GetAsJArray(type.ToString() + instance);
        }

        /// <summary>
        /// 
        /// Evaluate the map against a set of values
        /// 
        /// </summary>
        /// <param name="data">The store that holds the values</param>
        /// <returns></returns>
        public StoreClass Eval(StoreClass data, EnvironmentClass env)
        {
            StoreClass c_Ans = new StoreClass();

            // Do each entry
            foreach (string sKey in this.Values.Keys)
            {
                if (sKey.StartsWith(PPDocTypes.PreDoc.ToString()) ||
                    sKey.StartsWith(PPDocTypes.PostDoc.ToString()))
                {
                    // Skip these codes
                }
                else
                {
                    // Simple eval
                    c_Ans[sKey] = env.Eval(this.Values[sKey], data).Value;
                }
            }

            return c_Ans;
        }

        /// <summary>
        ///  
        /// Updates the map with the current field codes
        /// 
        /// </summary>
        /// <param name="doc">The source document</param>
        public void MakeFields(DocumentClass doc)
        {
            // The list of fields
            List<string> c_Fields = new List<string>();

            // And temp item
            List<FieldInfoClass> c_Wkg = null;

            // According to the type
            switch (doc.Extension)
            {
                case "docx":
                    using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                    {
                        // And merge
                        c_Wkg = c_Filler.Fields(doc.ValueAsBytes);
                    }
                    break;

                case "pdf":
                case "fdf":
                    using (Vendors.PDFClass c_Filler = new Vendors.PDFClass())
                    {
                        c_Wkg = c_Filler.Fields(doc.ValueAsBytes);
                    }
                    break;
            }

            // Make field list
            if (c_Wkg != null)
            {
                // Loop thru
                foreach (FieldInfoClass c_Field in c_Wkg)
                {
                    c_Fields.Add(c_Field.Name);
                }
            }

            // Loop thru known
            foreach (string sKey in this.Values.Keys)
            {
                // Is it a special one?
                if (sKey.StartsWith(PPDocTypes.PreDoc.ToString()) ||
                    sKey.StartsWith(PPDocTypes.PostDoc.ToString()))
                {
                    // Skip these codes
                }
                else
                {
                    // Is it in the new list?
                    if (c_Fields.Contains(sKey))
                    {
                        // Remove it from new list
                        c_Fields.Remove(sKey);
                    }
                    else
                    {
                        // Remove it from map
                        this.Values.Remove(sKey);
                    }
                }
            }

            // What is left in the fields list is new!
            foreach (string sKey in c_Fields)
            {
                // Add
                this.Values.Add(sKey, "");
            }

            // And store
            this.Document.Value = this.Values.ToString();
        }
        #endregion
    }
}