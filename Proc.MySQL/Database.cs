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
/// Install-Package MySql.Data -Version 8.0.21
/// 

using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

using NX.Shared;

namespace Proc.MySQL
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
        private MySqlConnection IInterface { get; set; }
        public MySqlConnection Interface
        {
            get
            {
                // Already setup?
                if (this.IInterface == null && this.Parent.IsAvailable)
                {
                    // Setup the connection string
                    string sConn = "server=" + this.Parent.Location.RemoveProtocol();
                    if (this.Parent.Parent["mysql_user"].HasValue()) sConn += ";userid=" + this.Parent["mysql_user"];
                    if (this.Parent.Parent["mysql_pwd"].HasValue()) sConn += ";password=" + this.Parent["mysql_pwd"];
                    sConn += ";database=" + this.Name;

                    // Make the client
                    this.IInterface = new MySqlConnection(sConn);
                }

                return this.IInterface;
            }
        }
        #endregion

        #region Methods
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

        /// <summary>
        /// 
        /// Creates a statement
        /// 
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns></returns>
        public StatementClass New(string statement)
        {
            return new StatementClass(this, statement);
        }
        #endregion
    }
}