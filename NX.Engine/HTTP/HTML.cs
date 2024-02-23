///--------------------------------------------------------------------------------
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

using System;
using System.Text;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// HTML Generator
    /// 
    /// </summary>
    public class HTMLClass : IDisposable
    {
        #region Constructor
        public HTMLClass()
        { }
        #endregion

        #region IDisposable
        /// <summary>
        /// 
        /// Housekeeping
        /// 
        /// </summary>
        public void Dispose()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Output buffer
        /// 
        /// </summary>
        private StringBuilder Buffer { get; set; } = new StringBuilder();
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Gets the HTML built
        /// 
        /// </summary>
        /// <returns>The HTML</returns>
        public override string ToString()
        {
            return this.Buffer.ToString();
        }

        /// <summary>
        /// 
        /// Adds a html string to the buffer
        /// 
        /// </summary>
        /// <param name="values">The html to add</param>
        public void Add(params string[] values)
        {
            // Loop thru
            foreach (string sPiece in values)
            {
                // Add
                this.Buffer.Append(sPiece.IfEmpty());
            }
        }

        /// <summary>
        /// 
        /// Generic tag maker
        /// 
        /// </summary>
        /// <param name="tag">The tag</param>
        /// <param name="values">The inner html</param>
        public string Tag(string tag, params string[] values)
        {
            // Assume nothing
            string sAns = "";

            // Add start tag
            sAns += "<{0}>".FormatString(tag);

            // Loop thru
            foreach (string sPiece in values)
            {
                // Add
                sAns += sPiece.IfEmpty();
            }

            // Add end tag
            sAns += "</{0}>".FormatString(tag);

            return sAns;
        }

        /// <summary>
        ///  
        /// The tags
        ///  
        /// </summary>
        public string a(params string[] values) { return this.Tag("a", values); }
        public string abbr(params string[] values) { return this.Tag("abbr", values); }
        public string address(params string[] values) { return this.Tag("address", values); }
        public string area(params string[] values) { return this.Tag("area", values); }
        public string map(params string[] values) { return this.Tag("map", values); }
        public string article(params string[] values) { return this.Tag("article", values); }
        public string aside(params string[] values) { return this.Tag("aside", values); }
        public string audio(params string[] values) { return this.Tag("audio", values); }
        public string b(params string[] values) { return this.Tag("b", values); }
        public string em(params string[] values) { return this.Tag("em", values); }
        public string strong(params string[] values) { return this.Tag("strong", values); }
        public string _base(params string[] values) { return this.Tag("_base", values); }
        public string bdi(params string[] values) { return this.Tag("bdi", values); }
        public string bdo(params string[] values) { return this.Tag("bdo", values); }
        public string blockquote(params string[] values) { return this.Tag("blockquote", values); }
        public string body(params string[] values) { return this.Tag("body", values); }
        public string br(params string[] values) { return this.Tag("br", values); }
        public string button(params string[] values) { return this.Tag("button", values); }
        public string canvas(params string[] values) { return this.Tag("canvas", values); }
        public string caption(params string[] values) { return this.Tag("caption", values); }
        public string table(params string[] values) { return this.Tag("table", values); }
        public string cite(params string[] values) { return this.Tag("cite", values); }
        public string code(params string[] values) { return this.Tag("code", values); }
        public string samp(params string[] values) { return this.Tag("samp", values); }
        public string kbd(params string[] values) { return this.Tag("kbd", values); }
        public string col(params string[] values) { return this.Tag("col", values); }
        public string colgroup(params string[] values) { return this.Tag("colgroup", values); }
        public string data(params string[] values) { return this.Tag("data", values); }
        public string datalist(params string[] values) { return this.Tag("datalist", values); }
        public string input(params string[] values) { return this.Tag("input", values); }
        public string dd(params string[] values) { return this.Tag("dd", values); }
        public string dt(params string[] values) { return this.Tag("dt", values); }
        public string dl(params string[] values) { return this.Tag("dl", values); }
        public string del(params string[] values) { return this.Tag("del", values); }
        public string details(params string[] values) { return this.Tag("details", values); }
        public string dfn(params string[] values) { return this.Tag("dfn", values); }
        public string dialog(params string[] values) { return this.Tag("dialog", values); }
        public string div(params string[] values) { return this.Tag("div", values); }
        public string embed(params string[] values) { return this.Tag("embed", values); }
        public string fieldset(params string[] values) { return this.Tag("fieldset", values); }
        public string figure(params string[] values) { return this.Tag("figure", values); }
        public string footer(params string[] values) { return this.Tag("footer", values); }
        public string form(params string[] values) { return this.Tag("form", values); }
        public string h1(params string[] values) { return this.Tag("h1", values); }
        public string h2(params string[] values) { return this.Tag("h2", values); }
        public string h3(params string[] values) { return this.Tag("h3", values); }
        public string h4(params string[] values) { return this.Tag("h4", values); }
        public string h5(params string[] values) { return this.Tag("h5", values); }
        public string h6(params string[] values) { return this.Tag("h6", values); }
        public string head(params string[] values) { return this.Tag("head", values); }
        public string header(params string[] values) { return this.Tag("header", values); }
        public string hgroup(params string[] values) { return this.Tag("hgroup", values); }
        public string hr(params string[] values) { return this.Tag("hr", values); }
        public string html(params string[] values) { return this.Tag("html", values); }
        public string i(params string[] values) { return this.Tag("i", values); }
        public string iframe(params string[] values) { return this.Tag("iframe", values); }
        public string img(params string[] values) { return this.Tag("img", values); }
        public string ins(params string[] values) { return this.Tag("ins", values); }
        public string keygen(params string[] values) { return this.Tag("keygen", values); }
        public string label(params string[] values) { return this.Tag("label", values); }
        public string legend(params string[] values) { return this.Tag("legend", values); }
        public string li(params string[] values) { return this.Tag("li", values); }
        public string ol(params string[] values) { return this.Tag("ol", values); }
        public string ul(params string[] values) { return this.Tag("ul", values); }
        public string link(params string[] values) { return this.Tag("link", values); }
        public string main(params string[] values) { return this.Tag("main", values); }
        public string mark(params string[] values) { return this.Tag("mark", values); }
        public string menu(params string[] values) { return this.Tag("menu", values); }
        public string menuitem(params string[] values) { return this.Tag("menuitem", values); }
        public string meta(params string[] values) { return this.Tag("meta", values); }
        public string meter(params string[] values) { return this.Tag("meter", values); }
        public string nav(params string[] values) { return this.Tag("nav", values); }
        public string noscript(params string[] values) { return this.Tag("noscript", values); }
        public string _object(params string[] values) { return this.Tag("_object", values); }
        public string optgroup(params string[] values) { return this.Tag("optgroup", values); }
        public string option(params string[] values) { return this.Tag("option", values); }
        public string select(params string[] values) { return this.Tag("select", values); }
        public string output(params string[] values) { return this.Tag("output", values); }
        public string p(params string[] values) { return this.Tag("p", values); }
        public string param(params string[] values) { return this.Tag("param", values); }
        public string pre(params string[] values) { return this.Tag("pre", values); }
        public string progress(params string[] values) { return this.Tag("progress", values); }
        public string q(params string[] values) { return this.Tag("q", values); }
        public string rb(params string[] values) { return this.Tag("rb", values); }
        public string rp(params string[] values) { return this.Tag("rp", values); }
        public string rt(params string[] values) { return this.Tag("rt", values); }
        public string rtc(params string[] values) { return this.Tag("rtc", values); }
        public string ruby(params string[] values) { return this.Tag("ruby", values); }
        public string s(params string[] values) { return this.Tag("s", values); }
        public string script(params string[] values) { return this.Tag("script", values); }
        public string section(params string[] values) { return this.Tag("section", values); }
        public string small(params string[] values) { return this.Tag("small", values); }
        public string source(params string[] values) { return this.Tag("source", values); }
        public string video(params string[] values) { return this.Tag("video", values); }
        public string span(params string[] values) { return this.Tag("span", values); }
        public string style(params string[] values) { return this.Tag("style", values); }
        public string sub(params string[] values) { return this.Tag("sub", values); }
        public string sup(params string[] values) { return this.Tag("sup", values); }
        public string summary(params string[] values) { return this.Tag("summary", values); }
        public string tbody(params string[] values) { return this.Tag("tbody", values); }
        public string td(params string[] values) { return this.Tag("td", values); }
        public string template(params string[] values) { return this.Tag("template", values); }
        public string textarea(params string[] values) { return this.Tag("textarea", values); }
        public string tfoot(params string[] values) { return this.Tag("tfoot", values); }
        public string th(params string[] values) { return this.Tag("th", values); }
        public string thead(params string[] values) { return this.Tag("thead", values); }
        public string time(params string[] values) { return this.Tag("time", values); }
        public string title(params string[] values) { return this.Tag("title", values); }
        public string tr(params string[] values) { return this.Tag("tr", values); }
        public string track(params string[] values) { return this.Tag("track", values); }
        public string u(params string[] values) { return this.Tag("u", values); }
        public string var(params string[] values) { return this.Tag("var", values); }
        public string wbr(params string[] values) { return this.Tag("wbr", values); }

        #endregion
    }
}