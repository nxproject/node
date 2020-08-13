﻿///--------------------------------------------------------------------------------
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
/// Install-Package SocketIOClient -Version 2.0.2.3
/// 

using SocketIOClient;

using NX.Shared;

namespace Proc.SocketIO
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

            // Connect
            this.Parent.Client.On(this.Name, response =>
            {
                // Loop thru
                for(int i=0;i < response.Count;i++)
                {
                    // Make the message
                    using(MessageClass c_Msg = new MessageClass(this, response.GetValue<string>(i)))
                    {
                        // Tell world
                        this.MessageReceived?.Invoke(c_Msg);
                    }
                }
            });
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