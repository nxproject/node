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
            // Only if a genome is given
            if (genome.HasValue())
            {
                // Tell world
                this.Parent.LogVerbose("{0} bumble bee created".FormatString(genome));

                // Save
                this.Genome = genome;

                // Track queen changes
                this.Parent.Hive.Roster.QueenChanged += delegate ()
                {
                // Signal
                this.SignalQueenChange();
                };

                // Track task changes
                this.Tracker = new Hive.TrackerClass(this.Parent.Hive, TrackerClass.TrackType.DNA,
                    delegate (string value, List<string> url)
                    {
                    // Reset
                    this.Location = null;
                        this.Field = null;
                        this.Bee = null;

                    // Did we get someone to talk to?
                    if (url.Count > 0)
                        {
                        // Set the location
                        this.Location = url[0];
                        // And now the field from the location
                        this.Field = this.Parent.Hive.FieldFromLocation(this.Location);
                        // Do we have one?
                        if (this.Field != null)
                            {
                            // And the bee
                            this.Bee = this.Field.BeeFromLocation(this.Location);
                            }
                        }

                    //
                    this.Parent.LogInfo("{0} genome bumble bee is {1}available".FormatString(this.Genome, this.Location.HasValue() ? "" : "not "));

                    // Tell the world
                    this.AvailabilityChanged?.Invoke(this.IsAvailable);

                    }, this.Genome);

                // Handle the setup
                this.Parent.Hive.SetupCompleted += delegate (bool hassetup)
                {
                    this.Parent.Hive.AssureDNACount(this.Genome, 1);
                };

                // And once we are running
                if (this.Parent.Hive.HasSetup)
                {
                    // And assure at least one
                    this.Parent.Hive.AssureDNACount(this.Genome, 1);
                }
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
        /// The bee itself
        /// 
        /// </summary>
        public BeeClass Bee { get; private set; }

        /// <summary>
        /// 
        /// The field the bee is at
        /// 
        /// </summary>
        public FieldClass Field { get; private set; }

        /// <summary>
        /// 
        /// Returns true manager is available
        /// 
        /// </summary>
        public virtual bool IsAvailable
        {
            get
            {
                // Do we have a location?
                return this.Location.HasValue();
            }
        }

        /// <summary>
        /// 
        /// Are we the queen?
        /// 
        /// </summary>
        public bool IsQueen
        {
            get
            {
                bool bIsQueen = false;
                // Do we have both a me and queen bees?
                if (this.Parent.Hive.Roster.MeBee != null)
                {
                    // One and the same?
                    bIsQueen = this.Parent.Hive.Roster.MeBee.IsSameAs(this.Parent.Hive.Roster.QueenBee);
                }

                return bIsQueen;
            }
        }

        /// <summary>
        /// 
        /// Tracker
        /// 
        /// </summary>
        private Hive.TrackerClass Tracker { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Waits for the bumble bee to become available
        /// 
        /// </summary>
        public void Wait()
        {
            // Is it available?
            while (!this.IsAvailable)
            {
                // Sleep
                5.SecondsAsTimeSpan().Sleep();
            }
        }

        /// <summary>
        /// 
        /// Bootstrap check
        /// 
        /// </summary>
        public void CheckForAvailability()
        {
            // Protect
            try
            {
                // Tracking?
                if (this.Tracker!= null)
                {
                    // Check current
                    this.Tracker.Trigger(this.Genome, this.Parent.Hive.Roster.GetLocationsForDNA(this.Genome));
                }
            }
            catch (Exception e)
            {
                this.Parent.LogException(e);
            }
        }

        /// <summary>
        /// 
        /// Invokes the QueenChanged event
        /// 
        /// </summary>
        public void SignalQueenChange()
        {
            // Call
            this.QueenChanged?.Invoke(this.IsQueen);
        }

        /// <summary>
        /// 
        /// Sets the location and rewite parameters
        /// 
        /// </summary>
        /// <param name="location">The location, overrides genome</param>
        /// <param name="rewrite">If true, the location will be removed from the URL</param>
        public void SetNginxInformation(string location, bool rewrite)
        {
            // Make
            var c_Info = new NginX.InformationClass(this.Parent.NginXInfo, location, rewrite);
            // Set
            this.Parent.NginXInfo[this.Genome, NginX.ServicesClass.Types.BumbleBee] = c_Info;
            // And update environment
            this.Parent.Add("routing_bumble", this.Genome + "=" + location);
        }
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for the AvailabilityChanged event
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
        /// The delegate for the QueenChanged event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnCQhangedHandler(bool isqueen);

        /// <summary>
        /// 
        /// Defines the event to be raised when the queen changes
        /// 
        /// </summary>
        public event OnCQhangedHandler QueenChanged;
        #endregion
    }
}