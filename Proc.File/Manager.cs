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
/// Install-Package Minio -Version 3.1.13
/// 

using System;
using System.IO;
using System.Collections.Generic;

using Minio;

using NX.Engine;
using NX.Shared;
using iTextSharp.text.pdf;

namespace Proc.File
{
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public ManagerClass(EnvironmentClass env)
            : base(env, "minio")
        {
            // Handle the event
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Kill current
                if (this.Client != null)
                {
                    this.Client = null;
                }

                // Accordingly
                if (isavailable)
                {
                    // Get the settings
                    string sAccessKey = this.Parent["minio_access"];
                    // Assure
                    if (!sAccessKey.HasValue())
                    {
                        // Create
                        sAccessKey = "MINIO".GUID();
                        // Save
                        this.Parent["minio_access"] = sAccessKey;
                    }

                    string sSecret = this.Parent["minio_secret"];
                    // Assure
                    if (!sSecret.HasValue())
                    {
                        // Create
                        sSecret = "MINIO".GUID();
                        // Save
                        this.Parent["minio_secret"] = sSecret;
                    }

                    // Get the url
                    string sURL = this.Location;

                    // Must have all three
                    if (sURL.HasValue() && sAccessKey.HasValue() && sSecret.HasValue())
                    {
                        // Make the client
                        this.Client = new MinioClient(sURL, sAccessKey, sSecret);

                        // Get the root directory
                        string sRoot = env.DocumentFolder;
                        // Get all of the files
                        List<string> c_Files = sRoot.GetTreeInPath();
                        // Loop thru
                        foreach (string sFile in c_Files)
                        {
                            // Make the document
                            using (DocumentClass c_Doc = new DocumentClass(this, sFile.Substring(sRoot.Length)))
                            {
                                // Copy from local to Minio
                                c_Doc.ValueAsBytes = sFile.ReadFileAsBytes();
                                // Delete local copy
                                sFile.DeleteFile();
                            }
                        }
                    }
                }
                else
                {
                    // Do we have a client?
                    if (this.Client != null)
                    {
                        // Bye
                        this.Client = null;
                    }
                }
            };
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The Minio client
        /// 
        /// </summary>
        public MinioClient Client { get; set; }

        /// <summary>
        /// 
        /// Is the client available
        /// 
        /// </summary>
        public override bool IsAvailable => this.Client != null;
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Uploads a file
        /// 
        /// The following code modified from https://stackoverflow.com/questions/8466703/httplistener-and-file-upload
        /// </summary>
        public void Upload(HTTPCallClass call, DocumentClass doc)
        {
            try
            {
                var input = call.Request.InputStream;
                Byte[] boundaryBytes = call.Request.ContentEncoding.GetBytes(this.GetBoundary(call.Request.ContentType));
                Int32 boundaryLen = boundaryBytes.Length;

                using (FileStream output = new FileStream(doc.Location, FileMode.Create, FileAccess.Write))
                {
                    Byte[] buffer = new Byte[1024];
                    Int32 len = input.Read(buffer, 0, 1024);
                    Int32 startPos = -1;

                    // Find start boundary
                    while (true)
                    {
                        if (len == 0)
                        {
                            throw new Exception("Start Boundary Not Found");
                        }

                        startPos = buffer.IndexOf(len, boundaryBytes);
                        if (startPos >= 0)
                        {
                            break;
                        }
                        else
                        {
                            Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                            len = input.Read(buffer, boundaryLen, 1024 - boundaryLen);
                        }
                    }

                    // Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
                    for (Int32 i = 0; i < 4; i++)
                    {
                        while (true)
                        {
                            if (len == 0)
                            {
                                throw new Exception("Preamble not Found.");
                            }

                            startPos = Array.IndexOf(buffer, call.Request.ContentEncoding.GetBytes("\n")[0], startPos);
                            if (startPos >= 0)
                            {
                                startPos++;
                                break;
                            }
                            else
                            {
                                len = input.Read(buffer, 0, 1024);
                            }
                        }
                    }

                    Array.Copy(buffer, startPos, buffer, 0, len - startPos);
                    len = len - startPos;

                    while (true)
                    {
                        Int32 endPos = buffer.IndexOf(len, boundaryBytes);
                        if (endPos >= 0)
                        {
                            if (endPos > 0) output.Write(buffer, 0, endPos - 2);
                            break;
                        }
                        else if (len <= boundaryLen)
                        {
                            throw new Exception("End Boundary Not Found");
                        }
                        else
                        {
                            output.Write(buffer, 0, len - boundaryLen);
                            Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                            len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen;
                        }
                    }
                }

                // Do we need to move to Minio?
                if (this.IsAvailable)
                {
                    // Copy from local to Minio
                    doc.ValueAsBytes = doc.Location.ReadFileAsBytes();
                    // Delete local copy
                    doc.Location.DeleteFile();
                }

                call.RespondWithOK();
            }
            catch (Exception e)
            {
                call.RespondWithError(e.Message);
            }
            finally
            {
                call.RespondWithOK();
            }
        }

        /// <summary>
        /// 
        /// Returns the file boundary string for a MIM upload
        /// 
        /// </summary>
        /// <param name="ctype"></param>
        /// <returns>The boundary</returns>
        private string GetBoundary(string ctype)
        {
            return "--" + ctype.Split(';')[1].Split('=')[1];
        }

        /// <summary>
        /// 
        /// Collapses a path, removing known base
        /// 
        /// </summary>
        /// <param name="path">The path to collapse</param>
        /// <returns>The collapsed path</returns>
        public string Collapse(string path)
        {
            // Does it start with the base?
            if (path.StartsWith(this.Parent.DocumentFolder))
            {
                path = path.Substring(this.Parent.DocumentFolder.Length);
            }

            return path;
        }

        /// <summary>
        /// 
        /// Expands the path, adding known baase
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string Expand(string path)
        {
            return this.Parent.DocumentFolder.CombinePath(path);
        }
        #endregion
    }
}