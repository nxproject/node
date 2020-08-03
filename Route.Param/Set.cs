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

namespace Route.Param
{
    /// <summary>
    /// 
    /// Adds an environmnt parameter
    /// 
    /// Passed in store:
    /// 
    /// name            - The name of the parameter
    /// value           - The value
    /// 
    /// Returns:
    /// 
    /// OK/Fail
    /// 
    /// </summary>
    public class Set : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.POST_SECURE, Support.Route};
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Do we have a name?
            if (store["name"].HasValue())
            {
                // Store
                call.Env[store["name"]] = store["value"];

                call.RespondWithOK();
            }
            else
            {
                call.RespondWithFail();
            }
        }
    }
}