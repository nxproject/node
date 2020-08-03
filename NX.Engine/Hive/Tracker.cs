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

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// Tracks availability of tasks and ports
    /// 
    /// </summary>
    public class TrackerClass : ChildOfClass<HiveClass>
    {
        #region Constructor
        public TrackerClass(HiveClass hive, TrackType type, Action<string, List<string>> cb, 
                                params string[] matches)
            : base(hive)
        {
            // Save 
            this.Matches = new List<string>( matches);
            this.Callback = cb;

            // According to type
            switch (type)
            {
                case TrackType.DNA:
                    this.Parent.Roster.DNAChanged += delegate (string value, List<string> url)
                    {
                        // Is the a task we are looking for?
                        if (this.Matches.Contains(value))
                        {
                            if (this.Callback != null) this.Callback(value, url);
                        }
                    };
                    break;

                case TrackType.Port:
                    this.Parent.Roster.DNAChanged += delegate (string value, List<string> url)
                    {
                        // Is this a port we are looking for?
                        if (this.Matches.Contains(value))
                        {
                            if (this.Callback != null) this.Callback(value, url);
                        }
                    };
                    break;
            }
        }
        #endregion

        #region Enums
        public enum TrackType
        {
            DNA,
            Port
        }
        #endregion

        #region Properties
        public List<string> Matches { get; private set; }
        public Action<string, List<string>> Callback { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Like event trigger
        /// 
        /// </summary>
        /// <param name="value">The value to pass</param>
        /// <param name="url">The list of URLs</param>
        public void Trigger(string value, List<string> url)
        {
            if (this.Callback != null) this.Callback(value, url);
        }
        #endregion
    }
}