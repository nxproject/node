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

using System;
using System.Collections.Generic;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// An in-memory object store.
    /// 
    /// The object must be of type xxx : ChildOfClass<EnvironmentClass>
    /// 
    /// </summary>
    public class ObjectsClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        public ObjectsClass(EnvironmentClass env)
            : base(env)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The actual store
        /// 
        /// </summary>
        private NamedListClass<object> Values { get; set; } = new NamedListClass<object>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Getter
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="cb">Callback if the object does not exist.  Callback should pass back a new object</param>
        /// <returns>The object stored in the Globals space</returns>
        public T Get<T>(string name = null) where T : ChildOfClass<EnvironmentClass>
        {
            // Assume none
            T c_Ans;

            // Name missing?
            if (!name.HasValue())
            {
                // Sse the type
                name = typeof(T).FullName;
            }

            // Already in memory?
            if (!this.Values.ContainsKey(name))
            {
                try
                {
                    // Nope, make one
                    c_Ans = (T)Activator.CreateInstance(typeof(T), this.Parent);
                }
                catch (Exception e)
                {
                    //
                    this.Parent.LogException("Creating {0}".FormatString(typeof(T).FullName), e);
                    c_Ans = default(T);
                }

                // Save
                this.Values.Add(name, c_Ans);
            }
            else
            {
                c_Ans = (T)this.Values[name];
            }

            //
            return c_Ans;
        }

        /// <summary>
        /// 
        /// Setter
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <returns>The object stored in the Globals space</returns>
        public void Set<T>(string name, T obj) where T : ChildOfClass<EnvironmentClass>
        {
            // And save
            this.Values[name] = obj;
        }

        /// <summary>
        /// 
        /// Removes
        /// 
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="cb">Callback if the object does not exist.  Callback should pass back a new object</param>
        /// <returns>The object stored in the Globals space</returns>
        public void Remove<T>(string name = null) where T : ChildOfClass<EnvironmentClass>
        {
            // Name missing?
            if (!name.HasValue())
            {
                // Sse the type
                name = typeof(T).FullName;
            }

            // Already in memory?
            if (this.Values.ContainsKey(name))
            {
                try
                {
                    this.Values.Remove(name);
                }
                catch { }
            }
        }
        #endregion
    }
}