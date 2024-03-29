﻿///--------------------------------------------------------------------------------
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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package TimeZoneConverter -Version 3.2.0
/// 

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;

using HandlebarsDotNet;

namespace NX.Engine
{
    public static class HandlebarsExtensionsClass
    {
        #region Handlebars
        public static void Register(string fn, HandlebarsBlockHelper helper)
        {
            HandlebarsDotNet.Handlebars.RegisterHelper(fn, helper);
        }

        /// <summary>
        /// 
        /// Has the handlebars extension been initioalized?
        /// 
        /// </summary>
        private static bool IsHandlebarsInit { get; set; }

        /// <summary>
        /// 
        /// Initialize handlebars extensions
        /// 
        /// </summary>
        private static void HandlebarsInit()
        {
            // Only once
            if (!IsHandlebarsInit)
            {
                // Flag
                IsHandlebarsInit = true;

                //
                HandlebarsDotNet.Handlebars.Configuration.Compatibility.RelaxedHelperNaming = true;

                // If
                HandlebarsExtensionsClass.Register("iff", (output, options, context, arguments) =>
                {
                    if (arguments.Length != 3)
                    {
                        throw new HandlebarsException("{{#iff}} helper must have exactly three arguments");
                    }

                    // Params
                    var field = arguments.At<string>(0);
                    var op = arguments.At<string>(1);
                    var value = arguments.At<string>(2);

                    // Get the field value
                    var fieldvalue = context.GetValue<string>(field);

                    // Assume failure
                    bool bCmp = false;

                    // Now according to op
                    switch (op)
                    {
                        case "=":
                            bCmp = value.IsSameValue(fieldvalue);
                            break;

                        case "==":
                            bCmp = value.IsExactSameValue(fieldvalue);
                            break;

                        case "!=":
                            bCmp = !value.IsSameValue(fieldvalue);
                            break;

                        case "!==":
                            bCmp = !value.IsExactSameValue(fieldvalue);
                            break;

                        case ">":
                            bCmp = fieldvalue.CompareTo(value) > 0;
                            break;

                        case ">=":
                            bCmp = fieldvalue.CompareTo(value) >= 0;
                            break;

                        case "<":
                            bCmp = fieldvalue.CompareTo(value) < 0;
                            break;

                        case "<=":
                            bCmp = fieldvalue.CompareTo(value) <= 0;
                            break;
                    }

                    // Do
                    if (bCmp)
                    {
                        options.Template(output, context);
                    }
                    else
                    {
                        options.Inverse(output, context);
                    }
                });

                // Is
                HandlebarsExtensionsClass.Register("is", (output, options, context, arguments) =>
                {
                    if (arguments.Length != 2)
                    {
                        throw new HandlebarsException("{{#is}} helper must have exactly two arguments");
                    }

                    // Params
                    var field = arguments.At<string>(0);
                    var value = arguments.At<string>(1);

                    // Get the field value
                    var fieldvalue = context.GetValue<string>(field);

                    // Check
                    bool bCmp = value.IsSameValue(fieldvalue);

                    // Do
                    if (bCmp)
                    {
                        options.Template(output, context);
                    }
                    else
                    {
                        options.Inverse(output, context);
                    }
                });

                // Is Not
                HandlebarsExtensionsClass.Register("isnt", (output, options, context, arguments) =>
                {
                    if (arguments.Length != 2)
                    {
                        throw new HandlebarsException("{{#isnt}} helper must have exactly two arguments");
                    }

                    // Params
                    var field = arguments.At<string>(0);
                    var value = arguments.At<string>(1);

                    // Get the field value
                    var fieldvalue = context.GetValue<string>(field);

                    // Check
                    bool bCmp = !value.IsSameValue(fieldvalue);

                    // Do
                    if (bCmp)
                    {
                        options.Template(output, context);
                    }
                    else
                    {
                        options.Inverse(output, context);
                    }
                });

                // Eval
                HandlebarsExtensionsClass.Register("eval", (output, options, context, arguments) =>
                {
                    // Make a eval context
                    NX.Engine.Context c_Ctx = new Context(cb: delegate(ExprCBParams cbp)
                    {
                        //
                        string sAns = null;

                        // According to call
                        switch(cbp.Mode)
                        {
                            case ExprCBParams.Modes.Get:
                                sAns = context[cbp.Field].ToStringSafe();
                                break;

                            //case ExprCBParams.Modes.Set:
                            //    context[cbp.Field] = cbp.Value;
                            //    break;
                        }

                        return sAns;
                    });

                    // Set the delimiter
                    string sDelim = "";

                    // Loop thru
                    foreach (var c_Arg in arguments)
                    {
                        // Delimiter
                        output.WriteSafeString(sDelim);
                        // Evaluate
                        string sValue = c_Ctx.Eval(c_Arg.ToStringSafe()).Value;
                        // Write it
                        output.WriteSafeString(sValue.IfEmpty());

                        // Rset
                        sDelim = " ";
                    }

                    // Output
                    options.Template(output, context);
                });
            }
        }

