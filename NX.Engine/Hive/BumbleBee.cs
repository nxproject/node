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

using System.Collections.Generic;

using NX.Engine.Hive;
using NX.Shared;

namespace NX.Engine
{
    public class BumbleBeeClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public BumbleBeeClass(EnvironmentClass env, string genome)
            : base(env)
        {
            // Save
            this.Genome = genome;

            // Track task changes
            this.Tracker = new Hive.TrackerClass(this.Parent.Hive, TrackerClass.TrackType.DNA,
                delegate (string value, List<string> url)
            {
                    // Reset
                    this.Location = null;
                    // Did we get someone to talk to?
                    if (url.Count > 0) this.Location = url[0];

                    //
                    this.Parent.LogInfo("{0} genome bumble bee is {1}available", this.Genome, this.Location.HasValue() ? "" : "not ");

                    // Tell the world
                    this.AvailabilityChanged?.Invoke(this.IsAvailable);

            }, this.Genome);

            // Check current
            this.Tracker.Trigger(this.Genome, this.Parent.Hive.Roster.GetLocationsForDNA(this.Genome));

            // And once we are running
            if (this.Parent.Hive.BeeCount > 0)
            {
                // And assure at least one
                this.Parent.Hive.AssureDNACount(this.Genome, 1);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The genome of the bee
        /// 
        /// </summary>
        public string Genome { get; private set; }

        /// <summary>
        /// 
        /// The URL to the task
        /// 
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// 
        /// Returns true manager is available
        /// 
        /// </summary>
        public virtual bool IsAvailable { get { return this.Location.HasValue(); } }

        /// <summary>
        /// 
        /// Tracker
        /// 
        /// </summary>
        private Hive.TrackerClass Tracker { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for all of the events
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
        #endregion
    }
}