﻿///--------------------------------------------------------------------------------
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
///--------------------------------------------------------------------------------

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Parameters for callback
    /// 
    /// </summary>
    public class ExprCBParams : ChildOfClass<Context>
    {
        #region Constructor
        public ExprCBParams(Context ctx, string prefix, string field, string value, Modes mode)
            : base(ctx)
        {
            //
            this.Prefix = prefix;
            this.Field = field;
            this.Value = value;
            this.Mode = mode;
        }
        #endregion

        #region Enums
        public enum Modes
        {
            Get,
            Set,

            Map
        }
        #endregion

        #region Properties
        public string Prefix { get; internal set; }
        public string Field { get; internal set; }
        public string Value { get; internal set; }
        public JObject Extras { get; internal set; } = new JObject();
        public Modes Mode { get; internal set; }
        #endregion
    }
}