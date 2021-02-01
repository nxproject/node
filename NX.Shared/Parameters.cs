﻿///--------------------------------------------------------------------------------
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

using NX.Shared;

namespace NX.Shared
{
    public class ParamDefinitionClass
    {
        #region Consructor
        public ParamDefinitionClass(Types type, string desc, string elsatype = "expression", List<string> choices = null)
        {
            //
            this.Type = type;
            this.Description = desc;
            this.ElsaType = elsatype;
            this.Choices = choices;
        }
        #endregion

        #region Enum
        public enum Types
        {
            Required,
            Optional
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The field type (Required/Optional)
        /// 
        /// </summary>
        public Types Type { get; private set; }

        /// <summary>
        /// 
        /// The description
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// The type if Elsa
        /// 
        /// </summary>
        public string ElsaType { get; set; }

        /// <summary>
        /// 
        /// Choices if type is select
        /// </summary>
        public List<string> Choices { get; private set; }
        #endregion
    }
}