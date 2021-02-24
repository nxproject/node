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

using System.Collections.Generic;

using NX.Engine;
using NX.Shared;

namespace Route.Dynamic
{
    /// <summary>
    /// 
    /// Adds a route code block, which can be a DLL or C# text
    /// 
    /// Passed in store:
    /// 
    /// name            - The name of the DLL
    /// 
    /// Body is the .cs or .dll contents
    /// 
    /// Returns:
    /// 
    /// OK/Fail
    /// 
    /// </summary>
    public class Add : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.POST(Types.Secured), Support.Route, "add" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            if (call.Env.DynamicFolder.HasValue() && call.Body.Length > 0)
            {
                // Get the name
                string sName = store["name"].GetFileNameFromPath();
                // Get the extension
                string sExt = sName.GetExtensionFromPath();
                // And remove it
                sName = sName.Substring(0, sName.Length - (sExt.Length + (sExt.Length > 0 ? 1 : 0)));
                // Form the DLL name
                sName += ".dll";
                // Must have proper prfix
                if (!sName.StartsWith("Route.") && 
                    !sName.StartsWith("Fn.") && 
                    !sName.StartsWith("Proc."))
                {
                    // Assume function
                    sName = "Fn." + sName;
                }

                // Get the location where to store
                string sPath = call.Env.DynamicFolder.CombinePath(sName);

                // Compile the body
                byte[] abCode = null;
                switch (sExt)
                {
                    case "cs":
                        abCode = Compilers.CSharp(sName, call.Body);
                        break;

                    case "vb":
                        abCode = Compilers.VB(sName, call.Body);
                        break;
                }

                // Result?
                if (abCode != null)
                {
                    // Write the new DLL
                    sPath.WriteFileAsBytes(abCode);
                    // Load it
                    call.Env.Use(sPath);

                    call.RespondWithOK();
                }
                else
                {
                    call.RespondWithFail();
                }
            }
            else
            {
                call.RespondWithFail();
            }
        }
    }
}