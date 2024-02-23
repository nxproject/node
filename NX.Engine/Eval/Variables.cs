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

using NX.Shared;

namespace NX.Engine
{
    public class Variables : ChildOfClass<Context>
    {
        #region Constructor
        public Variables(Context ctx)
            : base(ctx)
        { }
        #endregion

        #region Indexer
        public object this[string name]
        {
            get
            {
                object c_Ans = null;

                switch (name.ToLower())
                {
                    case "e":
                        c_Ans = Math.E;
                        break;

                    case "pi":
                        c_Ans = Math.PI;
                        break;

                    case "nl":
                        c_Ans = "\n";
                        break;

                    case "cr":
                        c_Ans = "\r";
                        break;

                    default:
                        if (Parent.HandlebarData != null)
                        {
                            c_Ans = this.Parent.HandlebarData[name];
                        }
                        else
                        {
                            using (DatumClass c_Datum = new DatumClass(this.Parent, name))
                            {
                                c_Ans = c_Datum.Value;
                            }
                        }
                        break;
                }
                return c_Ans;
            }
            set
            {
                if (Parent.HandlebarData != null)
                {
                    this.Parent.HandlebarData[name] = XCVT.ToString(value);
                }
                else
                {
                    using (DatumClass c_Datum = new DatumClass(this.Parent, name))
                    {
                        c_Datum.Value = value.ToStringSafe();
                    }
                }
            }
        }
        #endregion

        #region Properties
        public Context Context { get { return this.Root as Context; } }
        #endregion
    }
}