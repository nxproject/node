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

using System.Collections.Concurrent;

using NX.Engine;
using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// Traefik interface
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "redis")
        {
            // Get the hives
            ItemsClass c_Hives = new ItemsClass(this.Parent.GetAsJArray("hive_traefik"));
            // Any?
            if (c_Hives.Count > 0)
            {
                // Save the name
                Proc.Traefik.CommandClass.TraefikHive = c_Hives[0].Priority;
            }

            // Are we the hive that holds the bumble bee?
            if (this.Hive.IsSameValue(this.Parent.Hive.Name))
            {
                // Make it
                this.BumbleBee = new BumbleBeeClass(this.Parent, "traefik");

                // Handle the events
                this.BumbleBee.AvailabilityChanged += delegate (bool isavailable)
                {
                    // Check
                    this.MakeHandler(isavailable && this.BumbleBee.IsQueen && this.IsAvailable);
                };

                // Track queen changes
                this.BumbleBee.QueenChanged += delegate (bool isqueen)
                {
                    // Check
                    this.MakeHandler(isqueen && this.IsAvailable);
                };

                // Bootstap
                this.BumbleBee.CheckForAvailability();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Is the buble bee available?
        /// 
        /// </summary>
        public override bool IsAvailable => this.BumbleBee != null && this.BumbleBee.IsAvailable;

        /// <summary>
        /// 
        /// The hive that holds traefik
        /// 
        /// </summary>
        public string Hive { get { return Proc.Traefik.CommandClass.TraefikHive; } }

        /// <summary>
        /// 
        /// The bumble bee.  Found only in one hive
        /// 
        /// </summary>
        public BumbleBeeClass BumbleBee { get; private set; }

        /// <summary>
        /// 
        /// Traefik interface
        /// 
        /// </summary>
        public InterfaceClass Interface { get; private set; }
        #endregion

        #region Methods
        private void MakeHandler(bool isavailable)
        {
            // Clear
            if (this.Interface != null)
            // Available?l
            {
                this.Interface.Dispose();
                this.Interface = null;
            }

            // Available?
            if (isavailable)
            {
                // Make
                this.Interface = new InterfaceClass(this);
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// 
        /// Creates a command
        /// 
        /// </summary>
        /// <returns>The command</returns>
        public CommandClass New()
        {
            // Make
            CommandClass c_Ans = new CommandClass(this.Interface);

            // Setup
            c_Ans.From = this.Parent.Hive.Name;

            return c_Ans;
        }
        #endregion

        #region Methods
        #endregion
    }
}