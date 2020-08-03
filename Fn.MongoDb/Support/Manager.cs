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
/// Install-Package ServiceStack.Redis -Version 5.9.0
/// Install-Package MongoDb.Driver -Version 2.10.4
/// 

using System.Collections.Generic;

using MongoDB.Driver;

using NX.Engine;
using NX.Shared;

namespace Fn.MongoDb
{
    /// <summary>
    /// 
    /// MongoDb interface
    /// 
    /// Defined settings
    /// 
    /// The following settings are used:
    /// 
    /// mongo_host          -  The host url
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass    
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "percona")
        {
            //// Do we have a running task?
            //if (this.Parent.Hive.Roster.GetLocationsForDNA(ManagerClass.TaskName).Count == 0)
            //{
            //    // Launch via the queen
            //    env.Hive.Roster.FN(env.Hive.Roster.QueenBee, "Sytem.Assure", "dna".AsJObject(ManagerClass.TaskName));
            //}
        }
        #endregion
    }
}