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
/// Install-Package SocketIOClient -Version 2.0.2.6
/// 

using System.Collections.Generic;

using SocketIOClient;

using NX.Engine;
using NX.Engine.Hive;
using NX.Engine.NginX;
using NX.Shared;

namespace Proc.SocketIO
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
            : base(env, "socketio")
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
        #endregion

        #region Methods
        #endregion
    }
}