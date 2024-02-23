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

using System.Collections.Generic;

using NX.Engine.NginX;
using NX.Shared;

namespace Proc.NginX
{
    /// <summary>
    /// 
    /// Support class to create nginx.config
    /// 
    /// </summary>
    public static class ExtensionsNginx
    {
        #region Constants
        /// <summary>
        /// 
        /// An error code to always include
        /// 
        /// </summary>
        public const int ErrorCode = 444;

        /// <summary>
        ///  Methods allowed
        /// </summary>
        public const string DefaultMethods = "GET|POST|DELETE|PUT|PATCH";
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Generates a snippet line
        /// 
        /// </summary>
        /// <param name="value">The text</param>
        /// <param name="level">The indentation level</param>
        /// <returns>The snippet</returns>
        public static string NginxLine(this string value, int level)
        {
            string sPadding = "        ";

            while (level > 0)
            {
                value = sPadding + value;
                level--;
            }

            return value + "\n";
        }

        /// <summary>
        /// 
        /// Creates a comment line
        /// 
        /// </summary>
        /// <param name="value">The message</param>
        /// <param name="level">Indentation level</param>
        /// <returns>The snippet</returns>
        public static string NginxComment(this string value, int level)
        {
            return value.NginxComment(level, level == 1);
        }

        /// <summary>
        /// 
        /// Creates a comment line
        /// 
        /// </summary>
        /// <param name="value">The message</param>
        /// <param name="level">Indentation level</param>
        /// <param name="high">True if highlighted</param>
        /// <returns>The snippet</returns>
        public static string NginxComment(this string value, int level, bool high)
        {
            string sAns = "";

            string sSep = "# ----------------------------------------------------";
            if (high) sSep = sSep.Replace("-", "=");

            sAns += sSep.NginxLine(level);
            sAns += "# {0}".FormatString(value).NginxLine(level);
            sAns += sSep.NginxLine(level);

            return sAns;
        }

        /// <summary>
        /// 
        /// Sets up the error pages
        /// 
        /// </summary>
        /// <param name="x">Unused</param>
        /// <returns>The snippet</returns>
        public static string NginxErrors(this string x)
        {
            string sAns = "";

            sAns += 404.NginxError();

            return sAns;
        }

