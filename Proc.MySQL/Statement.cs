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
/// Install-Package MySql.Data -Version 8.0.21
/// 

using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

using NX.Shared;

namespace Proc.MySQL
{
    /// <summary>
    /// 
    /// SQL statement
    /// 
    /// </summary>
    public class StatementClass : ChildOfClass<DatabaseClass>
    {
        #region Constructor
        public StatementClass(DatabaseClass db, string statement)
            : base(db)
        {
            //
            this.Cmd = new MySqlCommand(statement, db.Interface);
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The MySQL command
        /// 
        /// </summary>
        private MySqlCommand Cmd { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Executes statement
        /// 
        /// </summary>
        public void Execute()
        {
            this.Cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// Returns one value from statement
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetScalar()
        {
            return this.Cmd.ExecuteScalar().ToString();
        }
        #endregion
    }
}