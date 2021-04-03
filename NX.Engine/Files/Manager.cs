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

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using NX.Engine;
using NX.Shared;

namespace NX.Engine.Files
{
    public class ManagerClass : BumbleBeeClass
    {
        #region Constants
        public const string MappedFolder = "/etc/files";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public ManagerClass(EnvironmentClass env)
            : base(env, null)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Is the manager available?
        /// 
        /// </summary>
        public override bool IsAvailable => true;
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Uploads a file
        /// 
        /// The following code modified from https://stackoverflow.com/questions/8466703/httplistener-and-file-upload
        /// </summary>
        public DocumentClass Upload(HTTPCallClass call, DocumentClass doc, ManagerClass mgr = null, string path = null)
        {
            try
            {
                var input = call.Request.InputStream;
                Byte[] boundaryBytes = call.Request.ContentEncoding.GetBytes(this.GetBoundary(call.Request.ContentType));
                Int32 boundaryLen = boundaryBytes.Length;

                Byte[] buffer = new Byte[1024];
                Int32 len = input.Read(buffer, 0, 1024);
                Int32 startPos = -1;

                // Do we have a file name?
                if (doc == null)
                {
                    // Convert to string
                    string sLine = buffer.FromBytes();
                    // Find name
                    Match c_Poss = Regex.Match(sLine, @"filename\x3D\x22(?<name>[^\x22]+)\x22");
                    if (c_Poss.Success)
                    {
                        using (FolderClass c_Folder = new FolderClass(mgr, path))
                        {
                            // Make path
                            c_Folder.AssurePath();
                            // And map document
                            doc = new DocumentClass(mgr, path.CombinePath(c_Poss.Groups["name"].Value));
                        }
                    }
                }

                if (doc != null)
                {
                    // Assure path
                    doc.Folder.AssurePath();

                    using (FileStream output = new FileStream(doc.Location, FileMode.Create, FileAccess.Write))
                    {
                        doc.Location.GetDirectoryFromPath().AssurePath();

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
                }
            }
            catch (Exception e)
            {
                call.Env.LogException("In upload: {0}".FormatString(doc.Path), e);
                call.RespondWithError(e.Message);
            }
            finally
            {
                if (doc != null)
                {
                    // Do we have a special handler?
                    if (ManagerClass.Conversion != null)
                    {
                        doc = ManagerClass.Conversion(doc);
                    }

                    // Make the parameter
                    FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Write, doc);

                    // Write to cloud
                    this.SignalChange(c_P);

                    // Was it handled?
                    if (c_P.Handled)
                    {
                        // Delete so not to call cloud
                        doc.Location.DeleteFile();
                    }
                }
                call.RespondWithOK();
            }

            return doc;
        }

        /// <summary>
        /// 
        /// Uploads a file
        /// 
        /// The following code modified from https://stackoverflow.com/questions/8466703/httplistener-and-file-upload
        /// </summary>
        public void UploadText(HTTPCallClass call, DocumentClass doc)
        {
            try
            {
                doc.Location.GetDirectoryFromPath().AssurePath();

                // Get the stream
                var input = call.Request.InputStream;
                // Make temp
                using (MemoryStream c_In = new MemoryStream())
                {
                    // Copy it
                    input.CopyTo(c_In);
                    // Into string
                    string sWkg = c_In.ToArray().FromBytes();
                    // Convert and save
                    doc.ValueAsBytes = sWkg.FromBase64Bytes();
                }
            }
            catch (Exception e)
            {
                call.Env.LogException("In uploadtext", e);
                call.RespondWithError(e.Message);
            }
            finally
            {
                // Do we have a special handler?
                if (ManagerClass.Conversion != null)
                {
                    doc = ManagerClass.Conversion(doc);
                }

                // Make the parameter
                FileSystemParamClass c_P = new FileSystemParamClass(FileSystemParamClass.Actions.Write, doc);

                // Write to cloud
                this.SignalChange(c_P);

                // Was it handled?
                if (c_P.Handled)
                {
                    // Delete so not to call cloud
                    doc.Location.DeleteFile();
                }
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
            // Assure
            path = path.IfEmpty();

            // Does it start with the base?
            if (path.StartsWith(MappedFolder))
            {
                path = path.Substring(MappedFolder.Length);
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
            return MappedFolder.CombinePath(path);
        }

        /// <summary>
        /// 
        /// Is the path an existent file?
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsFile(string path)
        {
            return this.Expand(this.Collapse(path)).FileExists();
        }

        /// <summary>
        /// 
        /// Is the path an existent folder?
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsFolder(string path)
        {
            return this.Expand(this.Collapse(path)).DirectoryExists();
        }
        #endregion

        #region Enums
        public enum Types
        {
            Name,

            All,

            Path,
            LastWrite,

            Child,
            ChildFolder
        }
        #endregion

        #region Events
        public void SignalChange(FileSystemParamClass param)
        {
            //
            this.FileSystemChanged?.Invoke(param);
        }

        /// <summary>
        /// 
        /// The delegate for the AvailabilityChanged event
        /// 
        /// </summary>
        /// <param name="isavailable">Is the bee available</param>
        public delegate void OnFileSystemChanged(FileSystemParamClass param);

        /// <summary>
        /// 
        /// Defines the event to be raised when a DNA is added/deleted
        /// 
        /// </summary>
        public event OnFileSystemChanged FileSystemChanged;
        #endregion

        #region Statics
        /// <summary>
        /// 
        /// Handler to handle conversions at upload
        /// 
        /// </summary>
        public static Func<DocumentClass, DocumentClass> Conversion { get; set; }
        #endregion
    }

    public class FileSystemParamClass
    {
        #region Constructor
        public FileSystemParamClass(Actions action, DocumentClass doc)
        {
            // 
            this.Action = action;
            this.Document = doc;
            this.Path = doc.Path;
        }

        public FileSystemParamClass(Actions action, FolderClass folder)
        {
            // 
            this.Action = action;
            this.Folder = folder;
            this.Path = this.Folder.Path;
        }
        #endregion

        #region Enums
        public enum Actions
        {
            CreatePath,
            Delete,
            Read,
            Write,
            GetLastWrite,
            GetStream,
            SetStream,
            ListFolders,
            ListFiles
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The action to perform
        /// 
        /// </summary>
        public Actions Action { get; set; }

        /// <summary>
        /// 
        /// Has the call been handled?
        /// 
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 
        /// The path
        /// 
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 
        /// The folder
        /// 
        /// </summary>
        public FolderClass Folder { get; set; }

        /// <summary>
        /// 
        /// The document
        /// 
        /// </summary>
        public DocumentClass Document { get; set; }

        /// <summary>
        /// 
        /// Any datetime
        /// 
        /// </summary>
        public DateTime? On { get; set; }

        /// <summary>
        /// 
        /// Data stream
        /// 
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// 
        /// Data stream
        /// 
        /// </summary>
        public Action<Stream> StreamCallback { get; set; }

        /// <summary>
        /// 
        /// List of files or folders
        /// 
        /// </summary>
        public List<string> List { get; set; }
        #endregion
    }
}