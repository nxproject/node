///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2024 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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

using System;
using System.Text.RegularExpressions;

using NX.Shared;

namespace NX.Engine
{
    public static class XCVT
    {
        #region Methods
        public static string ToString(object value)
        {
            string ans = "";

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        ans = (bool)value ? "1" : "0";
                    }
                    else if (value is DateTime)
                    {
                        ans = ((DateTime)value).ToDBDate();
                    }
                    else
                    {
                        ans = value.ToString();
                    }
                }
            }
            catch { }

            return ans;
        }

        public static string Numeric(object value)
        {
            return Regex.Replace(XCVT.ToString(value), @"[^0-9\x2D\x2E]", "");
        }

        public static int ToInt32(object value)
        {
            int ans = 0;

            try
            {
                ans = Convert.ToInt32(XCVT.Numeric(value));
            }
            catch { }

            return ans;
        }

        public static long ToInt64(object value)
        {
            long ans = 0;

            try
            {
                ans = Convert.ToInt64(XCVT.Numeric(value));
            }
            catch { }

            return ans;
        }

        public static long ToInt64(object value, int usebase)
        {
            long ans = 0;

            try
            {
                ans = Convert.ToInt64(XCVT.Numeric(value), usebase);
            }
            catch { }

            return ans;
        }

        public static double ToDouble(object value)
        {
            double ans = 0;

            try
            {
                ans = Convert.ToDouble(XCVT.Numeric(value));
            }
            catch { }

            return ans;
        }

        public static bool ToBoolean(object value)
        {
            bool ans = false;

            string sWkg = XCVT.ToString(value).ToLower();
            if (sWkg.IndexOf("t") != -1 || sWkg.IndexOf("y") != -1)
            {
                ans = true;
            }
            else if (sWkg.IndexOf("f") != -1 || sWkg.IndexOf("n") != -1)
            {
                ans = false;
            }
            else
            {
                ans = XCVT.ToDouble(value) != 0;
            }

            return ans;
        }

        public static DateTime ToDateTime(object value)
        {
            DateTime ans = DateTime.MinValue;

            if (value is DateTime)
            {
                ans = (DateTime)value;
            }
            else
            {
                ans = XCVT.ToString(value).FromDBDate();
            }

            return ans;
        }
        #endregion

        #region Testing
        public static bool IsString(object value)
        {
            return value is String;
        }

        public static bool IsInt32(object value)
        {
            return value is Int32;
        }

        public static bool IsInt64(object value)
        {
            return value is Int64;
        }

        public static bool IsDouble(object value)
        {
            return value is Double;
        }

        public static bool IsBoolean(object value)
        {
            return value is Boolean;
        }

        public static bool IsDateTime(object value)
        {
            return value is DateTime;
        }
        #endregion
    }
}