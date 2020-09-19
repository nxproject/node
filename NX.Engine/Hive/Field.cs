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

using System.Collections.Generic;
using System.Text;
using System.Linq;

using Docker.DotNet.Models;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A field where a hive's bees can wander
    /// 
    /// </summary>
    public class FieldClass : ChildOfClass<HiveClass>
    {
        #region Constructor
        public FieldClass(HiveClass hive, string ip)
            : base(hive)
        {
            // Parse
            ItemClass c_Loc = new ItemClass(ip);

            // The URL
            string sURL = "";

            // Has a value?
            if (c_Loc.Value.HasValue())
            {
                this.Name = c_Loc.Key;
                sURL = c_Loc.Value;
            }
            else
            {
                this.Name = c_Loc.Key.MD5HashString();
                sURL = c_Loc.Key;
            }

            // Remove trailing :
            if (sURL.EndsWith(":"))
            {
                sURL = sURL.Substring(0, sURL.Length - 1);
            }

            //
            this.Initialize(sURL);
        }

        public FieldClass(HiveClass hive, string name, string url)
           : base(hive)
        {
            this.Name = name;

            //
            this.Initialize(url);
        }

        private void Initialize(string url)
        {
            // The map
            this.Bees = new NamedListClass<BeeClass>();

            // Virtual?
            if (url.HasValue())
            {
                // Are we localhost in a container?
                if (url.IsSameValue("localhost") && "".InContainer())
                {
                    // Use internal
                    url = "host.docker.internal";
                }

                // Add port if needed
                if (!url.Contains(":")) url += ":2375";

                // Make the interface
                this.DockerIF = new DockerIFClass(this, url);

                // Save
                this.URL = url;

                //
                this.Parent.Parent.LogInfo("Cleaning up genomes");
                // Cleanup
                DockerIFClass c_Client = this.DockerIF;
                // Any?
                if (c_Client != null) c_Client.CleanupImages();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name of the field
        /// 
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 
        /// The URL where it's Docker services can be reached
        /// 
        /// </summary>
        public string URL { get; private set; }

        /// <summary>
        /// 
        /// Returns true if we can ping it
        /// 
        /// </summary>
        public bool IsAvailable
        {
            get { return this.DockerIF != null && this.DockerIF.IsAlive; }
        }

        /// <summary>
        /// 
        /// True if the field does not exist inside 
        /// the hive's hardware
        /// 
        /// </summary>
        public bool IsVirtual { get { return !this.URL.HasValue(); } }

        /// <summary>
        /// 
        /// An internal roster of bees
        /// 
        /// </summary>
        public NamedListClass<BeeClass> Bees { get; private set; } = new NamedListClass<BeeClass>();

        //public TickleAreaMapClass Bees { get; private set; }

        //private DockerIFClass IClient { get; set; }
        public DockerIFClass DockerIF { get; private set; }

        /// <summary>
        /// 
        /// A list of just worker bees
        /// 
        /// </summary>
        private List<string> WorkerBeeIDs { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// The bee count
        /// 
        /// </summary>
        public int BeeCount
        {
            get { return this.Bees.Count; }
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

        /// <summary>
        /// 
        /// Has a refresh taken place?
        /// 
        /// </summary>
        public bool HasSetup { get; private set; }

        /// <summary>
        /// 
        /// List used at refresh
        /// 
        /// </summary>
        private List<string> RefreshList { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Dumps the field
        /// 
        /// </summary>
        /// <returns>The dump</returns>
        public override string ToString()
        {
            //
            StringBuilder c_Buffer = new StringBuilder();

            c_Buffer.AppendLine("===========================================");
            c_Buffer.AppendLine("Name: {0}".FormatString(this.Name));
            c_Buffer.AppendLine("Bees:");

            foreach (BeeClass c_Bee in this.Bees.Values)
            {
                c_Buffer.AppendLine(c_Bee.ToString());
            }

            c_Buffer.AppendLine("===========================================");

            return c_Buffer.ToString();
        }

        /// <summary>
        /// 
        /// Gets a bee from its ID
        /// 
        /// </summary>
        /// <param name="id">The bee's ID</param>
        /// <returns>The bee if any</returns>
        public BeeClass GetBee(string id)
        {
            // Assume none
            BeeClass c_Ans = null;

            // Do we have it?
            if (this.Bees.ContainsKey(id))
            {
                // Get
                c_Ans = this.Bees[id];
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets a bee from its Docker ID
        /// 
        /// </summary>
        /// <param name="id">The Docker ID</param>
        /// <returns>The bee if any</returns>
        public BeeClass GetByDockerID(string id)
        {
            // Assume none
            BeeClass c_Ans = null;

            // Loop thru
            foreach (BeeClass c_Bee in this.Bees.Values)
            {
                // Do we have it?
                if (c_Bee.CV.Id.IsSameValue(id))
                {
                    // Get
                    c_Ans = c_Bee;
                    // Only one
                    break;
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets a new list of bees in the field
        /// 
        /// </summary>
        public void Refresh()
        {
            //
            this.HasSetup = true;

            // Virtual?
            if (!this.IsVirtual)
            {
                // Start the cycle
                this.StartRefresh();

                // Get the client
                DockerIFClass c_Client = this.DockerIF;
                // Any?
                if (c_Client != null)
                {
                    // Make the search label
                    string sSearchKey = "label"; ;
                    string sSearchValue = this.Parent.LabelHive + "_" + this.Parent.Name + "=Y";

                    this.Parent.Parent.LogVerbose("Getting field bees for {0}".FormatString(sSearchValue));

                    // Make the filter
                    DockerIFFilterClass c_Filter = new DockerIFFilterClass(sSearchKey, sSearchValue);
                    // Get the list
                    var c_List = c_Client.ListContainers(c_Filter);

                    this.Parent.Parent.LogVerbose("{0} bees seen...".FormatString(c_List.Count));

                    // Loop thru
                    foreach (ContainerListResponse c_Raw in c_List)
                    {
                        // Make into usable
                        BeeCVClass c_CV = new BeeCVClass(this, c_Raw);
                        // Get the bee by Docker ID
                        BeeClass c_Bee = this.GetByDockerID(c_Raw.ID);
                        // New?
                        if (c_Bee == null)
                        {
                            // Make the bee
                            c_Bee = new BeeClass(this, c_CV);
                        }
                        else
                        {
                            // Replace the CV
                            c_Bee.CV.Refresh();
                        }
                        // Skip ghost
                        if (!c_Bee.IsGhost)
                        {
                            // Did it exit?
                            if (!c_Bee.CV.IsInTrouble)
                            {
                                // Was it there?
                                if (this.RefreshList.Contains(c_Bee.Id))
                                {
                                    //
                                    this.RefreshList.Remove(c_Bee.Id);
                                }

                                // Add the bee
                                this.AddBee(c_Bee);
                            }
                            else
                            {
                                // Kill it
                                c_Bee.Kill(BeeClass.KillReason.FoundDead);
                            }
                        }
                    }
                }
                else
                {
                    this.Parent.Parent.LogVerbose("No DockerIF for {0}".FormatString(this.Name));
                }

                // Dump
                this.Parent.Parent.LogVerbose("\r\n" + this.ToString());

                // End the refresh
                this.EndRefresh();
            }
            else
            {
                // Dump
                this.Parent.Parent.LogVerbose("{0} is virtual!".FormatString(this.Name));
            }
        }

        /// <summary>
        /// 
        /// Returns a bee using a given location
        /// 
        /// </summary>
        /// <param name="location">The location (url)</param>
        /// <returns>The bee</returns>
        public BeeClass BeeFromLocation(string location)
        {
            // Assume none
            BeeClass c_Ans = null;

            // Loop thru
            foreach (BeeClass c_Bee in this.Bees.Values)
            {
                // Loop thru
                foreach (TickleAreaClass c_EP in c_Bee.GetTickleAreas())
                {
                    // Does the location match?
                    if (c_EP.Location.IsSameValue(location))
                    {
                        // Get
                        c_Ans = c_Bee;
                        // Only one
                        break;
                    }
                }

                // Only one
                if (c_Ans != null) break;
            }

            return c_Ans;
        }/// <summary>

        /// 
        /// Adds a bee to the field
        /// 
        /// </summary>
        /// <param name="bee">The bee to be added</param>
        public void AddBee(BeeClass bee)
        {
            // Must have real a bee
            if (bee != null)
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
                    if (bee.CV.NXID.IsSameValue(this.Parent.Parent.ID))
                    {
                        // Save
                        this.Parent.Roster.MeBee = bee;
                        //
                        this.Parent.Parent.LogInfo("I am bee {0}".FormatString(bee.Id));

                        // Set the loopback URL
                        this.Parent.Parent.LoopbackURL = bee.URL;
                    }

                    // The change lists
                    List<string> c_DNAsChanged = new List<string>();
                    // Add the DNA
                    c_DNAsChanged.Add(bee.CV.DNA);

                    string sMsg = "as " + bee.CV.DNA;

                    // Tell world
                    this.Parent.Parent.LogInfo("Added bee {0} to field {1} {2}".FormatString(bee.FullID, bee.Field.Name, sMsg));

                    // Handle the DNA changes
                    this.Parent.Roster.SignalDNAChanged(c_DNAsChanged);

                    // Now the other bees
                    if (this.Parent.Synch != null)
                    {
                        this.Parent.Synch.SendMessage(HiveClass.MessengerMClass,
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
        public void RemoveBee(BeeClass bee, BeeClass.KillReason reason)
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
                    // Add the DNA
                    c_DNAsChanged.Add(bee.CV.DNA);

                    // Tell user
                    this.Parent.Parent.LogInfo("Removed bee {0} from field {1}, reason".FormatString(bee.FullID, bee.Field.Name, reason));

                    // Handle the DNA changes
                    this.Parent.Roster.SignalDNAChanged(c_DNAsChanged);
                }
            }
        }


        #endregion

        #region Equal
        public override bool Equals(object obj)
        {
            // If the passed object is null
            if (obj == null)
            {
                return false;
            }
            if (!(obj is FieldClass))
            {
                return false;
            }
            return this.Name.IsSameValue(((FieldClass)obj).Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
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

            // Add the bee
            this.AddBee(bee);
        }

        /// <summary>
        /// 
        /// Ends the refresh cycle
        /// 
        /// </summary>
        public void EndRefresh()
        {
            // Only if any
            if (this.RefreshList.Count > 0)
            {
                // The actual count
                int iCount = 0;

                // What is left over are zombies
                foreach (string sZombie in this.RefreshList)
                {
                    // Get the bee
                    BeeClass c_Zombie = this.GetBee(sZombie);
                    // Any?
                    if (c_Zombie != null && !c_Zombie.IsGhost)
                    {
                        // Kill
                        c_Zombie.Kill(BeeClass.KillReason.Zombie);
                        // Add notch
                        iCount++;
                    }
                }


                // Tell of any zombies
                if (iCount > 0)
                {
                    this.Parent.Parent.LogInfo("{0} zombie(s) seen".FormatString(iCount));
                }
            }
        }
        #endregion
    }
}