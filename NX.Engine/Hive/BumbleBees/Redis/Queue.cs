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
/// Install-Package ServiceStack.Redis -Version 5.9.0
/// 

using System;

using NX.Shared;

namespace NX.Engine.BumbleBees.Redis
{
    /// <summary>
    /// 
    /// A generic queue
    /// 
    /// </summary>
    public class QueueClass : BaseClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="mgr">The current Redis manager</param>
        /// <param name="queue">Queue  name</param>
        public QueueClass(ManagerClass mgr, string queue)
            : base(mgr, queue)
        { 
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name to use for Redis
        /// 
        /// </summary>
        internal string PrivateName
        {
            get { return this.Parent.Parent.ApplySystemPrefix("queue"); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Delete the queue
        /// 
        /// </summary>
        public void Delete()
        {
            // Are we connected?
            if (this.Parent.Client != null)
            {
                // Bye
                this.Del(this.Name);
            }
        }
        #endregion

        #region PS interface
        /// <summary>
        /// 
        /// The listener for the signals
        /// 
        /// </summary>
        internal BaseClass SignalListener { get; set; }
        internal PSClass Subscription { get; set; }
        #endregion
    }

    /// <summary>
    /// 
    /// The provider queue.
    /// Note: Any number of providers may send to the same
    /// queue
    /// 
    /// </summary>
    public class ProviderQueueClass : QueueClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="mgr">The current Redis manager</param>
        /// <param name="queue">Queue  name</param>
        public ProviderQueueClass(ManagerClass mgr, string queue)
            : base(mgr, queue)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Puts a store into the queue
        /// 
        /// </summary>
        /// <param name="pkt">The store to send</param>
        public void Put(string pkt)
        {
            // Are we connected?
            if (this.Parent.Client != null)
            {
                // Send the store
                this.RPush(this.Name, pkt);
                // And tell everyone
                this.Subscription.Send("");
            }
        }

        /// <summary>
        /// 
        /// Returns the number of entries in the queue
        /// 
        /// </summary>
        public long Count
        {
            get
            {
                // Assume error
                long iAns = -1;

                // Are we connected?
                if (this.Parent.Client != null)
                {
                    // Get the count
                    iAns = this.LLen(this.Name);
                }

                return iAns;
            }
        }

        /// <summary>
        /// 
        /// Deletes the queue
        /// 
        /// </summary>
        public new void Delete()
        {
            // Are we connected?
            if (this.Parent.Client != null)
            {
                // Delete
                this.Del(this.Name);
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// A consumer queue.
    /// Note: Any number of consumers may obtain a message
    /// from the queue, but only one will get it, making
    /// this a FIFO queue
    /// 
    /// </summary>
    public class ConsumerQueueClass : QueueClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="mgr">The current Redis manager</param>
        /// <param name="queue">Queue  name</param>
        public ConsumerQueueClass(ManagerClass mgr, string queue)
            : base(mgr, queue)
        {
            // Make believe that we just got one
            this.LastOn = "".Now();
            // And set the default wait
            this.SleepPeriod = 5.SecondsAsTimeSpan();

            // And start ourselves
            this.Start();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The function that will be called when a message is available
        /// 
        /// </summary>
        public Action<ConsumerQueueClass, string> Callback { get; set; }

        /// <summary>
        /// 
        /// How long to wait between checking for a message
        /// 
        /// </summary>
        public TimeSpan SleepPeriod { get; set; }

        /// <summary>
        /// 
        /// Date and time that the last message was seen
        /// 
        /// </summary>
        public DateTime LastOn { get; set; }

        /// <summary>
        /// 
        /// Set to true to hold off any processing
        /// 
        /// </summary>
        public bool Hold { get; set; }

        /// <summary>
        /// 
        /// The possible number of outstanding messages
        /// 
        /// </summary>
        public uint Outstanding { get; private set; }
        #endregion

        #region Methods
        private void ProcessThread(object status)
        {
            // Setup
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Until we shutdown
            while (c_Status.IsActive)
            {
                // Only if we are allowed to process
                if (this.Callback != null && !this.Hold)
                {
                    // Are we connected?
                    if (this.Parent.Client != null)
                    {
                        // Any to do??
                        while (c_Status.IsActive && this.Outstanding > 0)
                        {
                            try
                            {
                                // Get the message, if there are none you will get a null
                                string sMsg = this.LPop(this.Name);
                                // Di we get one?
                                if (sMsg.HasValue())
                                {
                                    // set last seen
                                    this.LastOn = "".Now();

                                    // And do  the callback
                                        this.Callback(this, sMsg);
                                }
                            }
                            catch { }

                            lock (this)
                            {
                                // Update the count in the queue
                                this.Outstanding = (uint)this.LLen(this.Name);
                            }
                        }
                    }

                    // Wait a wile if nothing outstanding
                    if(this.Outstanding == 0) c_Status.WaitFor(this.SleepPeriod);
                }
            }
        }

        /// <summary>
        /// 
        /// Starts the process
        /// 
        /// </summary>
        public void Start()
        {
            // Do we need to do?
            if (this.SignalListener == null)
            {
                // Create
                this.SignalListener = new BaseClass(this.Parent, this.SignalName);
                {
                    // Subscribe
                    this.Subscription = new PSClass(this.Parent, this.SignalName, delegate (string msg)
                     {
                         lock (this)
                         {
                             // Tell the system that we may have another one
                             this.Outstanding++;
                         }
                     });
                }
            }

            // Start the processing thread
            (PrivateName + "_" + this.SignalName).StartThread(new System.Threading.ParameterizedThreadStart(ProcessThread));
        }

        /// <summary>
        /// 
        /// Stops the process
        /// 
        /// </summary>
        public void Stop()
        {
            // Stop the processing thread
            SafeThreadManagerClass.StopAllThreads();

            // Do we have a subscription
            if (this.Subscription != null)
            {
                this.Subscription.Dispose();
                this.Subscription = null;
            }

            // Do we have a listener
            if (this.SignalListener != null)
            {
                this.SignalListener.Dispose();
                this.SignalListener = null;
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            // Stop ourselves
            this.Stop();

            base.Dispose();
        }
        #endregion
    }
}