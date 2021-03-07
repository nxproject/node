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
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package TimeZoneConverter -Version 3.2.0
/// 

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// This is my toolkit.  It makes my life easier and I
    /// belive the code more readable.  It has been with me
    /// over many years, languages and OSs.
    /// 
    /// I will document as best as possible but in most
    /// cases the function name tells it all
    /// 
    /// </summary>
    public static class ExtensionsClass
    {
        #region To make coding easier

        /// <summary>
        /// 
        /// Calls a route
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="values">The values to pass as parameters</param>
        /// <param name="routetree">The route tree, first value is HTTP method</param>
        public static void ROUTE(this HTTPCallClass call, StoreClass values, params string[] routetree)
        {
            // Assure values
            if (values == null) values = new StoreClass();

            // Get the route
            RouteClass c_Route = call.Parent.Router.Get(values, routetree[0], new List<string>(routetree.SubArray(1, routetree.Length - 1)));
            // Any?
            if (c_Route != null)
            {
                try
                {
                    // Call
                    c_Route.Call(call, values);
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// Calls a function
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be called</param>
        /// <param name="values">The values to pass as parameters</param>
        /// <returns>Values returned by call</returns>
        public static StoreClass FN(this HTTPCallClass call, string name, StoreClass values = null)
        {
            StoreClass c_Ans = null;

            // Assure values
            if (values == null) values = new StoreClass();

            // --------------------------------------------------
            //
            // The following is done so the user can create DLLs
            // with names other than Fn.xxx.dll, which is the 
            // name used by the NX system.
            //
            // The naming convention is done so calls can be done
            // in an easy way, so if the DLL is named:
            //
            //      Fn.Circles.dll
            //
            // and the class definition is named:
            //
            //      public class Compute : FNCall
            //
            // anyone can call that function using:
            //
            //      Circles.Compute
            //
            // as the function name.
            //
            // If you believe that the DLL name may conflict,
            // for example Allen also has a DLL named:
            //
            //      Fn.Circles.dll
            //
            // then Allen can change his DLL name to:
            //
            //      Fn.Allen.Circles.dll
            //
            // and then use:
            //
            //      Allen.Circles.Compute
            //
            // as the function name.
            //
            // FYI: If you are going to let others use your code 
            // then use Fn.YOURID.MODULENAME.dll.  It will make
            // their life  easier and minimize your support as well.
            //
            // --------------------------------------------------

            // Get the function 
            FNClass c_CN = call.Parent.FNS.GetFN(name);

            // None?
            if(c_CN == null)
            {
                // Format the DLL name
                string sFnGroup = "Fn." + name.Substring(0, name.LastIndexOf("."));

                // Get it
                call.Parent.Use(sFnGroup);

                // And try again
                c_CN = call.Parent.FNS.GetFN(name);
            }

            // Only call if we found a function
            if (c_CN != null) c_Ans = c_CN.Do(call, values);

            // Assure result
            if (c_Ans == null) c_Ans = new StoreClass();

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Calls a function
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be called</param>
        /// <param name="values">The values to pass as parameters,  Note that the keys are prefixed with -- </param>
        /// <returns>Values returned by call</returns>
        public static StoreClass FN(this HTTPCallClass call, string name, string[] values)
        {
            StoreClass c_Params = new StoreClass();
            c_Params.Parse(values);

            return call.FN(name, c_Params);
        }

        /// <summary>
        /// 
        /// Checks to see if a function has been loaded
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be checked</param>
        /// <returns></returns>
        public static bool FNExists(this HTTPCallClass call, string name)
        {
            // Try to get the function 
            return call.Parent.FNS.GetFN(name) != null;
        }

        /// <summary>
        /// 
        /// Calls a process
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be called</param>
        /// <param name="values">The values to pass as parameters</param>
        /// <returns>Values returned by call</returns>
        public static StoreClass PROC(this HTTPCallClass call, string name, StoreClass values = null)
        {
            StoreClass c_Ans = null;

            // Assure values
            if (values == null) values = new StoreClass();

            // Get the function 
            ProcClass c_CN = call.Parent.Procs.GetProc(name);

            // None?
            if (c_CN == null)
            {
                // Format the DLL name
                string sFnGroup = "Proc." + name.Substring(0, name.LastIndexOf("."));

                // Get it
                call.Parent.Use(sFnGroup);

                // And try again
                c_CN = call.Parent.Procs.GetProc(name);
            }

            // Only call if we found a function
            if (c_CN != null) c_Ans = c_CN.Do(call, values);

            // Assure result
            if (c_Ans == null) c_Ans = new StoreClass();

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Calls a function
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be called</param>
        /// <param name="values">The values to pass as parameters,  Note that the keys are prefixed with -- </param>
        /// <returns>Values returned by call</returns>
        public static StoreClass PROC(this HTTPCallClass call, string name, string[] values)
        {
            StoreClass c_Params = new StoreClass();
            c_Params.Parse(values);

            return call.PROC(name, c_Params);
        }

        /// <summary>
        /// 
        /// Checks to see if a function has been loaded
        /// 
        /// </summary>
        /// <param name="call">The current HTTP call</param>
        /// <param name="name">Name of the function to be checked</param>
        /// <returns></returns>
        public static bool PROCExists(this HTTPCallClass call, string name)
        {
            // Try to get the function 
            return call.Parent.Procs.GetProc(name) != null;
        }
        #endregion

        #region Delayed execution
        /// <summary>
        /// 
        /// The list of tasks to do
        /// 
        /// </summary>
        private static NamedListClass<Tuple<DateTime, Action>> DelayedActions { get; set; } = new NamedListClass<Tuple<DateTime, Action>>();

        /// <summary>
        /// 
        /// The ID of the delayed thread
        /// 
        /// </summary>
        private static string DelayedThreadID { get; set; }

        /// <summary>
        /// 
        /// The caller
        /// 
        /// </summary>
        /// <param name="delay">TimeSpan to delay the start</param>
        /// <param name="cb">The callback when the delay expires</param>
        /// <returns>The ID of the task</returns>
        public static string WaitThenCall(this TimeSpan delay, Action cb)
        {
            // MAke the ID
            string sAns = "".GUID();

            // Multi-threaded
            lock (DelayedActions)
            {
                // Add
                DelayedActions.Add(sAns, new Tuple<DateTime, Action>(DateTime.Now.Add(delay), cb));
            }

            // Do we have a thread?
            if (!DelayedThreadID.HasValue())
            {
                // Launch
                DelayedThreadID = "".GUID().StartThread(new System.Threading.ParameterizedThreadStart(DelayedProcessing));
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Removes a delayed task
        /// 
        /// </summary>
        /// <param name="id">The ID of the task</param>
        public static void KillDelayed(this string id)
        {
            // Must have  ID
            if (id.HasValue())
            {
                // Remove
                DelayedActions.Remove(id);
            }
        }

        /// <summary>
        /// 
        /// The thread that runs delayed actions
        /// 
        /// </summary>
        /// <param name="status">The thread status</param>
        private static void DelayedProcessing(object status)
        {
            // Localize
            SafeThreadStatusClass c_Status = status as SafeThreadStatusClass;

            // Forever
            while (c_Status.IsActive)
            {
                // Do we have something to do?
                if (DelayedActions.Count == 0)
                {
                    // Done for the day
                    break;
                }
                else
                {
                    // Loop thru
                    foreach (string sKey in DelayedActions.Keys)
                    {
                        // Get the request
                        Tuple<DateTime, Action> c_Request = DelayedActions[sKey];
                        // Ready?
                        if (c_Request.Item1 < DateTime.Now)
                        {
                            // Multi-threaded
                            lock (DelayedActions)
                            {
                                // Remove
                                DelayedActions.Remove(sKey);
                            }

                            // And callback
                            try
                            {
                                c_Request.Item2();
                            }
                            catch { }
                        }
                    }
                }
            }

            // Reset it
            DelayedThreadID = null;
            // And kill ourselves
            c_Status.End();
        }
        #endregion
    }
}