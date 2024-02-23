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

using NX.Shared;

namespace NX.Engine.Hive.Mason
{
    public class QueueClass : ChildOfClass<ManagerClass>
    {
        #region Constructor
        internal QueueClass(ManagerClass mgr, 
                                    string queue,
                                    Action<MessageClass> cb,
                                    bool isrequest = false)
            : base(mgr)
        {
            //
            this.QueueName = queue;
            this.Callback = cb;
            this.IsRequest = isrequest;
        }
        #endregion

        #region IDIsposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            // Stop
            this.Stop();

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The queuename
        /// 
        /// </summary>
        private string QueueName { get; set; }

        /// <summary>
        /// 
        /// The callback if any requests come in
        /// 
        /// </summary>
        private Action<MessageClass> Callback { get; set; }

        /// <summary>
        /// 
        /// True is it request queue
        /// 
        /// </summary>
        private bool IsRequest { get; set; }

        /// <summary>
        /// 
        /// The queue that holds messages sent to us
        /// 
        /// </summary>
        public ConsumerQueueClass Queue { get; set; }

        /// <summary>
        /// 
        /// Is the queue available to receive
        /// 
        /// </summary>
        public bool IsAvailable {  get { return this.Queue != null; } }
        #endregion

        #region Methods
        public void Start()
        {
            // A bit of a cleanup
            this.Stop();

            // Do we have an callback?
            if (this.Callback != null)
            {
                // Create
                this.Queue = new ConsumerQueueClass(this.Parent.Redis,
                                                                    this.Parent.QueueName(this.QueueName));
                // And the callback
                this.Queue.Callback = delegate (ConsumerQueueClass queue, string msg)
                {
                    // Do we have a message?
                    if (msg.HasValue())
                    {
                        // Make the message
                        using (MessageClass c_Msg = new MessageClass(msg))
                        {
                            // Flag as doable
                            bool bDo = true;

                            // Setup
                            DateTime? c_TS = null;
                            TimeSpan c_Span = new TimeSpan();

                            // Request
                            if (this.IsRequest)
                            {
                                c_TS = c_Msg.RequestTimestamp;
                                c_Span = c_Msg.RequestTTL;
                            }
                            else
                            {
                                c_TS = c_Msg.ResponseTimestamp;
                                c_Span = c_Msg.ResponseTTL;

                                // Expired?
                                if (c_Span.TotalMilliseconds > 0)
                                {
                                    // Compute death at
                                    DateTime c_Till = ((DateTime)c_TS).Add(c_Span);
                                    // Are we over?
                                    if (c_Till < DateTime.Now)
                                    {
                                        // Bye
                                        bDo = false;
                                    }
                                }

                                // And call
                                if (bDo) this.Callback(c_Msg);
                            }
                        }
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// Stops the receiving of messages
        /// 
        /// </summary>
        public void Stop()
        {
            // Dispose of current
            if (this.Queue != null)
            {
                this.Queue.Dispose();
                this.Queue = null;
            }
        }
        #endregion
    }
}