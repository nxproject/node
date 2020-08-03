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

using System.Collections.Generic;

using NX.Engine;
using NX.Engine.Hive;
using NX.Shared;

namespace Route.Hive
{
    /// <summary>
    /// 
    /// Sets a task definition
    /// 
    /// Passed in store:
    /// 
    /// field           - The field where the genome resides
    /// name            - The genome to kill
    /// 
    /// Returns:
    /// 
    /// OK if the operation was successful
    /// 
    /// </summary>
    public class KillGenome : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.DELETE, Support.Route, "genome" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Get the manager
            HiveClass c_Mgr = call.Env.Hive;
            // On?
            if (c_Mgr != null)
            {
                // Do the call
                c_Mgr.KillGenome(c_Mgr.GetField(store["field"]), store["name"]);

                // And say OK
                call.RespondWithOK();
            }
        }
    }
}