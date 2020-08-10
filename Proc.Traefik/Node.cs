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

using System.Collections.Generic;

using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// A node in the settings
    /// 
    /// </summary>
    public class NodeClass : ChildOfClass<NodeClass>
    {
        #region Constructor
        internal NodeClass(SettingsClass sett, NodeClass parent, string name)
            : base(parent)
        {
            //
            this.Settings = sett;
            this.IName = name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The settings 
        /// 
        /// </summary>
        private SettingsClass Settings { get; set; }

        /// <summary>
        /// 
        /// The node name
        /// 
        /// </summary>
        private string IName { get; set; }

        /// <summary>
        /// 
        /// The value
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                // Assume none
                string sAns = null;

                // Protect as redis may be down
                try
                {
                    sAns = this.Settings.IGet(this.Keys());
                }
                catch { }

                return sAns.IfEmpty();
            }
            set
            {
                // Protect as redis may be down
                try
                {
                    this.Settings.IPut(value, this.Keys());
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// All the children node names
        /// 
        /// </summary>
        public List<string> ChildrenKeys
        {
            get
            {
                List<string> c_Ans = new List<string>();

                // Protect as redis may be down
                try
                {
                    // Make the prefix, so we can remove later
                    string sPrefix = this.Keys().Join("/");
                    // Get the list
                    List<string> c_Wkg = this.Settings.IChildrenKeys(this.Keys(null));
                    // Loop thru
                    foreach (string sWkg in c_Wkg)
                    {
                        // Remove the prefix
                        string sPoss = sWkg.Substring(sPrefix.Length + 1);
                        // Find delimiter
                        int iPos = sPoss.IndexOf("/");
                        // If found one, remove
                        if (iPos != -1) sPoss = sPoss.Substring(0, iPos);
                        // Only unique
                        if (!c_Ans.Contains(sPoss)) c_Ans.Add(sPoss);
                    }
                }
                catch { }

                return c_Ans;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns the list of child keys
        /// 
        /// </summary>
        /// <param name="key">The extra key, if any</param>
        /// <returns>The list of keys</returns>
        public List<string> Keys(string key = null)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Add parent keys
            if (this.Parent != null) c_Ans.AddRange(this.Parent.Keys(null));
            // Add us
            c_Ans.Add(this.IName);
            // Add the extra key
            if (key.HasValue()) c_Ans.Add(key);

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets a value from a given key
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public string Get(string key)
        {
            return this.Settings.IGet(this.Keys(key)).IfEmpty();
        }

        /// <summary>
        /// 
        /// Puts a value for a given key
        /// 
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="key">The key</param>
        /// <returns></returns>
        public bool Put(string value, string key)
        {
            return this.Settings.IPut(value, this.Keys(key));
        }

        /// <summary>
        /// 
        /// Gets a child node for a given key
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="clear">Clear children</param>
        /// <returns>The node</returns>
        public NodeClass Node(string key, bool clear = false)
        {
            NodeClass c_Ans = new NodeClass(this.Settings, this, key);
            if (clear)
            {
                try
                {
                    c_Ans.Delete();
                }
                catch { }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets teh nth child node
        /// 
        /// </summary>
        /// <param name="index">The node index</param>
        /// <param name="clear">Clear children</param>
        /// <returns>The node</returns>
        public NodeClass NodeIndex(int index, bool clear = false)
        {
            return this.Node(index.ToString(), clear);
        }

        /// <summary>
        /// 
        /// Deletes a child
        /// 
        /// </summary>
        /// <param name="key">The key</param>
        public void Delete(string key)
        {
            this.Settings.IDelete(this, key);
        }

        /// <summary>
        /// 
        /// Deletes node
        /// 
        /// </summary>
        public void Delete()
        {
            this.Settings.IDelete(this);
        }

        /// <summary>
        /// 
        /// Puts a space delimited set of keys as children
        /// 
        /// </summary>
        /// <param name="info">The space delimited key/value pairs</param>
        /// <param name="key">The child key</param>
        public void PutTree(string info, string key)
        {
            NodeClass c_Node = this.Node(key, true);

            List<string> c_Values = info.IfEmpty().SplitSpaces(true);
            for (int i = 0; i < c_Values.Count; i += 2)
            {
                c_Node.Put(c_Values[i + 1], c_Values[i]);
            }
        }
        #endregion
    }
}