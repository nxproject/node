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
    public class InformationClass : BasedObjectClass
    {
        #region Constructor
        internal InformationClass(string name,
                                            string url,
                                            string addr,
                                            string port,
                                            NodeClass value,
                                            string status,
                                            string statuson,
                                            NodeClass varnode)
            : base(value, true)
        {
            //
            this.Name = name;
            this.URL = url;
            this.Address = addr;
            this.Port = port;
            this.Status = status;
            this.StatusOn = statuson;

            try
            {
                if (varnode != null)
                {
                    List<string> c_Keys = varnode.ChildrenKeys;
                    foreach (string sKey in c_Keys)
                    {
                        string sValue = varnode.Get(sKey);

                        if (this.Extras.ContainsKey(sKey))
                        {
                            this.Extras[sKey] = sValue;
                        }
                        else
                        {
                            this.Extras.Add(sKey, sValue);
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        { }
        #endregion

        #region Indexer
        public string this[string key]
        {
            get { return this.Values.Get(key); }
            set { this.Values.Put(key, value); }
        }
        #endregion

        #region Properties
        private NodeClass Values { get { return this.Root as NodeClass; } }

        public string Name { get; private set; }
        public string URL { get; private set; }
        public string Address { get; private set; }
        public string Port { get; private set; }
        public string Status { get; private set; }
        public string StatusOn { get; private set; }
        public string CloudID
        {
            get
            {
                string sAns = "";

                if (this.Extras.ContainsKey("cloudid"))
                {
                    sAns = this.Extras["cloudid"];
                }

                return sAns;
            }
        }
        public List<string> Keys { get { return this.Values.Keys(); } }

        private NamedListClass<string> Extras = new NamedListClass<string>();
        public List<string> ExtraKeys { get { return new List<string>(this.Extras.Keys); } }

        public bool Used { get; set; }
        #endregion

        #region Methods
        public string GetExtra(string key)
        {
            string sAns = null;

            if (this.Extras.ContainsKey(key)) sAns = this.Extras[key];

            return sAns.IfEmpty();
        }
        #endregion
    }
}