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

using System.Collections.Generic;

using NX.Shared;

namespace NX.Engine.NginX
{
    /// <summary>
    /// 
    /// NginX route map
    /// 
    /// </summary>
    public class ServicesClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public ServicesClass(EnvironmentClass env)
            : base(env)
        { }
        #endregion

        #region Enums
        public enum Types
        {
            Proc,
            BumbleBee
        }
        #endregion

        #region Indexer
        public InformationClass this[string name, Types type]
        {
            get
            {
                // Make the name
                string sName = name + "." + type;

                // Assume none
                InformationClass c_Ans = this.Data[sName];

                // If none,  make temp
                if (c_Ans == null) c_Ans = new InformationClass(this, "", type != Types.Proc);

                return c_Ans;
            }
            set
            {
                // Make the name
                string sName = name + "." + type;

                // Any?
                if (value == null)
                {
                    // Do we have it?
                    if (this.Data.ContainsKey(sName))
                    {
                        // Delete
                        this.Data.Remove(sName);
                        // Tell the world
                        this.InformationChanged?.Invoke(name, type);
                    }
                }
                else
                {
                    // Add
                    this.Data[sName] = value;

                    // Tell the world
                    this.InformationChanged?.Invoke(name, type);
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// A table of all information
        /// 
        /// </summary>
        private NamedListClass<InformationClass> Data { get; set; } = new NamedListClass<InformationClass>();
        #endregion

        #region Events
        /// <summary>
        /// 
        /// The delegate for the InformationChanged event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnChangedHandler(string name, Types type);

        /// <summary>
        /// 
        /// Defines the event to be raised when the information changes
        /// 
        /// </summary>
        public event OnChangedHandler InformationChanged;
        #endregion
    }
}