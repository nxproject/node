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
/// IInstall-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package Jint -Version 2.11.58
/// Istall-Package HtmlAgilityPack - Version 1.11.24
/// 

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HtmlAgilityPack;
using Jint;

using NX.Engine;
using NX.Shared;

namespace Route.UI
{
    /// <summary>
    /// 
    /// A route that allows a "regular" website support
    /// 
    /// Make sure that all of the files to be served are in the #rootfolder#/ui 
    /// folder and that none of the subdirectories match a defined route
    /// 
    /// </summary>
    public class UI : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.ANY, "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Make the folder path
            string sPath = "".WorkingDirectory().CombinePath("ui." + call.Env.UI.Replace("+", "").ToLower()).AdjustPathToOS();

            // Get the full path
            sPath = store.PathFromEntry(sPath, "path");

            // Assure folder
            sPath.AssurePath();

            // If not a file, then try using index.html
            if (!sPath.FileExists()) sPath = sPath.CombinePath("index.html");

            // Assume no processor
            Func<FileStream, Stream> c_Proc = null;

            // HTML?
            if (sPath.GetExtensionFromPath().IsSameValue("html") && call.Env.UI.Contains("+"))
            {
                // Make JS interpreter
                var c_Engine = new Engine(cfg => cfg.AllowClr());
                // Add the objects
                c_Engine.SetValue("call", call);
                c_Engine.SetValue("env", call.Env);
                c_Engine.SetValue("store", store);
                c_Engine.SetValue("html", new HTMLClass());

                // Make our HTML processor
                call.RespondWithUIFile(sPath, delegate (FileStream stream)
                {
                    // Open a reader
                    using (StreamReader c_Reader = new StreamReader(stream))
                    {
                        // Read the page
                        string sPage = c_Reader.ReadToEnd();

                        // Handlebar
                        MatchCollection c_HB = Regex.Matches(sPage, @"\x7B\x7B[^\x7D]*\x7D\x7D");
                        // Loop thru
                        foreach(Match c_HBM in c_HB)
                        {
                            // Get code
                            string sAll = c_HBM.Value;
                            // Process
                            HTMLClass c_HTML = c_Engine.Execute(sAll.Substring(2, sAll.Length - 4)).GetValue("html").ToObject() as HTMLClass;
                            // Any?
                            if (c_HTML != null)
                            {
                                // Replace
                                sPage = sPage.Replace(sAll, c_HTML.ToString());
                            }
                        }

                        // Build the parser
                        HtmlDocument c_Page = new HtmlDocument();
                        //Parse
                        c_Page.LoadHtml(sPage);
                        // Find JS tags
                        var c_Nodes = c_Page.DocumentNode.SelectNodes("//nxjs");
                        // Any?
                        if (c_Nodes != null && c_Nodes.Count > 0)
                        {
                            // Loop thru
                            foreach (HtmlNode c_Node in c_Nodes)
                            {
                                // Make new node
                                HtmlDocument c_New = new HtmlDocument();

                                // Process
                                HTMLClass c_HTML = c_Engine.Execute(c_Node.InnerText).GetValue("html").ToObject() as HTMLClass;
                                // Any?
                                if (c_HTML != null)
                                {
                                    //Parse
                                    c_New.LoadHtml(c_HTML.ToString());
                                }

                                // Replace
                                c_Node.ParentNode.ReplaceChild(c_New.DocumentNode, c_Node);
                            }

                            // Make the output stream
                            MemoryStream c_Out = new MemoryStream(c_Page.DocumentNode.OuterHtml.ToBytes());

                            return c_Out;
                        }
                        else
                        {
                            // Nothing done
                            return null;
                        }
                    }
                });

                // Always deliver
                call.RespondWithUIFile(sPath, c_Proc);
            }
            else
            {
                // Handle caching
                call.RespondIf(sPath.GetLastWriteFromPath(), delegate ()
                {
                    // And deliver
                    call.RespondWithUIFile(sPath, c_Proc);
                });
            }
        }
    }
}