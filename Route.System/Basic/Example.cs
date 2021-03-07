///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
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

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace Route.System
{
    /// <summary>
    /// 
    /// Returns the different options for the RoutrTree
    /// 
    /// Returns:
    /// 
    /// A JSON object that explains the URL tree options
    ///  
    /// </summary>
    public class Example : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), "example" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            JObject c_Result = new JObject();

            c_Result.Set("text", "The text will be matched");
            c_Result.Set(":name", "The text will be pasedd as parameter 'name'");
            c_Result.Set("?opt", "(Optional) The text or empty string will be passed as paraments 'opt'");
            c_Result.Set("?opt?", "(Optional but must be last) The text or empty string will be passed as paraments 'opt1', opt2',...");

            call.RespondWithJSON(c_Result);
        }
    }
}