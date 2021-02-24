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
using System.Reflection;
using System.Runtime.Loader;

using NX.Shared;

namespace NX.Engine
{
    public class ExtManagerClass<T> : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public ExtManagerClass(EnvironmentClass env)
            : base(env)
        {
            //
            this.Type = typeof(T);
        }
        #endregion

        #region Properties
        private Type Type { get; set; }

        /// <summary>
        /// 
        /// The objects found
        /// 
        /// </summary>
        private NamedListClass<Type> Cache { get; set; } = new NamedListClass<Type>();

        /// <summary>
        ///  The list of names
        ///  
        /// </summary>
        public List<string> Names
        {
            get { return new List<string>(this.Cache.Keys); }
        }

        /// <summary>
        /// 
        /// The context
        /// 
        /// </summary>
        private AssemblyLoadContext LoadCtx { get; set; }

        private AssemblyDependencyResolver Resolver { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// List of files loaded
        /// 
        /// </summary>
        private List<string> LoadedFiles { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// Loads all calls in itself
        /// </summary>
        /// <param name="log"></param>
        public void SelfLoad(ILogger log)
        {
            this.LoadFile(this.GetType().Assembly.GetName().Name + ".dll", log);
        }

        /// <summary>
        /// 
        /// Loads all the pugins in an assembly
        /// 
        /// </summary>
        /// <param name="assm">The assembly to load from</param>
        public void LoadFile(string file, ILogger log)
        {
            // Must have an assembly
            if (file.HasValue() && !this.LoadedFiles.Contains(file))
            {
                // Sometimes assemblies do not behave well
                try
                {
                    // Add to done
                    this.LoadedFiles.Add(file);

                    // Default
                    Assembly c_Assm = file.LoadAssembly(log);

                    // Did we get an assembly?
                    if (c_Assm != null)
                    {
                        // Get the types in the assembly
                        System.Type[] sc_Types = c_Assm.GetTypes();
                        // And do each one
                        foreach (Type c_Type in sc_Types)
                        {
                            // One bad type should not ruin the assembly
                            try
                            {
                                // Is the type one of ours?
                                if (!c_Type.IsInterface && c_Type.CanBeTreatedAsType(this.Type))
                                {
                                    // Create
                                    IPlugIn c_Plug = Activator.CreateInstance(c_Type) as IPlugIn;
                                    // Ws it created?
                                    if (c_Plug != null && !string.IsNullOrEmpty(c_Plug.Name))
                                    {
                                        // Adjust name
                                        string sName = c_Plug.ObjectFullName();

                                        // Make room
                                        if (this.Cache == null) this.Cache = new NamedListClass<Type>();

                                        // Add
                                        this.Cache[sName] = c_Type;

                                        //
                                        try
                                        {
                                            // And finally initialize
                                            c_Plug.Initialize(this.Parent);
                                        }
                                        catch { }

                                        //
                                        this.Parent.LogVerbose("Loaded {0}".FormatString(sName));
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Tell user
                    this.Parent.LogException("while loading {0}".FormatString(file), e);
                }
            }
        }

        /// <summary>
        /// 
        /// Returns the module for a given name
        /// 
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <returns>The module (if any)</returns>
        public T Get(string name)
        {
            // Assume none
            T c_Ans = default(T);

            // Is it a name we know?
            if (this.Exists(name))
            {
                // Get the type
                Type c_Type = this.Cache[name] as Type;
                // Is it valid plugin?
                if (c_Type != null)
                {
                    // Sometimes the creation goes awry
                    try
                    {
                        // Create
                        c_Ans = (T)Activator.CreateInstance(c_Type);
                    }
                    catch { }
                }
            }

            // The module (if any)
            return c_Ans;
        }

        /// <summary>
        /// 
        /// Checks to see if the name is valid
        /// 
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <returns>Trues if it is a name we know</returns>
        public bool Exists(string name)
        {
            return this.Cache.ContainsKey(name);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// The basic items in a plugin
    /// 
    /// </summary>
    public interface IPlugIn : IDisposable
    {
        /// <summary>
        /// 
        /// The name of the plugin
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// The code to initialize the plugin
        /// 
        /// </summary>
        /// <param name="env">Current environment</param>
        void Initialize(EnvironmentClass env);
    }
}