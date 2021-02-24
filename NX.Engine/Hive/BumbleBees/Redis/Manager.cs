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

/// Packet Manager Requirements
/// 
/// Install-Package StackExchange.Redis -Version 2.1.58
/// 

using NX.Shared;
using StackExchange.Redis;

namespace NX.Engine.BumbleBees.Redis
{
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "redis")
        {
            //
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Remove
                if(this.DB != null)
                {
                    this.DB = null;
                }

                if (this.Client != null)
                {
                    this.Client.Dispose();
                    this.Client = null;
                }
                
                //
                if (isavailable)
                {
                    //
                    ConfigurationOptions c_Cfg = ConfigurationOptions.Parse(this.Location.RemoveProtocol());
                    this.Client = ConnectionMultiplexer.Connect(c_Cfg);

                    this.DB = this.Client.GetDatabase();
                }
            };

            // Bootstap
            this.CheckForAvailability();
        }
        #endregion

        #region Redis
        /// <summary>
        /// 
        /// The client
        /// 
        /// </summary>
        public ConnectionMultiplexer Client { get; private set; }

        /// <summary>
        /// 
        /// The database
        /// 
        /// </summary>
        public IDatabase DB { get; private set; }
        #endregion
    }
}