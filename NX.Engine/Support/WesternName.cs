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
using System.Text;
using System.Text.RegularExpressions;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Parses a western hemisphere name
    /// 
    /// </summary>
    public class WesternNameClass : IDisposable
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        public WesternNameClass()
        {
            this.Reset();
        }

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="name">The name</param>
        public WesternNameClass(string name)
        {
            this.Parse(name);
        }
        #endregion

        #region Statics
        public static string CapEachWord(string value)
        {
            string sAns = null;

            if (value != null)
            {
                sAns = string.Empty;
                string sWkg = value; //.ToLower();

                while (sWkg.IndexOf("  ") != -1) sWkg = sWkg.Replace("  ", " ");

                if (sWkg.Length > 0)
                {
                    sAns = sWkg.SmartCaps().Trim();
                }
            }

            return sAns;
        }

        public static string CapFirstWord(string value)
        {
            string sAns = null;

            if (value != null)
            {
                sAns = string.Empty;
                string sWkg = value; //.ToLower();

                while (sWkg.IndexOf("  ") != -1) sWkg = sWkg.Replace("  ", " ");

                if (sWkg.Length > 0)
                {
                    sAns = sWkg.SmartCaps(false).Trim();
                }
            }

            return sAns;
        }
        #endregion

        /// <summary>
        /// 
        /// the parts of a name
        /// 
        /// </summary>
        #region Properties
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Job { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns the name as a string
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sAns = string.Empty;

            if (this.FirstName.HasValue()) sAns += this.FirstName + " ";
            if (this.MiddleName.HasValue()) sAns += this.MiddleName + " ";
            if (this.LastName.HasValue()) sAns += this.LastName + " ";
            if (this.Suffix.HasValue()) sAns += this.Suffix + " ";
            if (this.Job.HasValue()) sAns += this.Job + " ";

            return sAns.Trim();
        }

        /// <summary>
        /// 
        /// Returns name in a sortable format
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToSortableString()
        {
            string sAns = string.Empty;

            if (this.LastName.HasValue()) sAns += this.LastName .Trim();
            sAns += ", ";

            if (this.FirstName.HasValue()) sAns += this.FirstName.Trim() + " ";
            if (this.MiddleName.HasValue()) sAns += this.MiddleName.Trim() + " ";
            if (this.Suffix.HasValue()) sAns += this.Suffix.Trim() + " ";
            if (this.Job.HasValue()) sAns += this.Job.Trim();

            return sAns.ToUpper();
        }

        /// <summary>
        /// 
        /// Resets the name parts to empty
        /// 
        /// </summary>
        public void Reset()
        {
            this.FirstName = "";
            this.MiddleName = "";
            this.LastName = "";
            this.Suffix = "";
        }

        /// <summary>
        /// 
        /// Joins any number of words into a single
        /// Regular Expression string
        /// 
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private string MakeWordRE(params string[] words)
        {
            return "[" + string.Join("|", words) + "]";
        }

        /// <summary>
        /// 
        /// Parses a name
        /// 
        /// HISTORY:  This is a very old class )over 10 years) 
        /// so I will have to come back at some point in time to 
        /// revire and document
        /// 
        /// </summary>
        /// <param name="name">The name to be parsed</param>
        public void Parse(string name)
        {
            // Clear all parts
            this.Reset();

            try
            {
                string sName = name.IfEmpty().ToLower();
                int iPos = sName.IndexOf(",");
                if (iPos != -1) sName = sName.Substring(iPos + 1) + " " + sName.Substring(0, iPos);

                sName = Regex.Replace(sName, @"(\.(?<char>[\w]))", @"${char}", RegexOptions.CultureInvariant);
                sName = " " + Regex.Replace(sName, @"[^\w\x2E\x27\x2D]", " ", RegexOptions.CultureInvariant).Replace(".", " ") + " ";
                while (sName.IndexOf("  ") != -1) sName = sName.Replace("  ", " ");

                string sRE = this.MakeWordRE("Esq", "Phd", "AA", "BA", "BS", "DC", "LVN", "MD", "MS", "RN");
                Match c_RE = Regex.Match(sName, sRE, RegexOptions.IgnoreCase);
                if (c_RE.Success)
                {
                    this.Job = c_RE.Value.Trim();
                    sName = Regex.Replace(sName, sRE, " ", RegexOptions.IgnoreCase);
                }

                List<string> c_Suff = new List<string>() {  };

                sRE = this.MakeWordRE("Jr", "Sr", "III", "IV");
                c_RE = Regex.Match(sName, sRE, RegexOptions.IgnoreCase);
                if (c_RE.Success)
                {
                    this.Suffix = c_RE.Value;
                    sName = Regex.Replace(sName, sRE, " ", RegexOptions.IgnoreCase);
                }

                List<string> c_Words = new List<string>(sName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                this.FirstName = c_Words[0]; c_Words.RemoveAt(0);
                this.LastName = c_Words[c_Words.Count - 1].SmartCaps(); c_Words.RemoveAt(c_Words.Count - 1);
                if (this.FirstName.Length == 1) { this.FirstName += ". " + c_Words[0]; c_Words.RemoveAt(0); }
                this.FirstName = WesternNameClass.CapEachWord(this.FirstName.IfEmpty());

                List<string> c_LN = new List<string>() { "della", "del", "der", "de", "di", "las", "los", "la", "mac", "mc", "san", "van", "von" }; ;
                bool bILastName = true;
                while (bILastName)
                {
                    bILastName = false;

                    string sPoss = c_Words[c_Words.Count - 1]; c_Words.RemoveAt(c_Words.Count - 1);
                    int iIndex = c_LN.IndexOf(sPoss);
                    while (iIndex != -1)
                    {
                        this.LastName = c_LN[iIndex] + " " + this.LastName;
                        sPoss = c_Words[c_Words.Count - 1]; c_Words.RemoveAt(c_Words.Count - 1);
                        iIndex = c_LN.IndexOf(sPoss);
                    }

                    if (c_Words.Count > 0 && sPoss.Length == 1 && "ye".IndexOf(sPoss) != -1)
                    {
                        this.LastName = sPoss + " " + this.LastName;
                        sPoss = c_Words[c_Words.Count - 1].SmartCaps(); c_Words.RemoveAt(c_Words.Count - 1);
                        this.LastName = sPoss + " " + this.LastName;

                        bILastName = true;
                    }
                    else
                    {
                        if (sPoss.HasValue()) c_Words.Add(sPoss);
                    }
                }

                for (int iLoop = 0; iLoop < c_Words.Count; iLoop++)
                {
                    string sWord = c_Words[iLoop];
                    if (sWord.Length == 1)
                    {
                        c_Words[iLoop] = sWord.ToUpper() + ".";
                    }
                    else
                    {
                        int iIndex = c_LN.IndexOf(sWord);
                        if (iIndex != -1)
                        {
                            c_Words[iLoop] = c_LN[iIndex];
                        }
                        else
                        {
                            c_Words[iLoop] = sWord.SmartCaps();
                        }
                    }
                }

                StringBuilder c_X = new StringBuilder();
                foreach (string sWord in c_Words)
                {
                    c_X.Append(sWord).Append(" ");
                }
                this.MiddleName = c_Suff.ToString().Trim();
                this.Suffix = WesternNameClass.CapEachWord(this.Suffix);
            }
            catch
            {
            }
        }
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

        #region Statics
        /// <summary>
        /// 
        /// Shortcut to create a western name object
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static WesternNameClass Make(object value)
        {
            return new WesternNameClass(value.ToStringSafe());
        }
        #endregion
    }
}