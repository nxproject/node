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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package Docker.DotNet -Version 3.125.2
/// 

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Docker.DotNet.Models;

using NX.Engine;
using NX.Engine.Hive;
using NX.Shared;

namespace Route.Docker
{
    /// <summary>
    /// 
    /// Gets the logs for a container
    /// 
    /// </summary>
    public class GetLogs : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), "getctxlogs" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Make response
            string sLogs = "";

            // Get the ID
            string sID = store["id"];

            // Loop thru fields
            foreach (FieldClass c_Field in call.Env.Hive.Fields.Values)
            {
                //
                DockerIFClass c_Client = c_Field.DockerIF;
                if (c_Client != null)
                {
                    foreach (string sCID in c_Client.ListContainersAll())
                    {
                        // Same ID?
                        if(sID.IsSameValue(sCID))
                        {
                            // Get the logs
                            sLogs = c_Client.GetLogsAsString(sID);
                        }
                    }
                }

            }

            //
            call.RespondWithJSON("logs".AsJObject(sLogs));
        }
    }
}