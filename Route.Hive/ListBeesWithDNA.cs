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

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Engine.Hive;
using NX.Shared;

namespace Route.Hive
{
    /// <summary>
    /// 
    /// Lists the running bees
    /// 
    /// Passed in store:
    /// 
    /// dna             - The DNA name
    /// 
    /// Returns:
    /// 
    /// The bee information as a JSON array
    /// 
    /// </summary>
    public class ListBeesWithDNA : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET, Support.Route, "list" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Get the manager
            HiveClass c_Mgr = call.Env.Hive;
            // On?
            if (c_Mgr != null)
            {
                // Make the result
                JArray c_Resp = new JArray();
                
                // Get the bees
                List<BeeClass> c_Bees = c_Mgr.Roster.GetBeesForDNA(store["dna"]);
                // Loop thru
                foreach (BeeClass c_Bee in c_Bees)
                {
                    // Make a small object
                    JObject c_Entry = new JObject();
                    // Add a few things
                    c_Entry.Set("id", c_Bee.CV.Id);
                    c_Entry.Set("field", c_Bee.Field.Name);
                    c_Entry.Set("genome", c_Bee.CV.DNA);
                    c_Entry.Set("proc", c_Bee.CV.Proc);

                    // Add to response
                    c_Resp.Add(c_Entry);
                }

                // Send it back
                call.RespondWithJSON(c_Resp);
            }
        }
    }
}