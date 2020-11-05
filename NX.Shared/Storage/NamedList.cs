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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// Thread safe dictionary
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NamedListClass<T> 
    {
        #region Constructor
        public NamedListClass()
        {
            //
            this.SynchObject = new Dictionary<string, T>();
        }

        public NamedListClass(JObject values)
            : this()
        {
            // Loop thru
            foreach(string sKey in values.Keys())
            {
                // 
                this[sKey] = values.GetAs<T>(sKey);
            }
        }
        #endregion

        #region Indexer
        public T this[string key] 
        {
            get
            {
                T c_Ans = default(T);

                //
                lock (this)
                {
                    //
                    if (this.SynchObject.ContainsKey(key))
                    {
                        c_Ans = this.SynchObject[key];
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
                    if (this.SynchObject.ContainsKey(key))
                    {
                        this.SynchObject[key] = value;
                    }
                    else
                    {
                        this.SynchObject.Add(key, value);
                    }
                }
            }
        }
        #endregion

        #region Properties
        private Dictionary<string, T> SynchObject { get; set; }

        /// <summary>
        /// 
        /// Returns the keys
        /// 
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                lock (this)
                {
                    return new List<string>(this.SynchObject.Keys);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns the values
        /// 
        /// </summary>
        public ICollection<T> Values
        {
            get
            {
                lock (this)
                {
                    return new List<T>(this.SynchObject.Values);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns the number of entries
        /// 
        /// </summary>
        public int Count {  get { return this.SynchObject.Count; } }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Adds a value to the table
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, T value)
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
            this[item.Key] = item.Value;
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
        public bool ContainsKey(string key)
        {
            bool bAns = false;

            //
            lock (this)
            {
                //
                bAns = this.SynchObject.ContainsKey(key);
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
        public bool Remove(string key)
        {
            bool bAns = false;

            //
            lock (this)
            {
                //
                if (this.SynchObject.ContainsKey(key))
                {
                    bAns = this.SynchObject.Remove(key);
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

        /// <summary>
        /// 
        /// Gets a value
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            lock(this)
            {
                return this.SynchObject.TryGetValue(key, out value);
            };
        }

        /// <summary>
        ///  Converts to JSON object
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject()
        {
            // Assume nume
            JObject c_Ans = new JObject();

            // Loop thru
            foreach(string sKey in this.Keys)
            {
                // Move
                c_Ans.Set(sKey, this[sKey]);
            }

            return c_Ans;
        }
        #endregion
    }
}