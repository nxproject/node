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
/// Install-Package MongoDb.Driver -Version 2.10.4
/// 

using System.Collections.Generic;

using MongoDB.Driver;

using NX.Engine;
using NX.Shared;
using RestSharp.Extensions;

namespace Proc.MongoDb
{
    /// <summary>
    /// 
    /// MongoDb interface
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, env["nosql"])
        {
            // Handle the event
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Kill current
                if (this.Client != null)
                {
                    this.Client = null;
                }

                // Accordingly
                if (isavailable)
                {
                    // Make the client
                    this.Client = new MongoClient(this.Location);
                }

                // Reset all databases
                foreach(DatabaseClass c_DB in this.Cache.Values)
                {
                    // Reset
                    c_DB.Reset();
                }
            };

            // Bootstap
            this.CheckForAvailability();
        }
        #endregion

        #region Indexer
        public DatabaseClass this[string db]
        {
            get
            {
                // Do we know of it?
                if(!this.Cache.Contains(db))
                {
                    // Make
                    this.Cache[db] = new DatabaseClass(this, db);
                }

                return this.Cache[db];
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The MongoDb client
        /// 
        /// </summary>
        public MongoClient Client { get; set; }

        /// <summary>
        /// 
        /// Is the client available
        /// 
        /// </summary>
        public override bool IsAvailable => this.Client != null;

        /// <summary>
        /// 
        /// Cache of databases
        /// 
        /// </summary>
        private NamedListClass<DatabaseClass> Cache { get; set; } = new NamedListClass<DatabaseClass>();

        /// <summary>
        /// 
        /// Het a list of databases
        /// 
        /// </summary>
        public List<string> Databases
        {
            get
            {
                // Assume none
                List<string> c_Ans = new List<string>();

                // can we get them?
                if(this.IsAvailable)
                {
                    // Do so
                    this.Client.ListDatabaseNames().ForEachAsync(db => c_Ans.Add(db));
                }

                return c_Ans;
            }
        }
        #endregion
    }
}