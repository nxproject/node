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
/// Install-Package docXn -Version 1.7.0
/// 

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Xceed.Words.NET;

using NX.Engine;
using NX.Shared;

namespace Proc.File.Vendors
{
    /// <summary>
    /// 
    /// A toolkit to merge .docx files.
    /// 
    /// Fields in the .docx file are in the format [xxxx]
    /// 
    /// </summary>
    public class DocXClass : IDisposable
    {
        #region Constants
        /// <summary>
        /// 
        /// The regular expression pattern to match a field
        /// 
        /// </summary>
        private const string PatternData = @"\x5B(.+?)\x5D";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        public DocXClass()
        { }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The active document
        /// 
        /// </summary>
        //private byte[] Document { get; set; }

        /// <summary>
        /// 
        /// The active key/value pairs to merge
        /// 
        /// </summary>
        private StoreClass Data { get; set; }
        
        /// <summary>
        /// 
        /// List of fields found during a fields search
        /// 
        /// </summary>
        private List<string> Found { get; set; } = new List<string>();
        #endregion

        #region Merge
        /// <summary>
        /// 
        /// Merges a store into a .docx template
        /// 
        /// </summary>
        /// <param name="doc">The byte array of the document contents</param>
        /// <param name="values">The store</param>
        /// <returns>A byte array of the merged document</returns>
        public byte[] Merge(byte[] doc, StoreClass values)
        {
            // Assume it did not go well
            byte[] c_Ans = null;

            // Save the data
            this.Data = values;

            try
            {
                // Make into an accessible stream
                using (MemoryStream c_Stream = new MemoryStream(doc))
                {
                    // Load it into stream
                    using (DocX document = DocX.Load(c_Stream))
                    {
                        // Replace
                        document.ReplaceText(PatternData, MergeFunc, false, RegexOptions.IgnoreCase, null);
                        // And write out
                        using (MemoryStream c_Out = new MemoryStream())
                        {
                            document.SaveAs(c_Out);
                            c_Ans = c_Out.ToArray();
                        }
                    }
                }
            }
            catch { }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the merge data value for a given field
        /// 
        /// </summary>
        /// <param name="field">The field name</param>
        /// <returns></returns>
        private string MergeFunc(string field)
        {
            return this.Data[field].IfEmpty("");
        }
        #endregion

        #region Fields
        /// <summary>
        /// 
        /// Returns a list of fields in the document
        /// 
        /// </summary>
        /// <param name="doc">The byte array of the document contents</param>
        /// <returns>The list of fields</returns>
        public List<FieldInfoClass> Fields(byte[] doc)
        {
            // Assume no fields
            List<FieldInfoClass> c_Ans = new List<FieldInfoClass>();

            try
            {
                // Make into an accessible stream
                using (MemoryStream c_Stream = new MemoryStream(doc))
                {
                    using (DocX document = DocX.Load(c_Stream))
                    {
                        // Empty the list
                        this.Found = new List<string>();
                        // And fill it
                        document.ReplaceText(PatternData, FieldsFunc, false, RegexOptions.IgnoreCase, null);
                        // Make 
                        foreach (string sPatt in this.Found)
                        {
                            // Each one into a field info
                            c_Ans.Add(new FieldInfoClass(sPatt));
                        }
                    }
                }
            }
            catch { }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Builds the list of found fields
        /// 
        /// </summary>
        /// <param name="field"The field found></param>
        /// <returns>An empty string (for compatibility purposes)</returns>
        private string FieldsFunc(string field)
        {
            string sFld = field.IfEmpty("");

            if (this.Found.IndexOf(sFld) == -1) this.Found.Add(sFld);

            return "";
        }
        #endregion

        #region Tools
        /// <summary>
        /// 
        /// Appends to arrays of byte into one
        /// 
        /// </summary>
        /// <param name="doc1">The first array</param>
        /// <param name="doc2">The second array</param>
        /// <param name="atend">If true, doc2 will be appended to the end of doc1, therwise doc1 will be appended at the end of doc2</param>
        /// <returns></returns>
        public byte[] Append(byte[] doc1, byte[] doc2, bool atend)
        {
            byte[] c_Ans = null;

            if (doc1 == null)
            {
                c_Ans = doc2;
            }
            else if (doc2 == null)
            {
                c_Ans = doc1;
            }
            else
            {
                using (MemoryStream c_Stream1 = new MemoryStream(doc1))
                {
                    using (DocX document1 = DocX.Load(c_Stream1))
                    {
                        using (MemoryStream c_Stream2 = new MemoryStream(doc2))
                        {
                            using (DocX document2 = DocX.Load(c_Stream2))
                            {
                                document1.InsertDocument(document2, atend);

                                using (MemoryStream c_Out = new MemoryStream())
                                {
                                    document1.SaveAs(c_Out);
                                    c_Ans = c_Out.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            return c_Ans;
        }
        #endregion
    }
}