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
/// Install-Package StackExchange.Redis -Version 2.1.58
/// 

using System;

using StackExchange.Redis;

using NX.Shared;

namespace NX.Engine.BumbleBees.Redis
{
    /// <summary>
    /// 
    /// The publish/subscribe Redis layer
    /// 
    /// </summary>
    public class PSClass : BaseClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="mgr">The current Redis manager</param>
        /// <param name="queue">Queue  name</param>
        public PSClass(ManagerClass mgr, string queue, Action<string> cb)
            : base(mgr, queue)
        {
            //
            this.Callback = cb;

            // Start it
            this.Start();
        }
        #endregion

        #region IIdsposable
        public override void Dispose()
        {
            //
            this.Stop();

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The callback when a message is received
        /// 
        /// </summary>
        public Action<string> Callback { get; set; }

        /// <summary>
        /// 
        /// The subscription
        /// 
        /// </summary>
        private ISubscriber Subscription { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Starts the listen for changes
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
                // Are we connected?
                if (this.Parent.DB != null && this.Subscription == null && this.Callback != null)
                {
                    //
                    this.Parent.Parent.LogVerbose("Opening subscription {0}", this.SignalName);

                    // Create a subscription
                    this.Subscription = this.Parent.Client.GetSubscriber();

                    //
                    this.Parent.Parent.LogVerbose("Subscribing to channel {0}", this.SignalName);

                    // Set the processor
                    this.Subscription.Subscribe(this.SignalName, (channel, msg) =>
                    {
                        // And callback, if one available
                        if (this.Callback != null)
                        {
                            this.Callback(msg);
                        }
                    });

                    //
                    this.Parent.Parent.LogVerbose("Subscription {0} done", this.SignalName);
                }
            }
            catch (Exception e)
            {
                //
                this.Parent.Parent.LogException("While subscribing to {0}".FormatString(this.SignalName), e);
            }
        }

        /// <summary>
        /// 
        /// Stops listening for changes
        /// 
        /// </summary>
        public void Stop()
        {
            // Are we subscribed?
            if (this.Subscription != null)
            {
                //
                this.Parent.Parent.LogVerbose("Closing subscription {0}", this.SignalName);

                //
                this.Subscription.UnsubscribeAll();

                // Close
                this.Subscription = null;
            }
        }

        /// <summary>
        /// 
        /// Sends a store as a message
        /// 
        /// </summary>
        /// <param name="data">The store to send</param>
        public void Send(string data)
        {
            // Are we connected?
            if (this.Subscription != null)
            {
                // And tell everyone
                this.Subscription.Publish(this.SignalName, data);
            }
        }
        #endregion
    }
}