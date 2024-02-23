///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2024 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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
    public class CollectionClass : ChildOfClass<DatabaseClass>
    {
        #region Constructor
        public CollectionClass(DatabaseClass db, string name)
            : base(db)
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
        /// The collection interface
        /// 
        /// </summary>
        private IMongoCollection<BsonDocument> IInterface { get; set; }
        public IMongoCollection<BsonDocument> Interface
        {
            get
            {
                // Already setup?
                if (this.IInterface == null && this.Parent.IsAvailable)
                {
                    // Make
                    this.IInterface = this.Parent.Interface.GetCollection< BsonDocument>(this.Name);
                }

                return this.IInterface;
            }
        }
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
            if (this.Parent.IsAvailable)
            {
                // Do
                this.Parent.Interface.DropCollection(this.Name);
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
        }
        #endregion
    }
}