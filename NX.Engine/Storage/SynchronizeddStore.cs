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

using Newtonsoft.Json.Linq;

using NX.Shared;
using NX.Engine.BumbleBees.Redis;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Extension to a StoreClass that allows for the synchronization
    /// between the store and the underlying file system.
    /// 
    /// NB:  Do not use . separated names, as they WILL NOT be synchronized
    /// 
    /// </summary>
    public class SynchronizedStoreClass : StoreClass
    {
        #region Constants
        /// <summary>
        /// 
        /// This key test to see if the Redis store exists.
        /// Do not use at any time!
        /// 
        /// </summary>
        private const string KeyExists = "___IEXIST";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor (plain)
        /// 
        /// </summary>
        public SynchronizedStoreClass(EnvironmentClass env, string synch)
            : base()
        {
            this.SynchID = "_store" + synch;
            if (env != null) this.SetupSynch(env);
        }

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="values">The values as a JObject</param>
        public SynchronizedStoreClass(EnvironmentClass env, string synch, JObject values)
            : base(values)
        {
            this.SynchID = synch;
            if (env != null) this.SetupSynch(env);
        }

        /// <summary>
        /// 
        /// Constructor (basic)
        /// 
        /// </summary>
        /// <param name="value">The value to use as the stating contents</param>
        public SynchronizedStoreClass(EnvironmentClass env, string synch, string value)
            : base(value)
        {
            this.SynchID = synch;
            if (env != null) this.SetupSynch(env);
        }

        public void SetupSynch(EnvironmentClass env)
        {
            // If not in make mode
            if (!env.InMakeMode)
            {
                // Setup the synch availability
                env.Messenger.AvailabilityChanged += delegate (bool isavailable)
                {
                    // Did it become available?
                    if (isavailable)
                    {
                        //
                        env.LogVerbose("Synch {0} is available", this.SynchID);

                        // Did we do this dance before?
                        if (!this.HasSynch)
                        {
                            // Only once
                            this.HasSynch = true;

                            // Get a key that we will never have or are we running outside?
                            if (!this.RedisGet(KeyExists).HasValue() || !"".InContainer())
                            {
                                // Save it
                                this.RedisSet(KeyExists, DateTime.Now.ToString());

                                //
                                env.LogVerbose("Synch {0} timestamped ", this, this.RedisGet(KeyExists));

                                //
                                env.LogInfo("Synchronizing store to {0}", this.SynchID);

                                // Write what we have
                                foreach (string sKey in this.SynchObject.Keys())
                                {
                                    // Get value
                                    object c_Value = this.GetAsObject(sKey);

                                    // Set
                                    this.Set(sKey, c_Value, SetOptions.SendOnly);
                                }
                            }
                            else
                            {
                                //
                                env.LogInfo("Synchronizing store from {0}", this.SynchID);

                                // Read what we have
                                foreach (string sKey in this.SynchObject.Keys())
                                {
                                    // Does it exist?
                                    if (this.RedisExists(sKey))
                                    {
                                        // Get value
                                        object c_Value = this.RedisGet(sKey).JDeserialize();
                                        // Set
                                        this.Set(sKey, c_Value, SetOptions.Skip);
                                    }
                                    else
                                    {
                                        // Get value
                                        object c_Value = this.GetAsObject(sKey);

                                        // Set
                                        this.Set(sKey, c_Value, SetOptions.SendOnly);
                                    }
                                }

                                //
                                env.LogVerbose("Store {0}: {1}", this.SynchID, this.SynchObject.ToSimpleString());
                            }
                        }
                    }
                    else
                    {
                        //
                        env.LogVerbose("Synch {0} is not available", this.SynchID);
                    }
                };

                // And handle messages
                env.Messenger.MessageReceived += delegate (MessengerClass.MessageClass msg)
                {
                    // Ours?
                    if (msg.MClass.IsSameValue(this.SynchID))
                    {
                        // Decode
                        object c_Value = msg["value"].JDeserialize();
                        // Any?
                        if (c_Value != null)
                        {
                            // Do the event
                            this.ChangedCalled?.Invoke(msg["key"], c_Value);
                        }
                    }
                };

                env.Messenger.CheckAvailability();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The synch
        /// 
        /// </summary>
        private MessengerClass Synch { get; set; }

        /// <summary>
        /// 
        /// The synch ID
        /// 
        /// </summary>
        private string SynchID { get; set; }

        /// <summary>
        /// 
        /// True if we already synched
        /// 
        /// </summary>
        private bool HasSynch { get; set; }

        /// <summary>
        /// 
        /// This callback is called before any value is stored.
        /// 
        /// </summary>
        public Func<string, object, SetOptions> CallbackBeforeSet { get; set; }
        #endregion

        #region Access
        /// <summary>
        /// 
        /// Returns the value of the key as a string
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The value as a string</returns>
        public override string GetAsString(string key)
        {
            // Assure
            this.Assure(key);

            return base.GetAsString(key);
        }

        /// <summary>
        /// 
        /// Gets a .NET object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The get to get</param>
        /// <returns>The object or the default object if none</returns>
        public override T GetAs<T>(string key)
        {
            // Assure
            this.Assure(key);

            return base.GetAs<T>(key);
        }

        /// <summary>
        /// 
        /// Gets a JSON object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The JSON object or empty if none</returns>
        public override JArray GetAsJArray(string key)
        {
            // Assure
            this.Assure(key);

            return base.GetAsJArray(key);
        }

        /// <summary>
        /// 
        /// Gets a JSON object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The JSON object or empty if none</returns>
        public override JObject GetAsJObject(string key)
        {
            // Assure
            this.Assure(key);

            return base.GetAsJObject(key);
        }

        /// <summary>
        /// 
        /// Gets a .NET object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The object or the default object if none</returns>
        public override object GetAsObject(string key)
        {
            // Assure
            this.Assure(key);

            return base.GetAsObject(key);
        }

        /// <summary>
        /// 
        /// Sets an entry to any type of object
        /// 
        /// </summary>
        /// <param name="key">The key to save into</param>
        /// <param name="value">The value to store</param>
        public override void Set(string key, object value, SetOptions opts = SetOptions.OK)
        {
            // Default
            SetOptions eOption = opts;
            // Any callback?
            if (this.CallbackBeforeSet != null && eOption == SetOptions.OK)
            {
                // Call
                eOption = this.CallbackBeforeSet(key, value);
            }

            // OK?
            if (eOption != SetOptions.Discard)
            {
                // Call the internal
                base.Set(key, value, opts);

                // Do we have a queue and can we save?
                if ((eOption == SetOptions.OK || eOption == SetOptions.SendOnly) && this.Synch != null && this.Synch.IsAvailable)
                {
                    // Make into string
                    string sSer = value.JSerialize();

                    // Valid?
                    if (sSer.HasValue())
                    {
                        // Save it
                        this.RedisSet(key, sSer);

                        // Send it
                        this.Synch.SendMessage(this.SynchID, "key", key, "value", sSer);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// Assures that a value is fetched from the synch
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        private void Assure(string key)
        {
            // Do we have it?
            if (!this.SynchObject.Contains(key) && this.Synch != null && this.Synch.IsAvailable)
            {
                // Fetch
                string sSer = this.RedisGet(key);
                // Any?
                if (sSer.HasValue())
                {
                    // Save
                    base.Set(key, sSer.Deserialize());
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// Defines the event to be raised when OnChanged is triggered
        /// 
        /// </summary>
        public delegate void OnChangedHandler(string key, object value);
        public event OnChangedHandler ChangedCalled;
        #endregion

        #region Redis Accesor
        /// <summary>
        /// 
        /// The name of the data portion of the synch
        /// 
        /// </summary>
        private string RedisName(string key)
        {
            return this.SynchID + "_data" + "_" + key;
        }

        /// <summary>
        /// 
        /// Sets a key/value pair in Redis
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void RedisSet(string key, string value)
        {
            // Available?
            if (this.Synch != null && this.Synch.Redis != null)
            {
                // Open a channel
                using (BaseClass c_DC = new BaseClass(this.Synch.Redis))
                {
                    // Set
                    c_DC.Set(this.RedisName(key), value);
                }
            }
        }

        /// <summary>
        /// 
        /// Gets a key/value pair from Redis
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public string RedisGet(string key)
        {
            // Assume none
            string sAns = null;

            // Available?
            if (this.Synch != null && this.Synch.Redis != null)
            {
                // Open a channel
                using (BaseClass c_DC = new BaseClass(this.Synch.Redis))
                {
                    // Set
                    sAns = c_DC.Get(this.RedisName(key));
                }
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Returns true if the key exists in Redis
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RedisExists(string key)
        {
            // Assume not
            bool bAns = false;

            // Available?
            if (this.Synch != null && this.Synch.Redis != null)
            {
                // Open a channel
                using (BaseClass c_DC = new BaseClass(this.Synch.Redis))
                {
                    // 
                    bAns = c_DC.Exists(this.RedisName(key));
                }
            }

            return bAns;
        }
        #endregion
    }
}