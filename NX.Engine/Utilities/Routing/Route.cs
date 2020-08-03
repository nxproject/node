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

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Base class for a route
    /// 
    /// </summary>
    public class RouteClass : IPlugIn
    {
        /// <summary>
        /// 
        /// The HTTP Methods available
        /// 
        /// </summary>
        #region Constants
        public const string GET = "GET";
        public const string POST = "POST";
        public const string PUT = "PUT";
        public const string DELETE = "DELETE";
        public const string PATCH = "PATCH";

        public const string GET_SECURE =  RouterClass.SecureRoutePrefix + "GET";
        public const string POST_SECURE = RouterClass.SecureRoutePrefix + "POST";
        public const string PUT_SECURE = RouterClass.SecureRoutePrefix + "PUT";
        public const string DELETE_SECURE = RouterClass.SecureRoutePrefix + "DELETE";
        public const string PATCH_SECURE = RouterClass.SecureRoutePrefix + "PATCH";

        public const string ANY = "*";
        #endregion

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        #region Constructor
        public RouteClass()
        { }
        #endregion

        #region IPlugIn
        /// <summary>
        /// 
        /// The name of the route.  Note that the system generates
        /// the name from the assembly and instance.  If the assembly
        /// is called Route.Sample and the instance is called CallX
        /// the name would be Sample.CallX
        /// 
        /// </summary>
        public string Name
        {
            get { return this.ObjectFullName(); }
        }

        /// <summary>
        /// 
        /// Code to be run when the route is loaded, once per
        /// session.
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public virtual void Initialize(EnvironmentClass env)
        {
            env.Router.Add(this);
        }

        /// <summary>
        /// 
        /// Code to run when the code is disposed
        /// 
        /// </summary>
        public virtual void Dispose()
        { }
        #endregion

        #region Routing
        /// <summary>
        /// 
        /// The entries in the URL that determine if the route
        /// is called.  The first entry is the HTTP method (GET/POST),
        /// followed by the URL entries.  There are four options for
        /// each entry:
        /// 
        /// text - The text will be matched
        /// :field - The text will be stored in the store for the call
        /// ?field - Optional entry.  If text is provided, it will be treated like :field
        /// ?field? - Multiple optional entries.  All of the entries will be stored as a JSON array
        /// 
        /// Examples:
        /// 
        /// "GET", "echo" 
        /// Triggered when /echo is called via a GET
        /// 
        /// "POST", "info", "sample"
        /// Triggered when /info/sample is called via a POST
        /// 
        /// "GET", "user", ":name"
        /// Triggered when /user/mike is called via a GET.  {"name: "mike"} is made part of the store
        /// 
        /// "GET", "site", "?siteid"
        /// Triggered when /site/alpha is called via a GET. {"siteid": "alpha"} is made part of the store
        /// Triggered when /site is called via a GET. {"siteid": ""} is made part of the store
        /// 
        /// "GET", "file", "?path?"
        /// Triggered when /file/folder1/folder2/sample.txt is called via a GET {"path": ["folder1", "folder2", "sample.txt"]} is made part of the store
        ///
        /// Note that any parameter passed in the URL is also passed in the store, for example:
        /// 
        /// /echo?name=john
        /// {"name": "john"} is passed in the store
        /// 
        /// /echo?name=john&size=medium
        /// "name": "john", "size": "medium"} are passed in the store
        /// 
        /// /site/alpha?name=john
        /// {"siteid": "alpha", "name": "john"} are passed in the store
        /// 
        /// /echo?name=john&size=medium&name=mike&size=large
        /// "name": ["john", mike"], "size": ["medium", "large"]} are passed in the store
        /// 
        /// /file/sample.txt?path=folder1&path=folder2
        /// {path: ["folder1", "folder2", "sample.txt"]} is passed in the store
        /// 
        /// </summary>
        public virtual List<string> RouteTree { get { return new List<string>() { this.Name }; } }

        /// <summary>
        /// 
        /// Code called when the route is triggered
        /// 
        /// </summary>
        /// <param name="call">The call object that received the call</param>
        /// <param name="store">The store where the URL params are stored</param>
        public virtual void Call(HTTPCallClass call, StoreClass store)
        { }
        #endregion
    }
}