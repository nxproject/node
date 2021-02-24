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
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    public class RouteLayerClass : ChildOfClass<RouterClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        public RouteLayerClass(RouterClass router)
            : base(router)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The name of the route to call
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// A dictionary of any sub-layers that can be called
        /// 
        /// </summary>
        public NamedListClass<RouteLayerClass> Routes { get; set; } = new NamedListClass<RouteLayerClass>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Adds a route to the layer
        /// 
        /// </summary>
        /// <param name="route">The route to call</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <param name="at">Position in the tree that we are working with</param>
        public void Add(RouteClass route, List<string> nodes, int at = 0)
        {
            // Must have nodes
            if (nodes != null && nodes.Count > 0)
            {
                // Are we done?
                if (at < nodes.Count)
                {
                    // Get the tree entry
                    string sEntry = nodes[at];

                    // New?
                    if (!this.Routes.ContainsKey(sEntry))
                    {
                        // Add a layer
                        this.Routes[sEntry] = new RouteLayerClass(this.Parent);
                    }

                    // Call sub-layer to keep processing
                    this.Routes[sEntry].Add(route, nodes, at + 1);
                }
                else
                {
                    // Yes, store route name
                    this.Name = route.Name;
                }
            }
        }

        /// <summary>
        /// 
        /// Removes a route from the tree
        /// 
        /// </summary>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <param name="at">Position in the tree that we are working with</param>
        public void Remove(List<string> nodes, int at = 0)
        {
            // Must have nodes
            if (nodes != null && nodes.Count > 0)
            {
                // Are we done?
                if (at < nodes.Count)
                {
                    // Get the tree entry
                    string sEntry = nodes[at];

                    // New?
                    if (this.Routes.ContainsKey(sEntry))
                    {
                        // Move down the tree
                        at++;

                        // Would we be done?
                        if (at < nodes.Count)
                        {
                            // Call sub-layer to keep processing
                            this.Routes[sEntry].Remove(nodes, at);
                        }
                        else
                        {
                            // Yes, remove entry
                            this.Routes.Remove(sEntry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// Gets the route name to be called
        /// 
        /// </summary>
        /// <param name="values">The passed parameters</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <param name="at">Position in the tree that we are working with</param>
        /// <returns>The route name</returns>
        public string Get(StoreClass values, List<string> nodes, int at = 0)
        {
            // Assume none
            string sAns = null;

            // Must have nodes
            if (nodes != null && nodes.Count > 0)
            {
                // Are we done?
                if (at < nodes.Count)
                {
                    // Get the tree entry
                    string sEntry = nodes[at];

                    // Is the entry the security key?
                    if(sEntry.IsSameValue(this.Parent.SecureCode) &&
                        !sEntry.IsSameValue(RouterClass.UnsecureCode))
                    {
                        // Convert
                        sEntry = RouteClass.SecureRoutePrefix;
                    }

                    // Sub-layer exists?
                    if (this.Routes.ContainsKey(sEntry))
                    {
                        // Process
                        sAns = this.Routes[sEntry].Get(values, nodes, at + 1);
                    }

                    // Do we have a route?
                    if (!sAns.HasValue())
                    {
                        // No handle like :xxx
                        sAns = this.HandleParam(sEntry, values, nodes, at);

                        // Do we have a route?
                        if (!sAns.HasValue())
                        {
                            // No, handle like a ?xxx
                            sAns = this.HandleOptional(values, nodes, at);
                        }
                    }
                }
                else
                {
                    // Yes, get the route name
                    sAns = this.Name;

                    // Do we have one?
                    if (!sAns.HasValue())
                    {
                        // No, handle like a ?xxx
                        sAns = this.HandleOptional(values, nodes, at);
                    }
                }
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Is the route defined?
        /// 
        /// </summary>
        /// <param name="route">Route</param>
        /// <returns>True if it is defined</returns>
        public bool Contains(string route)
        {
            return this.Routes.ContainsKey(route);
        }

        /// <summary>
        /// 
        /// Processes variable (:xxx) parameters
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="values">The passed parameters</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <param name="at">Position in the tree that we are working with</param>
        /// <returns>The route name</returns>
        /// <returns></returns>
        private string HandleParam(string entry, StoreClass values, List<string> nodes, int at = 0)
        {
            // Assume none
            string sAns = null;

            // Check each sub-layer
            foreach (string sPoss in this.Routes.Keys)
            {
                // Is it an variable one?
                if (sPoss.StartsWith(":"))
                {
                    // Can we process it?
                    sAns = this.Routes[sPoss].Get(values, nodes, at + 1);
                    // Got a route
                    if (sAns.HasValue())
                    {
                        // Store the variable in the parameteres store
                        values.Add(this.ParameterName(sPoss), entry);
                        // And stop checking sub-layers
                        break;
                    }
                }
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Processes optional (?xxx) parameters
        /// 
        /// </summary>
        /// <param name="values">The passed parameters</param>
        /// <param name="nodes">The list of URL params.  See RouteCall/RouteTree</param>
        /// <param name="at">Position in the tree that we are working with</param>
        /// <returns>The route name</returns>
        private string HandleOptional(StoreClass values, List<string> nodes, int at = 0)
        {
            // Assume none
            string sAns = null;

            // Check each sub-layer
            foreach (string sPoss in this.Routes.Keys)
            {
                // Is it an optional one?
                if (sPoss.StartsWith("?"))
                {
                    // Is it a reppeating optional?
                    if (sPoss.EndsWith("?"))
                    {
                        // Get the parameter name
                        string sName = this.ParameterName(sPoss);

                        do
                        {
                            // Assume no value
                            string sEntry = "";
                            // If we are still valid, get the value
                            if(at < nodes.Count) sEntry = nodes[at];
                            // Ans put into store
                            values.Add(sName, sEntry);
                            // Next
                            at++;
                        }
                        while (at < nodes.Count);

                        // Force into a JSON array
                        JArray c_P = values.GetAsJArray(sName);
                        values.Set(sName, c_P);

                        // Get the preious sub-layer route
                        sAns = this.Routes[sPoss].Name;
                        // And stop checking sub-layers
                        break;
                    }
                    else
                    {
                        // See if we can process sub
                        sAns = this.Routes[sPoss].Get(values, nodes, at + 1);

                        // Got a route
                        if (sAns.HasValue())
                        {
                            // Store the optional in the parameteres store
                            values.Add(this.ParameterName(sPoss), "");
                            // And stop checking sub-layers
                            break;
                        }
                    }
                }
            }

            // The route name, if any
            return sAns;
        }

        /// <summary>
        /// 
        /// Parse paraments and handle :xxx, ?xxx and ?xxx? options
        /// 
        /// </summary>
        /// <param name="name">The actual parameter name</param>
        /// <returns></returns>
        private string ParameterName(string name)
        {
            // Any name?
            if (name.HasValue())
            {
                // Always remove first character
                name = name.Substring(1);
                // Is it ?xxx?
                if (name.EndsWith("?"))
                {
                    // Remove trailing ?
                    name = name.Substring(0, name.Length - 1);
                }
            }

            // parsed name
            return name;
        }

        /// <summary>
        /// 
        /// Return the layer as a JSON object.
        /// Used for documentation purposes
        /// 
        /// </summary>
        /// <returns>The JSON object that is the equivalent of the layer</returns>
        public JObject AsJObject()
        {
            JObject c_Ans = new JObject();

            if (this.Routes.Keys.Count > 0)
            {
                foreach (string sKey in this.Routes.Keys)
                {
                    c_Ans.Set(sKey, this.Routes[sKey].AsJObject());
                }
            }
            else
            {
                c_Ans.Set("=>", this.Name);
            }

            return c_Ans;
        }
        #endregion
    }
}