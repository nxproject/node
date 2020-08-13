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
    /// <summary>
    /// 
    /// The context where the expression is evaluated
    /// 
    /// </summary>
    public class Context : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public Context(EnvironmentClass call, StoreClass store, Func<ExprCBParams, string> cb = null)
           : base(call)
        {
            // Start anew
            this.Reset();

            // And load the functions
            if (IFunctions == null) IFunctions = new FunctionsDefinitions();

            // The callback
            this.Callback = cb;

            // Save the passed store
            if (store != null) this.Stores["passed"] = store;
        }
        #endregion

        #region Properties
        
        /// <summary>
        /// 
        /// Extra callback
        /// 
        /// </summary>
        public Func<ExprCBParams, string> Callback { get; set; }

        /// <summary>
        /// 
        /// The stores
        /// 
        /// </summary>
        public NamedListClass<StoreClass> Stores = new NamedListClass<StoreClass>();

        /// <summary>
        /// 
        /// The store to use as default
        /// 
        /// </summary>
        public string UseStore { get; set; }

        /// <summary>
        /// 
        /// The documents
        /// 
        /// </summary>
        public NamedListClass<Files.DocumentClass> Documents = new NamedListClass<Files.DocumentClass>();

        /// <summary>
        /// 
        /// The document to use as default
        /// 
        /// </summary>
        public string UseDocument { get; set; }

        /// <summary>
        /// 
        /// Variables
        /// 
        /// </summary>
        public NamedListClass<string> Vars = new NamedListClass<string>();

        /// <summary>
        /// 
        /// To be used by callback
        /// 
        /// </summary>
        public NamedListClass<object> Locals = new NamedListClass<object>();

        /// <summary>
        /// 
        /// The local to use as default
        /// 
        /// </summary>
        public string UseLocal { get; set; }

        /// <summary>
        /// 
        /// A global storage area
        /// 
        /// </summary>
        public Variables Globals { get; private set; }

        /// <summary>
        /// 
        /// The functions
        /// 
        /// </summary>
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
        public static FunctionsDefinitions IFunctions { get; set; }
        #endregion
    }
}