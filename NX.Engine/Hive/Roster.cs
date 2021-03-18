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
using System.Threading;
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

        /// <summary>
        /// 
        /// The bees
        /// 
        /// </summary>
        public List<BeeClass> Bees
        {
            get
            {
                // Start with none
                List<BeeClass> c_Ans = new List<BeeClass>();

                // Loop thru
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Add
                    c_Ans.AddRange(c_Field.Bees.Values); ;
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Has a refresh taken place?
        /// 
        /// </summary>
        public bool HasSetup
        {
            get
            {
                // Assume none
                bool bAns = this.Parent.Fields.Values.Count > 0;

                // Loop thru
                foreach (FieldClass c_Field in this.Parent.Fields.Values)
                {
                    // Get the URLs
                    if (!c_Field.HasSetup) bAns = false;
                }

                return bAns;
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
            if (bee != null)
            {
                bee.Field.AddBee(bee);
            }
        }

        /// <summary>
        /// 
        /// Removes a bee according to the Docker ID
        /// 
        /// </summary>
        /// <param name="dockerid">The Docker ID</param>
        public void Remove(string dockerid, BeeClass.KillReason reason)
        {
            // Assume not here
            BeeClass c_Bee = this.GetByDockerID(dockerid);

            // Do we know it?
            if (c_Bee != null)
            {
                // Remove
                this.Remove(c_Bee, reason);
            }
        }

        /// <summary>
        /// 
        /// Removes a bee from the roster
        /// 
        /// </summary>
        /// <param name="bee">The bee to delete</param>
        public void Remove(BeeClass bee, BeeClass.KillReason reason)
        {
            // Remove
            if (bee != null)
            {
                bee.Field.RemoveBee(bee, reason);
            }
        }

        /// <summary>
        /// 
        /// Returns a list of DNAs
        /// 
        /// </summary>
        /// <returns>The list of all DNAs</returns>
        public List<string> GetDNAs()
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Loop thru
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Get the IDs
                List<string> c_BIDs = new List<string>(c_Field.Bees.Keys);
                // Loop thru
                foreach (string sBID in c_BIDs)
                {
                    // Get the bee
                    BeeClass c_Bee = c_Field.Bees[sBID];
                    // DNA already seen?
                    if (!c_Ans.Contains(c_Bee.CV.DNA))
                    {
                        // Add
                        c_Ans.Add(c_Bee.CV.DNA);
                    }
                }
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
        public List<BeeClass> GetBeesForDNA(string dna)
        {
            // Assume none
            List<BeeClass> c_Ans = new List<BeeClass>();

            // Loop thru
            foreach (FieldClass c_Field in this.Parent.Fields.Values)
            {
                // Get the IDs
                List<string> c_BIDs = new List<string>(c_Field.Bees.Keys);
                // Loop thru
                foreach (string sBID in c_BIDs)
                {
                    // Get the bee
                    BeeClass c_Bee = c_Field.Bees[sBID];
                    // Is this the DNA we are looking for?
                    if (c_Bee.CV.DNA.IsSameValue(dna))
                    {
                        // Add
                        c_Ans.Add(c_Bee);
                    }
                }
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
                // Get the IDs
                List<string> c_BIDs = new List<string>(c_Field.Bees.Keys);
                // Loop thru
                foreach (string sBID in c_BIDs)
                {
                    // Get the bee
                    BeeClass c_Bee = c_Field.Bees[sBID];

                    if (c_Bee.CV.DNA.IsSameValue(DNA))
                    {
                        // Loop thru tickle Areas
                        foreach (TickleAreaClass c_TA in c_Bee.GetTickleAreas())
                        {
                            // Already seen?
                            if (!c_Ans.Contains(c_TA.Location))
                            {
                                // Add
                                c_Ans.Add(c_TA.Location);
                            }
                        }
                    }
                }
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
            this.Parent.Parent.LogVerbose("Refreshing hive {0}".FormatString(this.Parent.Name));

            // Save the queen
            this.PreviousQueenBee = this.QueenBee;

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
                if (this.Parent.Roster.MeBee == null)
                {
                    // 
                    this.Parent.Parent.LogInfo("Creating a ghost bee as {0}".FormatString(this.Parent.Parent.ID));
                    // Make me
                    this.Parent.Roster.MeBee = new BeeClass(this.Parent.Fields.Values.First(),
                                                    this.Parent.Parent.ID,
                                                    BeeClass.Types.Ghost);
                    // Add to roster
                    this.Add(this.Parent.Roster.MeBee);
                }
            }

            // If no previous queen, use creator
            if (this.PreviousQueenBee == null)
            {
                this.PreviousQueenBee = this.Get(this.Parent.Parent["creator"]);
            }

            // Queen changed?
            if (this.QueenBee != null)
            {
                // Get queen
                BeeClass c_Queen = this.QueenBee;

                // Different than previous queen
                if (this.PreviousQueenBee == null || !this.PreviousQueenBee.IsSameAs(c_Queen))
                {
                    //
                    this.Parent.Parent.LogInfo("Asking {0} to be queen".FormatString(c_Queen.Id));

                    // Ascend
                    c_Queen.Handshake(HiveClass.States.Ascending);
                }
            }

            // Do the queens business
            this.CheckForQueen();

            // Tell user
            this.Parent.Parent.LogVerbose("End of refresh for hive {0}".FormatString(this.Parent.Name));
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
            // Do each
            foreach (string sDNA in changed)
            {
                try
                {
                    // Call event
                    this.DNAChanged?.Invoke(sDNA, this.GetLocationsForDNA(sDNA));
                }
                catch { }
            }
        }
        #endregion

        #region Me
        /// <summary>
        /// 
        /// Returns the running instance as a bee
        /// 
        /// </summary>
        public BeeClass MeBee { get; internal set; }

        /// <summary>
        /// 
        /// Am I the queen?
        /// 
        /// </summary>
        public bool IsQueen
        {
            get
            {
                return this.QueenBee != null &&
                            this.MeBee != null &&
                            this.QueenBee.IsSameAs(this.MeBee);
            }
        }
        #endregion

        #region Chain of Command
        /// <summary>
        /// 
        /// The previous queen, if any
        /// 
        /// </summary>
        private BeeClass PreviousQueenBee { get; set; }

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
                    BeeClass c_FieldLeader = c_Field.LeaderBee();
                    // Any?
                    if (c_FieldLeader != null)
                    {
                        // Higher?
                        if (c_Ans == null || c_FieldLeader.Id.CompareTo(c_Ans.Id) > 0) c_Ans = c_FieldLeader;
                    }
                }

                // Assume it is me
                if (c_Ans == null && !"".InContainer()) c_Ans = this.MeBee;

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
        private NamedListClass<Action> QueenDuties { get; set; } = new NamedListClass<Action>();

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
            // Kill
            this.QueenDuties.Remove(id);
        }

        /// <summary>
        /// 
        /// Off with their heads!
        /// 
        /// </summary>
        public void CheckForQueen(bool kill = false)
        {
            // Get the queen
            BeeClass c_Queen = this.QueenBee;

            this.Parent.Parent.LogVerbose("CheckForQueen");

            // Am I it?
            if (this.Parent.Roster.MeBee != null)
            {
                // Am I the queen?
                if (!kill && (c_Queen == null ||
                                c_Queen.Id.IsSameValue(this.Parent.Roster.MeBee.CV.NXID) ||
                                this.Parent.State == HiveClass.States.Ascending))
                {
                    // Already ascended?
                    if (SafeThreadManagerClass.Get(this.Parent.LabelQueen) == null)
                    {
                        //
                        this.Parent.Parent.LogInfo("Checking bee {0} as possible queen".FormatString(this.Parent.Roster.MeBee.Id));

                        // Mark ourselves
                        this.Parent.State = HiveClass.States.Ascending;

                        // Assue none
                        string sCreator = "";
                        // Do we have a queen?
                        if (this.PreviousQueenBee == null)
                        {
                            // Do we have a creator?
                            sCreator = this.Parent.Parent["creator"];
                        }
                        else
                        {
                            // Use queen bee
                            sCreator = this.PreviousQueenBee.Id;
                        }

                        // Do we have a creator?  And is it aanother bee?
                        if (sCreator.HasValue() && !sCreator.IsSameValue(this.MeBee.Id))
                        {
                            // Get creator as bee
                            BeeClass c_Bee = this.Get(sCreator);
                            // Doo we have a creator
                            if (c_Bee != null)
                            {
                                //
                                this.Parent.Parent.LogInfo("Asking bee {0} to relinquish".FormatString(sCreator));

                                // Tell creator to stand down
                                if (this.Parent.State == HiveClass.States.Ascending)
                                {
                                    // Get permission from previous queen
                                    if (!c_Bee.Handshake(this.MeBee.Id, true))
                                    {
                                        // Reset
                                        this.Parent.State = HiveClass.States.Bee;
                                    }
                                }
                            }
                        }

                        // Are we still on track?
                        if (this.Parent.State == HiveClass.States.Ascending)
                        {
                            // YES! Off with their heads
                            this.Parent.Parent.LogInfo("Bee {0} is becoming queen soon!".FormatString(this.Parent.Roster.MeBee.Id));

                            // Try to run the task but delay
                            this.QueenTaskID = 20.SecondsAsTimeSpan().WaitThenCall(delegate ()
                           {
                               if (SafeThreadManagerClass.StartThread(this.Parent.LabelQueen,
                                   new System.Threading.ParameterizedThreadStart(QueenToDo)).HasValue())
                               {
                                   // Just ascended!
                                   this.Parent.Parent.LogInfo("Bee {0} is now queen".FormatString(this.Parent.Roster.MeBee.Id));
                               }
                           });
                        }
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
        
        private bool QueenDutiesOnce { get; set; }

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

            // Did we get knocked off?
            if (this.Parent.State == HiveClass.States.Ascending ||
                this.Parent.State == HiveClass.States.Queen)
            {
                // Mark oursleves
                this.Parent.State = HiveClass.States.Queen;

                //
                this.Parent.Parent.LogVerbose("Started on queen's duties");

                // Tell the world that the queen changed
                this.Parent.SignalQueenChange();

                // Until someone replaces us
                while (c_Status.IsActive && this.Parent.State == HiveClass.States.Queen)
                {
                    // Mark oursleves
                    this.Parent.State = HiveClass.States.InQueenDuties;

                    // Refresh
                    this.Refresh();

                    // Get the list
                    List<string> c_Req = this.Parent.Parent.GetAsJArray("qd_uses").ToList();
                    // Must have nginx
                    if (!c_Req.Contains("Proc.NginX")) c_Req.Add("Proc.NginX");
                    // Get the list of required items
                    ItemsClass c_Uses = new ItemsClass(c_Req);
                    // Add Traefik
                    if (this.Parent.Parent.TraefikHive.HasValue())
                    {
                        c_Uses.Add(new ItemClass("Proc.Traefik"));
                    }
                    // Loop thru
                    foreach (ItemClass c_Item in c_Uses)
                    {
                        // Use
                        this.Parent.Parent.Use(c_Item.Priority);
                    }

                    // Assure NginX
                    this.Parent.Parent.AddToArray("qd_bumble", "nginx");
                    // Get list
                    c_Req = this.Parent.Parent.GetAsJArray("qd_bumble").ToList().Unique();
                    // Get the list of required bumble bees
                    ItemsClass c_Requests = new ItemsClass(c_Req);

                    this.Parent.Parent.LogVerbose("qd_bumble is {0}".FormatString(c_Req.Join(", ")));

                    // Loop thru
                    foreach (ItemClass c_Item in c_Requests)
                    {
                        string sGenome = c_Item.Priority;

                        // Kill?
                        if (!sGenome.StartsWith("!"))
                        {
                            // Do we need to recycle?
                            if(!this.QueenDutiesOnce && this.Parent.GenomeSource(sGenome).HasValue())
                            {
                                // Remove genome
                                this.Parent.RemoveGenome(sGenome);
                                this.Refresh();
                            }

                            // Call
                            this.Parent.AssureDNACount(sGenome, 1, 1); ;
                        }
                    }

                    // Reset
                    this.QueenDutiesOnce = true;

                    // Get the list of required worker bees
                    c_Req = this.Parent.Parent.GetAsJArray("qd_worker").ToList();
                    // Have at least one
                    if (c_Req.Count == 0) c_Req.Add("");
                    // Parse
                    c_Requests = new ItemsClass(c_Req);
                    // Build table of procs vs. count
                    NamedListClass<int> c_Counts = new NamedListClass<int>();
                    // Loop thru
                    foreach (ItemClass c_Item in c_Requests)
                    {
                        // How many
                        int iCount = 0;

                        // Handle number only
                        if (c_Item.Priority.ToInteger(-1) != -1)
                        {
                            // 
                            c_Item.AddOption(c_Item.Priority);
                            c_Item.Priority = "";
                        }

                        // DO we have any options?
                        if (c_Item.Option.HasValue())
                        {
                            // Use first
                            iCount = c_Item.Option.ToInteger(0);
                        }

                        // Is it a preocessor?
                        if(!c_Item.Priority.HasValue() && iCount < 1)
                        {
                            iCount = 1;
                        }

                        // Add
                        c_Counts[c_Item.Priority] = iCount;
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

                    // Mark oursleves
                    if (this.Parent.State == HiveClass.States.InQueenDuties)
                    {
                        this.Parent.State = HiveClass.States.Queen;
                    }

                    this.Parent.Parent.LogVerbose("End of queen's duties cycle");

                    // And do again in ten minutes
                    c_Status.WaitFor(this.Parent.Parent["qd_every"].ToInteger(1).MinutesAsTimeSpan());
                }

                // Mark oursleves
                this.Parent.State = HiveClass.States.Bee;

                // Tell the world
                this.Parent.SignalQueenChange();

                // Get the queen
                BeeClass c_Queen = this.QueenBee;

                // Tell new queen to ascend
                if (c_Queen != null)
                {
                    //
                    this.Parent.Parent.LogInfo("Asking {0} to be queen".FormatString(c_Queen.Id));

                    //
                    if (c_Queen.Handshake(HiveClass.States.Ascending))
                    {
                        //
                        this.Parent.Parent.LogInfo("Queen is now {0}".FormatString(c_Queen.Id));
                        //
                        this.CheckForQueen(true);
                    }
                    else
                    {
                        // Something went wrong, make ourselves queen again
                        this.Parent.State = HiveClass.States.Ascending;
                        // And again
                        this.CheckForQueen();
                    }
                }
                else
                {
                    // No new qeen?
                    this.Parent.Parent.LogInfo("No queen is available");
                }

                // Flag
                this.QueenDutiesOnce = true;

                //
                this.Parent.Parent.LogInfo("Queen's duties have ended");
            }
            else
            {
                this.Parent.Parent.LogInfo("Queen status is {0}, expected queen".FormatString(this.Parent.State));

                // Start ascension thread
                SafeThreadManagerClass.StartThread("".GUID(), new ParameterizedThreadStart(AscendThread));
            }
        }

        private void AscendThread(object status)
        {
            //
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Celan up
            SafeThreadManagerClass.StopThread(this.Parent.LabelQueen);

            // Something went wrong, make ourselves queen
            this.Parent.State = HiveClass.States.Ascending;
            // And again
            this.CheckForQueen();

            //
            c_Status.End();
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
                BeeClass c_Me = this.Parent.Roster.MeBee;
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
                        if (!c_Follower.IsSameAs(c_Me))
                        {
                            this.Parent.Parent.LogVerbose("Checking on {0}".FormatString(c_Follower.Id));

                            // Check
                            switch (c_Follower.IsAlive())
                            {
                                case BeeClass.States.Dead:
                                    // Was the follower the queen?
                                    if (this.QueenBee != null && c_Follower.IsSameAs(this.QueenBee))
                                    {
                                        //
                                        this.Parent.Parent.LogInfo("Asking {0} to be queen".FormatString(c_Follower.FollowerBee.Id));

                                        // Ascend the follower of the queen
                                        if (!c_Follower.FollowerBee.Handshake(HiveClass.States.Ascending))
                                        {
                                            // Something went wrong, make ourselves queen
                                            this.Parent.State = HiveClass.States.Ascending;
                                            // And again
                                            this.CheckForQueen();
                                        }
                                    }
                                    // Something happened, kill it
                                    c_Follower.Kill(BeeClass.KillReason.DeadAtCheck);
                                    break;

                                case BeeClass.States.Hiccup:
                                    // Not answering
                                    this.Parent.Parent.LogVerbose("Bee {0} is having issues".FormatString(c_Follower.Id));
                                    break;

                                case BeeClass.States.Alive:
                                    // Alive
                                    this.Parent.Parent.LogVerbose("Bee {0} is alive".FormatString(c_Follower.Id));
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