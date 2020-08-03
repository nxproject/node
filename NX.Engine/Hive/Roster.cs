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
/// 

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// The roster of all the bees
    /// 
    /// </summary>
    public class RosterClass : ChildOfClass<HiveClass>
    {
        #region Constructor
        public RosterClass(HiveClass hive)
            : base(hive)
        {
            // Are we making a genome?
            if (!this.Parent.Parent.InMakeMode)
            {
                // Start the health check
                this.Parent.LabelRoster.StartThread(new System.Threading.ParameterizedThreadStart(CheckFollower));
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            // Kill the health check
            SafeThreadManagerClass.StopThread(this.Parent.LabelRoster);

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The bee count
        /// 
        /// </summary>
        public int BeeCount
        {
            get
            {
                // Start with none
                int iAns = 0;

                // Loop thru
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Add
                    iAns += c_Field.BeeCount;
                }

                return iAns;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Gets a bee from it's ID
        /// 
        /// </summary>
        /// <param name="id">The I of the bee</param>
        /// <returns>The bee if any</returns>
        public BeeClass Get(string id)
        {
            // Assume none
            BeeClass c_Ans = null;

            // DO we know it?
            if (id.HasValue())
            {
                // Loop thru fields
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Get
                    c_Ans = c_Field.GetBee(id);
                    // Only one
                    if (c_Ans != null) break;
                }
            }

            return c_Ans;
        }
        /// <summary>
        /// 
        /// Gets a bee from it's ID
        /// 
        /// </summary>
        /// <param name="id">The I of the bee</param>
        /// <returns>The bee if any</returns>
        public BeeClass GetByDockerID(string dockerid)
        {
            // Assume none
            BeeClass c_Ans = null;

            // DO we know it?
            if (dockerid.HasValue())
            {
                // Loop thru fields
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Get
                    c_Ans = c_Field.GetByDockerID(dockerid);
                    // Only one
                    if (c_Ans != null) break;
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Adds a bee to the roster
        /// 
        /// </summary>
        /// <param name="bee">The bee to add</param>
        public void Add(BeeClass bee)
        {
            // Add
            if (bee != null) bee.Field.Bees.Add(bee);
        }

        /// <summary>
        /// 
        /// Removes a bee according to the Docker ID
        /// 
        /// </summary>
        /// <param name="dockerid">The Docker ID</param>
        public void Remove(string dockerid)
        {
            // Assume not here
            BeeClass c_Bee = this.GetByDockerID(dockerid);

            // Do we know it?
            if (c_Bee != null)
            {
                // Remove
                this.Remove(c_Bee);
            }
        }

        /// <summary>
        /// 
        /// Removes a bee from the roster
        /// 
        /// </summary>
        /// <param name="bee">The bee to delete</param>
        public void Remove(BeeClass bee)
        {
            // Remove
            if (bee != null) bee.Field.Bees.Remove(bee);
        }

        /// <summary>
        /// 
        /// Returns the bees for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns>A list of bees</returns>
        public List<BeeClass> GetBeesForDNA(string DNA)
        {
            // Assume none
            List<BeeClass> c_Ans = new List<BeeClass>();

            // Loop thru
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Get the URLs
                c_Ans.AddRange(c_Field.GetBeesForDNA(DNA));
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the bees for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns>A list of bees</returns>
        public List<BeeClass> GetBeesForProcess(string proc)
        {
            // By compound
            return this.GetBeesForDNA(HiveClass.ProcessorDNAName + "." + proc);
        }

        /// <summary>
        /// 
        /// Returns the URLs for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns>The list of locations</returns>
        public List<string> GetLocationsForDNA(string DNA)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Loop thru
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Add
                c_Ans.AddRange(c_Field.GetLocationsForDNA(DNA));
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the URLs for a given port
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <returns>The list of locations</returns>
        public List<string> GetLocationsForPort(string port)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Loop thru
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Add
                c_Ans.AddRange(c_Field.GetLocationsForPort(port));
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Refreshes the roster
        /// 
        /// </summary>
        public void Refresh()
        {
            // Tell user
            this.Parent.Parent.LogVerbose("Refreshing hive {0}", this.Parent.Name);

            // Refresh each field
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Refresh it
                c_Field.Refresh();
            }

            // Are we running outside container?
            if (!"".InContainer())
            {
                // Do we have a me?
                if (this.Parent.Me == null)
                {
                    // 
                    this.Parent.Parent.LogInfo("Creating a ghost bee as {0}", this.Parent.Parent.ID);
                    // Make me
                    this.Parent.Me = new BeeClass(this.Parent.Fields.Values.First(), 
                                                    this.Parent.Parent.ID, 
                                                    BeeClass.Types.Ghost);
                    // Add to roster
                    this.Add(this.Parent.Me);
                }
            }

            // Do the queens business
            this.CheckForQueen();

            // Tell user
            this.Parent.Parent.LogVerbose("End of refresh for hive {0}", this.Parent.Name);
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for all of the events
        /// 
        /// </summary>
        /// <param name="value">The subject</param>
        /// <param name="url">A list of URLs</param>
        public delegate void OnChangedHandler(string value, List<string> url);

        /// <summary>
        /// 
        /// Defines the event to be raised when a DNA is added/deleted
        /// 
        /// </summary>
        public event OnChangedHandler DNAChanged;

        /// <summary>
        /// 
        /// Signals chnages in the DNA
        /// 
        /// </summary>
        /// <param name="changed">The list of DNAs that changed</param>
        public void SignalDNAChanged(List<string> changed)
        {
            // Handle the DNA changes
            if (this.DNAChanged != null)
            {
                // Do each
                foreach (string sDNA in changed)
                {
                    // Call event
                    this.DNAChanged?.Invoke(sDNA, this.GetLocationsForDNA(sDNA));
                }
            }
        }

        /// <summary>
        /// 
        /// Defines the event to be raised when a port is added/deleted
        /// 
        /// </summary>
        public event OnChangedHandler PortChanged;

        /// <summary>
        /// 
        /// Signals chnages in the DNA
        /// 
        /// </summary>
        /// <param name="changed">The list of ports that changed</param>
        public void SignalPortChanged(List<string> changed)
        {
            // Handle the DNA changes
            if (this.DNAChanged != null)
            {
                // Do each
                foreach (string sPort in changed)
                {
                    // Call event
                    this.PortChanged?.Invoke(sPort, this.GetLocationsForPort(sPort));
                }
            }
        }
        #endregion

        #region Chain of Command
        /// <summary>
        /// 
        /// The queen
        /// 
        /// </summary>
        public BeeClass QueenBee
        {
            get
            {
                // Assume none
                BeeClass c_Ans = null;

                // Loop thru
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Get the field leader
                    BeeClass c_FieldLeader = c_Field.LeaderBee;
                    // Any?
                    if (c_FieldLeader != null)
                    {
                        // Higher?
                        if (c_Ans == null || c_FieldLeader.Id.CompareTo(c_Ans.Id) > 0) c_Ans = c_FieldLeader;
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Delayed task ID to run the Queen's ToDo
        /// 
        /// </summary>
        private string QueenTaskID { get; set; }

        /// <summary>
        /// 
        /// Calls a bee's System.FN route
        /// 
        /// </summary>
        /// <param name="bee">The bee to call</param>
        /// <param name="fn">The function to call</param>
        /// <param name="values"A store passed</param>
        /// <returns>A store of returned values</returns>
        public StoreClass FN(BeeClass bee, string fn, StoreClass values)
        {
            //
            return this.FN(bee, fn, values.SynchObject);
        }

        /// <summary>
        /// 
        /// Calls a bee's System.FN route
        /// 
        /// </summary>
        /// <param name="bee">The bee to call</param>
        /// <param name="fn">The function to call</param>
        /// <param name="values"A JSON object passed</param>
        /// <returns>A store of returned values</returns>
        public StoreClass FN(BeeClass bee, string fn, JObject values)
        {
            // Assume bad call
            StoreClass c_Ans = null;

            // Do we have a bee?
            if (bee != null)
            {
                // and the bee's URL
                string sURL = bee.URL;

                // Call
                JObject c_Resp = sURL.URLNX(values, bee.CV.NXID, fn);
                // If any, make into store
                if (c_Resp.HasValue()) c_Ans = new StoreClass(c_Resp);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Duties to be carried out by the Queen
        /// 
        /// </summary>
        private Dictionary<string, Action> QueenDuties { get; set; } = new Dictionary<string, Action>();

        /// <summary>
        /// 
        /// The callback of something the Queen has to do
        /// 
        /// </summary>
        /// <param name="cb">The callback</param>
        /// <returns>The ID</returns>
        public string AddQueenToDo(Action cb)
        {
            // MAke the ID
            string sAns = "".GUID();

            // Multi-threaded
            lock (this.QueenDuties)
            {
                // Add
                this.QueenDuties.Add(sAns, cb);
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Removes a ToDo from the Queens list
        /// 
        /// </summary>
        /// <param name="id">The ID to remove</param>
        public void RemoveQueenToDo(string id)
        {
            // Multi threaded
            lock (this.QueenDuties)
            {
                // Exists?
                if (this.QueenDuties.ContainsKey(id))
                {
                    this.QueenDuties.Remove(id);
                }
            }
        }

        /// <summary>
        /// 
        /// Off with their heads!
        /// 
        /// </summary>
        private void CheckForQueen()
        {
            // Get the queen
            BeeClass c_Queen = this.QueenBee;

            // Am I it?
            if (this.Parent.Me != null)
            {
                // Am I the queen?
                if (c_Queen == null || c_Queen.Id.IsSameValue(this.Parent.Me.CV.NXID))
                {
                    // Already ascended?
                    if (SafeThreadManagerClass.Get(this.Parent.LabelQueen) == null)
                    {
                        // YES! Off with their heads
                        this.Parent.Parent.LogInfo("Bee {0} is becoming queen soon!", this.Parent.Me.Id);

                        // Try to run the task but delay
                        this.QueenTaskID = 20.SecondsAsTimeSpan().WaitThenCall(delegate ()
                       {
                           if (SafeThreadManagerClass.StartThread(this.Parent.LabelQueen,
                               new System.Threading.ParameterizedThreadStart(QueenToDo)).HasValue())
                           {
                               // Just ascended!
                               this.Parent.Parent.LogInfo("Bee {0} is now queen", this.Parent.Me.Id);
                           }
                       });
                    }
                }
                else
                {
                    // Kill the task
                    this.QueenTaskID.KillDelayed();

                    // Nope, bow down
                    SafeThreadManagerClass.StopThread(this.Parent.LabelQueen);
                }
            }
        }

        /// <summary>
        /// 
        /// The Queens tasks
        /// 
        /// </summary>
        /// <param name="status">The thread status</param>
        private void QueenToDo(object status)
        {
            // Setup
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            //
            this.Parent.Parent.LogInfo("Started on queen's duties");

            // Forever
            while (c_Status.IsActive)
            {
                // Refresh
                this.Refresh();

                //// Are we connected?
                //if (!this.Parent.Parent.Redis.IsAvailable)
                //{
                //    // Refresh
                //    this.Refresh();
                //}

                // Get the list of required bumble bees
                ItemsClass c_Requests = new ItemsClass(this.Parent.Parent.GetAsJArray("qd_bumble"));

                // Do we not have redis?
                if (!c_Requests.Contains("redis"))
                {
                    // Do we have not redis?
                    if (!c_Requests.Contains("!redis"))
                    {
                        // Add it
                        c_Requests.Add(new ItemClass("redis"));
                    }
                }

                // Loop thru
                foreach (ItemClass c_Item in c_Requests)
                {
                    // Kill?
                    if (!c_Item.Priority.StartsWith("!"))
                    {
                        // Call
                        this.Parent.AssureDNACount(c_Item.Priority, 1, 1); ;
                    }
                }

                // Get the list of required worker bees
                c_Requests = new ItemsClass(this.Parent.Parent.GetAsJArray("qd_worker").ToList());
                // Build table of procs vs. count
                Dictionary<string, int> c_Counts = new Dictionary<string, int>();
                // Loop thru
                foreach (ItemClass c_Item in c_Requests)
                {
                    // How many
                    int iCount = 0;

                    // Handle number only
                    if (c_Item.Priority.ToInteger(-1) != -1)
                    {
                        // 
                        if (c_Item.Modifiers == null) c_Item.Modifiers = new List<string>();
                        c_Item.Modifiers.Add(c_Item.Priority);
                        c_Item.Priority = "";
                    }

                    // DO we have any options?
                    if (c_Item.ModifierCount > 0)
                    {
                        // Use first
                        iCount = c_Item.Modifiers[0].ToInteger(0);
                    }

                    // Add?
                    if (c_Counts.ContainsKey(c_Item.Priority))
                    {
                        // Replace
                        c_Counts[c_Item.Priority] = iCount;
                    }
                    else
                    {
                        // Add
                        c_Counts.Add(c_Item.Priority, iCount);
                    }
                }
                // Make a new array
                JArray c_Updated = new JArray();
                // Loop thru
                foreach (string sProc in c_Counts.Keys)
                {
                    // Get the count
                    int iCount = c_Counts[sProc];
                    // Do
                    this.Parent.AssureDNACount(HiveClass.ProcessorDNAName + "." + sProc, iCount, iCount);

                    // Add to updated list
                    if (sProc.HasValue())
                    {
                        c_Updated.Add(sProc + ":" + iCount);
                    }
                    else
                    {
                        c_Updated.Add(iCount.ToString());
                    }
                }

                // Update the list.  This will remove duplicate proc entries
                this.Parent.Parent.Set("qd_worker", c_Updated);

                // And do any orchestration
                lock (this.QueenDuties)
                {
                    // Do each
                    foreach (Action cb in this.QueenDuties.Values)
                    {
                        // Just in case
                        try
                        {
                            cb();
                        }
                        catch { }
                    }
                }

                // And do again in ten minutes
                c_Status.WaitFor(this.Parent.Parent["qd_every"].ToInteger(1).MinutesAsTimeSpan());
            }

            //
            this.Parent.Parent.LogInfo("Queen's duties have ended");
        }

        /// <summary>
        /// 
        /// Check the health of the follower, if none, check the Queen
        /// 
        /// </summary>
        /// <param name="status">The thread status</param>
        private void CheckFollower(object status)
        {
            // Setup
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Forever
            while (c_Status.IsActive)
            {
                // Wait a minute
                c_Status.WaitFor(5.MinutesAsTimeSpan());

                // Do I know myself?
                BeeClass c_Me = this.Parent.Me;
                if (c_Me != null)
                {
                    // Get the follower
                    BeeClass c_Follower = c_Me.FollowerBee;
                    // If none, let's check the queen
                    if (c_Follower == null) c_Follower = c_Me.QueenBee;

                    // Follower?
                    if (c_Follower != null)
                    {
                        // Do not check on ourselves
                        if (!c_Follower.Id.IsSameValue(c_Me.Id))
                        {
                            this.Parent.Parent.LogVerbose("Checking on {0}", c_Follower.Id);

                            // Check
                            switch (c_Follower.IsAlive())
                            {
                                case BeeClass.States.Dead:
                                    // Something happened, kill it
                                    c_Follower.Kill(BeeClass.KillReason.DeadAtCheck);
                                    break;

                                case BeeClass.States.Hiccup:
                                    // Not answering
                                    this.Parent.Parent.LogVerbose("Bee {0} is having issues", c_Follower.Id);
                                    break;

                                case BeeClass.States.Alive:
                                    // Alive
                                    this.Parent.Parent.LogVerbose("Bee {0} is alive", c_Follower.Id);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}