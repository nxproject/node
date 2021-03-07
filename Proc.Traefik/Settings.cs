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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package StackExchange.Redis -Version 2.1.58
/// 

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using StackExchange.Redis;

using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// Traefik settings
    /// 
    /// </summary>
    public class SettingsClass : ChildOfClass<InterfaceClass>
    {
        #region Constants
        private const string PrefixAO = "ao/";
        private const string PrefixStore = PrefixAO + "store";
        private const string PrefixCore = PrefixAO + "core";
        private const string PrefixTraefik = "traefik";
        #endregion

        #region Constructor
        public SettingsClass(InterfaceClass itf)
            : base(itf)
        {
            //
            string sEP = this.Parent.Parent.Location.URLMake().RemoveProtocol();

            //
            ConfigurationOptions c_Cfg = ConfigurationOptions.Parse(sEP);
            this.Client = ConnectionMultiplexer.Connect(c_Cfg);

            this.DB = this.Client.GetDatabase();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The client
        /// 
        /// </summary>
        public ConnectionMultiplexer Client { get; private set; }

        /// <summary>
        /// 
        /// The database
        /// 
        /// </summary>
        public IDatabase DB { get; private set; }

        /// <summary>
        /// 
        /// Error handler
        /// 
        /// </summary>
        public Action<string, bool> OnError { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Converts all settings info JSO object
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Assume empty
            JObject c_Ans = new JObject();

            // Assure store
            if (this.IStoreNode == null)
            {
                this.IStoreNode = new NodeClass(this, null, PrefixStore);
            }
            // Dump
            this.DumpNode(this.IStoreNode, c_Ans.AssureJObject("store"));

            // Assure core
            if (this.ICoreNode == null)
            {
                this.ICoreNode = new NodeClass(this, null, PrefixCore);
            }
            // Dump
            this.DumpNode(this.ICoreNode, c_Ans.AssureJObject("core"));

            // Assure Traefik
            if (this.ITraefikNode == null)
            {
                this.ITraefikNode = new NodeClass(this, null, PrefixTraefik);
            }
            // Dump
            this.DumpNode(this.ITraefikNode, c_Ans.AssureJObject("traefik"));

            //
            return c_Ans.ToSimpleString();
        }

        /// <summary>
        /// 
        /// Loads JSON object into settings
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Load(JObject value)
        {
            // Assure store node
            if (this.IStoreNode == null)
            {
                this.IStoreNode = new NodeClass(this, null, PrefixStore);
            }
            // Load it
            this.LoadNode(this.IStoreNode, value.AssureJObject("store"));

            // Assure core node
            if (this.ICoreNode == null)
            {
                this.ICoreNode = new NodeClass(this, null, PrefixCore);
            }
            // Load it
            this.LoadNode(this.ICoreNode, value.AssureJObject("core"));

            // Assure Traefik
            if (this.ITraefikNode == null)
            {
                this.ITraefikNode = new NodeClass(this, null, PrefixTraefik);
            }
            // Load it
            this.LoadNode(this.ITraefikNode, value.AssureJObject("traefik"));
        }

        /// <summary>
        /// 
        /// Dumps a node into JSON object
        /// 
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="store">The JSON object</param>
        private void DumpNode(NodeClass node, JObject store)
        {
            // Get the value
            string sValue = node.Value;
            // Store if any
            if (sValue.HasValue()) store.Set("v", sValue);

            // Get the children
            List<string> c_Child = node.ChildrenKeys;
            // Any?
            if (c_Child != null && c_Child.Count > 0)
            {
                // Make root node
                JObject c_Root = store.AssureJObject("c");
                // Loop thru
                foreach (string sChild in c_Child)
                {
                    // Add child
                    this.DumpNode(node.Node(sChild), c_Root.AssureJObject(sChild));
                }
            }
        }

        /// <summary>
        /// 
        /// Loads a node from a JSON object
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="store"></param>
        private void LoadNode(NodeClass node, JObject store)
        {
            // Load teh value, if any
            if (store.Contains("v")) node.Value = store.Get("v");

            // Children?
            if (store.Contains("c"))
            {
                // Get data
                JObject c_Root = store.AssureJObject("c");
                // Get names
                List<string> c_Child = c_Root.Keys();
                // Any?
                if (c_Child != null && c_Child.Count > 0)
                {
                    // Loop thru
                    foreach (string sChild in c_Child)
                    {
                        // Load child
                        this.LoadNode(node.Node(sChild), c_Root.AssureJObject(sChild));
                    }
                }
            }
        }
        #endregion

        #region Generic
        /// <summary>
        /// 
        /// Adds a prefix to a key chain
        /// 
        /// </summary>
        /// <param name="prefix">Prefix to add</param>
        /// <param name="keychain">The list of keys</param>
        /// <returns>The updated keychain</returns>
        private List<string> AddPrefixX(string prefix, List<string> keychain)
        {
            List<string> c_Ans = new List<string>();

            c_Ans.Add(prefix);
            c_Ans.AddRange(keychain);

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Makes a real key from the keychain
        /// 
        /// </summary>
        /// <param name="keychain">The list of keys</param>
        /// <param name="extra">Extra key</param>
        /// <returns>The FQ key</returns>
        private string MakeKey(List<string> keychain, string extra = null, string prefix = null)
        {
            // Assume none
            string sAns = "";
            // Prefix?
            if (prefix.HasValue()) sAns += "/" + prefix;
            // The list of keys
            sAns += keychain.Join("/");
            // Extra?
            if (extra.HasValue()) sAns += "/" + extra;
            // remove leading
            while (sAns.StartsWith("/")) sAns = sAns.Substring(1);

            return sAns;
        }

        /// <summary>
        /// 
        /// Gets the value of a keychanin
        /// 
        /// </summary>
        /// <param name="keychain">The list of keys</param>
        /// <param name="extra">Extra key</param>
        /// <param name="prefix">Prefix key</param>
        /// <returns></returns>
        internal string IGet(List<string> keychain, string extra = null, string prefix = null)
        {
            // Assume none
            string sAns = null;

            // Make the key
            string sKey = this.MakeKey(keychain, extra, prefix);

            // Protect as redis may be down
            try
            {
                // Get
                sAns = this.DB.StringGet(sKey);
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While reading: {0} - {1}".FormatString(sKey, e.GetAllExceptions()), true);
                }
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Sets the value of a keychanin
        /// 
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="keychain">The list of keys</param>
        /// <param name="extra">Extra key</param>
        /// <param name="prefix">Prefix key</param>
        internal bool IPut(string value, List<string> keychain, string prefix = null)
        {
            // Assume failure
            bool bAns = false;

            // Make the key
            string sKey = this.MakeKey(keychain, prefix: prefix);

            // Protect as redis may be down
            try
            {
                this.DB.StringSet(sKey, value);

            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While writing: {0} - {1}".FormatString(sKey, e.GetAllExceptions()), false);
                }
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Deletes a child node
        /// 
        /// </summary>
        /// <param name="node">The node to delete</param>
        /// <param name="key">The child key</param>
        /// <returns>True if succesful</returns>
        internal bool IDelete(NodeClass node, string key)
        {
            // Assume fail
            bool bAns = false;

            // Make the key
            string sKey = this.MakeKey(node.Keys(), key);

            // Protect as redis may be down
            try
            {
                this.DB.KeyDelete(sKey);
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While deleting: {0} - {1}".FormatString(sKey, e.GetAllExceptions()), true);
                }
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Deletes a node and all children
        /// 
        /// </summary>
        /// <param name="node">The node to delete</param>
        /// <returns>True if succesful</returns>
        internal bool IDelete(NodeClass node)
        {
            // Assume failure
            bool bAns = false;

            // Make the key
            string sKey = this.MakeKey(node.Keys());

            // Protect as redis may be down
            try
            {
                this.DB.KeyDelete(sKey);
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While deleting: {0} - {1}".FormatString(sKey, e.GetAllExceptions()), true);
                }
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// List the children keys
        /// 
        /// </summary>
        /// <param name="keychain">The list of keys</param>
        /// <returns></returns>
        internal List<string> IChildrenKeys(List<string> keychain)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Make the key
            string sKey = this.MakeKey(keychain);

            //
            string sEP = this.Parent.Parent.Location.URLMake().RemoveProtocol();

            // Protect as redis may be down
            try
            {
                // Get the keys
                foreach (var c_Key in this.Client.GetServer(sEP).Keys(pattern: sKey + "*"))
                {
                    // Add
                    c_Ans.Add(c_Key.ToString());
                }
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While listing: {0} - {1}".FormatString(sKey, e.GetAllExceptions()), true);
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the value as a JSON object
        /// 
        /// </summary>
        /// <param name="keychain">The list of keys</param>
        /// <returns>The value as JSON object</returns>
        internal JObject IChildrenAsJObject(List<string> keychain)
        {
            // Assume none
            JObject c_Ans = new JObject();

            // Make the key
            string sKey = this.MakeKey(keychain);

            // Protect as redis may be down
            try
            {
                // Loop thru
                foreach (string sCKey in this.IChildrenKeys(keychain))
                {
                    // Set
                    c_Ans.Set(sKey, this.IGet(keychain, sCKey));
                }
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                {
                    this.OnError("While asjobject: {0} - {1}".FormatString(this.MakeKey(keychain), e.GetAllExceptions()), true);
                }
            }

            return c_Ans;
        }
        #endregion

        /// <summary>
        /// 
        /// Wrappers for the store section
        /// 
        /// </summary>
        #region Store
        public string StoreGet(params string[] keychain)
        {
            return this.StoreGet(new List<string>(keychain));
        }

        public bool StorePut(string value, params string[] keychain)
        {
            return this.StorePut(value, new List<string>(keychain));
        }

        public string StoreGet(List<string> keychain)
        {
            return this.IGet(keychain, prefix: PrefixStore);
        }

        public bool StorePut(string value, List<string> keychain)
        {
            return this.IPut(value, keychain, prefix: PrefixStore);
        }

        private NodeClass IStoreNode { get; set; }
        public NodeClass StoreNode(string key, bool clear = false)
        {
            if (this.IStoreNode == null)
            {
                this.IStoreNode = new NodeClass(this, null, PrefixStore);
            }

            NodeClass c_Ans = this.IStoreNode.Node(key);
            if (clear) c_Ans.Delete();

            return c_Ans;
        }
        #endregion

        /// <summary>
        /// 
        /// Wrappers for the core section
        /// 
        /// </summary>
        #region Core
        public string CoreGet(params string[] keychain)
        {
            return this.CoreGet(new List<string>(keychain));
        }

        public bool CorePut(string value, params string[] keychain)
        {
            return this.CorePut(value, new List<string>(keychain));
        }

        public string CoreGet(List<string> keychain)
        {
            return this.IGet(keychain, prefix: PrefixCore);
        }

        public bool CorePut(string value, List<string> keychain)
        {
            return this.IPut(value, keychain, prefix: PrefixCore);
        }

        private NodeClass ICoreNode { get; set; }
        public NodeClass CoreNode(string key, bool clear = false)
        {
            if (this.ICoreNode == null)
            {
                this.ICoreNode = new NodeClass(this, null, PrefixCore);
            }

            NodeClass c_Ans = this.ICoreNode.Node(key);
            if (clear) c_Ans.Delete();

            return c_Ans;
        }
        #endregion

        /// <summary>
        /// 
        /// Wrappers for the Traefik section
        /// 
        /// </summary>
        #region Traefik
        public string TraefikGet(params string[] keychain)
        {
            return this.TraefikGet(new List<string>(keychain));
        }

        public bool TraefikPut(string value, params string[] keychain)
        {
            return this.TraefikPut(value, new List<string>(keychain));
        }

        public string TraefikGet(List<string> keychain)
        {
            return this.IGet(keychain, prefix: PrefixTraefik);
        }

        public bool TraefikPut(string value, List<string> keychain)
        {
            return this.IPut(value, keychain, prefix: PrefixTraefik);
        }

        private NodeClass ITraefikNode { get; set; }
        public NodeClass TraefikNode(string key, bool clear = false)
        {
            if (this.ITraefikNode == null)
            {
                this.ITraefikNode = new NodeClass(this, null, PrefixTraefik);
            }

            NodeClass c_Ans = this.ITraefikNode.Node(key);
            if (clear) c_Ans.Delete();

            return c_Ans;
        }
        #endregion
    }
}