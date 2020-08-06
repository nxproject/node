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
using Microsoft.CodeAnalysis.VisualBasic;
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

        public const string GET_ROUTED = RouterClass.RoutedRoutePrefix + "GET";
        public const string POST_ROUTED = RouterClass.RoutedRoutePrefix + "POST";
        public const string PUT_ROUTED = RouterClass.RoutedRoutePrefix + "PUT";
        public const string DELETE_ROUTED = RouterClass.RoutedRoutePrefix + "DELETE";
        public const string PATCH_ROUTED = RouterClass.RoutedRoutePrefix + "PATCH";

        public const string GET_ROUTED_SECURE = RouterClass.SecureRoutePrefix + RouterClass.RoutedRoutePrefix + "GET";
        public const string POST_ROUTED_SECURE = RouterClass.SecureRoutePrefix + RouterClass.RoutedRoutePrefix + "POST";
        public const string PUT_ROUTED_SECURE = RouterClass.SecureRoutePrefix + RouterClass.RoutedRoutePrefix + "PUT";
        public const string DELETE_ROUTED_SECURE = RouterClass.SecureRoutePrefix + RouterClass.RoutedRoutePrefix + "DELETE";
        public const string PATCH_ROUTED_SECURE = RouterClass.SecureRoutePrefix + RouterClass.RoutedRoutePrefix + "PATCH";

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
        /// followed by the URL entries.
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