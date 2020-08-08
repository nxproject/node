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
///--------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// Thread safe dictionary
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NamedListClass<T> : Dictionary<string, T>
    {
        #region Constructor
        public NamedListClass()
        { }
        #endregion

        #region Indexer
        public new T this[string key] 
        {
            get
            {
                T c_Ans = default(T);

                //
                lock (this)
                {
                    //
                    if (base.ContainsKey(key))
                    {
                        c_Ans = base[key];
                    }
                }

                return c_Ans;
            }
            set
            {
                //
                lock (this)
                {
                    //
                    if (base.ContainsKey(key))
                    {
                        base[key] = value;
                    }
                    else
                    {
                        base.Add(key, value);
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Returns the keys
        /// 
        /// </summary>
        public new ICollection<string> Keys
        {
            get
            {
                lock (this)
                {
                    return new List<string>(base.Keys);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns the values
        /// 
        /// </summary>
        public new ICollection<T> Values
        {
            get
            {
                lock (this)
                {
                    return new List<T>(base.Values);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Adds a value to the table
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(string key, T value)
        {
            this[key] = value;
        }

        /// <summary>
        /// 
        /// Adds a kv to the table
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, T> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// 
        /// Checks to see if key is in the dictionary
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, T> item)
        {
            return this.ContainsKey(item.Key);
        }

        /// <summary>
        /// 
        /// Checks to see if key is in the dictionary
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new bool ContainsKey(string key)
        {
            bool bAns = false;

            //
            lock (this)
            {
                //
                bAns = base.ContainsKey(key);
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Checks to see if key is in the dictionary
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return this.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// Removes a key from the dictionary
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new bool Remove(string key)
        {
            bool bAns = false;

            //
            lock (this)
            {
                //
                if (base.ContainsKey(key))
                {
                    bAns = base.Remove(key);
                }
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Removes a key from the dictionary
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        public new bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            lock(this)
            {
                return base.TryGetValue(key, out value);
            };
        }
        #endregion
    }
}