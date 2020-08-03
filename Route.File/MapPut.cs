﻿///--------------------------------------------------------------------------------
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

using Proc.File;

using NX.Engine;
using NX.Shared;

namespace Route.File
{
    /// <summary>
    /// 
    /// Uploads a file merge map
    /// 
    /// Uses from passed store:
    /// 
    /// path        - The JSON array of the part sections
    /// 
    /// Returns:
    /// 
    /// #json#      - OK/Fail.
    ///               The object has the format of:
    ///               {
    ///                 "ok": "0/1"
    ///               }
    /// 
    /// NOTE: The body is treated as JSON object and stored as the map
    /// 
    /// </summary>
    public class MapPut : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.POST, Support.Route, "map", "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Get the full path
            string sPath = store.PathFromEntry(call.Env.DocumentFolder, "path");

            // Get the manager
            ManagerClass c_Mgr = call.Env.Globals.Get<ManagerClass>();

            // And upload
            using (DocumentClass c_Doc = new DocumentClass(c_Mgr.Storage, sPath))
            {
                // The map
                using (DocumentClass c_Map = c_Doc.MergeMapDocument)
                {
                    c_Mgr.Upload(call, c_Map);
                }
            }
        }
    }
}