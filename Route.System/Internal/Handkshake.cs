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

using NX.Engine;
using NX.Shared;

namespace Route.System
{
    /// <summary>
    /// 
    /// Returns the ID of this instance
    /// 
    /// Returns:
    /// 
    /// The ID
    ///  
    /// </summary>
    public class Handshake : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), "handshake", ":request" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Is ita request?
            string sReq = store["request"];
            if (sReq.HasValue())
            {
                //
                call.Env.LogVerbose("Handshake request of {0}".FormatString(sReq));

                // According to request
                switch(sReq)
                {
                    case "Ascending":
                        // Only if we are a normal bee
                        if (call.Env.Hive.State == NX.Engine.Hive.HiveClass.States.Bee)
                        {
                            // Set
                            call.Env.Hive.State = NX.Engine.Hive.HiveClass.States.Ascending;
                            // Start the duties
                            call.Env.Hive.Roster.CheckForQueen();
                        }
                        break;

                    default:
                        // Are we in the middle of duties?
                        if (call.Env.Hive.State == NX.Engine.Hive.HiveClass.States.InQueenDuties)
                        {
                            // Cannot
                            call.RespondWithFail();
                        }
                        else if(call.Env.Hive.State == NX.Engine.Hive.HiveClass.States.Queen)
                        {
                            // Make ourselves normal
                            call.Env.Hive.State = NX.Engine.Hive.HiveClass.States.Bee;
                            // End duties
                            call.Env.Hive.Roster.CheckForQueen(true);

                            // Allow
                            call.RespondWithOK();
                        }
                        else
                        {
                            // We cannot give permission because we are not queen
                            call.RespondWithFail();
                        }
                        break;
                }
            }

            // Tell caller
            call.RespondWithOK();
        }
    }
}