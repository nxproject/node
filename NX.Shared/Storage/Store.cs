///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
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
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace NX.Shared
{
    /// <summary>
    /// 
    /// The basic store object.  Can be thought as a JSON object
    /// but it has a few interesting additions
    /// 
    /// </summary>
    public class StoreClass : IDisposable
    {
        #region Constants
        /// <summary>
        /// 
        /// The entry that holds  a push/pop stack
        /// 
        /// </summary>
        private const string StackStore = "_stack";

        /// <summary>
        /// 
        /// Delimiter to indicate a stacked name
        /// 
        /// </summary>
        private const string Delimiter = ".";

        /// <summary>
        /// 
        /// When parsing, if no key use this as key
        /// 
        /// </summary>
        private const string DefaultKey = "args";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor (plain)
        /// 
        /// </summary>
        public StoreClass()
        {
            this.SynchObject = new JObject();
        }

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="values">The values as a JObject</param>
        public StoreClass(JObject values)
        {
            this.SynchObject = values.Clone();
        }

        /// <summary>
        /// 
        /// Constructor (basic)
        /// 
        /// </summary>
        /// <param name="value">The value to use as the stating contents</param>
        /// <param name="indeserialize">If true, we are in a desisialization loop</param>
        public StoreClass(string value)
        {
            // Normal create
            this.SynchObject = value.ToJObject();

            // Assure that we have something to work with
            if (this.SynchObject == null) this.SynchObject = new JObject();
        }

        /// <summary>
        /// 
        /// Constuctor
        /// 
        /// </summary>
        /// <param name="values">The string array of key/values pair</param>
        public StoreClass(params string[] values)
        {
            this.SynchObject = new JObject();

            // Loop thru
            for (int i = 0; i < values.Length; i += 2)
            {
                // Set
                this[values[i]] = values[i + 1];
            }
        }
        #endregion

        #region Enums
        /// <summary>
        /// 
        /// The different parsing methods
        /// 
        /// </summary>
        public enum ParseTypes
        {
            Spaces,
            URL,
            CommandLine
        }
        
        public enum SetOptions
        {
            OK,                 // Process normally
            Discard,            // Do not set
            SaveButLocal,       // Set but do not propagate
            Skip,               // Do nothing
            SendOnly            // Only send to synch
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public virtual void Dispose()
        { }
        #endregion

        #region Indexer
        /// <summary>
        /// 
        /// Accessor
        /// 
        /// </summary>
        /// <param name="key">The key to get the value for</param>
        /// <returns>The value or an empty string</returns>
        public virtual string this[string key]
        {
            get
            {
                // Assume none
                string sAns = null;

                // Does the key have a delimiter?
                int iPos = key.IndexOf(Delimiter);
                if (iPos != -1)
                {
                    // Get the first portion of the key as a store
                    StoreClass c_Store = this.GetAsStore(key.Substring(0, iPos));
                    // And get the value in that store
                    sAns = c_Store[key.Substring(iPos + Delimiter.Length)];
                }
                else
                {
                    // Nothing else to recurse to
                    sAns = this.GetAsString(key);
                }

                // Assure empty string if none
                return sAns.IfEmpty();
            }
            set
            {
                if (key.HasValue())
                {
                    // Does the key have a delimiter?
                    int iPos = key.IndexOf(Delimiter);
                    if (iPos != -1)
                    {
                        // Get the first portion of the key as a store
                        StoreClass c_Store = this.GetAsStore(key.Substring(0, iPos));
                        // And set the value there
                        c_Store[key.Substring(iPos + Delimiter.Length)] = value;
                    }
                    else
                    {
                        // Assure
                        value = value.IfEmpty();

                        // Is it a remove?
                        if (value.StartsWith("-"))
                        {
                            // Get as array
                            JArray c_Values = this.GetAsJArray(key);
                            // Remove
                            c_Values.Remove(value.Substring(1));
                            // And put back
                            this.Set(key, c_Values);
                        }
                        else if (value.StartsWith("+"))
                        {
                            // Get as array
                            JArray c_Values = this.GetAsJArray(key);
                            // Adjust value
                            string sAdj = value.Substring(1);
                            // Add if not already there
                            if (!c_Values.Contains(sAdj))
                            {
                                c_Values.Add(sAdj);
                                // And put back
                                this.Set(key, c_Values);
                            }
                        }
                        else
                        {
                            // Nothing else to recurse to
                            this.Set(key, value);
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The underlying JSON object
        /// 
        /// </summary>
        public JObject SynchObject { get; private set; }

        /// <summary>
        /// 
        /// The list of keys in the underlying object
        /// </summary>
        public List<string> Keys
        {
            get { return this.SynchObject.Keys(); }
        }

        public List<string> AsCommandLine
        {
            get { return this.SynchObject.AsCommandLine(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Stringify
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //
            return this.SynchObject.ToSimpleString();
        }

        /// <summary>
        /// 
        /// Parses a string into key/value pairs
        /// 
        /// The following parsing methods are available:
        /// 
        /// Spaces          - The string are key/value pairs delimited by spaces.
        ///                   Any key and/or value can contain spaces if the item
        ///                   is surrounded by " or '
        ///                   
        ///                   If the key is used multiple times, the value is 
        ///                   converted to a JSON array
        ///                   
        /// CommandLine     - The string are keys and values delimited by spaces,
        ///                   Keys are defined by prefixed the key name with --,
        ///                   otherwise a value is assumed.
        ///                   
        ///                   If the key is used multiple times, the value is 
        ///                   converted to a JSON array
        /// </summary>
        /// <param name="value">The string to be parsed</param>
        /// <param name="type">The type of parsing to be done</param>
        public void Parse(string value, ParseTypes type = ParseTypes.Spaces)
        {
            // Workarea
            List<string> c_Pieces = null;
            string sKey = null;
            
            // And depending on the type of parsing
            switch (type)
            {
                case ParseTypes.Spaces:
                    // Split
                    c_Pieces = value.SplitSpaces(true);
                    // And go thru each key/value pair
                    for (int i = 0; i < c_Pieces.Count; i += 2)
                    {
                        // The first is the key, the second the value
                        this.Add(c_Pieces[i], c_Pieces[i + 1]);
                    }
                    break;


                case ParseTypes.CommandLine:
                    // Split
                    c_Pieces = value.SplitSpaces(true);
                    // Loop thru
                    foreach (string sPiece in c_Pieces)
                    {
                        // Key?
                        if (sPiece.StartsWith("--"))
                        {
                            // Save for later
                            sKey = sPiece.Substring(2);
                        }
                        else
                        {
                            // Nope, value so store it
                            this.Add(sKey.IfEmpty(DefaultKey), sPiece);
                        }
                    }
                    break;

                case ParseTypes.URL:
                    // An URL is & delimited
                    ItemsClass c_Items = new ItemsClass(value, new ItemDefinitionClass()
                    {
                        ItemDelimiter = "&"
                    });
                    foreach (ItemClass c_Item in c_Items)
                    {
                        // Store
                        this.Add(c_Item.Key, c_Item.Value.IfEmpty().URLDecode());
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// Parses a string array, like the parsed command line
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void Parse(string[] args, bool skipnd = false)
        {
            // Start with no key
            string sKey = null;

            // Loop thru
            foreach (string sValue in args)
            {
                // Key?
                if (sValue.StartsWith("--"))
                {
                    // Save
                    sKey = sValue.Substring(2).IfEmpty(DefaultKey);
                }
                else if (sKey.HasValue())
                {
                    // Is it a remove?
                    if (sValue.StartsWith("-"))
                    {
                        // Get as array
                        JArray c_Values = this.GetAsJArray(sKey);
                        // Remove
                        c_Values.Remove(sValue.Substring(1));
                        // And put back
                        this.Set(sKey, c_Values);
                    }
                    else if (sValue.StartsWith("+"))
                    {
                        // Get as array
                        JArray c_Values = this.GetAsJArray(sKey);
                        // Adjust value
                        string sAdj = sValue.Substring(1);
                        // Add if not already there
                        if (!c_Values.Contains(sAdj))
                        {
                            c_Values.Add(sAdj);
                            // And put back
                            this.Set(sKey, c_Values);
                        }
                    }
                    else if (skipnd && sValue.StartsWith("{") && sValue.EndsWith("}"))
                    { }
                    else
                    {
                        // Store
                        this.Add(sKey.IfEmpty("args"), sValue);
                    }
                }
                else
                {
                    // Store
                    this.Add(sKey.IfEmpty("args"), sValue);
                }
            }
        }

        /// <summary>
        /// 
        /// Load from JSON object
        /// 
        /// </summary>
        /// <param name="values"></param>
        public void LoadFrom(JObject values)
        {
            //
            this.SynchObject = values;
        }

        /// <summary>
        /// 
        /// Adds a value to a key.
        /// If the key already exists, the value is treated 
        /// like a JSON array
        /// 
        /// </summary>
        /// <param name="key">The key to add int</param>
        /// <param name="value">The value</param>
        public void Add(string key, string value)
        {
            // Already?
            if (this.SynchObject.Contains(key))
            {
                // Get the value as a JSON array
                JArray c_Values = this.GetAsJArray(key);
                // Add the value to the array
                c_Values.Add(value);
                // And store
                this.SynchObject.Set(key, c_Values);
            }
            else
            {
                // Simple string save
                this.SynchObject.Set(key, value);
            }
        }

        /// <summary>
        /// 
        /// Removes a key from the underlying object
        /// 
        /// </summary>
        /// <param name="key">The key to remove</param>
        public void Remove(string key)
        {
            // If there, remove
            if (this.SynchObject.Contains(key)) this.SynchObject.Remove(key);
        }

        /// <summary>
        /// 
        /// Clears store of all values
        /// 
        /// </summary>
        public void Clear()
        {
            // Clear underlying
            this.SynchObject.RemoveAll();
        }

        /// <summary>
        /// 
        /// Makes a shallow copy of the store
        /// 
        /// </summary>
        /// <returns>The cloned store</returns>
        public StoreClass Clone()
        {
            // Make the new store
            StoreClass c_Ans = new StoreClass(this.SynchObject.Clone());

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns the file path from a key
        /// 
        /// </summary>
        /// <param name="root">The root directory</param>
        /// <param name="key">The key to be treated as a JSON array of the path pieces</param>
        /// <param name="file">The file name to append</param>
        /// <returns></returns>
        public string PathFromEntry(string root, string key, string file = null)
        {
            // The starting path 
            string sAns = root;

            // Get the "dir" array
            JArray c_Dirs = this.GetAsJArray(key);
            // And build the path
            c_Dirs.ForEach(delegate (string piece)
            {
                sAns = sAns.CombinePath(piece);
            });

            // And the file name last, if any
            if (file.HasValue()) sAns = sAns.CombinePath(file);

            return sAns;
        }
        #endregion

        #region Access
        /// <summary>
        /// 
        /// Returns the value of the key as a string
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The value as a string</returns>
        public virtual string GetAsString(string key)
        {
            return this.SynchObject.Get(key);
        }

        /// <summary>
        /// 
        /// Gets a JSON object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The JSON object or empty if none</returns>
        public virtual JArray GetAsJArray(string key)
        {
            // Get
            JArray c_Ans = this.SynchObject.GetJArray(key);
            //Not a JSON array?
            if (c_Ans == null)
            {
                // Make a new one
                c_Ans = new JArray();
                // Does the underlying object have a value?
                if (this.SynchObject.Contains(key))
                {
                    // Get the value?
                    string sValue = this.GetAsString(key);
                    // Is there one?
                    if (sValue.HasValue())
                    {
                        // Add it to the array
                        c_Ans.Add(sValue);
                    }
                }

                // Save
                this.SynchObject.Set(key, c_Ans);
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Gets a JSON object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The JSON object or empty if none</returns>
        public virtual JObject GetAsJObject(string key)
        {
            // Get
            return this.SynchObject.AssureJObject(key);
        }

        /// <summary>
        /// 
        /// Gets a .NET object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The get to get</param>
        /// <returns>The object or the default object if none</returns>
        public virtual T GetAs<T>(string key)
        {
            return this.SynchObject.GetObject<T>(key);
        }

        /// <summary>
        /// 
        /// Gets a .NET object from the underlying object
        /// Note:  The key must not be a delimited name
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The object or the default object if none</returns>
        public virtual object GetAsObject(string key)
        {
            return this.SynchObject.GetObject(key);
        }

        /// <summary>
        /// 
        /// Sets an entry to any type of object
        /// 
        /// </summary>
        /// <param name="key">The key to save into</param>
        /// <param name="value">The value to store</param>
        public virtual void Set(string key, object value, SetOptions opts = SetOptions.OK)
        {
            // Do we skip?
            if (opts == SetOptions.OK || opts == SetOptions.SaveButLocal)
            {
                // Store?
                if (value is StoreClass)
                {
                    // Save
                    this.SynchObject.Set(key, ((StoreClass)value).SynchObject);
                }
                else
                {
                    // Save
                    this.SynchObject.Set(key, value);
                }
            }
        }

        /// <summary>
        /// 
        /// Replaces any key found in the value into the key value
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Format(string value, bool skipifmissing = false, params string[] extras)
        {
            // Assure
            value = value.IfEmpty();

            // Find all of the keys
            MatchCollection c_Matches = Regex.Matches(value, @"{[^}]+}");
            // Only do each key once
            List<string> c_Done = new List<string>();
            // In case of extras
            JObject c_Extras = null;

            // Loop thru
            foreach (Match c_Match in c_Matches)
            {
                // Get the pattern
                string sPatt = c_Match.Value;
                // Done?
                if (!c_Done.Contains(sPatt))
                {
                    // Add
                    c_Done.Add(sPatt);
                    // Make the key
                    string sKey = sPatt.Substring(1, sPatt.Length - 2);
                    // And the value
                    string sValue = null;
                    // Is the value one we know of?
                    if (this.SynchObject.Contains(sKey))
                    {
                        // Get it
                        sValue = this[sKey];
                    }
                    else if (extras.Length > 0)
                    {
                        // Already made?
                        if (c_Extras == null)
                        {
                            // Make
                            c_Extras = new JObject();
                            // Fill
                            for (int i = 0; i < extras.Length; i += 2)
                            {
                                // Add
                                c_Extras.Set(extras[i], extras[i + 1]);
                            }
                        }

                        if (c_Extras.Contains(sKey))
                        {
                            // Get it
                            sValue = c_Extras.Get(sKey);
                        }
                    }

                    // Missing?
                    if (sValue == null && !skipifmissing)
                    {
                        // Make into space
                        sValue = "";
                    }

                    // Replace
                    if (sValue != null) value = value.Replace(sPatt, sValue);
                }
            }

            return value;
        }

        public JArray FormatJArray(JArray values, bool skipifmissing = false, bool allowall = false)
        {
            // Formatted
            JArray c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make formatted
                c_Ans = new JArray();
                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Get value
                    string sValue = values.Get(i);
                    // All?
                    if (allowall && sValue.IsSameValue("*"))
                    {
                        // Convert store into command line
                        List<string> c_Params = this.AsCommandLine;
                        // Loop thru
                        foreach (string sItem in c_Params)
                        {
                            // Append
                            c_Ans.Add(sItem);
                        }
                    }
                    else
                    {
                        // Format
                        c_Ans.Add(this.Format(sValue, skipifmissing));
                    }
                }
            }

            return c_Ans;
        }
        #endregion

        #region Stores
        /// <summary>
        /// 
        /// Retuns the key values that are stores
        /// 
        /// </summary>
        public List<string> StoreNames
        {
            get
            {
                List<string> c_Ans = new List<string>();

                foreach (string sName in this.SynchObject.Keys())
                {
                    if (this.GetAsStore(sName) != null)
                    {
                        c_Ans.Add(sName);
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Returns the store
        /// 
        /// </summary>
        /// <param name="key">The get to get</param>
        /// <returns>The store</returns>
        public StoreClass GetAsStore(string key)
        {
            // Assume none
            StoreClass c_Ans = null;

            // Get as JObject
            JObject c_Wkg = this.SynchObject.GetJObject(key);

            // Any?
            if (c_Wkg == null)
            {
                c_Ans = new StoreClass();
            }
            else
            {
                c_Ans = new StoreClass(c_Wkg);
            }

            return c_Ans;
        }

        /// <summary>
        ///  
        /// Create a stack save point and stores the keys
        /// 
        /// </summary>
        /// <param name="keys">A list of keys</param>
        public void Push(params string[] keys)
        {
            // The save point
            JObject c_Entry = new JObject();

            // Save the previous save point
            c_Entry.Set(StackStore, this.GetAsJObject(StackStore));

            // Loop thru
            foreach (string sKey in keys)
            {
                // Save
                c_Entry.Set(sKey, this.SynchObject.GetObject(StackStore));
            }

            // Save the new save point
            this.Set(StackStore, c_Entry);
        }

        /// <summary>
        /// 
        /// Restore from the last save point
        /// 
        /// </summary>
        public void Pop()
        {
            JObject c_Entry = this.GetAsJObject(StackStore);

            foreach (string sKey in c_Entry.Keys())
            {
                this.Set(sKey, c_Entry.GetObject(sKey));
            }
        }
        #endregion

        #region Storage
        /// <summary>
        /// 
        /// Loads the storage from disk
        /// 
        /// </summary>
        /// <param name="path"The file path></param>
        public void Load(string path)
        {
            // Do
            this.SynchObject = path.ReadFile().ToJObject();
        }

        /// <summary>
        /// 
        /// Saves the storage to disk
        /// 
        /// </summary>
        /// <param name="path"The file path></param>
        public void Save(string path)
        {
            // Do
            path.WriteFile(this.SynchObject.ToSimpleString());
        }

        /// <summary>
        /// 
        /// Loads th store from a JObject
        /// 
        /// </summary>
        /// <param name="values"></param>
        public void Load(JObject values)
        {
            this.SynchObject = values;
            if (this.SynchObject == null) this.SynchObject = new JObject();
        }
        #endregion

        #region Statics
        /// <summary>
        /// 
        /// Quick store maker
        /// 
        /// </summary>
        /// <param name="values">A list of key/value pairs</param>
        /// <returns></returns>
        public static StoreClass Make(params string[] values)
        {
            // Make the store
            StoreClass c_Ans = new StoreClass();
            // Loop thru
            for (int i = 0; i < values.Length; i += 2)
            {
                // Add
                c_Ans[values[i]] = values[i + 1];
            }

            return c_Ans;
        }
        #endregion
    }
}