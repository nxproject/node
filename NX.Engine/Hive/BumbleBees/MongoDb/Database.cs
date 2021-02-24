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

/// Packet Manager Requirements
/// 
/// Install-Package MongoDb.Driver -Version 2.10.4
/// Install-Package MongoDb.Bson -Version 2.10.4
/// 

using System.Collections.Generic;

using MongoDB.Driver;
using MongoDB.Bson;

using NX.Shared;

namespace NX.Engine.BumbleBees.MongoDb
{
    public class DatabaseClass : ChildOfClass<ManagerClass>
    {
        #region Constructor
        public DatabaseClass(ManagerClass mgr, string name)
            : base(mgr)
        {
            //
            this.Name = name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name of the database
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// Is the client available
        /// 
        /// </summary>
        public bool IsAvailable => this.Interface != null;

        /// <summary>
        /// 
        /// The databse interface
        /// 
        /// </summary>
        private IMongoDatabase IInterface { get; set; }
        public IMongoDatabase Interface
        {
            get
            {
                // Already setup?
                if (this.IInterface == null && this.Parent.IsAvailable && this.Parent.Client != null)
                {
                    // Make
                    this.IInterface = this.Parent.Client.GetDatabase(this.Name);
                }

                return this.IInterface;
            }
        }

        /// <summary>
        /// 
        /// Cache of collections
        /// 
        /// </summary>
        private NamedListClass<CollectionClass> Cache { get; set; } = new NamedListClass<CollectionClass>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Drops a database
        /// 
        /// </summary>
        public void Drop()
        {
            // Can we do it?
            if (this.Parent.IsAvailable && this.Parent.Client != null)
            {
                // Do
                this.Parent.Client.DropDatabase(this.Name);
            }
        }

        /// <summary>
        /// 
        /// Resets a database
        /// 
        /// </summary>
        public void Reset()
        {
            // Clear
            this.IInterface = null;

            // Do cache
            foreach(CollectionClass c_Coll in this.Cache.Values)
            {
                // Reset
                c_Coll.Reset();
            }
        }
        #endregion
    }
}