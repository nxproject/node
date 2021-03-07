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
using NX.Engine.Files;
using NX.Shared;

namespace Route.File
{
    /// <summary>
    /// 
    /// Retrieves a folder content
    /// 
    /// Uses from passed store:
    /// 
    /// path        - The JSON array of the part sections
    /// 
    /// Returns:
    /// 
    /// #tree#      - A JSON object, if the folder exists
    ///               The object has the format of:
    ///               {
    ///                 "files": [],
    ///                 "folders": []
    ///               }
    /// 
    /// </summary>
    public class Folder : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), Support.Route, "folder", "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Get the full path
            string sPath = store.PathFromEntry(NX.Engine.Files.ManagerClass.MappedFolder, "path").URLDecode(); ;

            // Get the manager
            NX.Engine.Files.ManagerClass c_Mgr = call.Env.Globals.Get<NX.Engine.Files.ManagerClass>();

            // And make
            using (FolderClass c_Ref = new FolderClass(c_Mgr, sPath))
            {
                // Make result
                StoreClass c_Ans = new StoreClass();

                JArray c_Files = new JArray();
                JArray c_Folders = new JArray();

                // Loop thru
                foreach(DocumentClass c_Doc in c_Ref.Files)
                {
                    c_Files.Add(c_Doc.Path);
                }
                foreach(FolderClass c_Folder in c_Ref.Folders)
                {
                    c_Folders.Add(c_Folder.Path);
                }

                // Store
                c_Ans.Set("files", c_Files);
                c_Ans.Set("folders", c_Folders);

                // And deliver
                call.RespondWithStore(c_Ans);
            }
        }
    }
}