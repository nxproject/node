///--------------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;

using NX.Shared;

namespace NX.Engine.Hive.Mason
{
    public class ManagerClass : ChildOfClass<HiveClass>
    {
        #region Constructor
        public ManagerClass(HiveClass hive)
            : base(hive)
        { }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            // Delete request queue
            this.Cleanup(this.Parent.Parent.ID);

            // Delete request queue
            if(this.RequestQueue != null)
            {
                this.RequestQueue.Dispose();
                this.RequestQueue = null;
            }

            // Delete work queues
            foreach(QueueClass c_WQ in this.WorkQueues.Values)
            {
                c_WQ.Dispose();
            }

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The callback to do when an inbound message comes in
        /// 
        /// </summary>
        private QueueClass RequestQueue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private NamedListClass<QueueClass> WorkQueues { get; set; } = new NamedListClass<QueueClass>();
        #endregion

        #region Method
        /// <summary>
        /// 
        /// Adds a work queue
        /// 
        /// </summary>
        /// <param name="queue">The queue</param>
        /// <param name="cb">The callback when a message comes in</param>
        public void Listen(string queue, Action<MessageClass> cb = null)
        {
            // Multi-thread support
            lock (this)
            {
                // Do we know it already?
                if (this.WorkQueues.ContainsKey(queue))
                {
                    // Map
                    QueueClass c_WQ = this.WorkQueues[queue];
                    // Remove
                    this.WorkQueues.Remove(queue);
                    // Dispose
                    c_WQ.Dispose();
                    // And end
                    c_WQ = null;
                }

                // Do we have a callback?
                if (cb != null)
                {
                    // Create
                    QueueClass c_WQ = new QueueClass(this, queue, cb);
                    // Save
                    this.WorkQueues.Add(queue, c_WQ);
                }
            }
        }

        /// <summary>
        /// 
        /// Respond to a request
        /// 
        /// </summary>
        /// <param name="msg">The message</param>
        public void Respond(MessageClass msg)
        {
            //// Redis available?
            //if (this.Redis.IsAvailable)
            //{
            //    // Do we have a queue?
            //    if (msg != null && !msg.DoNotRespond)
            //    {
            //        // Make the queue
            //        using (ProviderQueueClass c_Queue = new ProviderQueueClass(this.Redis, this.QueueName(msg.Queue)))
            //        {
            //            // Set the timestamp
            //            msg[MessageClass.KeyMasonBee] = this.Parent.Parent.ID;
            //            msg[MessageClass.KeyResponseTS] = DateTime.Now.ToUniversalTime().ToString();

            //            // And send the message
            //            c_Queue.Put(msg.ToString());
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 
        /// Sends a message to a work queue
        /// 
        /// </summary>
        /// <param name="msg">The message.  Nust have a queue</param>
        /// <returns>The message ID if the message was sent, null otherwise</returns>
        public string Send(MessageClass msg)
        {
            // Assume none
            string sAns = null;

            //// Redis available?
            //if (this.Redis.IsAvailable)
            //{
            //    // Do we have a queue?
            //    if (msg != null && msg.Queue.HasValue())
            //    {
            //        // Make the queue
            //        using (ProviderQueueClass c_Queue = new ProviderQueueClass(this.Redis, this.QueueName(msg.Queue)))
            //        {
            //            // Set key items
            //            msg[MessageClass.KeyRequestorBee] = this.Parent.Parent.ID;
            //            msg[MessageClass.KeyRequestTS] = DateTime.Now.ToUniversalTime().ToString();
            //            // Responses?
            //            if(this.RequestQueue == null || !this.RequestQueue.IsAvailable)
            //            {
            //                // If no callback force to no response
            //                msg[MessageClass.KeyDoNotRespond] = "true";
            //            }

            //            // And send the message
            //            c_Queue.Put(msg.ToString());
            //            // Set the ID
            //            sAns = msg.ID;
            //        }
            //    }
            //}

            return sAns;
        }

        /// <summary>
        /// 
        /// Set the inbound message callbackm
        /// 
        /// </summary>
        /// <param name="cb">The callback</param>
        public void Handle(Action<MessageClass> cb)
        {
            // Do we have a queue?
            if (this.RequestQueue != null)
            {
                // Delete
                this.RequestQueue.Dispose();
                this.RequestQueue = null;
            }

            // Callback?
            if (cb != null)
            {
                // Create 
                this.RequestQueue = new QueueClass(this, this.Parent.Parent.ID, cb, true);
            }
            else
            {
                // Cleanup
                this.Cleanup(this.Parent.Parent.ID);
            }
        }

        private void HandleRedis(bool isavailable)
        { 
            // Is Redis available?
            if (isavailable)
            {
                // Do the request queue
                if (this.RequestQueue != null) this.RequestQueue.Start();

                // Now do the work queues
                foreach (QueueClass c_WQ in this.WorkQueues.Values)
                {
                    // Kill
                    c_WQ.Start();
                }
            }
            else
            {
                // Do we have an inbound queue?
                if (this.RequestQueue != null)
                {
                    // Delete
                    this.RequestQueue.Dispose();
                    this.RequestQueue = null;
                }

                // Delete the work queues
                foreach (QueueClass c_WQ in this.WorkQueues.Values)
                {
                    // Kill
                    c_WQ.Stop();
                }
            }
        }
        #endregion

        #region Support
        /// <summary>
        /// 
        /// A queue name
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal string QueueName(string id)
        {
            return this.Parent.Parent.ApplySystemPrefix("mason") + "_" + id;
        }

        public void Cleanup(Hive.BeeClass bee)
        {
            // By ID
            this.Cleanup(bee.CV.NXID);
        }

        public void Cleanup(string beeid)
        {
            //// If available
            //if (this.Redis.IsAvailable)
            //{
            //    // Make the queue
            //    using (ProviderQueueClass c_Queue = new ProviderQueueClass(this.Redis, this.QueueName(beeid)))
            //    {
            //        // And delete it
            //        c_Queue.Delete();
            //    }
            //}
        }
        #endregion
    }
}