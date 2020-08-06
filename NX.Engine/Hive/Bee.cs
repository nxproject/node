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
using System.Drawing;
using System.Linq;
using System.Text;

using Docker.DotNet.Models;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// A bee
    /// 
    /// </summary>
    public class BeeClass : ChildOfClass<FieldClass>
    {
        #region Constructor
        public BeeClass(FieldClass field, BeeCVClass cv)
            : base(field)
        {
            // Store the CV
            this.CV = cv;

            // Back way
            this.IsGhost = this.CV.IsGhost;
            this.IsVirtual = this.CV.IsVirtual;

            // Loop thru
            foreach (Port c_Port in this.CV.Ports)
            {
                // Make the TickleArea
                TickleAreaClass c_EP = new TickleAreaClass(this, c_Port.PublicPort.ToString(),
                                                                    c_Port.PrivatePort.ToString());

                // Is it reachable?
                if (c_EP.IsAvailable)
                {
                    // Add
                    this.TickleAreas.Add(c_EP);
                }
            }
        }

        internal BeeClass()
            : base(null)
        { }

        internal BeeClass(FieldClass field, string id, Types type = Types.Bee)
            : base(field)
        {
            //
            this.IsGhost = type == Types.Ghost;
            this.IsVirtual = type == Types.Virtual;

            // Save in case
            string sLoc = id;
            // Handle virtual
            if (this.IsVirtual)
            {
                // Create phony ID
                id = "VB".GUID();
            }

            // Phony
            this.CV = new BeeCVClass(field, id, type);

            // Setup by type
            if (this.IsGhost)
            {
                // If ghost, phony the URL
                this.IURL = "localhost:{0}".FormatString(this.Parent.Parent.Parent.HTTPPort).URLMake();

                // Make the TickleArea
                TickleAreaClass c_EP = new TickleAreaClass(this, this.Parent.Parent.Parent.HTTPPort.ToString(),
                                                                    this.Parent.Parent.Parent.HTTPPort.ToString());
                // Add
                this.TickleAreas.Add(c_EP);
            }
            else if (this.IsVirtual)
            {
                // Parse 
                ItemClass c_Info = new ItemClass(sLoc);

                // Must have modifier (port)
                if (c_Info.ModifierCount > 0)
                {
                    // The key portion is the DNA
                    this.CV.DNA = c_Info.Key;

                    // Make a tickle area with the rest
                    TickleAreaClass c_Tickle = new TickleAreaClass(this, 
                                                                c_Info.Modifiers[0], 
                                                                c_Info.Modifiers[0],
                                                                c_Info.Value +  ":" + c_Info.Modifiers[0]);

                    // Add
                    this.TickleAreas.Add(c_Tickle);
                }
            }
        }
        #endregion

        #region Enums
        public enum Types
        {
            Bee,
            Ghost,
            Virtual
        }

        public enum States
        {
            Alive,
            Hiccup,
            Dead
        }

        public enum KillReason
        {
            FoundDead,
            DiedAtBirth,
            DeadAtCheck,
            Zombie,

            FieldDropped,

            TooMany,

            ViaRoute
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The bee's unique ID in the hive
        /// 
        /// </summary>
        public string Id
        { get { return this.CV.NXID; } }

        /// <summary>
        /// 
        ///  Is this bee a ghost?
        ///  
        /// </summary>
        public bool IsGhost { get; private set; }


        /// <summary>
        ///  Is this bee virtual?
        ///  
        /// </summary>
        public bool IsVirtual { get; private set; }

        /// <summary>
        /// 
        /// The Docker ID of the container
        /// 
        /// </summary>
        public string DockerID { get { return this.CV.Id; } }

        /// <summary>
        /// 
        /// The CV for the bee
        /// 
        /// </summary>
        public BeeCVClass CV { get; private set; }

        /// <summary>
        /// 
        /// Areas where the bee can be tickled (open ports)
        /// 
        /// </summary>
        public List<TickleAreaClass> TickleAreas { get; private set; } = new List<TickleAreaClass>();

        /// <summary>
        /// 
        /// The field where the bee resides
        /// 
        /// </summary>
        public FieldClass Field
        {
            get { return this.Parent; }
        }
        #endregion

        #region Chain of Command
        /// <summary>
        /// 
        /// Returns the Queen bee
        /// 
        /// </summary>
        public BeeClass QueenBee
        {
            get { return this.Parent.Parent.Roster.QueenBee; }
        }

        /// <summary>
        /// 
        /// Return the bee that leads me
        /// 
        /// </summary>
        public BeeClass LeaderBee
        {
            get { return this.Field.Bees.LeaderBee(this); }
        }

        /// <summary>
        /// 
        /// Returns the bee that follows me
        /// 
        /// </summary>
        public BeeClass FollowerBee
        {
            get { return this.Field.Bees.FollowerBee(this); }
        }

        /// <summary>
        /// 
        /// Returns the bee's URL
        /// 
        /// </summary>
        private string IURL { get; set; }
        public string URL
        {
            get
            {
                // Assume none
                string sAns = this.IURL;

                // If none
                if (!sAns.HasValue())
                {
                    // Virtual?
                    if (this.IsVirtual)
                    {
                        // Use first tickle area
                        TickleAreaClass c_Only = this.TickleAreas.First();
                        // Any?
                        if (c_Only != null)
                        {
                            // Get the location
                            sAns = c_Only.Location;
                        }
                    }
                    else
                    {
                        // Find the proper tickle area
                        foreach (TickleAreaClass c_TA in this.TickleAreas)
                        {
                            // Is this the one?
                            if (c_TA.PrivatePort.IsSameValue("80"))
                            {
                                // Yes, get the field
                                sAns = this.Field.DockerIF.URL;
                                // Remove port
                                int iPos = sAns.LastIndexOf(":");
                                if (iPos != -1) sAns = sAns.Substring(0, iPos);
                                // And add the private
                                sAns += ":" + c_TA.PublicPort;

                                // Only one call
                                break;
                            }
                        }
                    }

                    // Any?
                    if (sAns.HasValue())
                    {
                        sAns = sAns.URLMake();
                        this.IURL = sAns;
                    }
                }

                return sAns;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Dumps bee
        /// 
        /// </summary>
        /// <returns>The dump</returns>
        public override string ToString()
        {
            //
            StringBuilder c_Buffer = new StringBuilder();

            c_Buffer.AppendLine("-------------------------------------------");
            c_Buffer.AppendLine("ID: {0}".FormatString(this.Id));
            c_Buffer.AppendLine("DockerID: {0}".FormatString(this.DockerID));
            c_Buffer.AppendLine("DNA: {0}".FormatString(this.CV.DNA));
            c_Buffer.AppendLine("Tickle Points:");

            foreach (TickleAreaClass c_EP in this.TickleAreas)
            {
                c_Buffer.AppendLine("Location: {0}".FormatString(c_EP.Location));
                c_Buffer.AppendLine("DNA: {0}".FormatString(c_EP.DNA));
                c_Buffer.AppendLine("Public: {0}".FormatString(c_EP.PublicPort));
                c_Buffer.AppendLine("Private: {0}".FormatString(c_EP.PrivatePort));
                c_Buffer.AppendLine("");
            }
            c_Buffer.AppendLine("-------------------------------------------");

            return c_Buffer.ToString();
        }

        /// <summary>
        /// 
        /// Kills this bee
        /// 
        /// </summary>
        public void Kill(KillReason reason)
        {
            // Get location
            DockerIFClass c_Client = this.Parent.DockerIF;
            // Available?
            if (c_Client != null)
            {
                // Dump the logs
                this.Parent.Parent.Parent.LogInfo("Dump for bee {0}, reason: {1}", this.Id.IfEmpty(this.CV.Id), reason);
                c_Client.GetLogs(this.CV.Id);

                //
                c_Client.RemoveContainer(this.CV.Id);

                // Remove from roster
                this.Parent.Parent.Roster.Remove(this);

                // Synch
                if (this.Parent.Parent.Synch != null)
                {
                    this.Parent.Parent.Synch.SendMessage(HiveClass.MessengerMClass,
                        "field", this.Field.Name,
                        "id", this.DockerID,
                        "state", "isdead");
                }
            }
        }

        /// <summary>
        /// 
        /// Calls another bee using direct channel
        /// 
        /// </summary>
        /// <param name="bee"></param>
        /// <param name="fn"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public StoreClass Call(BeeClass bee, string fn, StoreClass data = null)
        {
            // Assume fail
            StoreClass c_Ans = null;

            // Get he bee's url
            string sURL = bee.URL;
            // Any?
            if (sURL.HasValue())
            {
                // Assure data
                if (data == null) data = new StoreClass();

                // Call
                JObject c_Resp = sURL.URLPost(data,
                                                this.Parent.Parent.Parent[EnvironmentClass.KeySecureCode],
                                                bee.Id,
                                                fn).FromBytes().ToJObject();
                // And make return
                c_Ans = new StoreClass(c_Resp);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Pings a bee to check for liveliness
        /// 
        /// </summary>
        /// <returns>True if bee responded properly</returns>
        public States IsAlive()
        {
            // Assume fail
            States eAns = States.Dead;

            // Get my URL
            string sURL = this.URL;

            //
            this.Parent.Parent.Parent.LogInfo("Checking bee {0} at {1}", this.Id, this.URL);

            // Any?
            if (sURL.HasValue())
            {
                // Call
                string sID = sURL.URLNX("id").NXReturnValue();

                //
                this.Parent.Parent.Parent.LogInfo("Return was {0}", sID);

                //// ID's must match
                //eAns = sID.IsSameValue(this.Id) ? States.Alive : States.Dead;

                // --------------------------------------------------
                // 
                // The following code does a secondary check using 
                // Docker, but we have noticed that sometimes Docker
                // believes that the container is running boot no
                // active processes are found.  The use of Healthcheck
                // SHOULD remove that, but if you have any issues with
                // zombie containers, use the commented line above
                // and comment out this section.
                //
                // --------------------------------------------------

                // ID's must match
                eAns = sID.IsSameValue(this.Id) ? States.Alive : States.Hiccup;

                // If not
                if (eAns != States.Alive)
                {
                    // Refresh
                    this.CV.Refresh();

                    // And get from CV
                    eAns = this.CV.IsInTrouble ? States.Dead : eAns;
                }
            }

            return eAns;
        }
        #endregion
    }
}