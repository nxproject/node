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

using System;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace Proc.Redis
{
    public class MessengerClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public MessengerClass(EnvironmentClass env)
            : base(env)
        {
            //
            this.Redis = this.Parent.Globals.Get<ManagerClass>();

            // Link
            this.Redis.AvailabilityChanged += delegate (bool isavailable)
            {
                // Handle
                this.HandleRedis(isavailable);
            };

            // And start
            this.HandleRedis(this.Redis.IsAvailable);
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The ID of the synch
        /// 
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// 
        /// Is the synch available
        /// 
        /// </summary>
        public bool IsAvailable { get { return this.Signal != null; } }

        /// <summary>
        /// 
        /// The Redis publish/subscribe
        /// 
        /// </summary>
        private PSClass Signal { get; set; }

        /// <summary>
        /// 
        /// The Redis manager
        /// 
        /// </summary>
        public ManagerClass Redis { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Sends a message
        /// 
        /// </summary>
        /// <param name="fn">The function</param>
        /// <param name="payload">The payload</param>
        public void SendMessage(MessageClass msg)
        {
            // Is the synch available?
            if (this.IsAvailable)
            {
                // Send
                this.Signal.Send(msg.ToString());

                //
                this.Parent.LogVerbose("{0} --> {1}".FormatString(this.ID, msg.ToString()));
            }
        }

        /// <summary>
        /// 
        /// Builds a message and sends it
        /// 
        /// </summary>
        /// <param name="mclass"></param>
        /// <param name="kv"></param>
        public void SendMessage(string mclass, params string[] kv)
        {
            using (MessageClass c_Msg = new MessageClass(this.Parent, mclass, kv))
            {
                this.SendMessage(c_Msg);
            }
        }

        /// <summary>
        /// 
        /// Handle Redis connect/disconnect
        /// 
        /// </summary>
        /// <param name="isavailable"></param>
        private void HandleRedis(bool isavailable)
        {
            //
            if (isavailable)
            {
                // And setup signal
                this.Signal = new PSClass(this.Redis, this.ID + "_synch", delegate (string value)
                {
                    //
                    this.Parent.LogVerbose("{0} <-- {1}".FormatString(this.ID, value));

                    // Decode packet
                    using (MessageClass c_Payload = new MessageClass(this.Parent, value))
                    {
                        // Our own?
                        if (!c_Payload.IsEcho)
                        {
                            // Tell the rest
                            this.MessageReceived?.Invoke(c_Payload);
                        }
                    }
                });
            }
            else
            {
                // Destroy the PS
                if (this.Signal != null)
                {
                    this.Signal.Dispose();
                    this.Signal = null;
                }
            }

            // Callback
            this.AvailabilityChanged?.Invoke(isavailable);
        }

        /// <summary>
        /// 
        /// Run at messenger user to start the events rolling
        /// 
        /// </summary>
        public void CheckAvailability()
        {
            //
            this.HandleRedis(this.IsAvailable);
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for availability event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnChangedHandler(bool isavailable);

        /// <summary>
        /// 
        /// Defines the event to be raised when a DNA is added/deleted
        /// 
        /// </summary>
        public event OnChangedHandler AvailabilityChanged;

        /// <summary>
        /// 
        /// The delegate for receive event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnMessageReceived(MessageClass msg);

        /// <summary>
        /// 
        /// Defines the event to be raised when a DNA is added/deleted
        /// 
        /// </summary>
        public event OnMessageReceived MessageReceived;
        #endregion

        public class MessageClass : IDisposable
        {
            #region Constructor
            public MessageClass(EnvironmentClass env, string mclass, params string[] values)
            {
                //
                this.EnvID = env.ID;
                this.From = this.EnvID;
                this.MClass = mclass;

                // According to count
                switch (values.Length)
                {
                    case 0:
                        this.Values = new JObject();
                        break;

                    case 1:
                        // Deserializer
                        this.Values = values[0].IfEmpty().ToJObject();
                        break;

                    default:
                        this.Values = new JObject();
                        // Loop thru
                        for (int i = 0; i < values.Length; i += 2)
                        {
                            // Set
                            this.Data.Set(values[i], values[i + 1]);
                        }
                        break;

                }
            }

            public MessageClass(MessengerClass sync, string mclass)
            {
                //
                this.EnvID = sync.Parent.ID;
                this.From = this.EnvID;
                this.MClass = mclass;
            }
            #endregion

            #region IDisposable
            /// <summary>
            /// 
            /// Housekeeping
            /// 
            /// </summary>
            public void Dispose()
            { }
            #endregion

            #region Indexer
            public string this[string key]
            {
                get { return this.Data.Get(key); }
                set { this.Data.Set(key, value); }
            }
            #endregion

            #region Properties
            /// <summary>
            /// 
            /// The message
            /// 
            /// </summary>
            private JObject Values { get; set; } = new JObject();

            /// <summary>
            /// 
            /// The message class
            /// 
            /// </summary>
            public string MClass
            {
                get { return this.Values.Get("mc").IfEmpty(); }
                set { this.Values.Set("mc", value.IfEmpty()); }
            }

            /// <summary>
            /// 
            /// Who the message is from
            /// 
            /// </summary>
            public string From
            {
                get { return this.Values.Get("from").IfEmpty(); }
                private set { this.Values.Set("from", value.IfEmpty()); }
            }

            /// <summary>
            /// 
            /// The data
            /// 
            /// </summary>
            private JObject IData { get; set; }
            public JObject Data
            {
                get
                {
                    if (this.IData == null)
                    {
                        this.IData = this.Values.AssureJObject("data");
                    }
                    return this.IData;
                }
            }

            /// <summary>
            /// 
            /// The ID
            /// 
            /// </summary>
            private string EnvID { get; set; }

            /// <summary>
            /// 
            /// True is is an echo of what was sent
            /// 
            /// </summary>
            public bool IsEcho
            {
                get { return this.EnvID.IsSameValue(this.From); }
            }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// Serializer
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return this.Values.ToSimpleString();
            }
            #endregion
        }
    }
}