///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// This class is a multi-threaded HTTP server, where
    /// each HTTP call gets queued up and then processed by a 
    /// processor thread
    /// 
    /// </summary>
    public class HTTPClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public HTTPClass(EnvironmentClass env)
            : base(env)
        {
            // Start it
            this.Start();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The HTTP listener
        /// 
        /// </summary>
        private HttpListener Listener { get; set; }

        /// <summary>
        /// 
        /// Queue for HTTP calls
        /// 
        /// </summary>
        private BlockingCollection<HttpListenerContext> Requests { get; set; } = new BlockingCollection<HttpListenerContext>();

        /// <summary>
        /// 
        /// The authentication scheme to use
        /// 
        /// </summary>
        public AuthenticationSchemes Scheme { get; private set; } = AuthenticationSchemes.Anonymous;

        /// <summary>
        /// 
        /// The master thread 
        /// 
        /// </summary>
        private string ThreadID { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Starts the HTTP server
        /// 
        /// </summary>
        public void Start()
        {
            // Only if we are able to run
            if (this.Listener == null)
            {
                // Create the listener
                try
                {
                    this.Listener = new HttpListener();
                    this.Listener.Prefixes.Add("http://*:" + this.Parent.HTTPPort.ToString() + "/");
                    this.Parent.LogInfo("Listening at port {0}, {1} worker threads".FormatString(this.Parent.HTTPPort, this.Parent.HTTPThreads));

                    // Set the authentication
                    this.Listener.AuthenticationSchemes = this.Scheme;

                    // Start
                    this.Listener.Start();
                    // And create thread
                    this.ThreadID = "".StartThread(new ParameterizedThreadStart(AddRequest));
                    //
                    // Create worker threads  
                    //
                    this.SetThreadCount(this.Parent.HTTPThreads);
                }
                catch (Exception e)
                {
                    // Clean up
                    this.Listener = null;

                    this.Parent.LogException("At HTTP Setup", e);
                }
            }
        }

        /// <summary>
        /// 
        /// Stops the HTTP server
        /// 
        /// </summary>
        public void Stop()
        {
            // Only if we are able to run
            if (this.Listener != null)
            {
                // Stop the threads
                SafeThreadManagerClass.StopThreadsMatching(this.ThreadID + "*");

                // Stop listening
                try
                {
                    // Housekeeping
                    this.Listener.Stop();
                }
                catch (Exception e)
                {
                    this.Parent.LogException("Stop", e);
                }

                // End the listener
                this.Listener.Close();
                // And remove it
                this.Listener = null;
            }
        }

        /// <summary>
        /// 
        /// Adds a request to the queue
        /// 
        /// </summary>
        /// <param name="status">The thread status object</param>
        private void AddRequest(object status)
        {
            // Make usable
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Until stop is called
            while (c_Status.IsActive)
            {
                // One bad call does not kill the listener
                try
                {
                    // Get the call
                    HttpListenerContext c_Ctx = this.Listener.GetContext();

                    // Valid?
                    if (c_Ctx != null)
                    {
                        // Add to queue
                        this.Requests.Add(c_Ctx);
                    }
                    else
                    {
                        // Tell user
                        this.Parent.LogError("Unable to get context");

                        // Recycle
                        this.Stop();
                        this.Start();
                    }
                }
                catch (Exception e)
                {
                    // In case of error
                    this.Parent.LogException("ListenThread failed", e);

                    // Recycle
                    this.Stop();
                    this.Start();
                }
            }
        }

        /// <summary>
        /// 
        /// Process a request
        /// 
        /// </summary>
        /// <param name="status">The thread status object</param>
        private void ProcessRequest(object status)
        {
            // Make usable
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Until stop is called
            while (c_Status.IsActive)
            {
                // Just in case
                try
                {
                    // Grab
                    HttpListenerContext c_Ctx = this.Requests.Take();
                    // Any?
                    if (c_Ctx != null)
                    {
                        // Make the wrapper
                        using (HTTPCallClass c_Call = new HTTPCallClass(this.Parent, c_Ctx))
                        {
                            // The parameters
                            StoreClass c_Params = new StoreClass();

                            // First from URL after ?
                            string sURL = c_Ctx.Request.RawUrl;
                            // Find the ?
                            int iPos = sURL.IndexOf("?");
                            if (iPos != -1)
                            {
                                // Parse the URL parameters
                                c_Params.Parse(sURL.Substring(iPos + 1), StoreClass.ParseTypes.URL);
                                // And remove from the URL
                                sURL = sURL.Substring(0, iPos);
                            }

                            // Split the URL
                            List<string> c_Nodes = new List<string>(sURL.Substring(1).Split('/'));

                            // Find the route
                            RouteClass c_Route = this.Parent.Router.Get(c_Params, c_Ctx.Request.HttpMethod, c_Nodes);
                            // We got one
                            if (c_Route != null)
                            {
                                // Just in case the programmer missed something
                                try
                                {
                                    // Is the user kosher?
                                    if (!c_Call.UserInfo.Valid)
                                    {
                                        // Validate
                                        if (this.Parent.ValidatonCallback != null)
                                        {
                                            c_Call.UserInfo.Valid = this.Parent.ValidatonCallback();
                                        }
                                    }

                                    // Ok?
                                    if (c_Call.UserInfo.Valid)
                                    {
                                        // Call the route
                                        c_Route.Call(c_Call, c_Params);
                                    }
                                    else
                                    {
                                        this.Parent.LogError("HTTP: Invalid user");
                                    }
                                }
                                catch (Exception e)
                                {
                                    this.Parent.LogException("HTTP: ProcessRequest", e);
                                }
                            }

                            // Was the call competed?
                            if (!c_Call.ResponseCompleted)
                            {
                                // No, answer with an HTTP 500 error
                                c_Call.RespondWithError();
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// Sets the number of woker thread to run
        /// 
        /// </summary>
        /// <param name="count">The number of threads</param>
        public void SetThreadCount(int count)
        {
            // Must be at least one
            if (count < 1) count = 1;

            // Get the threads
            List<string> c_Threads = SafeThreadManagerClass.GetMatching(this.ThreadID + "_.+");

            // Add threads
            while (c_Threads.Count < count)
            {
                // Make a name
                string sName = this.ThreadID + "_".GUID();
                // And start
                if (sName.StartThread(new ParameterizedThreadStart(ProcessRequest)).HasValue())
                {
                    // Save time so we do not have to get list again
                    c_Threads.Add(sName);
                }
            }

            // Remove threads
            while (c_Threads.Count > count)
            {
                // Stop the first
                SafeThreadManagerClass.StopThread(c_Threads[0]);
                // Remove
                c_Threads.RemoveAt(0);
            }

            //Set
            this.Parent.HTTPThreads = count;

            this.Parent.LogInfo("Running {0} threads".FormatString(this.Parent.HTTPThreads));
        }

        /// <summary>
        /// 
        /// Sets the authentication scheme
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        public void SetAuthenticationScheme(string scheme)
        {
            // Parse
            try
            {
                // Save
                this.Scheme = (AuthenticationSchemes)Enum.Parse(typeof(AuthenticationSchemes), scheme, true);

                // Are we running?
                if (this.Listener != null)
                {
                    // Set
                    this.Listener.AuthenticationSchemes = this.Scheme;
                }
            }
            catch { }
        }
        #endregion
    }
}