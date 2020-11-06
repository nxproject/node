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
                    foreach (ContainerListResponse c_Ctx in c_Client.ListContainers())
                    {
                        // Same ID?
                        if(sID.IsSameValue(c_Ctx.ID))
                        {
                            // Get the logs
                            sLogs = c_Client.GetLogsAsString(sID);
                        }
                        //// 
                        //JObject c_Entry = new JObject();

                        //c_Entry.Set("ID", c_Ctx.ID);
                        //c_Entry.Set("Names", new List<string>(c_Ctx.Names).Join(", "));
                        //c_Entry.Set("Image", c_Ctx.Image);
                        //c_Entry.Set("ImageID", c_Ctx.ImageID);
                        //c_Entry.Set("Command", c_Ctx.Command);
                        //c_Entry.Set("Created", c_Ctx.Created.ToString());

                        //List<string> c_Values = new List<string>();
                        //foreach (Port c_Port in c_Ctx.Ports)
                        //{
                        //    string sWkg = "{0} {1} {2}:{3}".FormatString(c_Port.Type, c_Port.IP, c_Port.PrivatePort, c_Port.PublicPort);
                        //    c_Values.Add(sWkg);
                        //}
                        //c_Entry.Set("Ports", c_Values.Join(", "));

                        //c_Entry.Set("SizeRw", c_Ctx.SizeRw);
                        //c_Entry.Set("SizeRootFs", c_Ctx.SizeRootFs);

                        //c_Values = new List<string>();
                        //foreach (string sKey in c_Ctx.Labels.Keys)
                        //{
                        //    string sWkg = "{0}={1}".FormatString(sKey, c_Ctx.Labels[sKey]);
                        //    c_Values.Add(sWkg);
                        //}
                        //c_Entry.Set("Labels", c_Values.Join(", "));

                        //c_Entry.Set("State", c_Ctx.State);
                        //c_Entry.Set("Status", c_Ctx.Status);

                        ////c_Entry.Set("ID", c_Ctx.ID);
                        ////[DataMember(Name = "NetworkSettings", EmitDefaultValue = false)]
                        ////public SummaryNetworkSettings NetworkSettings { get; set; }

                        //c_Values = new List<string>();
                        //foreach (MountPoint c_Mount in c_Ctx.Mounts)
                        //{
                        //    string sWkg = "{0}={1}".FormatString(c_Mount.Source, c_Mount.Destination);
                        //    c_Values.Add(sWkg);
                        //}
                        //c_Entry.Set("Mounts", c_Values.Join(", "));

                        //c_List.Add(c_Entry);
                    }
                }

            }

            //
            call.RespondWithJSON("logs".AsJObject(sLogs));
        }
    }
}