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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;
using System.Linq;

using Docker.DotNet.Models;
using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A bee's CV with all the information about it
    /// 
    /// </summary>
    public class BeeCVClass : ChildOfClass<FieldClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="values">The values as a JObject</param>
        public BeeCVClass(FieldClass field, string id, BeeClass.Types type = BeeClass.Types.Bee)
            : base(field)
        {
            //
            this.IsGhost = type == BeeClass.Types.Ghost;
            this.IsVirtual = type == BeeClass.Types.Virtual;

            // Make a phony values
            this.Values = new ContainerListResponse();
            // Set the Id
            this.Values.ID = id;

            // Handle non-bee setup
            if (this.IsGhost || this.IsVirtual)
            {
                // make phony labels
                this.Labels = new Dictionary<string, string>();
                // Fill
                this.Labels.Add(this.TheHive.LabelID, id);
                this.Labels.Add(this.TheHive.LabelUUID, "".GUID());
                this.Labels.Add(this.TheHive.LabelDNA, HiveClass.ProcessorDNAName + "_");
                this.Labels.Add(this.TheHive.LabelHive + "_" + this.Parent.Name, "Y");
                this.Labels.Add(this.TheHive.LabelField, field.Name);
                this.Labels.Add(this.TheHive.LabelProc, field.Parent.Parent.Process);
            }

            // And refresh
            this.Refresh();
        }

        public BeeCVClass(FieldClass field, ContainerListResponse cv)
            : base(field)
        {
            // Make the UUID
            this.Values = cv;

            //
            this.BuildTickleAreas();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Obtained from Docker
        /// 
        /// </summary>
        private ContainerListResponse Values { get; set; }

        /// <summary>
        /// 
        /// Is this a ghost bee?
        /// 
        /// </summary>
        public bool IsGhost { get; private set; }

        /// <summary>
        /// 
        /// Is this a virtual bee?
        /// 
        /// </summary>
        public bool IsVirtual { get; private set; }

        /// <summary>
        /// 
        /// Is the bee running?
        /// 
        /// </summary>
        public bool IsRunning
        {
            get { return this.State.IsSameValue("running"); }
        }

        /// <summary>
        /// 
        /// The bee is in trouble!
        /// 
        /// </summary>
        public bool IsInTrouble
        {
            get
            {
                return this.State.IsSameValue("exited") ||
                     this.State.IsSameValue("unhealthy") ||
                     this.State.IsSameValue("restarting");
            }
        }
        #endregion

        #region CV
        public string Id
        {
            get { return this.Values.ID; }
        }

        public List<string> Names
        {
            get { return this.Values.Names as List<string>; }
        }

        public string Image
        {
            get { return this.Values.Image; }
        }

        public string ImageID
        {
            get { return this.Values.ImageID; }
        }

        public string Command
        {
            get { return this.Values.Command; }
        }

        public DateTime Created
        {
            get { return this.Values.Created; }
        }

        public string State
        {
            get { return this.Values.State; }
        }

        public string Status
        {
            get { return this.Values.Status; }
        }

        public List<Port> Ports
        {
            get { return new List<Port>(this.Values.Ports); }
        }

        public IDictionary<string, string> Labels
        {
            get { return this.Values.Labels; }
            private set { this.Values.Labels = value; }
        }

        public long SizeRw
        {
            get { return this.Values.SizeRw; }
        }

        public long SizeRootFs
        {
            get { return this.Values.SizeRootFs; }
        }

        public SummaryNetworkSettings NetworkSettings
        {
            get { return this.Values.NetworkSettings; }
        }

        public IList<MountPoint> Mounts
        {
            get { return this.Values.Mounts; }
        }
        #endregion

        #region Extras
        /// <summary>
        /// 
        /// The unique ID for the container
        /// 
        /// </summary>
        public string NXID
        {
            get { return this.GetLabel(this.TheHive.LabelID); }
        }

        /// <summary>
        /// 
        /// The unique ID for the container
        /// 
        /// </summary>
        public string GUID
        {
            get { return this.GetLabel(this.TheHive.LabelUUID); }
        }

        /// <summary>
        /// 
        /// The genome that created this container
        /// 
        /// </summary>
        public string DNA
        {
            get { return this.GetLabel(this.TheHive.LabelDNA).IfEmpty().Replace("_", "."); }
            set { this.SetLabel(this.TheHive.LabelDNA, value.IfEmpty().Replace(".", "_")); }
        }

        /// <summary>
        /// 
        /// The sites that created this container
        /// 
        /// </summary>
        public List<string> Hives
        {
            get
            {
                // Assume none
                List<string> c_Ans = new List<string>();

                // Any Labels
                if (this.Labels != null)
                {
                    // Make the pattern
                    string sPatt = this.TheHive.LabelHive + "_";
                    // Loop thru
                    foreach (string sKey in this.Labels.Keys)
                    {
                        // Hive?
                        if (sKey.StartsWith(sPatt))
                        {
                            // Get the name
                            c_Ans.Add(sKey.Substring(sPatt.Length));
                        }
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The member that holds this container
        /// 
        /// </summary>
        public string Field
        {
            get { return this.GetLabel(this.TheHive.LabelField); }
        }

        /// <summary>
        /// 
        /// The original Cmd
        /// 
        /// </summary>
        public JArray Cmd
        {
            get { return this.GetLabel(this.TheHive.LabelCmd).ToJArray(); }
        }

        /// <summary>
        /// 
        /// Returns the bee proc value
        /// 
        /// </summary>
        public string Proc
        {
            get { return this.GetLabel(this.TheHive.LabelProc); }
        }

        /// <summary>
        /// 
        /// The tickle areas
        /// 
        /// </summary>
        public List<TickleAreaClass> TickleAreas { get; private set; }
        #endregion

        #region Hive
        private HiveClass TheHive { get { return this.Parent.Parent; } }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// The labels
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        private string GetLabel(string label)
        {
            // Assume none
            string sAns = "";

            // Do we have labels?
            if (this.Labels != null && this.Labels.ContainsKey(label))
            {
                // Get
                sAns = this.Labels[label];
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Sets a labels
        /// 
        /// </summary>
        /// <param name="label">The label to set</param>
        /// <param name="value">The value to set it to</param>
        private void SetLabel(string label, string value)
        {
            // Do we have labels?
            if (this.Labels == null)
            {
                // Make them
                this.Labels = new Dictionary<string, string>();
            }

            //
            this.Labels[label] = value;
        }

        /// <summary>
        /// 
        /// Waits until a container has reached a state
        /// 
        /// </summary>
        /// <param name="member">The member holding the container</param>
        /// <param name="id">The container Id</param>
        /// <param name="cb">The callback when the container has reached a state</param>
        /// <param name="states">List of states that we are looking for</param>
        public void Wait(HiveClass hive, params WaitClass[] states)
        {
            // Make the states into a list
            List<WaitClass> c_States = new List<WaitClass>(states);

            // Do till no more
            WaitClass.TriggerReturns eState = WaitClass.TriggerReturns.Continue;

            do
            {
                // Wait a bit
                500.MillisecondsAsTimeSpan().Sleep();
                // Get info
                this.Refresh();
                // Loop thru
                foreach (WaitClass c_Wait in c_States)
                {
                    // Triggered?
                    eState = c_Wait.Trigger(this.State);
                    // Only one
                    if (eState == WaitClass.TriggerReturns.EndWait) break;
                }

            }
            while (eState == WaitClass.TriggerReturns.Continue);
        }

        /// <summary>
        /// 
        /// Refreshes the CV contents
        /// 
        /// </summary>
        public void Refresh()
        {
            // Skip if ghost or virtual
            if (!this.IsGhost && !this.IsVirtual)
            {
                // Get client
                DockerIFClass c_Client = this.Parent.DockerIF;
                // Any?
                if (c_Client != null)
                {
                    // Protect
                    try
                    {
                        // Get
                        IList<ContainerListResponse> c_CVs = c_Client.ListContainers(new DockerIFFilterClass("id", this.Id));
                        // Any?
                        if (c_CVs.Count > 0)
                        {
                            // Get the first and only
                            this.Values = c_CVs.First();
                        }
                    }
                    catch { }
                }
            }

            //
            this.BuildTickleAreas();
        }

        /// <summary>
        /// 
        /// Makes a state change trigger
        /// 
        /// </summary>
        /// <param name="cb">The callback</param>
        /// <param name="states">The states that will trigger the callback</param>
        /// <returns></returns>
        public WaitClass MakeWait(Func<BeeCVClass, WaitClass.TriggerReturns> cb, params string[] states)
        {
            return new WaitClass(this, cb, states);
        }

        /// <summary>
        /// 
        /// Builds the list of tickle areas
        /// 
        /// </summary>
        private void BuildTickleAreas()
        {
            // reset
            this.TickleAreas = new List<TickleAreaClass>();

            TickleAreaClass c_EP = null;

            // Setup by type
            if (this.IsGhost)
            {
                // If ghost, phony the URL
                //this.IURL = "".GetLocalIP() + ":{0}".FormatString(this.Parent.Parent.Parent.HTTPPort).URLMake();

                // Make the TickleArea
                c_EP = new TickleAreaClass(this, this.Parent.Parent.Parent.HTTPPort.ToString(),
                                                                    this.Parent.Parent.Parent.HTTPPort.ToString());
                // Add
                this.AddTickleArea(c_EP);
            }
            else if (this.IsVirtual)
            {
                // Parse 
                ItemClass c_Info = new ItemClass(this.Values.ID);

                // Must have modifier (port)
                string sPort = c_Info.Option;
                if (sPort.HasValue())
                {
                    // The key portion is the DNA
                    this.DNA = c_Info.Key;

                    // Make a tickle area with the rest
                    TickleAreaClass c_Tickle = new TickleAreaClass(this,
                                                                sPort,
                                                                sPort,
                                                                c_Info.Value + ":" + sPort);

                    // Add
                    this.AddTickleArea(c_Tickle);
                }
            }
            else
            {
                // Loop thru
                foreach (Port c_Port in this.Ports)
                {
                    // Make the TickleArea
                    c_EP = new TickleAreaClass(this, c_Port.PublicPort.ToString(),
                                                                        c_Port.PrivatePort.ToString());

                    // Is it reachable?
                    if (c_EP.IsAvailable)
                    {
                        // Add
                        this.AddTickleArea(c_EP);
                    }
                }
            }
        }

        private void AddTickleArea(TickleAreaClass ep)
        {
            // Assume not there
            bool bFound = false;

            // Loop thru
            foreach(TickleAreaClass c_EP in this.TickleAreas)
            {
                // Same?
                bFound = c_EP.ToString().IsSameValue(ep.ToString());
                // Only one
                if (bFound) break;
            }

            // Only if new
            if (!bFound) this.TickleAreas.Add(ep);
        }
        #endregion

        /// <summary>
        /// 
        /// Allows for a callback when a state changes
        /// 
        /// </summary>
        public class WaitClass : ChildOfClass<BeeCVClass>
        {
            #region Constructor
            internal WaitClass(BeeCVClass cv, Func<BeeCVClass, TriggerReturns> cb, params string[] states)
                : base(cv)
            {
                // Make
                this.States = new List<string>(states);
                this.Callback = cb;
            }
            #endregion

            #region Enums
            public enum TriggerReturns
            {
                Continue,
                EndWait
            }
            #endregion

            #region Properties
            /// <summary>
            /// 
            /// The states that will trigger the callback
            /// 
            /// </summary>
            private List<string> States { get; set; }

            /// <summary>
            /// 
            /// The callback
            /// 
            /// </summary>
            private Func<BeeCVClass, TriggerReturns> Callback { get; set; }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// Triggers a callback on a state
            /// 
            /// </summary>
            /// <param name="state"></param>
            /// <returns></returns>
            public TriggerReturns Trigger(string state)
            {
                // Assume not
                TriggerReturns eNext = TriggerReturns.Continue;

                // In a state we are looking for?
                if (this.States.Contains(state))
                {
                    // Do we have a callback?
                    if (this.Callback != null)
                    {
                        // Do
                        eNext = this.Callback(this.Parent);
                    }
                }

                return eNext;
            }
            #endregion
        }
    }
}