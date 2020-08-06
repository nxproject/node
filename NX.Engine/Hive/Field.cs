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
            this.Bees = new TickleAreaMapClass(this);

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
        public TickleAreaMapClass Bees { get; private set; }

        //private DockerIFClass IClient { get; set; }
        public DockerIFClass DockerIF { get; private set; }

        /// <summary>
        /// 
        /// The bee count
        /// 
        /// </summary>
        public int BeeCount
        {
            get { return this.Bees.BeeCount; }
        }

        /// <summary>
        /// 
        /// Returns the loader bee
        /// 
        /// </summary>
        public BeeClass LeaderBee
        {
            get { return this.Bees.LeaderBee(); }
        }
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

            foreach (BeeClass c_Bee in this.Bees.Bees.Values)
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
            return this.Bees.GetBee(id);
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
            return this.Bees.GetBeeByDockerID(id);
        }

        /// <summary>
        /// 
        /// Gets a new list of bees in the field
        /// 
        /// </summary>
        public void Refresh()
        {
            // Virtual?
            if (!this.IsVirtual)
            {
                // Start the cycle
                this.Bees.StartRefresh();

                // Get the client
                DockerIFClass c_Client = this.DockerIF;
                // Any?
                if (c_Client != null)
                {
                    // Make the search label
                    string sSearchKey = "label"; ;
                    string sSearchValue = this.Parent.LabelHive + "_" + this.Parent.Name + "=Y";

                    this.Parent.Parent.LogVerbose("Getting field bees for {0}", sSearchValue);

                    // Make the filter
                    DockerIFFilterClass c_Filter = new DockerIFFilterClass(sSearchKey, sSearchValue);
                    // Get the list
                    var c_List = c_Client.ListContainers(c_Filter);

                    this.Parent.Parent.LogVerbose("{0} bees seen...", c_List.Count);

                    // Loop thru
                    foreach (ContainerListResponse c_Raw in c_List)
                    {
                        // Make into usable
                        BeeCVClass c_CV = new BeeCVClass(this, c_Raw);
                        // Make the bee
                        BeeClass c_Bee = new BeeClass(this, c_CV);
                        // Did it exit?
                        if (!c_Bee.CV.IsInTrouble)
                        {
                            // Tell tracker
                            this.Bees.SawBee(c_Bee);
                        }
                        else
                        {
                            // Kill it
                            c_Bee.Kill(BeeClass.KillReason.FoundDead);
                        }
                    }
                }
                else
                {
                    this.Parent.Parent.LogVerbose("No DockerIF for {0}", this.Name);
                }

                // Dump
                this.Parent.Parent.LogVerbose("\r\n" + this.ToString());

                // End the refresh
                this.Bees.EndRefresh();
            }
            else
            {
                // Dump
                this.Parent.Parent.LogVerbose("{0} is virtual!", this.Name);
            }
        }

        /// <summary>
        /// 
        /// Returns a list of DNAs
        /// 
        /// </summary>
        /// <returns>The list of DNAs</returns>
        public List<string> GetDNAs()
        {
            return this.Bees.GetDNAs();
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
            return this.Bees.GetLocationsForDNA(DNA);
        }

        /// <summary>
        /// 
        /// Returns the bees for a given DNA
        /// 
        /// </summary>
        /// <param name="DNA">The DNA</param>
        /// <returns>The list of locations</returns>
        public List<BeeClass> GetBeesForDNA(string DNA)
        {
            return this.Bees.GetBeesForDNA(DNA);
        }

        /// <summary>
        /// 
        /// Returns a list of ports
        /// 
        /// </summary>
        /// <returns>The list of ports</returns>
        public List<string> GetPorts()
        {
            return this.Bees.GetPorts();
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
            return this.Bees.GetLocationsForPort(port);
        }

        /// <summary>
        /// 
        /// Returns the bees for a given port
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <returns>The list of locations</returns>
        public List<BeeClass> GetBeesForPort(string port)
        {
            return this.Bees.GetBeesForPort(port);
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
            return this.Bees.GetBeeFromLocation(location);
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
    }
}