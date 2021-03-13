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
/// Install-Package SocketIOClient -Version 2.0.2.3
/// 

using System;

using Newtonsoft.Json.Linq;
using SocketIOClient;

using NX.Shared;

namespace NX.Engine.SocketIO
{
    /// <summary>
    /// 
    /// A Socket.IO event
    /// 
    /// </summary>
    public class EventClass : ChildOfClass<ManagerClass>
    {
        #region Constructor
        public EventClass(ManagerClass mgr, string name)
            : base(mgr)
        {
            //
            this.Name = name;

            this.Parent.Parent.LogVerbose("SocketIO Event: Setting up for '{0}'".FormatString(this.Name));

            // Connect
            this.Parent.Client.On(this.Name, response =>
            {
                //this.Parent.Parent.LogInfo("SocketIO Event: Reeceived '{0}': {1} count".FormatString(this.Name, response.Count));

                // Loop thru
                for (int i=0;i < response.Count;i++)
                {
                    //
                    var c_Raw = response.GetValue(i);
                    string sMsg = null;
                    switch(c_Raw.GetType().Name)
                    {
                        case "JObject":
                            sMsg = (c_Raw as JObject).ToSimpleString();
                            break;
                        default:
                            try
                            {
                                sMsg = c_Raw.ToString();
                            }
                            catch (Exception e)
                            { 
                                this.Parent.Parent.LogException(e);
                            }
                            break;
                    }

                    if (sMsg.HasValue())
                    {
                        //this.Parent.Parent.LogInfo("SocketIO Event: Reeceived '{0}': RAW-{1}".FormatString(this.Name, sMsg));

                        // Make the message
                        using (MessageClass c_Msg = new MessageClass(this, sMsg))
                        {
                            //
                           // this.Parent.Parent.LogInfo("SocketIO Event: Reeceived '{0}': {1}".FormatString(this.Name, c_Msg.ToString()));

                            // Tell world
                            this.MessageReceived?.Invoke(c_Msg);
                        }
                    }
                }
            });

            this.Parent.Parent.LogInfo("SocketIO Event: Listening for '{0}'".FormatString(this.Name));
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name of the room
        /// 
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Creates a new message
        /// 
        /// </summary>
        /// <returns></returns>
        public MessageClass New()
        {
            return new MessageClass(this);
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for the AvailabilityChanged event
        /// 
        /// </summary>
        /// <param name="msg">The message</param>
        public delegate void OnReceivedHandler(MessageClass msg);

        /// <summary>
        /// 
        /// Defines the event to be raised when a message is received
        /// 
        /// </summary>
        public event OnReceivedHandler MessageReceived;
        #endregion
    }
}