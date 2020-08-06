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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// This class handles the routing of URL calls
    /// to a pre-defined set of routes.
    /// 
    /// </summary>
    public class RouterClass : ExtManagerClass
    {
        #region Constants
        public const string SecureRoutePrefix = "@";
        public const string RoutedRoutePrefix = "^";
        public const string UnsecureCode = "unsecured";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public RouterClass(EnvironmentClass env)
            : base(env, typeof(RouteClass))
        {
            // Make the root layer
            this.Routes = new RouteLayerClass(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The routing table
        /// 
        /// </summary>
        private RouteLayerClass Routes { get; set; }

        /// <summary>
        /// 
        /// The current security code
        /// 
        /// </summary>
        public string SecureCode
        {
            get { return this.Parent[EnvironmentClass.KeySecureCode].IfEmpty(UnsecureCode); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Adds a route using the default RouteTree values
        /// 
        /// </summary>
        /// <param name="route">The route to add</param>
        public void Add(RouteClass route)
        {
            this.Add(route, route.RouteTree);
        }

        /// <summary>
        /// 
        /// Adds a route
        /// Note that this call is used when a route is called using a different URL call
        /// 
        /// </summary>
        /// <param name="route">The route to add</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        public void Add(RouteClass route, List<string> nodes)
        {
            // Must have a tree
            if (nodes != null && nodes.Count > 0)
            {
                // Get the first entry
                string sAt = nodes[0];

                // Is this a secured route?
                if (sAt.StartsWith(RouterClass.SecureRoutePrefix))
                {
                    // Change it to normal
                    sAt = sAt.Substring(1);
                    // And add secure to the route
                    if (nodes.Count == 1)
                    {
                        // Add
                        nodes.Add(RouterClass.SecureRoutePrefix);
                    }
                    else
                    {
                        // Insert
                        nodes.Insert(1, RouterClass.SecureRoutePrefix);
                    }
                }

                // Routed?
                if(sAt.StartsWith(RouterClass.RoutedRoutePrefix))
                {
                    // Get the module name
                    string sModule = route.ModuleName().ToLower();
                    // Change it to normal
                    sAt = sAt.Substring(1);
                    // And add secure to the route
                    if (nodes.Count == 1)
                    {
                        // Add
                        nodes.Add(sModule);
                    }
                    else
                    {
                        // Insert
                        nodes.Insert(1, sModule);
                    }
                }

                // Put back resolved
                nodes[0] = sAt;

                // Make the substitution store
                using (StoreClass c_Fmt = new StoreClass(this.Parent.AsParameters))
                {
                    // Adjust the nodes to handle the delimiter
                    List<string> c_Nodes = new List<string>();
                    // Loop thru
                    foreach (string sPiece in nodes)
                    {
                        // Handle
                        c_Nodes.AddRange(sPiece.Split("/", System.StringSplitOptions.RemoveEmptyEntries));
                    }

                    // Format each node
                    for (int i = 0; i < c_Nodes.Count; i++)
                    {
                        //
                        c_Nodes[i] = c_Fmt.Format(c_Nodes[i]);
                    }

                    // Add using top layer
                    this.Routes.Add(route, c_Nodes);
                }
            }
        }

        /// <summary>
        /// 
        /// Removes a route
        /// 
        /// </summary>
        /// <param name="route">The route to remove</param>
        public void Remove(RouteClass route)
        {
            this.Remove(route.RouteTree);
        }

        /// <summary>
        /// 
        /// Removes a tree entry
        /// 
        /// </summary>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        public void Remove(List<string> nodes)
        {
            // Must have a tree
            if (nodes != null && nodes.Count > 0)
            {
                // Remove using top layer
                this.Routes.Remove(nodes);
            }
        }

        /// <summary>
        /// 
        /// Returns the route (code) 
        /// 
        /// </summary>
        /// <param name="values">The parameters from the HTTP call</param>
        /// <param name="method">The HTTP method</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <returns></returns>
        public RouteClass Get(StoreClass values, string method, List<string> nodes)
        {
            // Assume none
            RouteClass c_Ans = null;

            // Build a temporary tree
            List<string> c_Tree = new List<string>() { method };
            // Add the tree which could be empty
            if (nodes != null) c_Tree.AddRange(nodes);

            // First try the method passed
            string sRoute = this.Routes.Get(values, c_Tree);

            // No route?
            if (!sRoute.HasValue())
            {
                // Try wildcard
                c_Tree[0] = RouteClass.ANY;
                // And process
                sRoute = this.Routes.Get(values, c_Tree);
            }

            // And get the code
            if (sRoute.HasValue()) c_Ans = (RouteClass)this.Get(sRoute);

            // And pass back
            return c_Ans;
        }

        /// <summary>
        /// 
        /// Return routes as a JSON object.
        /// Used for documentation purposes
        /// 
        /// </summary>
        /// <returns>The JSON object that is the equivalent of the layer</returns>
        public JObject AsJObject()
        {
            return this.Routes.AsJObject();
        }

        /// <summary>
        /// 
        /// Is the route defined?
        /// 
        /// </summary>
        /// <param name="route">Route</param>
        /// <returns>True if it is defined</returns>
        public bool IsDefined(string route)
        {
            return this.Routes.Contains(route);
        }
        #endregion
    }
}