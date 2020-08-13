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
    public static class ConversionClass
    {
        #region PDF
        public static byte[] DOCX2PDF(byte[] value, string name = "sample.docx")
        {
            // Convert to HTML
            byte[] abAns = DOCX2HTML(value, name);

            // Any?
            if (abAns != null && abAns.Length > 0)
            {
                //
                using (MemoryStream c_Source = new System.IO.MemoryStream(abAns))
                {
                    //
                    using (StreamReader c_Reader = new StreamReader(c_Source))
                    {
                        using (MemoryStream c_Stream = new System.IO.MemoryStream())
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
                            abAns = c_Stream.ToArray();
                        }
                    }
                }
            }

            return abAns;
        }
        #endregion

        #region HTML
        public static byte[] DOCX2HTML(byte[] value, string name = "sample.docx")
        {
            // Assume noting
            byte[] abAns = null;

            // Protect
            try
            {
                // Parse
                abAns = ParseDOCX(name, value).ToBytes();
            }
            catch (OpenXmlPackageException e)
            {
                // Image?
                if (e.ToString().Contains("Invalid Hyperlink"))
                {
                    // Make into stream
                    using (MemoryStream c_Stream = new MemoryStream(value))
                    {
                        // Fix
                        UriFixer.FixInvalidUri(c_Stream, brokenUri => FixUri(brokenUri));
                        // Rewind
                        c_Stream.Seek(0, SeekOrigin.Begin);
                        // And again
                        abAns = ParseDOCX(name, c_Stream.ToArray()).ToBytes();
                    }
                }
            }

            return abAns;
        }

        /// <summary>
        /// 
        /// From: https://github.com/OfficeDev/Open-Xml-PowerTools/issues/52
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static byte[] HTML2DOCX(byte[] value, string name = "sample.docx")
        {
            // Assume noting
            byte[] abAns = null;

            try
            {
                //See http://openxmldeveloper.org/blog/b/openxmldeveloper/archive/2015/10/12/screen-cast-introducing-the-htmltowmlconverter-module.aspx

                //XHTML can only have 1 root - so we wrap in html/body
                XElement html = XElement.Parse(string.Format("<html><body>{0}</body></html>", value.FromBytes()));
                var settings = HtmlToWmlConverter.GetDefaultSettings();

                //set directory for images? etc....

                //var usedAuthorCss = HtmlToWmlConverter.CleanUpCss((string)html.Descendants().FirstOrDefault(d => d.Name.LocalName.ToLower() == "style") ?? dummyCss);
                var usedAuthorCss = HtmlToWmlConverter.CleanUpCss(dummyCss);
                var userCss = HtmlToWmlConverter.CleanUpCss(dummyCss);

                var document = HtmlToWmlConverter.ConvertHtmlToWml(defaultCss, defaultCss, defaultCss, html, settings);
                abAns = document.DocumentByteArray;
            }
            catch { }

            return abAns;
        }
        #endregion

        #region Support
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

        private static Uri FixUri(string brokenUri)
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

        private const string defaultCss =
   @"html, address,
blockquote,
body, dd, div,
dl, dt, fieldset, form,
frame, frameset,
h1, h2, h3, h4,
h5, h6, noframes,
ol, p, ul, center,
dir, hr, menu, pre { display: block; unicode-bidi: embed }
li { display: list-item }
head { display: none }
table { display: table }
tr { display: table-row }
thead { display: table-header-group }
tbody { display: table-row-group }
tfoot { display: table-footer-group }
col { display: table-column }
colgroup { display: table-column-group }
td, th { display: table-cell }
caption { display: table-caption }
th { font-weight: bolder; text-align: center }
caption { text-align: center }
body { margin: auto; }
h1 { font-size: 2em; margin: auto; }
h2 { font-size: 1.5em; margin: auto; }
h3 { font-size: 1.17em; margin: auto; }
h4, p,
blockquote, ul,
fieldset, form,
ol, dl, dir,
menu { margin: auto }
a { color: blue; }
h5 { font-size: .83em; margin: auto }
h6 { font-size: .75em; margin: auto }
h1, h2, h3, h4,
h5, h6, b,
strong { font-weight: bolder }
blockquote { margin-left: 40px; margin-right: 40px }
i, cite, em,
var, address { font-style: italic }
pre, tt, code,
kbd, samp { font-family: monospace }
pre { white-space: pre }
button, textarea,
input, select { display: inline-block }
big { font-size: 1.17em }
small, sub, sup { font-size: .83em }
sub { vertical-align: sub }
sup { vertical-align: super }
table { border-spacing: 2px; }
thead, tbody,
tfoot { vertical-align: middle }
td, th, tr { vertical-align: inherit }
s, strike, del { text-decoration: line-through }
hr { border: 1px inset }
ol, ul, dir,
menu, dd { margin-left: 40px }
ol { list-style-type: decimal }
ol ul, ul ol,
ul ul, ol ol { margin-top: 0; margin-bottom: 0 }
u, ins { text-decoration: underline }
br:before { content: ""\A""; white-space: pre-line }
center { text-align: center }
:link, :visited { text-decoration: underline }
:focus { outline: thin dotted invert }
/* Begin bidirectionality settings (do not change) */
BDO[DIR=""ltr""] { direction: ltr; unicode-bidi: bidi-override }
BDO[DIR=""rtl""] { direction: rtl; unicode-bidi: bidi-override }
*[DIR=""ltr""] { direction: ltr; unicode-bidi: embed }
*[DIR=""rtl""] { direction: rtl; unicode-bidi: embed }
";

        private const string dummyCss = "";
        #endregion
    }
}