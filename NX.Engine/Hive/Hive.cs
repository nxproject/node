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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;

using NX.Shared;
using NX.Engine.Hive.Mason;
using Octokit;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// The hive, where all the bees reside
    /// 
    /// </summary>
    public class HiveClass : ChildOfClass<EnvironmentClass>
    {
        #region Constants
        public const string ProcessorDNAName = "processor";

        public const string MessengerMClass = "bee";
        #endregion

        #region Constructor
        public HiveClass(EnvironmentClass env)
            : base(env)
        { }

        public void Initialize()
        {
            // Flag as so
            this.InInitialize = true;

            // Mark ourselves as a bee
            this.State = States.Bee;

            // Setup the fields
            this.SetupFields();

            // And make the roster
            this.Roster = new RosterClass(this);

            // Setup for normal running
            if (!this.Parent.InMakeMode)
            {
                // Track changes in the environment
                this.Parent.ChangedCalled += delegate (string key, object value)
                {
                    // According to what was changed
                    switch (key)
                    {
                        case "fields":
                        case "external":
                            this.SetupFields();
                            break;
                    }
                };

                // Tell user
                this.Parent.LogVerbose("Setting up bee synch...");

                // Setup the synch availability
                this.Parent.Messenger.AvailabilityChanged += delegate (bool isavailable)
                {
                    //// We're alive!
                    //if (this.Me != null)
                    //{
                    //    // Synch
                    //    this.Synch.Send("bee",
                    //        "field", this.Me.Field.Name,
                    //        "id", this.Me.DockerID,
                    //        "state", "isalive");
                    //}

                    // Is it available?
                    if (isavailable)
                    {
                        // Stop any thread
                        SafeThreadManagerClass.StopThread(this.LabelHive);
                    }
                    else
                    {
                        // Setup a thread to refresh
                        this.LabelHive.StartThread(new ParameterizedThreadStart(AutoRefresh));
                    }
                };

                // And the message receive
                this.Parent.Messenger.MessageReceived += delegate (MessengerClass.MessageClass msg)
                {
                    switch (msg.MClass)
                    {
                        case MessengerMClass:
                            // Get field
                            FieldClass c_Field = this.GetField(msg["field"]);
                            // Valid?
                            if (c_Field != null)
                            {
                                // Dead?
                                if (msg["state"].IsSameValue("isdead"))
                                {
                                    // Remove
                                    this.Roster.Remove(msg["id"], BeeClass.KillReason.FoundDead);
                                }
                                else
                                {
                                    // Make the CV
                                    BeeCVClass c_CV = new BeeCVClass(c_Field, msg["id"]);
                                    // Make bee
                                    BeeClass c_Bee = new BeeClass(c_Field, c_CV);

                                    // Add
                                    this.Roster.Add(c_Bee);
                                }
                            }
                            break;
                    }
                };

                // Handle shutdown
                AppDomain.CurrentDomain.ProcessExit += delegate (object sender, EventArgs e)
                {
                    // Are we the queen?
                    if (this.Roster.QueenBee != null &&
                            this.Roster.MeBee != null &&
                            this.Roster.QueenBee.IsSameAs(this.Roster.MeBee))
                    {
                        // Bring up the follower
                        if (this.Roster.MeBee.FollowerBee != null &&
                            !this.Roster.MeBee.FollowerBee.Id.IsSameValue(this.Roster.MeBee.Id))
                        {
                            this.Roster.MeBee.FollowerBee.Handshake(HiveClass.States.Ascending);
                        }
                    }
                };
            }

            // Out
            this.InInitialize = false;

            this.Parent.Messenger.CheckAvailability();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name of the hive
        /// 
        /// </summary>
        public string Name { get { return this.Parent[EnvironmentClass.KeyHive]; } }

        /// <summary>
        /// 
        /// The hive's roster
        /// 
        /// </summary>
        public RosterClass Roster { get; private set; }

        /// <summary>
        /// 
        /// True if the hive is being initilaized
        /// 
        /// </summary>
        public bool InInitialize { get; set; }

        /// <summary>
        /// 
        /// Has the hive setup?
        /// 
        /// </summary>
        public bool HasSetup { get { return this.Roster.HasSetup; } }

        /// <summary>
        /// 
        /// The number of bees in the hive
        /// 
        /// </summary>
        public int BeeCount
        {
            get { return this.Roster.BeeCount; }
        }

        /// <summary>
        /// 
        /// A list of all the bees
        /// 
        /// </summary>
        public List<BeeClass> Bees
        {
            get { return this.Roster.Bees; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// 
        /// List of fields that this hive's bees can wonder
        /// 
        /// </summary>
        public NamedListClass<FieldClass> Fields { get; private set; } = new NamedListClass<FieldClass>();

        /// <summary>
        /// 
        /// Handle the setup of fields
        /// 
        /// </summary>
        private void SetupFields()
        {
            // Old ones
            List<FieldClass> c_Current = new List<FieldClass>();
            // Do we have any?
            if (this.Fields != null)
            {
                // Save
                c_Current = new List<FieldClass>(this.Fields.Values);
            }

            // Reset
            this.Fields = new NamedListClass<FieldClass>();

            // Get external
            JArray c_External = this.Parent.GetAsJArray("external");

            // Parse
            ItemsClass c_Parsed = new ItemsClass(c_External, true);
            // Anu?
            if (c_Parsed.Count > 0)
            {
                // Make a virtual field
                FieldClass c_Field = new FieldClass(this, null);
                // Crazy name
                c_Field.Name = "VF".GUID();
                // Add
                this.Fields.Add(c_Field.Name, c_Field);

                // And create each one
                foreach (ItemClass c_Loc in c_Parsed)
                {
                    // Make a virtual bee
                    BeeClass c_Bee = new BeeClass(c_Field, c_Loc.ToString(), BeeClass.Types.Virtual);

                    // Treat like any other bee
                    c_Field.AddBee(c_Bee);
                }
            }

            // Get a list of locations
            JArray c_Locs = this.Parent.GetAsJArray("field");
            // If none, use ourselves
            if (c_Locs.Count == 0)
            {
                c_Locs.Add("me=localhost");
            }

            // Parse
            c_Parsed = new ItemsClass(c_Locs, true);

            // And create each one
            foreach (ItemClass c_Loc in c_Parsed)
            {
                // Do we have an option?
                if (c_Loc.ModifierCount > 0)
                {
                    c_Loc.Value += ":" + c_Loc.Modifiers[0];
                }

                // Make room
                FieldClass c_Field = null;

                // Check to see if we already knw
                foreach (FieldClass c_Present in c_Current)
                {
                    // Same?
                    if (c_Present.URL.IsSameValue(c_Loc.Value))
                    {
                        // Yes, reuse
                        c_Field = c_Present;
                        // But change the name
                        c_Field.Name = c_Loc.Key.IfEmpty("me");
                        // Remove from current
                        c_Current.Remove(c_Field);
                        // Only one
                        break;
                    }
                }

                // Create
                if (c_Field == null)
                {
                    c_Field = new FieldClass(this, c_Loc.Key.IfEmpty("me"), c_Loc.Value);
                }

                // OK to add
                bool bOK = true;
                // See what we already have
                foreach (FieldClass c_Present in this.Fields.Values)
                {
                    // SAme location?
                    if (c_Present.URL.IsSameValue(c_Field.URL))
                    {
                        // Already here
                        bOK = false;
                        // Only once
                        break;
                    }
                }

                // Valid?
                if (bOK)
                {
                    // Add
                    this.Fields[c_Field.Name] = c_Field;
                }
            }

            // Delete dropped fields
            foreach (FieldClass c_Present in c_Current)
            {
                // Skip virtual
                if (!c_Present.IsVirtual)
                {
                    // Kill the bees
                    foreach (BeeClass c_Bee in c_Present.Bees.Values)
                    {
                        c_Bee.Kill(BeeClass.KillReason.FieldDropped);
                    }
                }
            }

            //
            this.Parent.LogVerbose("Field(s) {0} seen".FormatString(new List<string>(this.Fields.Keys).Join(", ")));
        }

        /// <summary>
        /// 
        /// The field where I am in
        /// 
        /// </summary>
        public FieldClass MeField
        {
            get
            {
                // Assume none
                FieldClass c_Ans = null;

                // Are we in a bee?
                if (this.Roster.MeBee != null)
                {
                    // Get the field from it
                    c_Ans = this.Roster.MeBee.Field;
                }
                else
                {
                    // Get the first
                    c_Ans = this.Fields.Values.First();
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Returns a field from its name
        /// 
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The field</returns>
        public FieldClass GetField(string name)
        {
            return this.Fields[name];
        }

        /// <summary>
        /// 
        /// Returns a fields Docker client from its name
        /// 
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The Docker client</returns>
        public DockerIFClass GetFieldDockerIF(string name)
        {
            // Assume none
            DockerIFClass c_Ans = null;

            // Get the field
            FieldClass c_Field = this.GetField(name);
            // Known?
            if (c_Field != null)
            {
                // Get the location
                c_Ans = c_Field.DockerIF;
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns a fields URL from its name
        /// 
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The location</returns>
        public string GetFieldURL(string name)
        {
            // Assume none
            string sAns = null;

            // Get the field
            FieldClass c_Field = this.GetField(name);
            // Known?
            if (c_Field != null)
            {
                // Get the location
                sAns = c_Field.DockerIF.URL;
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Returns the field where a bee is located
        /// 
        /// </summary>
        /// <param name="location">The location (url)</param>
        /// <returns>The field</returns>
        public FieldClass FieldFromLocation(string location)
        {
            // Assume none
            FieldClass c_Ans = null;

            // Get the IP
            string sLoc = location.RemoveProtocol().RemovePort();

            // Loop thru
            foreach (FieldClass c_Field in this.Fields.Values)
            {
                // IP match?
                if (sLoc.IsSameValue(c_Field.URL.RemoveProtocol().RemovePort()))
                {
                    // Fund it
                    c_Ans = c_Field;
                    // Only one
                    break;
                }
            }

            return c_Ans;
        }
        #endregion

        #region Sync
        /// <summary>
        /// 
        /// Refreshes the roster every minute
        /// 
        /// </summary>
        /// <param name="status">The thread status</param>
        private void AutoRefresh(object status)
        {
            //
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            //
            this.Parent.LogInfo("Hive auto refresh has started");

            // The sleep cycle
            TimeSpan c_Wait = 5.SecondsAsTimeSpan();

            // Forever
            while (c_Status.IsActive)
            {
                // Every minute
                c_Status.WaitFor(c_Wait);

                // See if first time
                bool bFirst = !this.HasSetup;

                // Reload
                this.Roster.Refresh();

                // Now go to slow mode
                c_Wait = 5.MinutesAsTimeSpan();

                // If first time, do the setup
                if (bFirst)
                {
                    // Get the uses list
                    ItemsClass c_Uses = new ItemsClass(this.Parent.GetAsJArray("uses"));
                    // Loop thru
                    foreach (ItemClass c_Item in c_Uses)
                    {
                        // Load
                        this.Parent.Use(c_Item.Priority);
                    }

                    // And handle process option
                    this.Parent.Use("Proc." + this.Parent.Process.IfEmpty("Default"));

                    // Tell the world
                    this.SetupCompleted?.Invoke(this.HasSetup);
                }
            }

            //
            this.Parent.LogInfo("Hive auto refresh has ended");
        }
        #endregion

        #region Genomes
        /// <summary>
        /// 
        /// Assures that a genme is available.
        /// The genome can come from a cr, already in place or created
        /// 
        /// </summary>
        /// <param name="field">The field to pull the Genome into</param>
        /// <param name="Genome"></param>
        /// <returns>True if the Genome is available</returns>
        public void AssureGenome(FieldClass field, DockerIFNameClass name, List<FieldClass> otherfields, Action cb)
        {
            // Get the project
            string sProj = this.Parent[EnvironmentClass.KeyRepoProject];

            // Handle the creation
            Action<bool> c_CB = delegate (bool done)
            {
                // If not done
                if (!done)
                {
                    // Make
                    this.MakeGenome(field, name.LocalNameWithTag);
                }
                // Callback
                if (cb != null) cb();
            };

            // Get Client
            DockerIFClass c_Client = field.DockerIF;
            // Any?
            if (c_Client! != null)
            {
                // Do we have it?
                bool bHaveIt = c_Client.CheckForImage(name);

                // Do we need to pull?
                if (!bHaveIt)
                {
                    // Save the repo name
                    string sRepo = name.Repo;
                    // As not found
                    bool bFound = false;

                    // Loop thru
                    foreach (FieldClass c_Field in otherfields)
                    {
                        // Us?
                        if (c_Field != field)
                        {
                            // Get client
                            DockerIFClass c_Remote = c_Field.DockerIF;
                            // Any?
                            if (c_Remote != null)
                            {
                                // Exist?
                                if (c_Remote.CheckForImage(name))
                                {
                                    // Make remote name
                                    DockerIFNameClass c_RName = DockerIFNameClass.Make(name);
                                    // Add repo
                                    c_RName.Repo = c_Field.DockerIF.URL;
                                    // Pull
                                    c_Client.PullImage(name, delegate (bool found)
                                    {
                                        bFound = found;
                                    });
                                }
                            }
                        }

                        // First found
                        if (bFound) break;
                    }

                    // Not found?
                    if (!bFound)
                    {
                        // Callback
                        c_CB(false);
                    }
                }
                else
                {
                    // Callback
                    c_CB(true);
                }
            }
        }

        /// <summary>
        /// 
        /// Makes a Docker Genome from a definition
        /// 
        /// </summary>
        /// <param name="field">The field where to create the gnome</param>
        /// <param name="def">The definition (Dockerfile contents)</param>
        /// <param name="Genome">Genome name</param>
        /// <param name="dir">The source directory</param>
        public void MakeGenome(FieldClass field, string genome, string dir = null)
        {
            // Get client
            DockerIFClass c_Client = field.DockerIF;
            // Do we have one?
            if (c_Client != null)
            {
                // Make the name
                DockerIFNameClass c_Name = DockerIFNameClass.Make(this.Parent, genome);

                // Make source directory
                string sSourceDir = "".WorkingDirectory().CombinePath("Hive").CombinePath("Genomes").CombinePath(c_Name.Name).AdjustPathToOS();

                // Is directory there?
                if (!dir.HasValue())
                {
                    // Make
                    dir = sSourceDir;
                }
                else
                {
                    // Copy contents of source into target
                    var c_Files = Directory.GetFiles(sSourceDir, "*.*", SearchOption.TopDirectoryOnly);
                    // Loop thru
                    foreach (string sFile in c_Files)
                    {
                        sFile.CopyFile(dir.CombinePath(sFile.GetFileNameFromPath()));
                    }
                }

                // Do we need a base?
                if (!c_Name.Name.IsSameValue("base"))
                {
                   // Make the base name
                    DockerIFNameClass c_BName = DockerIFNameClass.Make(c_Name, "base");
                    // The directory
                    string sDir = "".WorkingDirectory().CombinePath("Hive").CombinePath("Genomes").CombinePath(c_BName.Name);
                    // Check to see if already made
                    if (!c_Client.CheckForImage(c_BName))
                    {
                        // Build it
                        c_Client.BuildImage(c_BName, sDir);
                    }
                }

                // Build it
                c_Client.BuildImage(c_Name, dir);
            }
        }

        /// <summary>
        ///  
        /// Makes the working directory into a Docker Genome
        /// 
        /// </summary>
        /// <param name="dir">The directory where the code is at</param>
        /// <param name="config">Path to config file</param>
        /// <param name="Genome"></param
        public void MakeSelfIntoGenome(string dir)
        {
            // Adjust the environment
            this.Parent.SynchObject.Set("loc", dir);

            // Do
            this.MakeGenome(this.MeField, ProcessorDNAName, dir);
        }

        /// <summary>
        /// 
        /// Deletes the Genome definition
        /// 
        /// </summary>
        /// <param name="field">The field that holds the Genome</param>
        /// <param name="Genome">The Genome name</param>
        public void KillGenome(FieldClass field, string genome)
        {
            // Must have a field
            if (field != null)
            {
                // Get client
                DockerIFClass c_Client = field.DockerIF;
                // Any?
                if (c_Client != null)
                {
                    // Delete
                    c_Client.DeleteImage(DockerIFNameClass.Make(this.Parent, genome));
                }
            }
        }

        /// <summary>
        /// 
        /// Returns the name of the Genome, without project or tag
        /// 
        /// </summary>
        /// <param name="Genome">The full Genome name</param>
        /// <returns>The pain name</returns>
        private string PlainName(string value)
        {
            // Find where the name starts
            int iPos = value.LastIndexOf("/");
            // Remove if any
            if (iPos != -1) value = value.Substring(iPos + 1);
            // Find tag
            iPos = value.IndexOf(":");
            // Remove if any
            if (iPos != -1) value = value.Substring(0, iPos);

            return Regex.Replace(value, @"[^a-zA-Z0-9_.-]", "");
        }

        /// <summary>
        /// 
        /// Returns the tag of the genome name
        /// 
        /// </summary>
        /// <param name="value">The genome name</param>
        /// <returns>The tag</returns>
        internal string GenomeTag(string value)
        {
            // Assume none
            string sAns = "";

            // Find tag
            int iPos = value.IndexOf(":");
            // If there is one, get it
            if (iPos != -1) sAns = value.Substring(iPos + 1);

            return sAns;
        }

        /// <summary>
        /// 
        /// Returns the genome name without the tag
        /// 
        /// </summary>
        /// <param name="value">The genome name</param>
        /// <returns>The untaged name</returns>
        internal string GenomeRemoveTag(string value)
        {
            // Assume none
            string sAns = value;

            // Find tag
            int iPos = value.IndexOf(":");
            // If there is one, get it
            if (iPos != -1) sAns = value.Substring(0, iPos);

            return sAns;
        }
        #endregion

        #region DNA
        /// <summary>
        /// 
        /// Gets a definition
        /// 
        /// </summary>
        /// <param name="name">The name of the definition</param>
        /// <returns>The definition object</returns>
        public BeeDNAClass GetDNA(string task = ProcessorDNAName)
        {
            // Assume none
            BeeDNAClass c_Ans = null;

            // Get the underlying
            JObject c_Wkg = "".WorkingDirectory().CombinePath("Hive").CombinePath("DNA").CombinePath(task).ReadFile().ToJObject();
            // Any?
            if (c_Wkg.HasValue())
            {
                // Create wrapper
                c_Ans = new BeeDNAClass(task, c_Wkg);
            }

            // Get it
            return c_Ans;
        }

        /// <summary>
        /// 
        /// Assures that a number of containers running a task are running
        /// 
        /// </summary>
        /// <param name="task">The task to assure</param>
        /// <param name="min">The minimum number allowed</param>
        /// <param name="max">The maximum number allowed</param>
        /// <param name="data">Any extra data for parameters</param>
        public void AssureDNACount(string task, int min = 1, int max = int.MaxValue, StoreClass data = null)
        {
            // Must have a task
            if (task.HasValue() && this.HasSetup)
            {
                // Get a list of the containers
                List<BeeClass> c_Bees = this.Roster.GetBeesForDNA(task);
                // Get the count
                int iCount = c_Bees.Count;

                // Loop thru
                foreach (BeeClass c_Bee in c_Bees)
                {
                    // Only valid
                    if (c_Bee != null && !c_Bee.IsGhost)
                    {
                        // Refresh CV
                        c_Bee.CV.Refresh();
                        // OK?
                        if (!c_Bee.CV.IsRunning)
                        {
                            // Kill
                            c_Bee.Kill(BeeClass.KillReason.FoundDead);
                            // One less
                            iCount--;
                        }
                    }
                }

                // And how many we have changed
                int iDiff = 0;

                // Too many?
                while (iCount > max)
                {
                    // Get the first
                    BeeClass c_Bee = c_Bees.Last();

                    // Remove from list
                    c_Bees.Remove(c_Bee);
                    // And kill it
                    c_Bee.Kill(BeeClass.KillReason.TooMany);
                    // Update
                    iCount--;
                    iDiff--;
                }

                // Too few?
                while (iCount < min)
                {
                    // Make
                    BeeClass c_Bee = this.MakeBee(task, data);
                    // Check
                    if (c_Bee != null)
                    {
                        // And to system
                        this.Roster.Add(c_Bee);
                    }

                    // Update
                    iCount++;
                    iDiff++;
                }

                // Any changes?
                if (iDiff != 0)
                {
                    // What kind
                    string sKind = iDiff > 0 ? "Added" : "Removed";
                    // Make positive
                    if (iDiff < 0) iDiff = -iDiff;
                    // Plural
                    string sS = iDiff != 1 ? "s" : "";
                    // Tell user
                    this.Parent.LogInfo("{0} {1} {2} bee{3}".FormatString(sKind, iDiff, task, sS));
                }
            }
        }

        /// <summary>
        /// 
        /// Recycles bees taht have a given DNA
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void RecycleDNA(string task)
        {
            // Get a list of the containers
            List<BeeClass> c_Bees = this.Roster.GetBeesForDNA(task);

            // Loop thru
            foreach (BeeClass c_Bee in c_Bees)
            {
                // Check for DNA
                if (!task.IsSameValue(c_Bee.CV.DNA))
                {
                    this.Parent.LogError("Saw {0} while trying to restart {1} - {2}".FormatString(c_Bee.CV.DNA, task, c_Bee.FullID));
                }
                else
                {
                    // Get DOckerIF
                    DockerIFClass c_IF = c_Bee.Field.DockerIF;
                    // Any?
                    if (c_IF != null)
                    {
                        // Recycle
                        c_IF.RestartContainer(c_Bee.DockerID, c_Bee.Id, task);
                    }
                }
            }
        }
        #endregion

        #region Bees
        /// <summary>
        /// 
        /// Makes a bee
        /// 
        /// </summary>
        /// <param name="name">The name of the definition</param>
        public BeeClass MakeBee(string dna = ProcessorDNAName,
                                    StoreClass data = null,
                                    BeeDNAClass usedna = null,
                                    JArray cmd = null)
        {
            // Assume failure
            BeeClass c_Ans = null;

            // Make the next id
            string sNextID = "".GUID();

            // Get the definition
            BeeDNAClass c_Def = usedna;
            // Did we get one?
            if (c_Def == null)
            {
                // Only main part
                string sWkg = dna;
                if (sWkg.Contains(".")) sWkg = sWkg.Substring(0, sWkg.IndexOf("."));

                // Get from settings
                c_Def = this.GetDNA(sWkg);
            }

            // Any?
            if (c_Def != null)
            {
                // Meet any requirements
                JArray c_Required = c_Def.Requires;
                // Any?
                if (c_Required.HasValue())
                {
                    // Loop thru
                    for (int i = 0; i < c_Required.Count; i++)
                    {
                        // Make sure that they are running
                        this.AssureDNACount(c_Required.Get(i), 1, 1);
                    }
                }

                // Are we cloning?
                if (cmd != null)
                {
                    // Make copy
                    c_Def = new BeeDNAClass(c_Def.DNA, c_Def.SynchObject);
                    // And replace the command
                    c_Def.Cmd = cmd;
                }

                // Get the fields available
                ItemsClass c_Allowed = new ItemsClass(new List<string>(this.Fields.Keys), true);
                // Get the fields wanted
                ItemsClass c_Wanted = new ItemsClass(this.Parent.GetAsJArray("field_" + c_Def.DNA), true);
                // Only keep allowed
                c_Wanted = c_Wanted.In(c_Allowed);
                // Any?
                if (c_Wanted.Count == 0)
                {
                    // Go back to allowed
                    c_Wanted = c_Allowed;
                }

                // Default to first one
                FieldClass c_Field = this.GetField(c_Wanted[0].Priority);

                // Do we have more than one?
                if (c_Wanted.Count > 1)
                {
                    // Get the count for the starting container
                    int iCount = c_Field.BeeCount;
                    // Now do the rest
                    foreach (ItemClass c_Poss in c_Wanted)
                    {
                        // Get the next field
                        FieldClass c_Next = this.GetField(c_Poss.Priority);
                        // Get the count
                        int iPoss = c_Next.BeeCount;
                        // Less loaded?
                        if (iPoss < iCount)
                        {
                            // Use this one
                            c_Field = c_Next;
                            // And reset count
                            iCount = iPoss;
                        }
                    }
                }

                // Get the Genome
                string sDNA = c_Def.DNA;

                // Get the Genome
                this.AssureGenome(c_Field, DockerIFNameClass.Make(this.Parent, sDNA), new List<FieldClass>(this.Fields.Values), delegate ()
               {
                   // Get the client
                   DockerIFClass c_Client = c_Field.DockerIF;
                   // Any?
                   if (c_Client != null)
                   {
                       // Get the params
                       JObject c_Params = this.Parent.AsParameters;
                       // Make the Docker request
                       JObject c_Raw = c_Def.AsParameters(this, c_Field, dna, sNextID, c_Params, data);

                       // The  container ID
                       string sBeeName = null;

                       // Unique?
                       switch (c_Def.Unique.IfEmpty().ToLower())
                       {
                           case "y":
                               sBeeName = dna.IfEmpty().MD5HashString();
                               break;

                           case "*":
                               // From environemnt
                               JArray c_Hives = this.Parent.GetAsJArray("hive_" + sDNA);
                               // Any?
                               if (c_Hives.HasValue())
                               {
                                   // Mash
                                   sBeeName = c_Hives.ToList().Join(".").MD5HashString();
                               }
                               else
                               {
                                   sBeeName = "global";
                               }
                               break;

                           default:
                               sBeeName = sNextID;
                               break;
                       }


                       // Now the config
                       using (DDNConfigClass c_Resolve = new DDNConfigClass(c_Raw, sDNA.IsSameValue(ProcessorDNAName)))
                       {
                           // Create the name
                           c_Resolve.Target.Name = this.Name + "_" + dna.Replace(".", "_") + "_" + sBeeName;

                           // Create container
                           string sID = c_Client.CreateContainer(c_Resolve);

                           // Do we have an id?
                           if (sID.HasValue())
                           {
                               // Get starting CV
                               BeeCVClass c_CV = new BeeCVClass(c_Field, sID);

                               // Wait until is is running
                               c_CV.Wait(this,

                                    c_CV.MakeWait(delegate (BeeCVClass cv)
                                    {
                                        // Start it
                                        c_Client.StartContainer(sID);

                                        return BeeCVClass.WaitClass.TriggerReturns.Continue;

                                    }, "created"),

                                    c_CV.MakeWait(delegate (BeeCVClass cv)
                                    {
                                        // Make
                                        c_Ans = new BeeClass(c_Field, cv);
                                        // Add the ports
                                        this.Roster.Add(c_Ans);

                                        return BeeCVClass.WaitClass.TriggerReturns.EndWait;

                                    }, "running"),

                                    c_CV.MakeWait(delegate (BeeCVClass cv)
                                    {
                                        // Make
                                        c_Ans = new BeeClass(c_Field, cv.Id);
                                        // And kill
                                        c_Ans.Kill(BeeClass.KillReason.DiedAtBirth);
                                        // Pass back none
                                        c_Ans = null;

                                        return BeeCVClass.WaitClass.TriggerReturns.EndWait;

                                    }, "exited")
                                );
                           }
                       }
                   }
               });
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Makes a worker bee, with an optional stating point
        /// 
        /// </summary>
        /// <param name="proc">The startup function</param>
        /// <returns>A worker bee</returns>
        public BeeClass MakeWorkerBee(string proc = null)
        {
            // Makes a worker bee running the process
            return this.MakeBee(ProcessorDNAName + "." + proc.IfEmpty(), new StoreClass("proc".AsJObject(proc.IfEmpty())));
        }

        /// <summary>
        /// 
        /// Clones a bee
        /// 
        /// </summary>
        /// <param name="bee"></param>
        /// <returns></returns>
        public BeeClass CloneBee(BeeClass bee)
        {
            // Clone
            return this.MakeBee(bee.CV.DNA, null, null, bee.CV.Cmd);
        }

        /// <summary>
        /// 
        /// Makes sure that a given count of worker bees exist
        /// 
        /// </summary>
        /// <param name="proc">The process of the bee</param>
        /// <param name="min">Minimum number allowed</param>
        /// <param name="max">MAximum number allowed</param>
        public void AssureWorkerBeeCount(string proc, int min, int max)
        {
            // Get a list of the containers
            List<BeeClass> c_Bees = this.Roster.GetBeesForProcess(proc);
            // Get the count
            int iCount = c_Bees.Count;

            //// Anf the changes
            //int iDiff = 0;

            // Too many?
            while (iCount > max)
            {
                // Get the first
                BeeClass c_Bee = c_Bees.Last();
                // Remove from list
                c_Bees.Remove(c_Bee);
                // And kill it
                c_Bee.Kill(BeeClass.KillReason.TooMany);
                // One less
                iCount--;
                //iDiff--;
            }

            // Too few?
            while (iCount < min)
            {
                // Make
                BeeClass c_Bee = this.MakeWorkerBee(proc);
                // Add to list
                c_Bees.Add(c_Bee);
                // And to system
                this.Roster.Add(c_Bee);
                // One more
                iCount++;
                //iDiff++;
            }

            //// Any changes?
            //if (iDiff != 0)
            //{
            //    // What kind
            //    string sKind = iDiff > 0 ? "Added" : "Removed";
            //    // Make positive
            //    if (iDiff < 0) iDiff = -iDiff;
            //    // Plural
            //    string sS = iDiff != 1 ? "s" : "";
            //    // Tell user
            //    this.Parent.LogInfo("{0} {1} {2} worker bee{3}", sKind, iDiff, proc, sS);
            //}
        }

        /// <summary>
        /// 
        /// Removes all the processor bees
        /// 
        /// </summary>
        public void KillProcessorBees()
        {
            //
            this.Parent.LogInfo("Killing all processor bees!");

            // Refresh
            this.Roster.Refresh();

            // Loop thru
            foreach (BeeClass c_Bee in this.Bees)
            {
                // Processor?
                if (c_Bee.CV.DNA.StartsWith(HiveClass.ProcessorDNAName))
                {
                    // Kill like a zombie so no log is generated
                    c_Bee.Kill(BeeClass.KillReason.NoLogs);
                }
            }
        }
        #endregion

        #region Synch
        public MessengerClass Synch { get; private set; }
        #endregion

        #region Labels
        public string LabelUUID
        {
            get { return this.MakeLabel("uuid"); }
        }
        public string LabelHive
        {
            get { return this.MakeLabel("hive"); }
        }
        public string LabelDNA
        {
            get { return this.MakeLabel("dna"); }
        }
        public string LabelField
        {
            get { return this.MakeLabel("field"); }
        }
        public string LabelID
        {
            get { return this.MakeLabel("id"); }
        }
        public string LabelCmd
        {
            get { return this.MakeLabel("cmd"); }
        }
        public string LabelRoster
        {
            get { return this.MakeLabel("roster"); }
        }
        public string LabelQueen
        {
            get { return this.MakeLabel("queen"); }
        }
        public string LabelGenome
        {
            get { return this.MakeLabel("genome"); }
        }
        public string LabelProc
        {
            get { return this.MakeLabel("proc"); }
        }

        private string MakeLabel(string value)
        {
            return this.Name + "_" + value;
        }
        #endregion

        #region Mason
        /// <summary>
        /// 
        /// The Mason Bee support
        /// 
        /// </summary>
        private Mason.ManagerClass IMason { get; set; }
        public Mason.ManagerClass Mason
        {
            get
            {
                if (this.IMason == null)
                {
                    this.IMason = new Mason.ManagerClass(this);
                }

                return this.IMason;
            }
        }
        #endregion

        #region State
        public enum States
        {
            Unknown,

            Bee,

            Queen,
            InQueenDuties,

            Ascending
        }

        /// <summary>
        /// 
        /// Current state
        /// 
        /// </summary>
        public States State { get; set; } = States.Unknown;
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for the SetupCompleted event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnSetupCompletedHandler(bool hassetup);

        /// <summary>
        /// 
        /// Defines the event to be raised when the hhive has been setup
        /// 
        /// </summary>
        public event OnSetupCompletedHandler SetupCompleted;
        #endregion
    }
}