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
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// A thread manager.
    /// 
    /// </summary>
    public static class SafeThreadManagerClass 
    {
        #region Workarea
        /// <summary>
        /// 
        /// A table of all the threads created by the manager
        /// 
        /// </summary>
        internal static NamedListClass<SafeThreadClass> ThreadMap { get; set; } = new NamedListClass<SafeThreadClass>();

        ///// <summary>
        ///// 
        ///// The active thread ID
        ///// 
        ///// </summary>
        //private static string CurrentThreadID { get { return Thread.CurrentThread.GetHashCode().ToString(); } }

        //public static SafeThreadClass CurrentThread
        //{
        //    get { return SafeThreadManagerClass.GetByName("$"); }
        //}

        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns a thread given its ID
        /// 
        /// </summary>
        /// <param name="id">The ID of the thread</param>
        /// <returns>The thread if any</returns>
        public static SafeThreadClass Get(string name)
        {
            return SafeThreadManagerClass.ThreadMap[name];
        }

        /// <summary>
        /// 
        /// Start a thread
        /// 
        /// </summary>
        /// <param name="fn">The ParameterizedThreadStart to run</param>
        /// <param name="values">The array of parameters</param>
        /// <returns>True if the thread was started</returns>
        public static string StartThread(string name, ParameterizedThreadStart fn, params object[] values)
        {
            return SafeThreadManagerClass.StartThread(name, fn, new List<object>(values));
        }

        /// <summary>
        /// 
        /// Start a thread
        /// 
        /// </summary>
        /// <param name="fn">The ParameterizedThreadStart to run</param>
        /// <param name="values">The list of parameters</param>
        /// <returns>True if the thread was started</returns>
        public static string StartThread(string name, ParameterizedThreadStart fn, List<object> values)
        {
            // Assume it all fails
            string sAns = null;

            // Assure name
            name = name.IfEmpty("".GUID());

            // Name cannot be used
            if (!SafeThreadManagerClass.ThreadMap.ContainsKey(name))
            {
                // See if it launches
                try
                {
                    // Create the holding object
                    SafeThreadClass c_Thread = new SafeThreadClass(name, fn, values);
                    // And start it
                    c_Thread.Start();

                    // Get the ID
                    sAns = c_Thread.ID;

                    // Add
                    SafeThreadManagerClass.ThreadMap[name] = c_Thread;
                }
                catch { }
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Stops all threads
        /// 
        /// </summary>
        public static void StopAllThreads()
        {
            // Loop thru
            foreach (SafeThreadClass c_Thread in SafeThreadManagerClass.ThreadMap.Values)
            {
                // Kill
                c_Thread.Stop();
            }
        }

        /// <summary>
        /// 
        /// Stops all the threads of the current thread
        /// 
        /// </summary>
        public static void StopThread(string name)
        {
            // Do we have it?
            if(SafeThreadManagerClass.ThreadMap.ContainsKey(name))
            {
                // Kill it
                SafeThreadManagerClass.ThreadMap[name].Stop();
            }
        }

        /// <summary>
        /// 
        /// Returns a list of threads that match a pattern
        /// 
        /// </summary>
        /// <param name="patt"The regular expression pattern></param>
        /// <returns></returns>
        public static List<string> GetMatching(string patt)
        {
            // Assume none
            List<string> c_Ans = new List<string>();

            // Loop thru
            foreach(string sKey in SafeThreadManagerClass.ThreadMap.Keys)
            {
                // Do they match? If so add
                if (sKey.Matches(patt)) c_Ans.Add(sKey);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Stops all thread matchin a pattern
        /// 
        /// </summary>
        /// <param name="patt">The regular expression pattern</param>
        public static void StopThreadsMatching(string patt)
        {
            // Loop thru
            foreach(string sKey in SafeThreadManagerClass.GetMatching(patt))
            {
                // Stop
                SafeThreadManagerClass.StopThread(sKey);
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// Wrapper for a thread
    /// 
    /// </summary>
    public class SafeThreadClass : IDisposable
    {
        #region Constructor
        internal SafeThreadClass(string name, ParameterizedThreadStart thread, List<object> value)
        {
            //
            this.ThreadFn = thread;
            this.ThreadName = name;

            // The thread status
            this.Status = new SafeThreadStatusClass(this, name, value);
        }
        #endregion

        #region Properties
        public ParameterizedThreadStart ThreadFn { get; private set; }
        public string ThreadName { get; private set; }

        /// <summary>
        /// 
        /// The underlying thread object
        /// 
        /// </summary>
        public Thread SynchObject { get; private set; }

        /// <summary>
        ///  
        /// The thread ID
        /// 
        /// </summary>
        public string ID
        {
            get
            {
                // Assume none
                string sAns = null;

                // The ID is the hash of the thread
                if (this.SynchObject != null) sAns = this.SynchObject.GetHashCode().ToString();

                //
                return sAns;
            }
        }

        /// <summary>
        /// 
        /// Has the thread been created?
        /// 
        /// </summary>
        public bool IsCreated
        {
            get { return (this.SynchObject != null); }
        }

        /// <summary>
        /// 
        /// Is it running?
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // Check to see if it has been created
                bool bAns = this.IsCreated;

                // If so
                if (bAns)
                {
                    // Check the state
                    bAns = (this.SynchObject.ThreadState == System.Threading.ThreadState.Running)
                        || (this.SynchObject.ThreadState == System.Threading.ThreadState.WaitSleepJoin);
                }

                //
                return bAns;
            }
        }

        /// <summary>
        /// 
        /// The thread status
        /// 
        /// </summary>
        public SafeThreadStatusClass Status { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Starts the thread
        /// 
        /// </summary>
        /// <returns>The thread ID</returns>
        public string Start()
        {
            // Assume it failed
            string sAns = null;

            // And stop the thread if there
            this.Stop();

            // Must have a start
            if (this.ThreadFn != null)
            {
                // And set the state
                this.Status.IsActive = true;

                // Create 
                this.SynchObject = new Thread(this.ThreadFn);
                this.SynchObject.TrySetApartmentState(ApartmentState.MTA);
                this.SynchObject.Start(this.Status);

                // And get the ID
                sAns = this.SynchObject.GetHashCode().ToString();

                // And create ourselves
                SafeThreadManagerClass.ThreadMap[this.ThreadName] = this;
            }

            // Return the ID
            return sAns;
        }

        /// <summary>
        /// 
        /// Stops the thread
        /// 
        /// </summary>
        public virtual void Stop()
        {
            // Only if there was a thread
            if (this.SynchObject != null)
            {
                // Reset the state
                this.Status.IsActive = false;

                // Unlink from master table
                SafeThreadManagerClass.ThreadMap.Remove(this.ThreadName);

                // We are done in any case
                this.SynchObject = null;
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public void Dispose()
        {
            // Make sure all is dead
            this.Stop();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// The thread status
    /// 
    /// </summary>
    public class SafeThreadStatusClass : BasedObjectClass
    {
        #region Constructor
        internal SafeThreadStatusClass(SafeThreadClass thrd, string name, List<object> value)
            : base(thrd)
        {
            //
            this.Values = value;
            this.ThreadName = name;

            // Flag as never run before
            this.LastOn = DateTime.MinValue;
            // And activate
            this.IsActive = true;
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            // Stop
            this.ThisThread.Stop();

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The thread itself
        /// 
        /// </summary>
        public SafeThreadClass ThisThread { get { return this.Root as SafeThreadClass; } }

        /// <summary>
        /// 
        ///  The given name
        ///  
        /// </summary>
        public string ThreadName { get; private set; }

        /// <summary>
        /// 
        /// Is the thread active?  If false the thread should stop
        /// 
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 
        /// The parameters passed to the thread ast start
        /// 
        /// </summary>
        public List<object> Values { get; set; }

        /// <summary>
        /// 
        /// The thread ID
        /// 
        /// </summary>
        public string ThreadID { get { return this.ThisThread.ID; } }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// The last day that the chck was done
        /// 
        /// </summary>
        public DateTime LastOn { get; private set; }

        /// <summary>
        /// 
        /// Checks to see if the day has changed
        /// 
        /// </summary>
        /// <returns>True if it is a new day</returns>
        public bool IfNextDay()
        {
            // Chek for any hour
            return this.IfNextDay(-1);
        }

        /// <summary>
        /// 
        /// Checks to see if the day has changed
        /// 
        /// </summary>
        /// <param name="at"The first hour of the day to check></param>
        /// <returns></returns>
        public bool IfNextDay(int at)
        {
            // Assume that the day ghas changed
            bool bAns = true;

            // Do we have an hour to check after?
            if (at > -1)
            {
                // Check to see if on or after the hour
                bAns = at <= "".Now().Hour;
            }

            // Do we need to check?
            if (bAns)
            {
                // Is it a new day?
                bAns = this.LastOn != "".Today();
                // If so, save the day
                if (bAns) this.LastOn = "".Today();
            }

            //
            return bAns;
        }

        /// <summary>
        /// 
        /// Wait until a given date and time
        /// 
        /// </summary>
        /// <param name="on">The target date and time</param>
        public void WaitUntil(DateTime on)
        {
            // Do we have to?
            if (on.Ticks > "".Now().Ticks)
            {
                // Call the regular wait
                this.WaitFor("".Now().Subtract(on));
            }
        }

        /// <summary>
        /// 
        /// Wait for a given time span
        /// 
        /// </summary>
        /// <param name="wait"></param>
        public void WaitFor(TimeSpan wait)
        {
            // Call the helper
            wait.Sleep();
        }

        /// <summary>
        /// 
        /// Ends the current thread.  
        /// Should be called as the very last thing before
        /// the thread ends
        /// 
        /// </summary>
        public void End()
        {
            // End us
            SafeThreadManagerClass.StopThread(this.ThreadName);
        }
        #endregion
    }
}