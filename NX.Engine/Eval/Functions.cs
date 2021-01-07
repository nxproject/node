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
/// NB: Work derived from "a Tiny Parser Generator v1.2" by Herre Kuijpers
/// found at https://www.codeproject.com/Articles/28294/a-Tiny-Parser-Generator-v1-2
/// under the CPOL license found at https://www.codeproject.com/info/cpol10.aspx
/// 
///--------------------------------------------------------------------------------

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    public class FunctionsDefinitions : NamedListClass<Function>
    {
        #region Constructor
        public FunctionsDefinitions()
        {
            // Constants
            this.AddFn(new StaticFunction("cr", delegate (Context ctx, object[] ps) { return "\r"; }, 0, 0));
            this.AddFn(new StaticFunction("lf", delegate (Context ctx, object[] ps) { return "\n"; }, 0, 0));
            this.AddFn(new StaticFunction("crlf", delegate (Context ctx, object[] ps) { return "\r\n"; }, 0, 0));

            // Eval
            this.AddFn(new StaticFunction("eval", delegate (Context ctx, object[] ps)
            {
                ExpressionReturn c_Ret = ctx.Evaluate(XCVT.ToString(ps[0]));

                return c_Ret.Value.IfEmpty();
            }, 1, 1));

            // HTML
            this.AddFn(new StaticFunction("html", delegate (Context ctx, object[] ps) { return this.HTML(ps[0], XCVT.ToString(ps[1])); }, 2, 2));
            this.AddFn(new StaticFunction("htmlb", delegate (Context ctx, object[] ps) { return this.HTML(ps[0], "b"); }, 1, 1));
            this.AddFn(new StaticFunction("htmli", delegate (Context ctx, object[] ps) { return this.HTML(ps[0], "i"); }, 1, 1));
            this.AddFn(new StaticFunction("htmlu", delegate (Context ctx, object[] ps) { return this.HTML(ps[0], "u"); }, 1, 1));
            this.AddFn(new StaticFunction("htmlp", delegate (Context ctx, object[] ps) { return this.HTML(ps[0], "p"); }, 1, 1));

            // high precision functions
            this.AddFn(new StaticFunction("abs", delegate (Context ctx, object[] ps) { return Math.Abs(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("acos", delegate (Context ctx, object[] ps) { return Math.Acos(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("asin", delegate (Context ctx, object[] ps) { return Math.Asin(XCVT.ToDouble(ps[0])); }, 1, 1));

            this.AddFn(new StaticFunction("atan", delegate (Context ctx, object[] ps) { return Math.Atan(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("atan2", delegate (Context ctx, object[] ps) { return Math.Atan2(XCVT.ToDouble(ps[0]), XCVT.ToDouble(ps[1])); }, 2, 2));
            this.AddFn(new StaticFunction("ceiling", delegate (Context ctx, object[] ps) { return Math.Ceiling(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("cos", delegate (Context ctx, object[] ps) { return Math.Cos(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("cosh", delegate (Context ctx, object[] ps) { return Math.Cosh(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("exp", delegate (Context ctx, object[] ps) { return Math.Exp(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("int", delegate (Context ctx, object[] ps) { return (int)Math.Floor(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("fact", delegate (Context ctx, object[] ps)
            {
                double total = 1;

                for (int i = XCVT.ToInt32(ps[0]); i > 1; i--)
                    total *= i;

                return total;
            }, 1, 1)); // factorials 1*2*3*4...
            this.AddFn(new StaticFunction("floor", delegate (Context ctx, object[] ps) { return Math.Floor(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("log", delegate (Context ctx, object[] ps)
            {
                if (ps.Length == 1)
                    return Math.Log10(XCVT.ToDouble(ps[0]));

                if (ps.Length == 2)
                    return Math.Log(XCVT.ToDouble(ps[0]), XCVT.ToDouble(ps[1]));

                return null;
            }, 1, 2)); // log allows 1 or 2 parameters
            this.AddFn(new StaticFunction("ln", delegate (Context ctx, object[] ps) { return Math.Log(XCVT.ToDouble(ps[0])); }, 1, 1));

            this.AddFn(new StaticFunction("pow", delegate (Context ctx, object[] ps) { return Math.Pow(XCVT.ToDouble(ps[0]), XCVT.ToDouble(ps[1])); }, 2, 2));
            this.AddFn(new StaticFunction("round", delegate (Context ctx, object[] ps)
            {
                double dAns = 0;
                if (ps.Length == 2)
                {
                    dAns = Math.Round(XCVT.ToDouble(ps[0]), XCVT.ToInt32(ps[0]));
                }
                else
                {
                    dAns = Math.Round(XCVT.ToDouble(ps[0]));
                }
                return dAns;
            }, 1, 2));
            this.AddFn(new StaticFunction("sign", delegate (Context ctx, object[] ps) { return Math.Sign(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("sin", delegate (Context ctx, object[] ps) { return Math.Sin(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("sinh", delegate (Context ctx, object[] ps) { return Math.Sinh(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("sqr", delegate (Context ctx, object[] ps) { return XCVT.ToDouble(ps[0]) * XCVT.ToDouble(ps[0]); }, 1, 1));
            this.AddFn(new StaticFunction("sqrt", delegate (Context ctx, object[] ps) { return Math.Sqrt(XCVT.ToDouble(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("trunc", delegate (Context ctx, object[] ps) { return Math.Truncate(XCVT.ToDouble(ps[0])); }, 1, 1));

            // array functions
            this.AddFn(new StaticFunction("avg", this.Avg, 1, int.MaxValue));
            this.AddFn(new StaticFunction("stdev", delegate (Context ctx, object[] ps)
            {
                double var = XCVT.ToDouble(this.Var(ctx, ps));
                return Math.Sqrt(var);
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("var", this.Var, 1, int.MaxValue));
            this.AddFn(new StaticFunction("max", delegate (Context ctx, object[] ps)
            {
                double max = double.MinValue;

                foreach (object o in ps)
                {
                    double val = XCVT.ToDouble(o);
                    if (val > max)
                        max = val;
                }
                return max;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("median", delegate (Context ctx, object[] ps)
            {
                object[] ordered = ps.OrderBy(o => XCVT.ToDouble(o)).ToArray();

                if (ordered.Length % 2 == 1)
                    return ordered[ordered.Length / 2];
                else
                    return (XCVT.ToDouble(ordered[ordered.Length / 2]) + XCVT.ToDouble(ordered[ordered.Length / 2 - 1])) / 2;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("min", delegate (Context ctx, object[] ps)
            {
                double min = double.MaxValue;

                foreach (object o in ps)
                {
                    double val = XCVT.ToDouble(o);
                    if (val < min)
                        min = val;
                }
                return min;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("sum", delegate (Context ctx, object[] ps)
            {
                double min = 0;

                foreach (object o in ps)
                {
                    double val = XCVT.ToDouble(o);
                    min += val;
                }
                return min;
            }, 1, int.MaxValue));

            this.AddFn(new StaticFunction("datemax", delegate (Context ctx, object[] ps)
            {
                DateTime max = DateTime.MinValue;

                foreach (object o in ps)
                {
                    if (XCVT.ToString(o).HasValue())
                    {
                        DateTime val = XCVT.ToDateTime(o);
                        if (val.Ticks > max.Ticks)
                            max = val;
                    }
                }
                return max;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("datemin", delegate (Context ctx, object[] ps)
            {
                DateTime min = DateTime.MaxValue;

                foreach (object o in ps)
                {
                    if (XCVT.ToString(o).HasValue())
                    {
                        DateTime val = XCVT.ToDateTime(o);
                        if (val.Ticks < min.Ticks)
                            min = val;
                    }
                }
                return min;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("dateonly", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToDateTime(ps[0]).Date;
            }, 1, 1));
            this.AddFn(new StaticFunction("datesame", delegate (Context ctx, object[] ps)
{
    DateTime c_Date0 = XCVT.ToDateTime(ps[0]);
    DateTime c_Date1 = XCVT.ToDateTime(ps[1]);

    return c_Date0.Ticks == c_Date1.Ticks;
}, 2, 2));
            this.AddFn(new StaticFunction("datebefore", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date0 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date1 = XCVT.ToDateTime(ps[1]);

                return c_Date0.Ticks < c_Date1.Ticks;
            }, 2, 2));
            this.AddFn(new StaticFunction("dateafter", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date0 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date1 = XCVT.ToDateTime(ps[1]);

                return c_Date0.Ticks > c_Date1.Ticks;
            }, 2, 2));
            this.AddFn(new StaticFunction("dateonorbefore", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date0 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date1 = XCVT.ToDateTime(ps[1]);

                return c_Date0.Ticks <= c_Date1.Ticks;
            }, 2, 2));
            this.AddFn(new StaticFunction("dateonorafter", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date0 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date1 = XCVT.ToDateTime(ps[1]);

                return c_Date0.Ticks >= c_Date1.Ticks;
            }, 2, 2));
            this.AddFn(new StaticFunction("todate", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToDateTime(ps[0]).ToDBDate();
            }, 1, 1));
            this.AddFn(new StaticFunction("concat", delegate (Context ctx, object[] ps)
            {
                string ans = "";

                foreach (object o in ps)
                {
                    ans += XCVT.ToString(o);
                }
                return ans;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("concatdelim", delegate (Context ctx, object[] ps)
            {
                string ans = "";

                // The first one is the delimiter
                string sDelim = XCVT.ToString(ps[0]);

                for (int iLoop = 1; iLoop < ps.Length; iLoop++)
                {
                    string sValue = XCVT.ToString(ps[iLoop]);

                    if (sValue.HasValue())
                    {
                        if (ans.HasValue())
                        {
                            ans += sDelim;
                        }

                        ans += sValue;
                    }
                }
                return ans;
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("concatif", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                string sPart1 = XCVT.ToString(ps[0]);
                string sJoin = XCVT.ToString(ps[1]);
                string sPart2 = XCVT.ToString(ps[2]);

                if (!sPart1.HasValue())
                {
                    sAns = sPart2;
                }
                else if (!sPart2.HasValue())
                {
                    sAns = sPart1;
                }
                else
                {
                    sAns = sPart1 + sJoin + sPart2;
                }

                return sAns;
            }, 3, 3));
            this.AddFn(new StaticFunction("choice", this.Choice, 1, int.MaxValue));
            this.AddFn(new StaticFunction("case", this.Choice, 1, int.MaxValue));
            this.AddFn(new StaticFunction("ifte", delegate (Context ctx, object[] ps)
            {
                string ans = "";

                // The first if the if
                bool ifte = XCVT.ToBoolean(ps[0]);
                // If true, use second, otherwise third (if any)
                if (ifte)
                {
                    ans = XCVT.ToString(ps[1]);
                }
                else if (ps.Length == 3)
                {
                    ans = XCVT.ToString(ps[2]);
                }

                return ans;
            }, 2, 3));
            this.AddFn(new StaticFunction("concattext", delegate (Context ctx, object[] ps)
             {
                 string ans = "";

                 // The first one is the normal delimiter
                 string sDelim1 = XCVT.ToString(ps[0]);
                 // The second one is the last delimiter
                 string sDelim2 = XCVT.ToString(ps[1]);

                 for (int iLoop = 2; iLoop < ps.Length; iLoop++)
                 {
                     string sValue = XCVT.ToString(ps[iLoop]);
                     if (sValue.HasValue())
                     {
                         // Use normal delim
                         string sDelim = sDelim1;

                         // Are we in last?
                         if (iLoop == (ps.Length - 1))
                         {
                             // Use last
                             sDelim = sDelim2;
                         }

                         // Do we need a delim?
                         if (ans.HasValue())
                         {
                             ans += sDelim;
                         }

                         ans += sValue;
                     }
                 }
                 return ans;
             }, 2, int.MaxValue));
            this.AddFn(new StaticFunction("indexof", delegate (Context ctx, object[] ps)
            {
                string sValue = XCVT.ToString(ps[0]);
                string sMatch = XCVT.ToString(ps[1]);
                return sValue.IndexOf(sMatch);
            }, 2, 2));
            this.AddFn(new StaticFunction("lastindexof", delegate (Context ctx, object[] ps)
            {
                string sValue = XCVT.ToString(ps[0]);
                string sMatch = XCVT.ToString(ps[1]);
                return sValue.LastIndexOf(sMatch);
            }, 2, 2));

            //boolean functions
            this.AddFn(new StaticFunction("not", delegate (Context ctx, object[] ps) { return !XCVT.ToBoolean(ps[0]); }, 1, 1));
            this.AddFn(new StaticFunction("if", delegate (Context ctx, object[] ps)
            {
                object c_Ans = null;

                int iCount = ps.Length;
                bool bHasDefault = (iCount % 2) == 1;
                if (bHasDefault) iCount--;

                for (int iLoop = 0; iLoop < iCount; iLoop += 2)
                {
                    if (XCVT.ToBoolean(ps[iLoop]))
                    {
                        c_Ans = ps[iLoop + 1];
                        break;
                    }
                }

                if (c_Ans == null)
                {
                    if (bHasDefault)
                    {
                        c_Ans = ps[ps.Length - 1];
                    }
                    else
                    {
                        c_Ans = 0;
                    }
                }

                return c_Ans; // XCVT.ToBoolean(ps[0]) ? ps[1] : ps[2];
            }, 2, int.MaxValue));
            this.AddFn(new StaticFunction("and", delegate (Context ctx, object[] ps)
            {
                bool bAns = true;
                int iPos = ps.Length;
                while (bAns && iPos > 0)
                {
                    iPos--;
                    if (!XCVT.ToBoolean(ps[iPos]))
                    {
                        bAns = false;
                    }
                }
                return bAns;
            }, 2, int.MaxValue));
            this.AddFn(new StaticFunction("or", delegate (Context ctx, object[] ps)
            {
                bool bAns = false;
                int iPos = ps.Length;
                while (!bAns && iPos > 0)
                {
                    iPos--;
                    if (XCVT.ToBoolean(ps[iPos]))
                    {
                        bAns = true;
                    }
                }
                return bAns;
            }, 2, int.MaxValue));

            // string functions
            this.AddFn(new StaticFunction("trim", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).Trim(); }, 1, 1));
            this.AddFn(new StaticFunction("rtrim", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).TrimEnd(); }, 1, 1));
            this.AddFn(new StaticFunction("ltrim", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).TrimStart(); }, 1, 1));
            this.AddFn(new StaticFunction("yesno", delegate (Context ctx, object[] ps) { return XCVT.ToBoolean(ps[0]) ? "Yes" : "No"; }, 1, 1));
            this.AddFn(new StaticFunction("left", delegate (Context ctx, object[] ps)
            {
                string sValue = XCVT.ToString(ps[0]);
                int len = XCVT.ToInt32(ps[1]) < sValue.Length ? XCVT.ToInt32(ps[1]) : sValue.Length;
                return sValue.Substring(0, len);
            }, 2, 2));
            this.AddFn(new StaticFunction("right", delegate (Context ctx, object[] ps)
            {
                string sValue = XCVT.ToString(ps[0]);
                int len = XCVT.ToInt32(ps[1]) < sValue.Length ? XCVT.ToInt32(ps[1]) : sValue.Length;
                return sValue.Substring(sValue.Length - len, len);
            }, 2, 2));
            this.AddFn(new StaticFunction("mid", delegate (Context ctx, object[] ps)
            {
                string sValue = XCVT.ToString(ps[0]);
                int idx = XCVT.ToInt32(ps[1]) < sValue.Length ? XCVT.ToInt32(ps[1]) : sValue.Length;
                int len = XCVT.ToInt32(ps[2]) < sValue.Length - idx ? XCVT.ToInt32(ps[2]) : sValue.Length - idx;
                return sValue.Substring(idx, len);
            }, 3, 3));
            this.AddFn(new StaticFunction("startswith", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).ToLower().StartsWith(XCVT.ToString(ps[1]).ToLower());
            }, 2, 2));
            this.AddFn(new StaticFunction("endswith", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).ToLower().EndsWith(XCVT.ToString(ps[1]).ToLower());
            }, 2, 2));

            this.AddFn(new StaticFunction("hash", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).MD5HashString(); }, 1, 1));
            this.AddFn(new StaticFunction("aspassword", delegate (Context ctx, object[] ps)
            {
                string sAns = XCVT.ToString(ps[0]).IfEmpty();
                if (!sAns.IsMD5Hash()) sAns = sAns.MD5HashString();

                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("hex", delegate (Context ctx, object[] ps) { return String.Format("0x{0:X}", XCVT.ToInt32(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("format", delegate (Context ctx, object[] ps) { return string.Format("{0:" + XCVT.ToString(ps[0]) + "}", ps[1]); }, 2, 2));
            this.AddFn(new StaticFunction("len", delegate (Context ctx, object[] ps) { return XCVT.ToDouble(XCVT.ToString(ps[0]).Length); }, 1, 1));
            this.AddFn(new StaticFunction("lower", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).ToLowerInvariant(); }, 1, 1));
            this.AddFn(new StaticFunction("upper", delegate (Context ctx, object[] ps) { return XCVT.ToString(ps[0]).ToUpperInvariant(); }, 1, 1));
            this.AddFn(new StaticFunction("capword", delegate (Context ctx, object[] ps) { return WesternNameClass.CapEachWord(XCVT.ToString(ps[0])); }, 1, 1));
            this.AddFn(new StaticFunction("word", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                List<string> c_Values = XCVT.ToString(ps[0]).SplitSpaces();
                int iWord = XCVT.ToInt32(ps[1]) - 1;

                if (iWord >= 0 && iWord < c_Values.Count) sAns = c_Values[iWord];

                return sAns;
            }, 2, 2));
            this.AddFn(new StaticFunction("wm", delegate (Context ctx, object[] ps)
            {
                string sWkg = XCVT.ToString(ps[0]);
                int iWMax = XCVT.ToInt32(ps[1]);
                if (sWkg.Length > iWMax) sWkg = sWkg.Substring(0, iWMax);
                return sWkg;
            }, 2, 2));
            this.AddFn(new StaticFunction("wl", delegate (Context ctx, object[] ps)
            {
                string sWkg = XCVT.ToString(ps[0]);
                int iWMax = XCVT.ToInt32(ps[1]);
                if (sWkg.Length < iWMax) sWkg = sWkg.RPad(iWMax, " ");
                return sWkg.Substring(0, iWMax);
            }, 2, 2));
            this.AddFn(new StaticFunction("split", delegate (Context ctx, object[] ps)
            {
                string sWkg = XCVT.ToString(ps[0]);
                string sDel = null;
                if (ps.Length > 2) sDel = XCVT.ToString(ps[2]);
                int iIndex = XCVT.ToInt32(ps[1]);

                List<string> c_Pieces = new List<string>();
                if (sDel.HasValue())
                {
                    c_Pieces = new List<string>(sWkg.Replace(sDel, "\t").Split('\t'));
                }
                else
                {
                    c_Pieces = sWkg.SplitSpaces();
                }

                if (iIndex < 0) iIndex += c_Pieces.Count;
                if (iIndex < 0 || iIndex >= c_Pieces.Count)
                {
                    sWkg = "";
                }
                else
                {
                    sWkg = c_Pieces[iIndex];
                }

                return sWkg;
            }, 2, 3));

            //
            this.AddFn(new StaticFunction("httpget", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).URLGet();
            }, 1, 1));
            this.AddFn(new StaticFunction("httppost", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).URLPost(XCVT.ToString(ps[1]));
            }, 2, 2));
            this.AddFn(new StaticFunction("ifempty", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).IfEmpty(XCVT.ToString(ps[1]));
            }, 2, 2));

            this.AddFn(new StaticFunction("randomstring", delegate (Context ctx, object[] ps)
            {
                int iLen = XCVT.ToInt32(ps[0]);

                string sAns = "".GUID();
                while (sAns.Length < iLen) sAns += "".GUID();

                return sAns.Substring(0, iLen);
            }, 1, 1));
            this.AddFn(new StaticFunction("random", delegate (Context ctx, object[] ps)
            {
                int iLen = XCVT.ToInt32(ps[0]);

                Random c_Rnd = new System.Random();
                return c_Rnd.Next(iLen);
            }, 1, 1));

            // FIles
            this.AddFn(new StaticFunction("docname", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).GetFileNameFromPath();
            }, 1, 1));
            this.AddFn(new StaticFunction("docshortname", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).GetFileNameOnlyFromPath();
            }, 1, 1));
            this.AddFn(new StaticFunction("docextension", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).GetExtensionFromPath();
            }, 1, 1));
            this.AddFn(new StaticFunction("docexists", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).FileExists();
            }, 1, 1));

            // Store

            this.AddFn(new StaticFunction("asamt", delegate (Context ctx, object[] ps) { return XCVT.ToDouble(ps[0]); }, 1, 1));

            // Name functions
            this.AddFn(new StaticFunction("asname", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).ToString(); }, 1, 1));
            this.AddFn(new StaticFunction("namemi", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).MiddleInitial; }, 1, 1));
            this.AddFn(new StaticFunction("namemiddle", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).MiddleName; }, 1, 1));
            this.AddFn(new StaticFunction("namefirst", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).FirstName; }, 1, 1));
            this.AddFn(new StaticFunction("namelast", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).LastName; }, 1, 1));
            this.AddFn(new StaticFunction("namesuffix", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).Suffix; }, 1, 1));
            this.AddFn(new StaticFunction("namejob", delegate (Context ctx, object[] ps) { return WesternNameClass.Make(ps[0]).Job; }, 1, 1));

            // Date functions
            this.AddFn(new StaticFunction("tzlist", delegate (Context ctx, object[] ps)
            {
                return new List<string>("".TimeZones().Keys).Join(", ");
            }, 0, 0));
            this.AddFn(new StaticFunction("today", delegate (Context ctx, object[] ps)
            {
                DateTime c_Ans = "".DTNow();
                if (ps.Length == 0)
                {
                    c_Ans = "".Today();
                }
                else
                {
                    c_Ans = XCVT.ToString(ps[0]).Today();
                }
                return c_Ans.DateOnly().ToDBDate();
            }, 0, 1));
            this.AddFn(new StaticFunction("now", delegate (Context ctx, object[] ps)
            {
                DateTime c_Ans = "".DTNow();
                if (ps.Length == 0)
                {
                    c_Ans = "".Now();
                }
                else
                {
                    c_Ans = XCVT.ToString(ps[0]).Now();
                }
                return c_Ans.ToDBDate();
            }, 0, 1));
            this.AddFn(new StaticFunction("utcnow", delegate (Context ctx, object[] ps)
            {
                DateTime c_Ans = "".DTNow();
                return c_Ans.ToDBDate();
            }, 0, 0));
            this.AddFn(new StaticFunction("timestamp", delegate (Context ctx, object[] ps)
            {
                DateTime c_Ans = "".DTNow();
                if (ps.Length == 0)
                {
                    c_Ans = "".Now();
                }
                else
                {
                    c_Ans = XCVT.ToString(ps[0]).Now();
                }

                string sAns = "";
                if (ps.Length == 2)
                {
                    sAns = sAns = c_Ans.ToString(XCVT.ToString(ps[1]));
                }
                else
                {
                    sAns = c_Ans.ToString(SysConstantClass.DateFormatTS);
                }
                return sAns;
            }, 0, 2));
            this.AddFn(new StaticFunction("dateadjust", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToDateTime(ps[0]).AdjustTimezone(XCVT.ToString(ps[1]));
            }, 2, 2));
            this.AddFn(new StaticFunction("datemonthabbrev", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).FormattedAs("MMM"); }, 1, 1));
            this.AddFn(new StaticFunction("dateformal", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).FormattedAs("MMMM d, yyyy"); }, 1, 1));
            this.AddFn(new StaticFunction("datetimefull", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).FormattedAs("ddd, MMMM d, yyyy hh:mm tt"); }, 1, 1));
            this.AddFn(new StaticFunction("date", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                if (XCVT.ToString(ps[0]).HasValue())
                {
                    DateTime c_Date = XCVT.ToDateTime(ps[0]);
                    sAns = c_Date.FormattedAs(SysConstantClass.USDateFormat);
                }
                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("shortdate", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                if (XCVT.ToString(ps[0]).HasValue())
                {
                    DateTime c_Date = XCVT.ToDateTime(ps[0]);
                    sAns = c_Date.FormattedAs(SysConstantClass.USDateFormatShort);
                }
                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("datedayordinal", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                DateTime c_Date = XCVT.ToDateTime(ps[0]);
                switch (c_Date.Day)
                {
                    case 1:
                        sAns = "1st";
                        break;

                    case 2:
                        sAns = "2nd";
                        break;

                    case 3:
                        sAns = "3rd";
                        break;

                    default:
                        sAns = c_Date.Day + "th";
                        break;
                }
                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("datemonthname", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).FormattedAs("MMMM"); }, 1, 1));
            this.AddFn(new StaticFunction("datedayofweek", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).DayOfWeek.ToString(); }, 1, 1));
            this.AddFn(new StaticFunction("datesortable", delegate (Context ctx, object[] ps)
            {
                DateTime c_Wkg = XCVT.ToDateTime(ps[0]);
                if (ps.Length > 1) c_Wkg = c_Wkg.AdjustTimezone(XCVT.ToString(ps[1]), false);

                return c_Wkg.FormattedAs("yyy-MM-dd HH:mm");
            }, 1, 2));
            this.AddFn(new StaticFunction("dateonlysortable", delegate (Context ctx, object[] ps)
            {
                DateTime c_Wkg = XCVT.ToDateTime(ps[0]);
                if (ps.Length > 1) c_Wkg = c_Wkg.AdjustTimezone(XCVT.ToString(ps[1]), false);

                return c_Wkg.FormattedAs("yyy-MM-dd");
            }, 1, 2));
            this.AddFn(new StaticFunction("dateasfilename", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).FormattedAs("yyyMMddhhmm"); }, 1, 1));
            this.AddFn(new StaticFunction("dateyear", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Year; }, 1, 1));
            this.AddFn(new StaticFunction("datemonth", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Month; }, 1, 1));
            this.AddFn(new StaticFunction("dateday", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Day; }, 1, 1));
            this.AddFn(new StaticFunction("datehour", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Hour; }, 1, 1));
            this.AddFn(new StaticFunction("dateminute", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Minute; }, 1, 1));
            this.AddFn(new StaticFunction("datesecond", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).Second; }, 1, 1));
            this.AddFn(new StaticFunction("datebusiness", delegate (Context ctx, object[] ps) { return XCVT.ToDateTime(ps[0]).ToBusinessDay(); }, 1, 1));
            this.AddFn(new StaticFunction("dateadd", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date = XCVT.ToDateTime(ps[0]);
                int dSize = XCVT.ToInt32(ps[1]);

                switch (XCVT.ToString(ps[2]).ToLower())
                {
                    case "y":
                        c_Date = c_Date.AddYears(dSize);
                        break;

                    case "m":
                        c_Date = c_Date.AddMonths(dSize);
                        break;

                    case "w":
                        c_Date = c_Date.AddDays(dSize * 7);
                        break;

                    case "d":
                        c_Date = c_Date.AddDays(dSize);
                        break;

                    case "h":
                        c_Date = c_Date.AddHours(dSize);
                        break;

                    case "mi":
                        c_Date = c_Date.AddMinutes(dSize);
                        break;
                }
                return c_Date.ToDBDate();
            }, 3, 3));
            this.AddFn(new StaticFunction("datediff", delegate (Context ctx, object[] ps)
            {
                double fDiff = 0;

                DateTime c_Date1 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date2 = XCVT.ToDateTime(ps[1]);

                switch (XCVT.ToString(ps[2]).ToLower())
                {
                    case "d":
                        fDiff = c_Date1.Subtract(c_Date2).TotalDays;
                        break;

                    case "h":
                        fDiff = c_Date1.Subtract(c_Date2).TotalHours;
                        break;

                    case "mi":
                        fDiff = c_Date1.Subtract(c_Date2).TotalMinutes;
                        break;
                }
                return fDiff;
            }, 3, 3));
            this.AddFn(new StaticFunction("time", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                string sValue = XCVT.ToString(ps[0]);
                if (sValue.HasValue())
                {
                    if (Regex.Match(sValue, @"\d{2}:\d{2}\s(AM|PM)").Success)
                    {
                    }
                    else
                    {
                        DateTime c_Date = XCVT.ToDateTime(ps[0]);
                        sAns = c_Date.FormattedAs("HH:mm");
                    }
                }
                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("timeap", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                string sValue = XCVT.ToString(ps[0]);
                if (sValue.HasValue())
                {
                    //if (Regex.Match(sValue, @"\d{2}:\d{2}\s(AM|PM)").Success)
                    //{
                    //    sAns = ExtensionsClass.Parse(sValue).ToString("HH:mm");
                    //}
                    //else
                    //{
                    DateTime c_Date = XCVT.ToDateTime(ps[0]);
                    sAns = c_Date.FormattedAs("hh:mm tt");
                    //}
                }
                return sAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("adjusttimezone", delegate (Context ctx, object[] ps)
            {
                //
                DateTime c_Wkg = XCVT.ToDateTime(ps[0]);
                string sTZ = XCVT.ToString(ps[1]);
                bool bRev = false;
                if (ps.Length == 3) bRev = XCVT.ToBoolean(ps[2]);

                return c_Wkg.AdjustTimezone(sTZ, bRev);
            }, 2, 3));
            this.AddFn(new StaticFunction("istimezonevalid", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).GetTimezone() != null;
            }, 1, 1));
            this.AddFn(new StaticFunction("timezoneoffset", delegate (Context ctx, object[] ps)
            {
                double iAns = 0;

                TimeZoneInfo c_Info = XCVT.ToString(ps[0]).GetTimezone();
                if (c_Info != null) iAns = c_Info.BaseUtcOffset.TotalMinutes;

                return iAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("datecompare", delegate (Context ctx, object[] ps)
            {
                DateTime c_Date1 = XCVT.ToDateTime(ps[0]);
                DateTime c_Date2 = XCVT.ToDateTime(ps[1]);

                return c_Date1.CompareTo(c_Date2);
            }, 2, 2));

            this.AddFn(new StaticFunction("abbrev", delegate (Context ctx, object[] ps)
            {
                string sWkg = XCVT.ToString(ps[0]);
                int iLen = XCVT.ToInt32(ps[1]);

                return sWkg.Abbreviate(iLen);
            }, 2, 2));

            // Multicombo
            this.AddFn(new StaticFunction("mccount", delegate (Context ctx, object[] ps)
            {
                int iAns = 0;

                JArray c_Wkg = XCVT.ToString(ps[0]).ToJArrayOptional();
                iAns = c_Wkg.Count;

                return iAns;
            }, 1, 1));
            this.AddFn(new StaticFunction("mcitem", delegate (Context ctx, object[] ps)
            {
                string sAns = null;

                int iIndex = XCVT.ToInt32(ps[1]);
                string sNames = XCVT.ToString(ps[0]);

                JArray c_Wkg = sNames.ToJArrayOptional();
                if (iIndex >= 0 && iIndex < c_Wkg.Count)
                {
                    sAns = c_Wkg.Get(iIndex);
                }

                return sNames.IfEmpty();
            }, 2, 2));
            this.AddFn(new StaticFunction("mcindexof", delegate (Context ctx, object[] ps)
            {
                int iAns = -1;

                string sLU = XCVT.ToString(ps[1]);

                JArray c_Wkg = XCVT.ToString(ps[0]).ToJArrayOptional();
                for (int iLoop = 0; iLoop < c_Wkg.Count; iLoop++)
                {
                    if (sLU.IsSameValue(c_Wkg.Get(iLoop)))
                    {
                        iAns = iLoop;
                        break;
                    }
                }

                return iAns;
            }, 2, 2));
            this.AddFn(new StaticFunction("mclastindexof", delegate (Context ctx, object[] ps)
            {
                int iAns = -1;

                string sLU = XCVT.ToString(ps[1]);

                JArray c_Wkg = XCVT.ToString(ps[0]).ToJArrayOptional();
                for (int iLoop = 0; iLoop < c_Wkg.Count; iLoop++)
                {
                    if (sLU.IsSameValue(c_Wkg.Get(iLoop)))
                    {
                        iAns = iLoop;
                    }
                }

                return iAns;
            }, 2, 2));
            this.AddFn(new StaticFunction("mchas", delegate (Context ctx, object[] ps)
            {
                bool bAns = false;

                string sLU = XCVT.ToString(ps[1]);

                JArray c_Wkg = XCVT.ToString(ps[0]).ToJArrayOptional();
                for (int iLoop = 0; iLoop < c_Wkg.Count; iLoop++)
                {
                    if (sLU.IsSameValue(c_Wkg.Get(iLoop)))
                    {
                        bAns = true;
                        break;
                    }
                }

                return bAns;
            }, 2, 2));
            this.AddFn(new StaticFunction("mctext", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                JArray c_Wkg = XCVT.ToString(ps[0]).ToJArrayOptional();
                string sDel = ", ";
                if (ps.Length > 1) sDel = XCVT.ToString(ps[1]);
                sAns = c_Wkg.ToList().Join(sDel);

                return sAns.IfEmpty();
            }, 1, 2));

            // List
            this.AddFn(new StaticFunction("first", delegate (Context ctx, object[] ps)
            {
                return Nth(ctx, ps, 1);
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("last", delegate (Context ctx, object[] ps)
            {
                return Nth(ctx, ps, -1);
            }, 1, int.MaxValue));
            this.AddFn(new StaticFunction("nth", delegate (Context ctx, object[] ps)
            {
                int iIndex = XCVT.ToInt32(ps[0]);

                object[] c_Temp = new object[ps.Length - 1];
                Array.Copy(ps, 1, c_Temp, 0, ps.Length - 1);

                return Nth(ctx, c_Temp, iIndex);
            }, 2, int.MaxValue));
            // Parsing
            this.AddFn(new StaticFunction("parsename", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                string sAddr = XCVT.ToString(ps[0]);
                if (sAddr.HasValue())
                {
                    using (WesternNameClass c_Addr = new WesternNameClass(sAddr))
                    {

                        string sFill = "";
                        if (ps.Length > 2)
                        {
                            sFill = XCVT.ToString(ps[2]);
                        }

                        StoreClass c_Wkg = StoreClass.Make(
                                                "firstname", c_Addr.FirstName.IfEmpty(sFill),
                                                "mi", c_Addr.MiddleInitial.IfEmpty(sFill),
                                                "middlename", c_Addr.MiddleName.IfEmpty(sFill),
                                                "lastname", c_Addr.LastName.IfEmpty(sFill),
                                                "suffix", c_Addr.Suffix.IfEmpty(sFill),
                                                "job", c_Addr.Job.IfEmpty(sFill),
                                                "singleline", c_Addr.ToString());

                        if (ps.Length > 1)
                        {
                            ctx.Stores[XCVT.ToString(ps[1])] = c_Wkg;
                        }

                        sAns = c_Addr.ToString();
                    }
                }

                return sAns;
            }, 1, 3));
            this.Add("eamstext", new StaticFunction("eamstext", delegate (Context ctx, object[] ps)
            {
                return XCVT.ToString(ps[0]).RemoveExtraSpaces().Trim().ToUpper();
            }, 1, 1));
            this.Add("eams", new StaticFunction("eams", delegate (Context ctx, object[] ps)
            {
                return Regex.Replace(XCVT.ToString(ps[0]), @"[^\w]", " ").RemoveExtraSpaces().Trim().ToUpper();
            }, 1, 1));
            this.Add("eamsnumber", new StaticFunction("eamsnumber", delegate (Context ctx, object[] ps) { return Regex.Replace(XCVT.ToString(ps[0]), @"[^\d]", "").RemoveExtraSpaces().Trim().ToUpper(); }, 1, 1));
            this.Add("eamsdate", new StaticFunction("eamsdate", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                if (XCVT.ToString(ps[0]).HasValue())
                {
                    DateTime c_Date = XCVT.ToDateTime(ps[0]);
                    sAns = c_Date.FormattedAs("MM/dd/yyyy");
                }
                return sAns;
            }, 1, 1));
            this.Add("eamsamt", new StaticFunction("eamsamt", delegate (Context ctx, object[] ps)
            {
                string sAns = "";

                if (XCVT.ToString(ps[0]).HasValue())
                {
                    sAns = "{0:0.00}".FormatString(XCVT.ToDouble(ps[0]));
                }
                return sAns;
            }, 1, 1));
            this.Add("ssignature", new StaticFunction("ssignature", delegate (Context ctx, object[] ps)
            {
                string sAns = XCVT.ToString(ps[0]).ToUpper();

                if (sAns.HasValue())
                {
                    if (!sAns.StartsWith("S ")) sAns = "S " + sAns;
                }
                return sAns;
            }, 1, 1));

            // Add extensions
            if (FunctionsDefinitions.AddExtensions != null)
            {
                FunctionsDefinitions.AddExtensions(this);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Added by JG
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            JObject c_Ans = new JObject();

            List<string> c_Fns = new List<string>(this.Keys);
            c_Fns.Sort();

            foreach (string sKey in c_Fns)
            {
                Function c_Func = this[sKey];
                c_Ans.Set(c_Func.Formatted, c_Func.Explanation.IfEmpty());
            }

            return c_Ans.ToSimpleString();
        }

        public void AddFn(Function fn)
        {
            this.Add(fn.FN, fn);
        }

        private object Nth(Context ctx, object[] ps, int index)
        {
            string sAns = null;

            if (ps.Length > 0)
            {
                if (index > 0)
                {
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (ps[i] != null)
                        {
                            string sWkg = XCVT.ToString(ps[i]);
                            if (sWkg.HasValue())
                            {
                                index--;
                                if (index == 0)
                                {
                                    sAns = sWkg;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (index < 0)
                {
                    for (int i = ps.Length - 1; i >= 0; i--)
                    {
                        if (ps[i] != null)
                        {
                            string sWkg = XCVT.ToString(ps[i]);
                            if (sWkg.HasValue())
                            {
                                index++;
                                if (index == 0)
                                {
                                    sAns = sWkg;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return sAns;
        }

        private object HTML(object ps, string attr)
        {
            return @"<{0}>".FormatString(attr) +
                        System.Net.WebUtility.HtmlEncode(XCVT.ToString(ps)) +
                        @"</{0}>".FormatString(attr);
        }

        private object Choice(Context ctx, object[] ps)
        {
            string ans = "";

            // The first one is what we are looking for
            string sKey = XCVT.ToString(ps[0]);
            // If even count, the last is the default
            if (0 == (ps.Length % 2)) ans = XCVT.ToString(ps[ps.Length - 1]);

            // We look for each set, skipping last one if odd
            for (int iLoop = 1; iLoop < (ps.Length - 1); iLoop++)
            {
                // Get the value
                string sMatch = XCVT.ToString(ps[iLoop]);
                if (sMatch.IsSameValue(sKey))
                {
                    // Found!
                    ans = XCVT.ToString(ps[iLoop + 1]);
                    break;
                }
            }
            return ans;
        }

        private object Avg(Context ctx, object[] ps)
        {
            double avg = 0;

            if (ps.Length > 1)
            {
                foreach (object o in ps)
                {
                    avg += XCVT.ToDouble(o);
                }

                avg = avg / ps.Length;
            }

            return avg;
        }

        private object Var(Context ctx, object[] ps)
        {
            double total = 0;

            if (ps.Length > 1)
            {
                double avg = XCVT.ToDouble(this.Avg(ctx, ps));

                foreach (object o in ps)
                {
                    total += (XCVT.ToDouble(o) - avg) * (XCVT.ToDouble(o) - avg);
                }

                total = total / (ps.Length - 1);
            }

            return total;
        }
        #endregion

        #region Statics
        public static Action<FunctionsDefinitions> AddExtensions { get; set; }
        #endregion
    }
}