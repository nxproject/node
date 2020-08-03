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
using System.Reflection;

using NX.Shared;

namespace NX.Engine
{
    public class ExtManagerClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public ExtManagerClass(EnvironmentClass env, Type type)
            : base(env)
        {
            //
            this.Type = type;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Type of object kept
        /// 
        /// </summary>
        private Type Type { get; set; }

        /// <summary>
        /// 
        /// The objects found
        /// 
        /// </summary>
        private Dictionary<string, Type> Cache { get; set; } = new Dictionary<string, Type>();

        /// <summary>
        ///  The list of names
        ///  
        /// </summary>
        public List<string> Names
        {
            get { return new List<string>(this.Cache.Keys); }
        }

        private List<string> Loaded { get; set; } = new List<string>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Loads all the plug-ins
        /// 
        /// </summary>
        /// <param name="folders">A  list of extra folders to look at</param>
        public void Load(params string[] folders)
        {
            // Get the dynamic folder
            string sDynamic = this.Parent.DynamicFolder;

            // Make the list of directories that will be looked at
            List<string> c_Folders = new List<string>();
            c_Folders.Add("".WorkingDirectory());
            c_Folders.AddRange(folders);
            if (sDynamic.HasValue()) c_Folders.Add(sDynamic);

            // Do each folder
            foreach (string sFolder in c_Folders)
            {
                // Get the plug-ins in the folder
                this.LoadFolder(sFolder);
            }
        }

        /// <summary>
        /// 
        /// Loads the pugins in a folder
        /// 
        /// </summary>
        /// <param name="folder">FThe folder path</param>
        public void LoadFolder(string folder)
        {
            // Get all of the extension modules
            List<string> c_Modules = folder.GetMatchingFilesInPath("*.*.dll");

            // And do one at a time
            foreach (string sModule in c_Modules)
            {
                // Make a copy
                string sAssm = sModule;

                // Getthe file name
                string sName = sModule.GetFileNameFromPath();
                // Valid?
                if (sName.StartsWith("Fn.") ||
                    sName.StartsWith("Route.") ||
                    sName.StartsWith("Proc."))
                {
                    // Are we in MS Windows
                    if (!"".IsLinux())
                    {
                        // Sometimes MS Windows returns the assembly name in relative format
                        if (sModule.StartsWith("."))
                        {
                            // Must prefix with folder 
                            sAssm = folder.CombinePath(sModule.Substring(1));
                            // And adjust delimiters
                            if (this.Type.Module.FullyQualifiedName.IndexOf("/") == -1)
                            {
                                sAssm = sAssm.Replace("/", @"\");
                            }
                        }
                    }

                    // And load it
                    this.LoadFile(sAssm);
                }
            }
        }

        /// <summary>
        /// 
        /// Loads all the pugins in an assembly
        /// 
        /// </summary>
        /// <param name="assm">The assembly to load from</param>
        public void LoadFile(string file)
        {
            // Must have an assembly
            if (file.HasValue() && !this.Loaded.Contains(file))
            {
                // Sometimes assemblies do not behave well
                try
                {
                    // Add to done
                    this.Loaded.Add(file);

                    // Load the assembly
                    Assembly c_Assm = Assembly.LoadFile(file);

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
                                    if (this.Cache == null) this.Cache = new Dictionary<string, Type>();

                                    // Add (or replace)
                                    if (!this.Cache.ContainsKey(sName))
                                    {
                                        this.Cache.Add(sName, c_Type);
                                    }
                                    else
                                    {
                                        this.Cache[sName] = c_Type;
                                    }

                                    //
                                    try
                                    {
                                        // And finally initialize
                                        c_Plug.Initialize(this.Parent);
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// Returns the module for a given name
        /// 
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <returns>The module (if any)</returns>
        public object Get(string name)
        {
            // Assume none
            object c_Ans = null;

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
                        c_Ans = Activator.CreateInstance(c_Type);
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