        /// <summary>
        /// 
        /// Sets up the error page for a given code
        /// 
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns>The snippet</returns>
        public static string NginxError(this int code, params int[] codes)
        {
            string sAns = "";

            string sFmt = "error_page {0} ={1} http://$host;";

            sAns += sFmt.FormatString(code, ErrorCode).NginxLine(1);
            foreach (int iCode in codes)
            {
                sAns += sFmt.FormatString(iCode, ErrorCode).NginxLine(1);
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Sets up the not in service error page for a given code
        /// 
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns>The snippet</returns>
        public static string NginxErrorNIS(this int code)
        {
            string sAns = "";

            sAns += "error_page {0} /NIS.html;".FormatString(code).NginxLine(1);
            sAns += "location = /NIS.html {".NginxLine(1);
            sAns += "root /etc/nginx/mypages;".NginxLine(2);
            sAns += "internal;".NginxLine(2);
            sAns += "}".NginxLine(1);

            return sAns;
        }

        /// <summary>
        /// 
        /// Makes the upstream block for a definition
        /// 
        /// </summary>
        /// <param name="site">The site definition</param>
        /// <param name="urls">The URLs that can be used</param>
        /// <returns>The snippet</returns>
        public static string NginxUpstream(this InformationClass site, List<string> urls)
        {
            return site.Name.NginxUpstream(urls);
        }

        /// <summary>
        /// 
        /// Makes the upstream block for a site
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <param name="urls">The URLs that can be used</param>
        /// <returns>The snippet</returns>
        public static string NginxUpstream(this string site, List<string> urls)
        {
            // Assume nothing to add
            string sAns = "";

            // The header
            sAns += "Site for {0}".FormatString(site).NginxComment(1, true);

            // Prefix
            sAns += ("upstream nx_" + site.ToLower() + " {").NginxLine(1);
            sAns += "least_conn;".NginxLine(2);

            // Loop thru
            foreach (string sURL in urls)
            {
                sAns += ("server " + sURL.RemoveProtocol().ToString() + ";").NginxLine(2);
            }

            // Handle no bees
            if(urls.Count == 0)
            {
                sAns += "server localhost:9999;".NginxLine(2);
            }

            // Suffix
            sAns += "}".NginxLine(1);

            return sAns;
        }

        /// <summary>
        /// 
        /// Creates a location block
        /// 
        /// </summary>
        /// <param name="site">Information for site</param>
        /// <param name="alias">The route to use, if empty the site name will be used</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxLocation(this InformationClass site, params string[] body)
        {
            return site.Location.NginxLocation(body);
        }

        /// <summary>
        /// 
        /// Creates a location block
        /// 
        /// </summary>
        /// <param name="site">Location string</param>
        /// <param name="alias">The route to use, if empty the site name will be used</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxLocation(this string site, params string[] body)
        {
            string sAns = "";

            // The header
            sAns += "Location for {0}".FormatString(site.IfEmpty(site).IfEmpty("entry point")).NginxComment(2, false);

            sAns += ("location /" + site.IfEmpty(site).ToLower() + " {").NginxLine(2);

            sAns += "Headers".NginxComment(3);
            sAns += "proxy_set_header Host $host;".NginxLine(3);
            sAns += "proxy_hide_header X-Powered-By;".NginxLine(3);
            sAns += "proxy_http_version 1.1;".NginxLine(3);
            sAns += "proxy_set_header Upgrade $http_upgrade;".NginxLine(3);
            sAns += "proxy_set_header Connection \"Upgrade\";".NginxLine(3);
            sAns += "proxy_set_header X-Forwarded-Proto $the_scheme;".NginxLine(3);

            if (site.HasValue())
            {
                sAns += "proxy_set_header X-Forwarded-Host $the_host/{0};".FormatString(site).NginxLine(3);
            }
            else
            {
                sAns += "proxy_set_header X-Forwarded-Host $the_host;".NginxLine(3);
            }
            sAns += "proxy_set_header X-Address $x_address;".NginxLine(3);

            sAns += "Routing".NginxComment(3);
            foreach (string sLine in body)
            {
                sAns += sLine.IfEmpty();
            }
            sAns += "}".NginxLine(2);

            return sAns;
        }

        /// <summary>
        /// 
        /// Creates a proxy pass if a site is available
        /// 
        /// </summary>
        /// <param name="site">The site or empty string</param>
        /// <returns>The snippet</returns>
        public static string NginxProxy(this string site)
        {
            string sAns = "";

            if (site.HasValue()) sAns += site.NginxProxyPass();

            return sAns;
        }

        /// <summary>
        /// 
        /// Fowards a call
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="usessl"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string NginxProxyForwarded(this string x, bool usessl, int level)
        {
            string sAns = "";

            if (usessl)
            {
                sAns += "Forwarding SSL".NginxComment(level);

                sAns += "proxy_set_header X-Source-Host $remote_addr;".NginxLine(level);
                sAns += "proxy_set_header X-Source-SDN $ssl_client_s_dn;".NginxLine(level);
                sAns += "proxy_set_header X-Source-SERIAL $ssl_client_serial;".NginxLine(level);
                sAns += "proxy_set_header X-Source-FINGER $ssl_client_fingerprint;".NginxLine(level);
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Creates a proy pass to an upstream site
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <returns>The snippet</returns>
        public static string NginxProxyPass(this string site)
        {
            return ("proxy_pass http://nx_" + site + ";").NginxLine(3);
        }

        /// <summary>
        /// 
        /// Returns an error for a site
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <returns>The snippet</returns>
        public static string NginxLocationKickback(this string site)
        {
            return "".NginxLocation(ErrorCode.NginxReturn());
        }

        /// <summary>
        /// 
        /// Removes a location name from the URL
        /// 
        /// </summary>
        /// <param name="loc">The information</param>
        /// <returns>The snippet</returns>
        public static string NginxRemove(this InformationClass loc)
        {
            return loc.Rewrite ? loc.Location.NginxRemove() : null;
        }

        /// <summary>
        /// 
        /// Removes a location name from the URL
        /// 
        /// </summary>
        /// <param name="loc">The location name</param>
        /// <returns>The snippet</returns>
        public static string NginxRemove(this string loc)
        {
            return loc.HasValue() ?  @"rewrite ^/{0}/(.*) /$1 break;".FormatString(loc.ToLower()).NginxLine(3) : "";
        }

        /// <summary>
        /// 
        /// Creates a return statement
        /// 
        /// </summary>
        /// <param name="code">The code to return</param>
        /// <returns>The snippet</returns>
        public static string NginxReturn(this int code)
        {
            return "return {0};".FormatString(code).NginxLine(3);
        }

        /// <summary>
        /// 
        /// Sets up the HTTP methods allowed
        /// 
        /// </summary>
        /// <param name="codes">Codes separated by |.  If empty all</param>
        /// <returns>The snippet</returns>
        public static string NginxValidMethods(this string codes)
        {
            string sAns = "";

            if (!codes.HasValue()) codes = DefaultMethods;

            sAns += ("if ($request_method !~ (" + codes + ")) {").NginxLine(2);
            sAns += ErrorCode.NginxReturn();
            sAns += "}".NginxLine(2);

            return sAns;
        }

        /// <summary>
        /// 
        /// Creates listen block
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxListen(this string port, params string[] body)
        {
            return port.NginxListen(new List<string>(body));
        }

        /// <summary>
        /// 
        /// Creates listen block
        /// 
        /// </summary>
        /// <param name="port">The port</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxListen(this string port, List<string> body)
        {
            string sAns = "";

            sAns += "listen {0};".FormatString(port).NginxLine(2);

            sAns += "".NginxProxyForwarded(false, 2);

            foreach (string sLine in body)
            {
                sAns += sLine.IfEmpty();
            }

            return sAns;
        }

        /// <summary>
        /// 
        /// Starts a server block
        /// 
        /// </summary>
        /// <param name="servername">The name of the server</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxServerStart(this string servername, params string[] body)
        {
            return servername.NginxServerStart(new List<string>(body));
        }

        /// <summary>
        /// 
        /// Starts a server block
        /// 
        /// </summary>
        /// <param name="servername">The name of the server</param>
        /// <param name="body">Nginx statements to include</param>
        /// <returns>The snippet</returns>
        public static string NginxServerStart(this string servername, List<string> body)
        {
            string sAns = "";

            sAns += "server {".NginxLine(1);

            if (servername.HasValue()) sAns += "server_name {0};".FormatString(servername).NginxLine(2);

            foreach (string sLine in body)
            {
                sAns += sLine.IfEmpty();
            };

            return sAns;
        }

        /// <summary>
        /// 
        /// Ends the server block
        /// 
        /// </summary>
        /// <param name="value">Unused</param>
        /// <returns>The snippet</returns>
        public static string NginxServerEnd(this string value)
        {
            return "}".NginxLine(1);
        }

        public static string NginxListenSSL(this string cert, string key, bool secure, int port, params string[] body)
        {
            return cert.NginxListenSSL(key, secure, port, new List<string>(body));
        }

        public static string NginxListenSSLAt(this string cert, string key, int port, params string[] body)
        {
            return cert.NginxListenSSL(key, false, port, body);
        }

        public static string NginxListenSSL(this string cert, string key, params string[] body)
        {
            return cert.NginxListenSSL(key, false, 443, body);
        }

        public static string NginxListenSSL(this string cert, string key, bool secure, int port, List<string> body)
        {
            return cert.NginxListenSSL(key, false, secure, port, body);
        }

        public static string NginxListenSSL(this string cert, string key, bool getcert, bool secure, int port, List<string> body)
        {
            string sAns = "";

            sAns += "listen {0} ssl;".FormatString(secure ? 444 : port).NginxLine(2);
            sAns += ("ssl_certificate " + cert + ";").NginxLine(2);
            sAns += ("ssl_certificate_key " + key + ";").NginxLine(2);
            sAns += "ssl_protocols TLSv1.2;".NginxLine(2);
            //sAns += "ssl_ciphers HIGH:DHE-RSA-AES256-SHA:DHE-RSA-AES128-SHA:AES128-SHA:RC4+RSA:RC4-SHA;".NginxLine(2);
            sAns += "ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384;".NginxLine(2);

            //if (getcert)
            //{
            sAns += "ssl_verify_client optional_no_ca;".NginxLine(2);
            //}


            sAns += "".NginxProxyForwarded(true, 2);

            foreach (string sLine in body)
            {
                sAns += sLine;
            }

            return sAns;
        }
        #endregion

        #region Paths
        public static string StoragePath (this string domain, bool live, bool cert)
        {
            //
            string sDir = live ? "live" : "self";
            string sFile = cert ? "fullchain" : "privkey";

            return "/certs/{0}/{1}/{2}.pem".FormatString(sDir, domain, sFile);
        }
        #endregion
    }
}