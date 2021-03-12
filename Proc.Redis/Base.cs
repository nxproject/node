///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
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

using System.Collections.Generic;
using System;

using NX.Shared;
using StackExchange.Redis;
using System.Net;

namespace Proc.Redis
{
    /// <summary>
    /// 
    /// A wrapper class for all of the Redis objects
    /// 
    /// </summary>
    public class BaseClass : ChildOfClass<ManagerClass>
    {
        #region Constructor
        public BaseClass(ManagerClass mgr, string name = null)
            : base(mgr)
        {
            // Make the full name
            this.Name = name.IfEmpty("default");
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The object name
        /// 
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 
        /// And the name of any signal
        /// 
        /// </summary>
        public string SignalName { get { return this.Name + "_signal"; } }
        #endregion

        /// <summary>
        /// 
        /// Thin wrappers
        /// 
        /// </summary>
        #region Methods
        public string Get(string key)
        {
            return this.Parent.DB.StringGet(key);
        }

        public void Set(string key, string value)
        {
            this.Parent.DB.StringSet(key, value.ToBytes());
        }

        public bool Exists(string key)
        {
            return this.Parent.DB.KeyExists(key);
        }

        public List<string> Keys(string match = "")
        {
            List<string> c_Ans = new List<string>();

            try
            {
                // Each endpoint
                foreach(EndPoint c_EP in this.Parent.Client.GetEndPoints())
                {
                    // Get the keys
                    foreach(var  c_Key in this.Parent.Client.GetServer(c_EP).Keys(pattern: match + "*"))
                    {
                        // Add
                        c_Ans.Add(c_Key.ToString());
                    }
                }
            }
            catch { }

            return c_Ans;
        }

        public void Clear()
        {
            List<string> c_Keys = this.Keys();
            foreach (string sKey in c_Keys)
            {
                this.Del(sKey);
            }
        }

        public void Expire(string key, TimeSpan ttl)
        {
            try
            {
                this.Parent.DB.KeyExpire(key, ttl);
            }
            catch { }
        }

        public long Incr(string key)
        {
            long lAns = 0;

            try
            {
                lAns = this.Parent.DB.StringIncrement(key);
            }
            catch { }

            return lAns;
        }

        public void Del(string key)
        {
            try
            {
                this.Parent.DB.KeyDelete(key);
            }
            catch { }
        }

        public void RPush(string key, string value)
        {
            try
            {
                this.Parent.DB.ListRightPush(key, value);
            }
            catch { }
        }

        public string LPop(string key)
        {
            string sAns = null;

            try
            {
                sAns = this.Parent.DB.ListLeftPop(key).ToString();
            }
            catch { }

            return sAns;
        }

        public byte[] LIndex(string key, int index)
        {
            byte[] abAns = null;

            try
            {
                abAns = this.Parent.DB.ListGetByIndex(key, index);
            }
            catch { }

            return abAns;
        }

        public long LLen(string key)
        {
            long iAns = 0;

            try
            {
                iAns = this.Parent.DB.ListLength(key);
            }
            catch { }

            return iAns;
        }

        public string HGet(string hashid, string key)
        {
            string sAns = null;

            try
            {
                byte[] abBuffer = this.Parent.DB.HashGet(hashid, key.ToBytes());
                if (abBuffer != null) sAns = abBuffer.FromBytes();
            }
            catch { }

            return sAns;
        }

        public void HSet(string hashid, string key, string value)
        {
            try
            {
                this.Parent.DB.HashSet(hashid, key.ToBytes(), value.ToBytes());
            }
            catch { }
        }

        public List<string> HKeys(string hashid)
        {
            List<string> c_Ans = new List<string>();

            try
            {
                RedisValue[] aabKeys = this.Parent.DB.HashKeys(hashid);

                for (int iLoop = 0; iLoop < aabKeys.Length; iLoop++)
                {
                    try
                    {
                        c_Ans.Add(aabKeys[iLoop].ToString());
                    }
                    catch { }
                }
            }
            catch { }

            c_Ans.Sort();

            return c_Ans;
        }

        public void HDel(string hashid, string key)
        {
            try
            {
                this.Parent.DB.HashDelete(hashid, key.ToBytes());
            }
            catch { }
        }
        #endregion
    }
}