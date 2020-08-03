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
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package DocX -Version 1.7.0
/// 

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.IO;

using Xceed.Words.NET;

using NX.Shared;

namespace Proc.File
{
    /// <summary>
    /// 
    /// A .docx toolset
    /// 
    /// </summary>
    public class DocXClass : IDisposable
    {
        #region Constants
        /// <summary>
        /// 
        /// This is a field regular expression [xxxx]
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
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public void Dispose()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The merge data.  Internal use only!
        /// 
        /// </summary>
        private StoreClass Data { get; set; }

        /// <summary>
        /// 
        /// A list of fields found.  Internal use only!
        /// 
        /// </summary>
        private List<string> Found { get; set; } = new List<string>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Create a merged document
        /// </summary>
        /// <param name="doc">The original .docx document</param>
        /// <param name="values">Data to merge into fields</param>
        /// <returns></returns>
        public byte[] Merge(byte[] doc, StoreClass values)
        {
            // Assume it failed
            byte[] c_Ans = null;

            try
            {
                // Save the data
                this.Data = values;

                // Read the document
                using (MemoryStream c_Stream = new MemoryStream(doc))
                {
                    // The wonderful DocX
                    using (DocX document = DocX.Load(c_Stream))
                    {
                        // Replace
                        document.ReplaceText(PatternData, ReplaceFunc, false, RegexOptions.IgnoreCase, null);
                        // And write out
                        using (MemoryStream c_Out = new MemoryStream())
                        {
                            // Move
                            document.SaveAs(c_Out);
                            // And make into array
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
        /// Returns the field names in a .docx document
        /// 
        /// </summary>
        /// <param name="doc">The .docx document</param>
        /// <returns>List of field information for the merge fields</returns>
        public List<FieldInfoClass> Fields(byte[] doc)
        {
            // Assume none
            List<FieldInfoClass> c_Ans = new List<FieldInfoClass>();

            try
            {
                // Read the document
                using (MemoryStream c_Stream = new MemoryStream(doc))
                {
                    // The wonderful DocX
                    using (DocX document = DocX.Load(c_Stream))
                    {
                        // Empty the list
                        this.Found = new List<string>();
                        // And fill it
                        document.ReplaceText(PatternData, FindFunc, false, RegexOptions.IgnoreCase, null);
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
        /// This is the function used by the Merge code above
        /// 
        /// </summary>
        /// <param name="findStr">The field name found</param>
        /// <returns>The value for the field</returns>
        private string ReplaceFunc(string findStr)
        {
            return this.Data[findStr].IfEmpty("");
        }

        /// <summary>
        /// 
        /// This is the function used by the Fields code above
        /// 
        /// </summary>
        /// <param name="findStr">The field name found</param>
        /// <returns>An empty string</returns>
        private string FindFunc(string findStr)
        {
            string sFld = findStr.IfEmpty("");

            if (this.Found.IndexOf(sFld) == -1) this.Found.Add(sFld);

            return "";
        }
        #endregion
    }
}