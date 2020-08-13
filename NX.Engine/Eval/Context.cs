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

using NX.Shared;

namespace NX.Engine
{
    public class Context : BasedObjectClass
    {
        #region Constructor
        public Context(EnvironmentClass call, StoreClass store, Func<string, StoreClass, string> cb = null)
           : base(call)
        {
            this.Reset();

            if (IFunctions == null) IFunctions = new FunctionsDefinitions();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The active HTTP call
        /// 
        /// </summary>
        public EnvironmentClass Env { get { return this.Root as EnvironmentClass; } }

        /// <summary>
        /// 
        /// Data store for variables
        /// 
        /// </summary>
        public StoreClass Store { get; set; }

        /// <summary>
        /// 
        /// Extra callback
        /// 
        /// </summary>
        public Func<string, StoreClass, string> Callback {get;set;}

        /// <summary>
        /// 
        /// A global storage area
        /// 
        /// </summary>
        public Variables Globals { get; private set; }

        public FunctionsDefinitions Functions
        {
            get
            {
                if (IFunctions == null) IFunctions = new FunctionsDefinitions();

                return IFunctions;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// resets the context to its defaults
        /// </summary> 
        public void Reset()
        {
            Globals = new Variables(this);
        }
        #endregion

        #region Statics
        public static FunctionsDefinitions IFunctions { get;set;}
        #endregion
    }
}