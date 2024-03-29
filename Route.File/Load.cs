﻿///--------------------------------------------------------------------------------
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

using System.Collections.Generic;

using NX.Engine;
using NX.Engine.Files;
using NX.Shared;

namespace Route.File
{
    /// <summary>
    /// 
    /// Uploads a file
    /// 
    /// </summary>
    public class Load : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.POST(), Support.Route + "load", "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            //
            StoreClass c_Ans = new StoreClass();

            // Get the full path
            string sPath = store.PathFromEntry(NX.Engine.Files.ManagerClass.MappedFolder, "path").URLDecode();

            // Get the manager
            NX.Engine.Files.ManagerClass c_Mgr = call.Env.Globals.Get<NX.Engine.Files.ManagerClass>();

            // And upload
            using (DocumentClass c_Doc = new DocumentClass(c_Mgr, sPath))
            {
                // Get
                StoreClass c_Store = call.BodyAsStore;
                // And the contents
                string sContent = c_Store["content"];
                // Baase64?
                if(store["base64"].FromDBBoolean())
                {
                    sContent = sContent.FromBase64();
                }

                // Save
                c_Doc.Value = sContent;

                c_Ans["done"] = "y";

                call.RespondWithStore(c_Ans);
            }
        }
    }
}