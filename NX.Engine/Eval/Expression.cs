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

using System;
using System.Text;

using NX.Shared;

namespace NX.Engine
{
    public static class Expression
    {
        #region Methods
        public static ExpressionReturn Eval(this EnvironmentClass call, string expr, StoreClass store, Func<string, StoreClass, string> cb = null)
        {
            ExpressionReturn c_Ans = new ExpressionReturn();

            // Handle weird quotes
            expr = expr.Replace("’", "'");
            expr = expr.Replace("`", "'");

            using (Context c_Ctx = new Context(call, store, cb))
            {
                using (Scanner c_Scanner = new Scanner())
                {
                    using (ParseTreeEvaluator c_Tree = new ParseTreeEvaluator(c_Ctx))
                    {
                        using (Parser c_Parser = new Parser(c_Scanner, expr, c_Tree))
                        {
                            if (c_Tree.Errors.Count == 0)
                            {
                                c_Ans.Value = XCVT.ToString(c_Tree.Evaluate(c_Ctx, null));
                            }
                            else
                            {
                                StringBuilder c_Buffer = new StringBuilder();

                                c_Buffer.Append("TBD: {0}".FormatString(c_Scanner.TBD)).Append("; ");

                                foreach(ParseError c_Err in c_Tree.Errors)
                                {
                                    c_Buffer.Append(c_Err.ToString()).Append("; ");
                                }
                                c_Ans.Error = c_Buffer.ToString();
                            }
                        }
                    }
                }
            }

            return c_Ans;
        }
        #endregion
    }

    public class ExpressionReturn
    {
        public string Value { get; set; }
        public string Error { get; set; }
    }
}