        /// <summary>
        /// 
        /// Processes a handlerbars text using the given values
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Handlebars(this string text, HandlebarDataClass values)
        {
            // Init
            HandlebarsInit();

            // Compile
            var c_Template = HandlebarsDotNet.Handlebars.Compile(text);
            // Process
            text = c_Template(values.SynchObject);

            return text;
        }
        #endregion
    }

    public class HandlebarDataClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public HandlebarDataClass(EnvironmentClass env)
            : base(env)
        {
            // Into store
            this.SynchObject.Add("_env", env);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Indexer
        public string this[string key]
        {
            get { return this.GetAt(this.SynchObject, key); }
            set { this.SetAt(this.SynchObject, key, value); }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The storgae area
        /// 
        /// </summary>
        public Dictionary<string, object> SynchObject { get; private set; } = new Dictionary<string, object>();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// As JSON
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ConvertDictionary(this.SynchObject).ToSimpleString();
        }

        /// <summary>
        /// 
        /// Gets a value
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            // Assume none
            object c_Ans = null;

            // Do we have it?
            if (this.SynchObject.ContainsKey(key))
            {
                // Get
                c_Ans = this.SynchObject[key];
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Returns a string for the key
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetAsString(string key)
        {
            return this.Get(key).ToStringSafe();
        }

        /// <summary>
        /// 
        /// Sets a value
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Set(string key, object obj)
        {
            // Do conversions
            if (obj is JObject)
            {
                obj = this.ConvertJObject((JObject)obj);
            }
            else if (obj is JArray)
            {
                obj = this.ConvertJArray((JArray)obj);
            }

            // Already here?
            if (this.SynchObject.ContainsKey(key))
            {
                // Replace
                this.SynchObject[key] = obj;
            }
            else
            {
                // Add
                this.SynchObject.Add(key, obj);
            }
        }

        /// <summary>
        /// 
        /// Merges a data block
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Merge(HandlebarDataClass value)
        {
            // Loop thru
            foreach (string sKey in value.SynchObject.Keys)
            {
                // Add
                this.Set(sKey, value.SynchObject[sKey]);
            }
        }

        /// <summary>
        /// 
        /// Merges a data block
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Merge(Dictionary<string, object> value)
        {
            // Loop thru
            foreach (string sKey in value.Keys)
            {
                // Add
                this.Set(sKey, value[sKey]);
            }
        }

        /// <summary>
        /// 
        /// Merges a JSON object
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Merge(JObject value)
        {
            //
            this.Merge(this.ConvertJObject(value));
        }

        /// <summary>
        /// 
        /// Merges a store
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Merge(StoreClass value)
        {
            //
            this.Merge(this.ConvertJObject(value.SynchObject));
        }

        /// <summary>
        /// 
        /// Adds an object replacing if needed
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        private new void ISet(Dictionary<string, object> data, string key, object obj)
        {
            // Do conversions
            if (obj is JObject)
            {
                obj = this.ConvertJObject((JObject)obj);
            }
            else if (obj is JArray)
            {
                obj = this.ConvertJArray((JArray)obj);
            }

            // Already here?
            if (data.ContainsKey(key))
            {
                // Replace
                data[key] = obj;
            }
            else
            {
                // Add
                data.Add(key, obj);
            }
        }
        #endregion

        #region JSON
        private Dictionary<string, object> ConvertJObject(JObject value)
        {
            //
            Dictionary<string, object> c_Ans = new Dictionary<string, object>();

            // Loop thru
            foreach (string sKey in value.Keys())
            {
                // Try as JBoject
                JObject c_O = value.GetJObject(sKey);
                if (c_O != null)
                {
                    // Add
                    this.ISet(c_Ans, sKey, c_O);
                }
                else
                {
                    // Try JArray
                    JArray c_A = value.GetJArray(sKey);
                    if (c_A != null)
                    {
                        // Add
                        this.ISet(c_Ans, sKey, c_A);
                    }
                    else
                    {
                        // Add
                        this.ISet(c_Ans, sKey, value.Get(sKey));
                    }
                }
            }

            return c_Ans;
        }

        private List<object> ConvertJArray(JArray value)
        {
            //
            List<object> c_Ans = new List<object>();

            // Loop thru
            for (int i = 0; i < value.Count; i++)
            {
                // Try as JBoject
                JObject c_O = value.GetJObject(i);
                if (c_O != null)
                {
                    // Add
                    c_Ans.Add(this.ConvertJObject(c_O));
                }
                else
                {
                    // Try JArray
                    JArray c_A = value.GetJArray(i);
                    if (c_A != null)
                    {
                        // Add
                        c_Ans.Add(this.ConvertJArray(c_A));
                    }
                    else
                    {
                        // Add
                        c_Ans.Add(value.Get(i));
                    }
                }
            }

            return c_Ans;
        }

        private JObject ConvertDictionary(Dictionary<string, object> dict)
        {
            //
            JObject c_Ans = new JObject();

            // Loop thru
            foreach (string sKey in dict.Keys)
            {
                // Get the object
                object c_Entry = dict[sKey];
                if (c_Entry is List<object>)
                {
                    c_Ans.Set(sKey, this.ConvertList((List<object>)c_Entry));
                }
                else if (c_Entry is Dictionary<string, object>)
                {
                    c_Ans.Set(sKey, this.ConvertDictionary((Dictionary<string, object>)c_Entry));
                }
                else
                {
                    c_Ans.Set(sKey, c_Entry.ToString());
                }
            }

            return c_Ans;
        }

        private JArray ConvertList(List<object> dict)
        {
            //
            JArray c_Ans = new JArray();

            // Loop thru
            for(int i=0; i < dict.Count;i++)
            {
                // Get the object
                object c_Entry = dict[i];
                if (c_Entry is List<object>)
                {
                    c_Ans.Add(this.ConvertList((List<object>)c_Entry));
                }
                else if (c_Entry is Dictionary<string, object>)
                {
                    c_Ans.Add(this.ConvertDictionary((Dictionary<string, object>)c_Entry));
                }
                else
                {
                    c_Ans.Add(c_Entry.ToString());
                }
            }

            return c_Ans;
        }
        #endregion

        #region Indexer support
        private string GetAt(Dictionary<string, object> data, string key)
        {
            // Assume none
            string sAns = null;

            // Do we have a compund name?
            int iPos = key.IndexOf(".");
            if (iPos == -1)
            {
                // Simple get
                if (data.ContainsKey(key)) sAns = data[key].ToStringSafe();
            }
            else
            {
                // Split
                string sPrefix = key.Substring(0, iPos);
                key = key.Substring(iPos + 1);

                // Assure object is dictionary
                if (!data.ContainsKey(sPrefix))
                {
                    data.Add(sPrefix, new Dictionary<string, object>());
                }
                else if (data[sPrefix] is Dictionary<string, object>)
                { }
                else
                {
                    data[sPrefix] = new Dictionary<string, object>();
                }
                // Get
                Dictionary<string, object> c_Leaf = data[sPrefix] as Dictionary<string, object>;
                // Get
                sAns = this.GetAt(c_Leaf, key);
            }

            return sAns;
        }

        private void SetAt(Dictionary<string, object> data, string key, string value)
        {
            // Do we have a compund name?
            int iPos = key.IndexOf(".");
            if (iPos == -1)
            {
                // Simple get
                if (data.ContainsKey(key))
                {
                    data[key] = value;
                }
                else
                {
                    data.Add(key, value);
                }
            }
            else
            {
                // Split
                string sPrefix = key.Substring(0, iPos);
                key = key.Substring(iPos + 1);

                // Assure object is dictionary
                if (!data.ContainsKey(sPrefix))
                {
                    data.Add(sPrefix, new Dictionary<string, object>());
                }
                else if (data[sPrefix] is Dictionary<string, object>)
                { }
                else
                {
                    data[sPrefix] = new Dictionary<string, object>();
                }
                // Get
                Dictionary<string, object> c_Leaf = data[sPrefix] as Dictionary<string, object>;
                // Get
                this.SetAt(c_Leaf, key, value);
            }
        }
        #endregion
    }
}
