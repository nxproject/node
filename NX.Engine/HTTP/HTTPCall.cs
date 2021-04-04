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
using System.Net;
using System.IO;
using System.Security.Principal;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;
using Newtonsoft.Json;
using System.Xml;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Wrapper for the HTTP call from the HTTP listener
    /// at the server
    /// 
    /// </summary>
    public class HTTPCallClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor for a no call
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public HTTPCallClass(EnvironmentClass env)
            : base(env)
        {
            // Phony call
            this.IBody = new byte[0];
            this.ResponseCompleted = true;

            // Create an empty user
            this.UserInfo = new UserInfoClass(this);

            // Assume JSON
            this.ResponseFormat = this.Env.ResponseFormat;
        }

        /// <summary>
        /// 
        /// Normal constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <param name="ctx">The HTTP context</param>
        public HTTPCallClass(EnvironmentClass env, HttpListenerContext ctx)
            : base(env)
        {
            //
            this.Context = ctx;

            // If the request is nt a POST, make up a phony body
            if (!this.Request.HttpMethod.IsSameValue("POST"))
            {
                this.IBody = new byte[0];
            }

            // Update the site URL
            string sURL = this.Request.Url.Scheme + "://" + this.Request.Url.Host;
            // Changed?
            if (!this.Parent.SiteInfo.URL.IsSameValue(sURL))
            {
                // Update
                this.Parent.SiteInfo.URL = sURL;
            }

            // Create an empty user
            this.UserInfo = new UserInfoClass(this);

            // Fill it
            try
            {
                // Depending on the authentication scheme
                switch (this.Parent.HTTP.Scheme)
                {
                    case AuthenticationSchemes.Anonymous:
                        break;

                    case AuthenticationSchemes.None:
                        // Store
                        this.UserInfo.Authenticated = true;
                        this.UserInfo.Valid = false;
                        break;

                    case AuthenticationSchemes.Basic:
                        // Get the info
                        HttpListenerBasicIdentity c_Identity = (HttpListenerBasicIdentity)this.Context.User.Identity;
                        // Store
                        this.UserInfo.Name = c_Identity.Name;
                        this.UserInfo.Password = c_Identity.Password;
                        this.UserInfo.Authenticated = false;
                        this.UserInfo.Valid = false;
                        break;

                    default:
                        // Get the info
                        IIdentity c_WID = this.Context.User.Identity;
                        // Store
                        this.UserInfo.Name = c_WID.Name;
                        this.UserInfo.Authenticated = c_WID.IsAuthenticated;
                        this.UserInfo.Valid = this.UserInfo.Authenticated;
                        break;
                }
            }
            catch { }

            // Assume JSON
            this.ResponseFormat = this.Env.ResponseFormat;
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            // Assure thata response is done
            if (!this.ResponseCompleted) this.RespondWithError();

            // 
            if (this.IStore != null)
            {
                this.IStore.Dispose();
                this.IStore = null;
            }

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// These come from the HTTP context
        /// 
        /// </summary>
        public HttpListenerContext Context { get; private set; }
        public HttpListenerRequest Request { get { return this.Context.Request; } }
        public HttpListenerResponse Response { get { return this.Context.Response; } }

        /// <summary>
        ///  
        /// A shortcut to make calls readable
        ///  
        /// </summary>
        public EnvironmentClass Env { get { return this.Parent; } }

        /// <summary>
        /// 
        /// The ID if a JSON Rpc call
        /// 
        /// </summary>
        public string JSONRpcID { get; set; }

        /// <summary>
        /// 
        /// The HTTP method used
        /// 
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// 
        /// The route that was called
        /// 
        /// </summary>
        public List<string> RouteTree { get; set; }
        #endregion

        /// <summary>
        /// 
        /// These give the user a variety of ways to
        /// get the body of a POST request
        /// 
        /// </summary>
        #region Body
        private byte[] IBody { get; set; }

        /// <summary>
        /// 
        /// Body as an UTF-8 array
        /// 
        /// </summary>
        public byte[] Body
        {
            get
            {
                if (this.IBody == null)
                {
                    try
                    {
                        using (System.IO.Stream c_Stream = this.Request.InputStream)
                        {
                            using (System.IO.MemoryStream c_Temp = new System.IO.MemoryStream())
                            {
                                c_Stream.CopyTo(c_Temp);
                                this.IBody = c_Temp.ToArray();
                            }
                        }
                    }
                    catch
                    {
                        this.IBody = new byte[0];
                    }
                }
                return this.IBody;
            }
        }

        /// <summary>
        /// 
        /// Body asa C# string
        /// 
        /// </summary>
        public string BodyAsString
        {
            get { return this.Body.FromBytes(); }
        }

        /// <summary>
        /// 
        /// Body as a JSON object
        /// 
        /// </summary>
        public JObject BodyAsJObject
        {
            get
            {
                // Assume a JSON object
                JObject c_Ans = null;

                try
                {
                    // Assume a string
                    string sBody = this.BodyAsString;
                    // Check the first byte
                    if (sBody.Length > 0 && sBody[0] == '<')
                    {
                        // Set the response
                        if (this.ResponseFormat.IsSameValue("auto"))
                        {
                            this.ResponseFormat = "xml";
                        }

                        // Convert
                        c_Ans = sBody.ToJObjectFromXMLString();
                    }
                    else
                    {
                        // Set the response
                        if (this.ResponseFormat.IsSameValue("auto"))
                        {
                            this.ResponseFormat = "json";
                        }

                        // Simple conversion
                        c_Ans = sBody.ToJObject();

                        // Handle JSON RPC 2.0
                        // JSON Rpc?
                        if (c_Ans.Get("jsonrpc").IsSameValue("2.0"))
                        {
                            // Make new one
                            JObject c_New = new JObject();

                            // Save
                            this.JSONRpcID = c_Ans.Get("id");
                            // Get the params
                            string sFn = c_Ans.Get("method");
                            JArray c_P = c_Ans.GetJArray("params");
                            // Fill
                            if (c_P != null)
                            {
                                // Loop thru
                                for (int i = 0; i < c_P.Count; i++)
                                {
                                    // Merge
                                    c_New.Merge(c_P.GetJObject(i));
                                }
                            }
                            c_New.Set("fn", sFn);

                            // replace
                            c_Ans = c_New;
                        }
                    }
                }
                catch { }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Body as a JSON array
        /// 
        /// </summary>
        public JArray BodyAsJArray
        {
            get
            {
                // Assume a JSON object
                JArray c_Ans = null;

                try
                {
                    // Assume a string
                    string sBody = this.BodyAsString;
                    // Check the first byte
                    if (sBody.Length > 0 && sBody[0] == '<')
                    {
                        // Set the response
                        if (this.ResponseFormat.IsSameValue("auto"))
                        {
                            this.ResponseFormat = "xml";
                        }

                        // Convert
                        c_Ans = sBody.ToJArrayFromXMLString();
                    }
                    else
                    {
                        // Set the response
                        if (this.ResponseFormat.IsSameValue("auto"))
                        {
                            this.ResponseFormat = "json";
                        }

                        // Simple conversion
                        c_Ans = sBody.ToJArray();
                    }
                }
                catch { }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Body as a store
        /// Note: The store is kept as an object so reusing the
        /// store uses the same base object
        /// 
        /// </summary>
        private StoreClass IStore { get; set; }
        public StoreClass BodyAsStore
        {
            get
            {
                if (this.IStore == null) this.IStore = new StoreClass(this.BodyAsJObject);

                return this.IStore;
            }
        }
        #endregion

        // <summary>
        /// 
        /// These gives the user a variety of ways to respond 
        /// 
        /// </summary>
        #region Responses

        /// <summary>
        /// 
        /// The format of the response.
        /// 
        /// </summary>
        public string ResponseFormat { get; set; }

        /// <summary>
        /// 
        /// Base handler for responses
        /// 
        /// </summary>
        private void ResponseGeneric(byte[] value, string ct, HttpStatusCode code = HttpStatusCode.OK)
        {
            if (!this.ResponseCompleted)
            {
                // And send back
                this.Response.StatusCode = (int)code;

                //
                this.HandleETag();

                this.Response.ContentType = ct;
                this.Response.ContentLength64 = value.Length;
                this.Response.OutputStream.Write(value, 0, value.Length);

                this.Response.OutputStream.Flush();
                this.ResponseEnd();
            }
        }

        /// <summary>
        /// 
        /// Ends a response
        /// 
        /// </summary>
        private void ResponseEnd()
        {
            if (!this.ResponseCompleted)
            {
                this.ResponseCompleted = true;
                this.Response.Close();
            }
        }

        /// <summary>
        /// 
        /// Respond with a store.
        /// Returns the underlying JSO object
        /// 
        /// </summary>
        /// <param name="store"></param>
        public void RespondWithStore(StoreClass store)
        {
            this.RespondWithJSON(store.SynchObject);
        }

        /// <summary>
        /// 
        /// Respond with a JSON object
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void RespondWithJSON(JObject value)
        {
            if (!this.ResponseCompleted)
            {
                // The outbound
                string sResp = null;

                // According to type
                switch (this.ResponseFormat)
                {
                    case "xml":
                        sResp = value.ToXML().ToXMLString();
                        break;

                    default:
                        // JSON Rpc?
                        if (this.JSONRpcID.HasValue())
                        {
                            // Adjust
                            JObject c_JRPC = new JObject();
                            c_JRPC.Set("jsonrpc", "2.0");
                            c_JRPC.Set("id", this.JSONRpcID);
                            c_JRPC.Set("result", value);

                            value = c_JRPC;
                        }
                        sResp = value.ToSimpleString();
                        break;
                }

                // 
                this.ResponseGeneric(sResp.ToBytes(), "application/json");
            }
        }

        /// <summary>
        /// 
        /// Respond with a JSON array
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void RespondWithJSON(JArray value)
        {
            if (!this.ResponseCompleted)
            {
                if (!this.ResponseCompleted)
                {
                    // The outbound
                    string sResp = null;

                    // According to type
                    switch (this.ResponseFormat)
                    {
                        case "xml":
                            sResp = value.ToXML().ToXMLString();
                            break;

                        default:
                            // JSON Rpc?
                            if (this.JSONRpcID.HasValue())
                            {
                                // Adjust
                                JObject c_JRPC = new JObject();
                                c_JRPC.Set("jsonrpc", "2.0");
                                c_JRPC.Set("id", this.JSONRpcID);
                                c_JRPC.Set("result", value);

                                sResp = c_JRPC.ToSimpleString();
                            }
                            else
                            {
                                sResp = value.ToSimpleString();
                            }
                            break;
                    }

                    // 
                    this.ResponseGeneric(sResp.ToBytes(), "application/json");
                }
            }
        }

        /// <summary>
        /// 
        /// Respond with an error
        /// 
        /// </summary>
        /// <param name="msg">The error message.  ["Internal Error"]</param>
        /// <param name="code">The HTTP error code.  [500]</param>
        public void RespondWithError(string msg = "Internal error", HttpStatusCode code = HttpStatusCode.InternalServerError)
        {
            if (!this.ResponseCompleted)
            {
                // Respond
                JObject c_Ret = new JObject();
                c_Ret.Set("code", (int)code);
                c_Ret.Set("expl", code.ToString());
                c_Ret.Set("error", msg);
                this.RespondWithJSON(c_Ret);
            }
        }

        /// <summary>
        /// 
        /// Responds with {"ok": "1"}
        /// 
        /// </summary>
        public void RespondWithOK(params string[] values)
        {
            // Make base
            JObject c_Ans = "ok".AsJObject("1");

            // Loop thru
            for (int i = 0; i < values.Length; i += 2)
            {
                c_Ans.Set(values[i], values[i + 1]);
            }

            //
            this.RespondWithJSON(c_Ans);
        }

        /// <summary>
        /// 
        /// Responds with {"ok": "0"}
        /// 
        /// </summary>
        public void RespondWithFail()
        {
            this.RespondWithJSON("ok".AsJObject("0"));
        }

        /// <summary>
        /// 
        /// Respond with {"value": "text"}
        /// </summary>
        /// <param name="value"></param>
        public void RespondWithText(string value)
        {
            if (!this.ResponseCompleted)
            {
                // Respond
                JObject c_Ret = new JObject();
                c_Ret.Set("value", value);

                this.RespondWithJSON(c_Ret);
            }
        }

        /// <summary>
        /// 
        /// Special case for UI related files
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="proc">The stream processor</param>
        public void RespondWithUIFile(string path, Func<FileStream, Stream> proc = null)
        {
            this.RespondWithFile(path, false, null, proc);
        }

        /// <summary>
        /// 
        /// Special call for files
        /// 
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="download">True if the file is to be treated as attachment</param>
        /// <param name="ct">The content-type</param>
        public void RespondWithFile(string path, bool download = true, string ct = null, Func<FileStream, Stream> proc = null)
        {
            if (!this.ResponseCompleted)
            {
                if (path.FileExists())
                {
                    using (FileStream c_File = File.OpenRead(path))
                    {
                        // Make local
                        Stream c_Local = null;

                        // Do we have a processor
                        if (proc != null)
                        {
                            // Process
                            c_Local = proc(c_File);
                            // Any?
                            if (c_Local != null)
                            {
                                // Rewind
                                c_Local.Seek(0, SeekOrigin.Begin);
                            }
                        }

                        // No changes?
                        if (c_Local == null)
                        {
                            // Caching?
                            this.RespondIf(path.GetLastWriteFromPath(), delegate ()
                            {
                                // The original
                                this.RespondWithStream(path, ct, download, c_File);
                            });
                        }
                        else
                        {
                            // Send the stream
                            this.RespondWithStream(path, ct, download, c_Local);
                        }
                    }
                }
                else
                {
                    // The famous 404
                    this.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    this.ResponseEnd();
                }
            }
        }

        /// <summary>
        /// 
        /// Handle the processign stream
        /// 
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="ct">The content-type</param>
        /// <param name="download">True if the file is to be treated as attachment</param>
        /// <param name="source">The source stream</param>
        public void RespondWithStream(string path, string ct, bool download, Stream source)
        {
            // And send back
            this.Response.StatusCode = (int)HttpStatusCode.OK;

            //
            this.HandleETag();

            this.Response.ContentType = ct.IfEmpty(path.ContentTypeFromPath());
            this.Response.ContentLength64 = source.Length;
            this.Response.SendChunked = false;

            if (download)
            {
                this.Response.ContentType = ct.IfEmpty("application/" + path.GetExtensionFromPath());
                this.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                this.Response.AddHeader("Content-disposition", "attachment; filename=" + path.GetFileNameFromPath());
            }

            source.CopyTo(this.Response.OutputStream);
            this.Response.OutputStream.Flush();
            this.ResponseEnd();
        }

        /// <summary>
        /// 
        /// Call for sending texxt
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void RespondWithStatic(string value)
        {
            // Send it back
            this.ResponseGeneric(value.ToBytes(), "text/plain");
        }

        /// <summary>
        /// 
        /// Flag to tell system that a response has already been made
        /// preventing errors if multiple responses or making sure
        /// that at least one response is made
        /// 
        /// </summary>
        public bool ResponseCompleted { get; private set; }

        /// <summary>
        /// 
        /// The last modified timestamp
        /// 
        /// </summary>
        public DateTime? RespondLastModified { get; set; }

        /// <summary>
        /// 
        /// The ETag
        /// 
        /// </summary>
        private string ResponseETag
        {
            get
            {
                // Assume none
                string sAns = null;

                // Any?
                if (this.RespondLastModified != null)
                {
                    // Make tag
                    sAns = ((DateTime)this.RespondLastModified).ToString("R").MD5HashString();
                }

                return sAns;
            }
        }

        /// <summary>
        /// 
        /// Handles the check against an etag
        /// 
        /// </summary>
        /// <param name="lw">The last write time for the response</param>
        /// <param name="cb">Callback if etag mismatch</param>
        public void RespondIf(DateTime lw, Action cb)
        {
            // Set
            this.RespondLastModified = lw;
            // Match
            if (this.Request.Headers["If-None-Match"].IsSameValue(this.ResponseETag))
            {
                if (!this.ResponseCompleted)
                {
                    // And send back
                    this.Response.StatusCode = (int)HttpStatusCode.NotModified;

                    //
                    this.ResponseEnd();
                }
            }
            else
            {
                // Continue
                if (cb != null) cb();
            }
        }

        /// <summary>
        /// 
        /// Handle cache header values
        /// 
        /// </summary>
        private void HandleETag()
        {
            // Do we have a date?
            if (this.ResponseETag.HasValue())
            {
                this.Response.AddHeader("Etag", this.ResponseETag);
            }
        }
        #endregion

        #region User properties
        public UserInfoClass UserInfo { get; set; }
        #endregion
    }
}