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
using System.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A URl that can be connect to
    /// 
    /// </summary>
    public class TickleAreaClass : ChildOfClass<BeeClass>
    {
        #region Constructor
        public TickleAreaClass(BeeClass bee, string publicport, string privateport, string url = null)
            : base(bee)
        {
            // From parameters
            this.PublicPort = publicport;
            this.PrivatePort = privateport;

            // Get the field location
            string sURL = url;
            // If none, get from field
            if (!sURL.HasValue())
            {
                sURL = this.Parent.Parent.Parent.GetFieldURL(this.Field);
            }
            // One available?
            if (sURL.HasValue())
            {
                // Does it have a port
                int iPos = sURL.LastIndexOf(":");
                // Remove it
                if (iPos != -1) sURL = sURL.Substring(0, iPos);
                // Add public port
                this.Location = sURL + ":" + this.PublicPort;
            }

            // Make the DNA
            string sDNA = this.Parent.CV.DNA;
            // Process?
            if (sDNA.IsSameValue(HiveClass.ProcessorDNAName))
            {
                // Add the process
                sDNA += "." + this.Parent.CV.Proc;
            }

            // Save
            this.DNA = sDNA;
        }
        #endregion

        #region Properties
        public string BeeId { get { return this.Parent.CV.Id; } }
        public string Field { get { return this.Parent.CV.Field; } }
        public string DNA { get; private set; }

        public string Location { get; private set; }
        public string PublicPort { get; private set; }
        public string PrivatePort { get; private set; }

        public bool IsAvailable { get { return this.Location.HasValue(); } }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.DNA + ":" + this.PublicPort + ":" + this.PrivatePort;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// A list of TickleAreas for a given subject
    /// 
    /// </summary>
    public class TickleAreaListClass : List<TickleAreaClass>
    {
        #region Constructor
        public TickleAreaListClass()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Add an TickleArea if it is not in the list already
        /// 
        /// </summary>
        /// <param name="ep">The TickleArea</param>
        /// <returns>True if the TickleArea was added</returns>
        public bool AddTickleArea(TickleAreaClass ep)
        {
            // Assume no
            bool bAns = false;

            // There already?
            if (!this.Exists(x => x.Location.IsSameValue(ep.Location)))
            {
                // No, add
                this.Add(ep);
                // And reset
                bAns = true;
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Removes an TickleArea if it is in the list already
        /// 
        /// </summary>
        /// <param name="ep">The TickleArea</param>
        /// <returns>True if the TickleArea was added</returns>
        public bool RemoveTickleArea(TickleAreaClass ep)
        {
            // Assume no
            bool bAns = false;

            // There already?
            if (this.Exists(x => x.Location.IsSameValue(ep.Location)))
            {
                // Remove
                this.Remove(this.Find(x => x.Location.IsSameValue(ep.Location)));

                // And reset
                bAns = true;
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Returns the list of URLS
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetLocations()
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Loop thru
            foreach (TickleAreaClass c_EP in this)
            {
                // Add
                c_Ans.Add(c_EP.Location);
            }

            return c_Ans;
        }


        /// <summary>
        /// 
        /// Returns the list of bees
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BeeClass> GetBees()
        {
            // Assume none
            List<BeeClass> c_Ans = new List<BeeClass>();

            // Loop thru
            foreach (TickleAreaClass c_EP in this)
            {
                // Add
                c_Ans.Add(c_EP.Parent);
            }

            return c_Ans;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// A table of TickleAreas grouped by a subject
    /// 
    /// </summary>
    public class TickleAreaTableClass : Dictionary<string, TickleAreaListClass>
    {
        #region Constructor
        public TickleAreaTableClass()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Adds an TickleArea by a given subject
        /// 
        /// </summary>
        /// <param name="key">The subject</param>
        /// <param name="ep">The TickleArea</param>
        /// /// <returns>True if the TickleArea was added</returns>
        public bool AddTickleArea(string key, TickleAreaClass ep)
        {
            // Make room
            if (!this.ContainsKey(key)) this.Add(key, new TickleAreaListClass());

            // Add
            return this[key].AddTickleArea(ep);
        }

        /// <summary>
        /// 
        /// Removes an TickleArea by a given subject
        /// 
        /// </summary>
        /// <param name="key">The subject</param>
        /// <param name="ep">The TickleArea</param>
        /// /// <returns>True if the TickleArea was added</returns>
        public bool RemoveTickleArea(string key, TickleAreaClass ep)
        {
            // Assume not
            bool bAns = false;

            // A known subject
            if (this.ContainsKey(key))
            {
                // Remove
                bAns = this[key].RemoveTickleArea(ep);
            }

            return bAns;
        }

        /// <summary>
        /// 
        /// Gets the list from a given key
        /// 
        /// </summary>
        /// <param name="key">The key to lookup</param>
        /// <returns>The list in any</returns>
        public TickleAreaListClass Get(string key)
        {
            // Assume none
            TickleAreaListClass c_Ans = null;

            // Any?
            if (this.ContainsKey(key)) c_Ans = this[key];

            return c_Ans;
        }
        #endregion
    }

    public class TickleAreaMapClass : ChildOfClass<FieldClass>
    {
        #region Constructor
        public TickleAreaMapClass(FieldClass field)
            : base(field)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The list of bees
        /// 
        /// </summary>
        public Dictionary<string, BeeClass> Bees { get; private set; } = new Dictionary<string, BeeClass>();

        /// <summary>
        /// 
        /// A list of just worker bees
        /// 
        /// </summary>
        private List<string> WorkerBeeIDs { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// The DNAs
        /// 
        /// </summary>
        private TickleAreaTableClass DNAs { get; set; } = new TickleAreaTableClass();

        /// <summary>
        /// 
        /// The ports
        /// 
        /// </summary>
        private TickleAreaTableClass Ports { get; set; } = new TickleAreaTableClass();

        /// <summary>
        /// 
        /// List used at refresh
        /// 
        /// </summary>
        private List<string> RefreshList { get; set; }

        /// <summary>
        /// 
        /// Get the bee count
        /// 
        /// </summary>
        public int BeeCount
        {
            get { return this.Bees.Count; }
        }
        #endregion

        #region Bees
        /// <summary>
        /// 
        /// Adds a bee to the field
        /// 
        /// </summary>
        /// <param name="bee">The bee to be added</param>
        public void Add(BeeClass bee)
        {
            // Must have real a bee
            if (bee != null && !bee.IsGhost)
            {
                // Already there?
                if (!this.Bees.ContainsKey(bee.Id))
                {
                    // Do
                    this.Bees.Add(bee.Id, bee);

                    // Worker bee?
                    if (bee.CV.DNA.StartsWith(HiveClass.ProcessorDNAName))
                    {
                        // Add
                        if (!this.WorkerBeeIDs.Contains(bee.Id)) this.WorkerBeeIDs.Add(bee.Id);
                    }

                    // If this is us, save
                    if (bee.CV.NXID.IsSameValue(this.Parent.Parent.Parent.ID))
                    {
                        // Save
                        this.Parent.Parent.Me = bee;
                        //
                        this.Parent.Parent.Parent.LogInfo("I am bee {0}", bee.Id);

                        // Set the loopback URL
                        this.Parent.Parent.Parent.LoopbackURL = bee.URL;
                    }

                    // The change lists
                    List<string> c_DNAsChanged = new List<string>();
                    List<string> c_PortsChanged = new List<string>();

                    // Get the TickleAreas
                    List<TickleAreaClass> c_EPS = bee.TickleAreas;

                    // Make room for DNAs
                    List<string> c_DNAs = new List<string>();

                    // Loop thru
                    foreach (TickleAreaClass c_EP in c_EPS)
                    {
                        // Save
                        c_DNAs.Add(c_EP.ToString());

                        // Add it to the DNAs
                        if (this.DNAs.AddTickleArea(c_EP.DNA, c_EP))
                        {
                            // Changed so add it to the mix
                            if (!c_DNAsChanged.Contains(c_EP.DNA)) c_DNAsChanged.Add(c_EP.DNA);
                        }

                        // And the ports
                        if (this.Ports.AddTickleArea(c_EP.PrivatePort, c_EP))
                        {
                            // Changed so add it to the mix
                            if (!c_PortsChanged.Contains(c_EP.PrivatePort)) c_PortsChanged.Add(c_EP.PrivatePort);
                        }
                    }

                    // Handle the DNA changes
                    this.Parent.Parent.Roster.SignalDNAChanged(c_DNAsChanged);

                    // Handle the DNA changes
                    this.Parent.Parent.Roster.SignalPortChanged(c_PortsChanged);

                    string sMsg = "";
                    if (c_DNAs.Count > 0)
                    {
                        sMsg = "as " + c_DNAs.Join(", ");
                    }

                    // Tell world
                    this.Parent.Parent.Parent.LogInfo("Added bee {0} to field {1} {2}", bee.Id, bee.Field.Name, sMsg);

                    // Now the other bees
                    if (this.Parent.Parent.Synch != null)
                    {
                        this.Parent.Parent.Synch.SendMessage(HiveClass.MessengerMClass,
                            "field", bee.Field.Name,
                            "id", bee.DockerID,
                            "state", "isalive"
                            );
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// Removes a bee
        /// 
        /// </summary>
        /// <param name="bee">The bee to removed</param>
        public void Remove(BeeClass bee)
        {
            // Must have real a bee
            if (bee != null && !bee.IsGhost)
            {
                // Do we know it?
                if (this.Bees.ContainsKey(bee.Id))
                {
                    // Do
                    this.Bees.Remove(bee.Id);

                    // Worker bee?
                    if (bee.CV.DNA.StartsWith(HiveClass.ProcessorDNAName))
                    {
                        // Add
                        if (this.WorkerBeeIDs.Contains(bee.Id)) this.WorkerBeeIDs.Remove(bee.Id);
                    }

                    // The change lists
                    List<string> c_DNAsChanged = new List<string>();
                    List<string> c_PortsChanged = new List<string>();

                    // Get the TickleAreas
                    List<TickleAreaClass> c_EPS = bee.TickleAreas;

                    // Loop thru
                    foreach (TickleAreaClass c_EP in c_EPS)
                    {
                        // Add it to the DNAs
                        if (this.DNAs.RemoveTickleArea(c_EP.DNA, c_EP))
                        {
                            // Changed so add it to the mix
                            if (!c_DNAsChanged.Contains(c_EP.DNA)) c_DNAsChanged.Add(c_EP.DNA);
                        }

                        // And the ports
                        if (this.Ports.RemoveTickleArea(c_EP.PrivatePort, c_EP))
                        {
                            // Changed so add it to the mix
                            if (!c_PortsChanged.Contains(c_EP.PrivatePort)) c_PortsChanged.Add(c_EP.PrivatePort);
                        }
                    }

                    // Handle the DNA changes
                    this.Parent.Parent.Roster.SignalDNAChanged(c_DNAsChanged);

                    // Handle the DNA changes
                    this.Parent.Parent.Roster.SignalPortChanged(c_PortsChanged);

                    // Tell user
                    this.Parent.Parent.Parent.LogInfo("Removed bee {0} from field {1}", bee.Id, bee.Field.Name);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns a bee from its ID
        /// 
        /// </summary>
        /// <param name="id">The bee's id</param>
        /// <returns>The bee if any</returns>
        public BeeClass GetBee(string id)
        {
            // Assume none
            BeeClass c_Ans = null;

            // Any?
            if (this.Bees.ContainsKey(id)) c_Ans = this.Bees[id];

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns a bee from its Docker ID
        /// 
        /// </summary>
        /// <param name="id">The bee's ID</param>
        /// <returns>The bee if any</returns>
        public BeeClass GetBeeByDockerID(string dockerid)
        {
            // Assume none
            BeeClass c_Ans = null;

            // Loop thru
            foreach (BeeClass c_Bee in this.Bees.Values)
            {
                // Does the Docker ID match
                if (c_Bee.DockerID.IsSameValue(dockerid))
                {
                    // Save
                    c_Ans = c_Bee;
                    // Only one
                    break;
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// A list of all IDs
        /// 
        /// </summary>
        private List<string> AllBeeIDs
        {
            get { return new List<string>(this.Bees.Keys); }
        }

        /// <summary>
        /// 
        /// Returns the bee that leads a given bee
        /// 
        /// </summary>
        /// <param name="bee">The given bee</param>
        /// <returns></returns>
        public BeeClass LeaderBee(BeeClass bee = null)
        {
            // Assume none
            BeeClass c_Ans = null;

            // The ID of the selected bee
            string sID = null;

            // Get the list
            List<string> c_IDs = this.WorkerBeeIDs;
            // Any?
            if (c_IDs.Count > 0)
            {
                // Do we have a bee to target?
                if (bee == null)
                {
                    // Get the largest ID
                    sID = c_IDs.Max();
                }
                else
                {
                    // Get one after
                    sID = c_IDs.FindLast(x => x.CompareTo(bee.CV.NXID) > 0);
                }
            }

            // Do we have an ID?
            if (sID.HasValue())
            {
                // Get the bee
                c_Ans = this.GetBee(sID);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Return the bee that follows a given bee
        /// </summary>
        /// <param name="bee">The given bee</param>
        /// <returns></returns>
        public BeeClass FollowerBee(BeeClass bee)
        {
            // Assume none
            BeeClass c_Ans = null;

            // The ID of the selected bee
            string sID = null;

            // Get the list
            List<string> c_IDs = this.WorkerBeeIDs;
            // Any?
            if (c_IDs.Count > 0)
            {
                // Do we have a bee to target?
                if (bee == null)
                {
                    // Get the smallest ID
                    sID = c_IDs.Min();
                }
                else
                {
                    // Get one after
                    sID = c_IDs.FindLast(x => x.CompareTo(bee.Id) < 0);
                }
            }

            // Do we have an ID?
            if (sID.HasValue())
            {
                // Get the bee
                c_Ans = this.GetBee(sID);
            }

            return c_Ans;
        }
        #endregion

        #region Map
        /// <summary>
        /// 
        /// Returns the URLs for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns></returns>
        public List<string> GetLocationsForDNA(string DNA)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Do we have a DNA?
            if (this.DNAs.ContainsKey(DNA))
            {
                // Get the URLs
                c_Ans.AddRange(this.DNAs[DNA].GetLocations());
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the bees for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns></returns>
        public List<BeeClass> GetBeesForDNA(string DNA)
        {
            // Assume none
            List<BeeClass> c_Ans = new List<BeeClass>();

            // Do we have a DNA?
            if (this.DNAs.ContainsKey(DNA))
            {
                // Get the URLs
                c_Ans.AddRange(this.DNAs[DNA].GetBees());
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the URLs for a given port
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <returns></returns>
        public List<string> GetLocationsForPort(string port)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Do we have a DNA?
            if (this.Ports.ContainsKey(port))
            {
                // Get the URLs
                c_Ans.AddRange(this.DNAs[port].GetLocations());
            }

            return c_Ans;
        }


        /// <summary>
        /// 
        /// Returns the bee for a given port
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <returns></returns>
        public List<BeeClass> GetBeesForPort(string port)
        {
            // Assume none
            List<BeeClass> c_Ans = new List<BeeClass>();

            // Do we have a DNA?
            if (this.Ports.ContainsKey(port))
            {
                // Get the URLs
                c_Ans.AddRange(this.DNAs[port].GetBees());
            }

            return c_Ans;
        }
        #endregion

        #region Refresh
        /// <summary>
        /// 
        /// Starts a refresh cycle
        /// 
        /// </summary>
        public void StartRefresh()
        {
            // make the list
            this.RefreshList = new List<string>(this.Bees.Keys);
        }

        /// <summary>
        /// 
        /// Saw a bee during a refresh cycle
        /// 
        /// </summary>
        /// <param name="bee">The bee seen</param>
        public void SawBee(BeeClass bee)
        {
            // Was it there?
            if (this.RefreshList.Contains(bee.Id))
            {
                //
                this.RefreshList.Remove(bee.Id);
            }
            else
            {
                // Add the bee
                this.Add(bee);
            }
        }

        /// <summary>
        /// 
        /// Ends the refresh cycle
        /// 
        /// </summary>
        public void EndRefresh()
        {
            // Tell of any zombies
            this.Parent.Parent.Parent.LogVerbose("{0} zombie(s) seen", this.RefreshList.Count);

            // What is left over are zombies
            foreach (string sZombie in this.RefreshList)
            {
                // Get the bee
                BeeClass c_Zombie = this.GetBee(sZombie);
                // Any?
                if (c_Zombie != null)
                {
                    // Kill
                    c_Zombie.Kill(BeeClass.KillReason.Zombie);
                }
            }
        }
        #endregion
    }
}