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
using NX.Engine.Hive;
using NX.Shared;

namespace Route.Hive
{
    /// <summary>
    /// 
    /// Sets a DNA definition
    /// 
    /// Passed in store:
    /// 
    /// name            - The DNA to assure
    /// min             - The minimum number of instances allowed
    /// max             - The maximum number of instances allowed
    /// 
    /// The variables to be used are passed in the body as a JSON object
    /// 
    /// Returns:
    /// 
    /// OK if the operation was successful
    /// 
    /// </summary>
    public class AssureDNA : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), Support.Route, "assure" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Get the manager
            HiveClass c_Mgr = call.Env.Hive;
            // On?
            if (c_Mgr != null)
            {
                // Get the minimum
                int iMin = store["min"].ToInteger(1);
                // And the maximum
                int iMax = store["max"].ToInteger(iMin);

                // Get the variables
                StoreClass c_Data = new StoreClass(call.BodyAsJObject);

                // Do the call
                c_Mgr.AssureDNACount(store["name"], iMin, iMax, c_Data);

                // And say OK
                call.RespondWithOK();
            }
        }
    }
}