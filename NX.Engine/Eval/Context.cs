///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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
using System.Collections.Generic;

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

            // The callback
            this.Callback = cb;

            // Save the passed store
            if (store != null)
            {
                this.Stores.Use("passed");
                this.Stores[this.Stores.Default] = store;
            }
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
        public virtual ContextStoreClass<StoreClass> Stores { get; set; } = new ContextStoreClass<StoreClass>();

       /// <summary>
        /// 
        /// The documents
        /// 
        /// </summary>
        public virtual ContextStoreClass<Files.DocumentClass> Documents { get; set; } = new ContextStoreClass<Files.DocumentClass>();

        /// <summary>
        /// 
        /// Variables
        /// 
        /// </summary>
        public virtual ContextStoreClass<string> Vars { get; set; } = new ContextStoreClass<string>();

        /// <summary>
        /// 
        /// To be used by callback
        /// 
        /// </summary>
        public ContextStoreClass<object> Locals = new ContextStoreClass<object>();

        /// <summary>
        /// 
        /// List of things
        /// 
        /// </summary>
        public virtual ContextStoreClass<NamedListClass<string>> Lists { get; set; } = new ContextStoreClass<NamedListClass<string>>();

        /// <summary>
        /// 
        /// A global storage area
        /// 
        /// </summary>
        public virtual Variables Globals { get; private set; }

        /// <summary>
        /// 
        /// The functions
        /// 
        /// </summary>
        public FunctionsDefinitions Functions {  get { return FunctionsTable; } }
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
        private static FunctionsDefinitions IFunctionsTable { get; set; }
        public static FunctionsDefinitions FunctionsTable 
        { 
            get
            {
                if(IFunctionsTable == null)
                {
                    IFunctionsTable = new FunctionsDefinitions();
                }
                return IFunctionsTable;
            }
        }
        #endregion
    }
}