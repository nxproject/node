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
/// Install-Package SocketIOClient -Version 2.0.2.6
/// 

using System.Collections.Generic;

using SocketIOClient;

using NX.Engine;
using NX.Engine.Hive;
using NX.Engine.NginX;
using NX.Shared;

namespace NX.Engine.SocketIO
{
    /// <summary>
    /// 
    /// Socket.IO interface
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "socketio", true)
        {
            // Handle NginX
            this.SetNginxInformation("socket.io", false);

            // Handle the events
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Clear
                if (this.Client != null)
                {
                    this.Client = null;
                }

                // Is Socket.IO available
                if (this.IsAvailable)
                {
                    //
                    string sURL = this.Location.URLMake();

                    // Create
                    this.Client = new SocketIOClient.SocketIO(sURL);

                    // Handle disconnection
                    this.Client.OnDisconnected += delegate (object sender, string e)
                    {
                        // Redo
                        this.Client.ConnectAsync().Wait();
                    };

                    // Connect
                    this.Client.ConnectAsync().Wait();

                    this.Parent.LogInfo("SocketIO: Connected to {0}".FormatString(sURL));

                    // Open
                    this.OpenSys(this.Parent.SIOChannels[0]);
                }
                else
                {
                    // Close
                    this.CloseSys();
                }
            };

            // Bootstap
            this.CheckForAvailability();
        }
        #endregion

        #region Indexer
        public EventClass this[string name]
        {
            get
            {
                // Do we already know it?
                if(!this.Map.ContainsKey(name))
                {
                    // Create
                    this.Map[name] = new EventClass(this, name);
                }

                return this.Map[name];
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The Socket.IO client
        /// 
        /// </summary>
        internal SocketIOClient.SocketIO Client { get; set; }
        
        /// <summary>
        /// 
        /// A map of rooms
        /// 
        /// </summary>
        private NamedListClass<EventClass> Map { get; set; } = new NamedListClass<EventClass>();

        /// <summary>
        /// 
        /// The system event
        /// 
        /// </summary>
        private EventClass SystemEvent { get; set; }

        /// <summary>
        /// 
        /// Is the system channel available?
        /// 
        /// </summary>
        public bool SysEnabled { get; set; } = true;
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns a new message
        /// 
        /// </summary>
        /// <returns></returns>
        public MessageClass NewSys()
        {
            //
            MessageClass c_Ans = null;

            if (this.SystemEvent != null && this.Client != null)
            {
                c_Ans = new MessageClass(this.SystemEvent);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Opens a session
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void OpenSys(string id)
        {
            // If already created as another id, delete
            if(this.SystemEvent != null && !this.SystemEvent.Name.IsSameValue(id))
            {
                this.SystemEvent.Dispose();
                this.SystemEvent = null;
            }

            // Create
            if(this.SystemEvent == null)
            {
                this.SystemEvent = new EventClass(this, id);
                this.SystemEvent.MessageReceived += delegate (SocketIO.MessageClass msg)
                {
                    if (this.SysEnabled)
                    {
                        this.SysMessageReceived?.Invoke(msg);
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// Closes system channel
        /// 
        /// </summary>
        public void CloseSys()
        {
            //
            if(this.SystemEvent != null)
            {
                this.SystemEvent.Dispose();
                this.SystemEvent = null;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for the SysMessageReceived event
        /// 
        /// </summary>
        /// <param name="msg">The message</param>
        public delegate void OnSysReceivedHandler(MessageClass msg);

        /// <summary>
        /// 
        /// Defines the event to be raised when a message is received
        /// 
        /// </summary>
        public event OnSysReceivedHandler SysMessageReceived;
        #endregion
    }
}