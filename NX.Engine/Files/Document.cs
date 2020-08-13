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
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// Install-Package itextSharp -Version 5.5.13.1
/// Install-Package itextSharp.xmlworker -Version 5.5.13.1
/// Install-Package OpenXmlPowerTools -Version 4.5.3.2
/// Install-Package DocumentFormat.OpenXML -Version 2.11.3
/// 

using System;
using System.IO;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Linq;

using Newtonsoft.Json.Linq;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

using NX.Engine;
using NX.Shared;

namespace NX.Engine.Files
{
    /// <summary>
    /// 
    /// A document in the document tree
    /// 
    /// </summary>
    public class DocumentClass : ChildOfClass<ManagerClass>
    {
        #region Constants
        /// <summary>
        /// 
        /// A file that holds the path to the file
        /// 
        /// </summary>
        public const string ReversePointerFile = "orig.path";

        public const string MergeMapFile = "merge.map";
        #endregion

        #region Constructor
        public DocumentClass(ManagerClass mgr, string path)
            : base(mgr)
        {
            // Save
            this.Path = this.Parent.Collapse(path);
        }

        public DocumentClass(ManagerClass mgr, FolderClass folder, string name)
            : this(mgr, folder.Path.CombinePath(name))
        { }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public override void Dispose()
        {
            if (this.IMergeMap != null)
            {
                this.IMergeMap.Dispose();
                this.IMergeMap = null;
            }

            base.Dispose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Path to the file
        /// 
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// 
        /// The holding folder
        /// 
        /// </summary>
        public FolderClass Folder
        {
            get { return new FolderClass(this.Parent, this.Path.GetDirectoryFromPath()); }
        }

        /// <summary>
        /// 
        /// The full (physical) path
        /// 
        /// </summary>
        public string Location
        {
            get { return this.Parent.Expand(this.Path); }
        }

        /// <summary>
        /// 
        /// The name of the file with extension
        /// 
        /// </summary>
        public string Name
        {
            get { return this.Path.GetFileNameFromPath(); }
        }

        /// <summary>
        /// 
        /// The name of the file without extension
        /// 
        /// </summary>
        public string NameOnly
        {
            get { return this.Path.GetFileNameOnlyFromPath(); }
        }

        /// <summary>
        /// 
        /// The extension
        /// 
        /// </summary>
        public string Extension
        {
            get { return this.Path.GetExtensionFromPath(); }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a string
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                // Assume noting
                string sAns = null;

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Get
                    sAns = this.Parent.GetObject(this.Path);
                }
                else
                {
                    // Read physical
                    sAns = this.Location.ReadFile();
                }

                return sAns;
            }
            set
            {
                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Set
                    this.Parent.SetObject(this.Path, value);
                }
                else
                {
                    // Write physical
                    this.Location.WriteFile(value);
                }
            }
        }

        /// <summary>
        /// 
        /// Gets/sets the value of the file as a byte array
        /// 
        /// </summary>
        public byte[] ValueAsBytes
        {
            get
            {
                // Assume noting
                byte[] abAns = null;

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    abAns = this.Value.ToBytes();
                }
                else
                {
                    // Read physical
                    abAns = this.Location.ReadFileAsBytes();
                }

                return abAns;
            }
            set
            {
                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    this.Value = value.FromBytes();
                }
                else
                {
                    // Write physical
                    this.Location.WriteFileAsBytes(value);
                }
            }
        }

        /// <summary>
        /// 
        /// The date and time that the file was updated
        /// if new, then DateTime.MaxValue
        /// 
        /// </summary>
        public DateTime WrittenOn
        {
            get
            {
                // Assume never
                DateTime c_Ans = DateTime.MaxValue;

                // Is MinIO there?
                if (this.Parent.IsAvailable)
                {
                    // Via folder
                    using (FolderClass c_Folder = this.Folder)
                    {
                        // Get
                        c_Ans = this.Parent.GetAttribute(ManagerClass.Types.LastWrite, this.Path).FromDBDate();
                    }
                }
                else
                {
                    // Is it there?
                    if (this.Location.FileExists())
                    {
                        // Get it
                        c_Ans = this.Location.GetLastWriteFromPath();
                    }
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The URL at which the file can be obtained.
        /// Useful for sending a file reference without
        /// exposing path information
        /// 
        /// </summary>
        public string URL
        {
            get
            {
                // Make the ID
                string sAns = this.Path.SHA1HashString();

                // Save a pointer back to itself
                using (DocumentClass c_Doc = new DocumentClass(this.Parent, this.MetadataFolder, ReversePointerFile))
                {
                    // Write
                    c_Doc.Value = this.Path;
                }

                // Make it.
                // NB:  Must use the route defined in Routes.Files.Support
                return this.Parent.Parent.SiteInfo.URL.CombineURL("f", sAns);
            }
        }

        /// <summary>
        /// 
        /// The metadata folder for the document
        /// 
        /// </summary>
        private FolderClass IMetadataFolder { get; set; }
        public FolderClass MetadataFolder
        {
            get
            {
                if (this.IMetadataFolder == null)
                {
                    // Make the path
                    string sPath = DocumentClass.MetadataFolderRoot(this.Parent.Parent).CombinePath(this.Path.SHA1HashString());
                    // Create
                    this.IMetadataFolder = new FolderClass(this.Parent, sPath);
                }

                return this.IMetadataFolder;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Delete the file (correctly)
        /// 
        /// </summary>
        public void Delete()
        {
            // Delete the file itself
            if (this.Parent.IsAvailable)
            {
                // Delete
                this.Parent.DeleteObject(this.Path);
            }
            else
            {
                this.Location.DeleteFile();
            }
            // And any metadata
            this.MetadataFolder.Delete();
        }

        /// <summary>
        /// 
        /// Returns a file reference in a sub folder
        /// 
        /// </summary>
        /// <param name="folder">The sub folder name</param>
        /// <returns></returns>
        public DocumentClass InSubfolder(string folder)
        {
            // Make the path
            string sPath = this.Path.GetDirectoryFromPath().CombinePath(folder).CombinePath(this.Name);

            // And return the file
            return new DocumentClass(this.Parent, sPath);
        }
        #endregion

        #region Streams
        /// <summary>
        /// 
        /// Reads from file as a stream
        /// 
        /// </summary>
        /// <param name="cb">The callback using a stream</param>
        public void AsReadStream(Action<Stream> cb)
        {
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                // Get
                this.Parent.GetStream(this.Path, cb);
            }
            else
            {
                // Open
                using (FileStream c_Stream = new FileStream(this.Path, FileMode.Open, FileAccess.Read))
                {
                    // Call
                    cb(c_Stream);

                    // Close
                    try
                    {
                        c_Stream.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 
        /// Writes to the file as a stream
        /// 
        /// </summary>
        /// <param name="stream">The stream</param>
        public void AsWriteStream(Stream stream)
        {
            // Is MinIO there?
            if (this.Parent.IsAvailable)
            {
                // Get
                this.Parent.SetStream(this.Path, stream);
            }
            else
            {
                // Open
                using (FileStream c_Stream = new FileStream(this.Path, FileMode.Create, FileAccess.Write))
                {
                    //
                    stream.CopyTo(c_Stream);

                    // Close
                    try
                    {
                        c_Stream.Close();
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region Merge
        /// <summary>
        /// 
        /// The path to the merge map
        /// 
        /// </summary>
        public DocumentClass MergeMapDocument
        {
            get { return new DocumentClass(this.Parent, this.MetadataFolder, MergeMapFile); }
        }

        /// <summary>
        /// 
        /// The merge map
        /// 
        /// </summary>
        private MergeMapClass IMergeMap { get; set; }
        private MergeMapClass MergeMap
        {
            get
            {
                // Is it cached?
                if (this.IMergeMap == null)
                {
                    // No, create it
                    using (DocumentClass c_Map = this.MergeMapDocument)
                    {
                        // New?
                        bool bNew = c_Map.WrittenOn < this.WrittenOn;

                        // Load
                        this.IMergeMap = new MergeMapClass(c_Map);

                        // New?
                        if (bNew)
                        {
                            this.IMergeMap.MakeFields(this);
                        }
                    }
                }
                return this.IMergeMap;
            }
        }

        /// <summary>
        /// 
        /// Merges the document with a given store of data
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        public void Merge(DocumentClass result, StoreClass data)
        {
            // According to type
            switch (this.Extension)
            {
                case "docx":
                    // Create support object for MS Word
                    using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                    {
                        // And merge
                        result.ValueAsBytes = c_Filler.Merge(DocumentClass.MiniMerge(this.Parent, this, data), data);
                    }
                    break;

                case "pdf":
                case "fdf":
                    // Create support object for Adobe
                    using (Vendors.PDFClass c_Filler = new Vendors.PDFClass())
                    {
                        // And merge
                        result.ValueAsBytes = c_Filler.Merge(this.ValueAsBytes, data);
                    }
                    break;

                default:
                    // Otherwise an empty file
                    result.Value = "";
                    break;
            }
        }
        #endregion

        #region PDF
        /// <summary>
        /// 
        /// Returns PDF equivalent of document
        /// Works for PDF and DOCX only
        /// 
        /// </summary>
        public DocumentClass PDF
        {
            get
            {
                // Assume us
                DocumentClass c_Ans = this;

                // Are we PDF?
                if(!c_Ans.Extension.IsSameValue("pdf") && c_Ans.Extension.IsSameValue("docx"))
                {
                    // No, make new document
                    c_Ans = new DocumentClass(this.Parent, this.MetadataFolder.Path.CombinePath(this.NameOnly + ".pdf"));
                    // Is it older?
                    if(c_Ans.WrittenOn < this.WrittenOn)
                    {
                        // Get the text
                        byte[] abDoc = this.Location.ReadFileAsBytes();

                        // Assume failure
                        string sHTML = string.Empty;

                        // Protect
                        try
                        {
                            // Parse
                            sHTML = ParseDOCX(this.Name, abDoc);
                        }
                        catch (OpenXmlPackageException e)
                        {
                            // Image?
                            if (e.ToString().Contains("Invalid Hyperlink"))
                            {
                                // Make into stream
                                using (MemoryStream c_Stream = new MemoryStream(abDoc))
                                {
                                    // Fix
                                    UriFixer.FixInvalidUri(c_Stream, brokenUri => FixUri(brokenUri));
                                    // Rewind
                                    c_Stream.Seek(0, SeekOrigin.Begin);
                                    // And again
                                    sHTML = ParseDOCX(this.Name, c_Stream.ToArray());
                                }
                            }
                        }

                        //
                        using (StringReader c_Reader = new StringReader(sHTML))
                        {
                            using (MemoryStream c_Stream = new System.IO.MemoryStream(sHTML.ToBytes()))
                            {
                                // Create PDF
                                Document c_PDF = new Document(PageSize.LETTER, 10f, 10f, 100f, 0f);
                                // And the writer
                                PdfWriter c_Writer = PdfWriter.GetInstance(c_PDF, c_Stream);
                                // Open
                                c_PDF.Open();
                                // Convert
                                XMLWorkerHelper.GetInstance().ParseXHtml(c_Writer, c_PDF, c_Reader);
                                // Close
                                c_PDF.Close();
                                // Get
                                sHTML = c_Stream.ToArray().FromBytes();
                            }
                        }

                        // Save
                        c_Ans.Value = sHTML;
                    }
                }

                return c_Ans;
            }
        }
        #endregion

        #region Statics
        /// <summary>
        /// 
        /// Returns the folder that holds the metadata
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <returns></returns>
        public static string MetadataFolderRoot(EnvironmentClass env)
        {
            return env.DocumentFolder.CombinePath("_metadata");
        }

        /// <summary>
        /// 
        /// Does the work of merging a document.  Note that 
        /// this is a recursive operation
        /// 
        /// </summary>
        /// <param name="stg">The current storage manager</param>
        /// <param name="source">The source document</param>
        /// <param name="data">The merge data</param>
        /// <returns></returns>
        private static byte[] MiniMerge(ManagerClass mgr, DocumentClass source, StoreClass data)
        {
            byte[] abAns = source.ValueAsBytes; ;

            using (Vendors.DocXClass c_Merge = new Vendors.DocXClass())
            {
                for (int iIndex = 10; iIndex > 0; iIndex--)
                {
                    JArray c_Docs = source.MergeMap.GetDocs(MergeMapClass.PPDocTypes.PreDoc, iIndex);
                    if (c_Docs != null)
                    {
                        for (int iDoc = 0; iDoc < c_Docs.Count; iDoc++)
                        {
                            string sMDoc = c_Docs.Get(iDoc);
                            if (sMDoc.HasValue())
                            {
                                DocumentClass c_Wkg = new DocumentClass(mgr, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(mgr, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, mgr.Parent));
                                }

                                abAns = c_Merge.Append(abAns, abExtra, false);
                            }
                        }
                    }
                }

                for (int iIndex = 1; iIndex <= 10; iIndex++)
                {
                    JArray c_Docs = source.MergeMap.GetDocs(MergeMapClass.PPDocTypes.PostDoc, iIndex);
                    if (c_Docs != null)
                    {
                        for (int iDoc = 0; iDoc < c_Docs.Count; iDoc++)
                        {
                            string sMDoc = c_Docs.Get(iDoc);
                            if (sMDoc.HasValue())
                            {
                                DocumentClass c_Wkg = new DocumentClass(mgr, sMDoc);
                                byte[] abExtra = DocumentClass.MiniMerge(mgr, c_Wkg, data);

                                using (Vendors.DocXClass c_Filler = new Vendors.DocXClass())
                                {
                                    abExtra = c_Filler.Merge(abExtra, c_Wkg.MergeMap.Eval(data, mgr.Parent)); ;
                                }

                                abAns = c_Merge.Append(abAns, abExtra, true);
                            }
                        }
                    }
                }
            }

            return abAns;
        }

        /// <summary>
        /// 
        /// DOCX parser
        /// based on: https://stackoverflow.com/questions/46580718/convert-word-doc-and-docx-format-to-pdf-in-net-core-without-microsoft-office-in
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static string ParseDOCX(string name, byte[] value)
        {
            try
            {
                byte[] byteArray = value;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument wDoc =
                                                WordprocessingDocument.Open(memoryStream, true))
                    {
                        int imageCounter = 0;
                        var pageTitle = name;
                        var part = wDoc.CoreFilePropertiesPart;

                        WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
                        {
                            AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
                            PageTitle = pageTitle,
                            FabricateCssClasses = true,
                            CssClassPrefix = "pt-",
                            RestrictToSupportedLanguages = false,
                            RestrictToSupportedNumberingFormats = false,
                            ImageHandler = imageInfo =>
                            {
                                ++imageCounter;
                                string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                                ImageFormat imageFormat = null;
                                if (extension == "png") imageFormat = ImageFormat.Png;
                                else if (extension == "gif") imageFormat = ImageFormat.Gif;
                                else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
                                else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
                                else if (extension == "tiff")
                                {
                                    extension = "gif";
                                    imageFormat = ImageFormat.Gif;
                                }
                                else if (extension == "x-wmf")
                                {
                                    extension = "wmf";
                                    imageFormat = ImageFormat.Wmf;
                                }

                                if (imageFormat == null) return null;

                                string base64 = null;
                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        imageInfo.Bitmap.Save(ms, imageFormat);
                                        var ba = ms.ToArray();
                                        base64 = System.Convert.ToBase64String(ba);
                                    }
                                }
                                catch (System.Runtime.InteropServices.ExternalException)
                                { return null; }

                                ImageFormat format = imageInfo.Bitmap.RawFormat;
                                ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                                                            .First(c => c.FormatID == format.Guid);
                                string mimeType = codec.MimeType;

                                string imageSource =
                                        string.Format("data:{0};base64,{1}", mimeType, base64);

                                XElement img = new XElement(Xhtml.img,
                                        new XAttribute(NoNamespace.src, imageSource),
                                        imageInfo.ImgStyleAttribute,
                                        imageInfo.AltText != null ?
                                            new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return img;
                            }
                        };

                        XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);
                        var html = new XDocument(new XDocumentType("html", null, null, null),
                                                                                    htmlElement);
                        var htmlString = html.ToString(SaveOptions.DisableFormatting);
                        return htmlString;
                    }
                }
            }
            catch
            {
                return "The file is either open, please close it or contains corrupt data";
            }
        }

        public static Uri FixUri(string brokenUri)
        {
            string newURI = string.Empty;
            if (brokenUri.Contains("mailto:"))
            {
                int mailToCount = "mailto:".Length;
                brokenUri = brokenUri.Remove(0, mailToCount);
                newURI = brokenUri;
            }
            else
            {
                newURI = " ";
            }
            return new Uri(newURI);
        }
        #endregion
    }
}