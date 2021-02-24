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
///--------------------------------------------------------------------------------

using System;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// This class keeps a weak or strong reference to a parent object
    /// 
    /// </summary>
    public class BasedObjectClass : IDisposable
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="root">The object to keep rerence of</param>
        /// <param name="keepref">If true, keep strong reference</param>
        public BasedObjectClass(object root, bool keepstrong = false)
        {
            // As strong?
            if (keepstrong)
            {
                // Keep
                this.IRef = root;
            }
            else if (root != null)
            {
                // Otherwise keep a weak reference
                this.IRootRef = new WeakReference(root);
            }
        }
        #endregion

        #region IDisposable
        public virtual void Dispose()
        {
            if (this.IRef != null) this.IRef = null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// A strong reference to the object
        /// 
        /// </summary>
        private object IRef { get; set; }

        /// <summary>
        /// 
        /// A weak reference to the object
        /// 
        /// </summary>
        private WeakReference IRootRef { get; set; }

        /// <summary>
        /// 
        /// Returns the referenced object
        /// 
        /// </summary>
        public object Root
        {
            get
            {
                // Assume strong
                object c_Ans = this.IRef;
                // Is there one?
                if (c_Ans == null)
                {
                    // No, is there a weak reference?
                    if (this.IRootRef != null)
                    {
                        // It it still good?
                        if (this.IRootRef.IsAlive)
                        {
                            // Return
                            c_Ans = this.IRootRef.Target;
                        }
                    }
                }

                return c_Ans;
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// Shortcut class for classes that have an T class as the parent
    /// 
    /// </summary>
    public class ChildOfClass<T> : BasedObjectClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public ChildOfClass(T env)
            : base(env)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The current environment
        /// 
        /// </summary>
        public T Parent { get { return (T)this.Root; } }

        /// <summary>
        /// 
        /// Storage for other items
        /// 
        /// </summary>
        public object Storage { get; set; }
        #endregion
    }
}