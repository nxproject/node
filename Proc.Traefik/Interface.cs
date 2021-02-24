///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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
using System.Linq;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// Traefik interface
    /// 
    /// </summary>
    public class InterfaceClass : ChildOfClass<ManagerClass>
    {
        #region Constants
        private const string KeyEntryPoints = "entryPoints";
        private const string KeyEntryPoint = "entryPoint";
        private const string KeyAddress = "address";
        private const string KeyCompress = "compress";
        private const string KeyTLS = "tls";
        private const string KeyMinVersion = "minVersion";
        private const string KeyCipherSuite = "cipherSuites";
        private const string KeyACME = "acme";
        private const string KeyEMail = "email";
        private const string KeyStorage = "storage";
        private const string KeyCAServer = "caServer";
        private const string KeyRedirect = "redirect";
        private const string KeyOnHostRule = "onHostRule";
        private const string KeyHTTPChallenge = "httpChallenge";
        private const string KeyDNSChallenge = "dnsChallenge";
        private const string KeyDomains = "domains";
        private const string KeyVanity = "vanity";
        private const string KeyMain = "main";
        private const string KeyFrontEnds = "frontends";
        private const string KeyRoutes = "routes";
        private const string KeyRule = "rule";
        private const string KeyBackEnds = "backends";
        private const string KeyBackEnd = "backend";
        private const string KeyAny = "any";
        private const string KeyServers = "servers";
        private const string KeyServer = "server";
        private const string KeyURL = "url";
        private const string KeyProvider = "provider";
        private const string KeyDelayBC = "delayBeforeCheck ";
        private const string KeySans = "sans";
        private const string KeyACMELoggging = "acmeLogging";
        private const string KeyNoVerify = "insecureSkipVerify";
        private const string KeyVar = "var";
        private const string KeyRegex = "regex";
        private const string KeyReplacement = "replacement";
        private const string KeyPermanent = "permanent";

        private const string KeyInfoNode = "info";
        private const string KeyInfoHosts = "hosts";
        private const string KeyInfoSites = "sites";
        private const string KeyInfoData = "data";
        private const string KeyInfoAddress = "addr";
        private const string KeyStatus = "status";
        private const string KeyStatusOn = "statuson";

        private const string ProtocolHTTP = "http";
        private const string ProtocolHTTPS = "https";

        private const string ValuePort80 = ":80";
        private const string ValuePort443 = ":443";
        private const string ValueTrue = "true";
        private const string ValueFalse = "false";
        private const string ValueTLSVersion = "VersionTLS12";
        private const string ValueCipher1 = "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256";
        private const string ValueCipher2 = "TLS_RSA_WITH_AES_256_GCM_SHA384";
        private const string ValueStorage = "traefik/certs/account";
        //private const string ValueLEURL = "https://acme-staging-v02.api.letsencrypt.org/directory";
        private const string ValueLEURL = "https://acme-v02.api.letsencrypt.org/directory";
        private const string ValueComma = ",";
        private const string ValueDot = ".";
        private const string Value0 = "0";
        private const string ValueDelayBC = "900";
        private const string ValueHTTPRegex = @"^(http://|[^(https://)])(.*)$";
        private const string ValueHTTPReplacement = @"https://${2}";

        private const string FmtHost = "Host:{0}";
        private const string FmtHTTP = "http://{0}";
        private const string FmtHTTPS = "https://{0}";
        private const string FmtPathPrefix = ";PathPrefix:{0}";

        private const string MsgSiteSetup = "Setting up {0} in Traefik";
        private const string MsgSiteNotSetup = "Unable to set up {0} in Traefik";
        private const string MsgSiteCompleted = "Setup of {0} in Traefik completed";
        private const string MsgSiteRemove = "Removing {0} in Traefik";
        private const string MsgSiteRemoved = "Removal of {0} in Traefik completed";

        private const string Wildcard = "*.";
        #endregion

        #region Constructor
        public InterfaceClass(ManagerClass mgr)
            : base(mgr)
        {
            // Open client
            this.Settings = new SettingsClass(this);
            //
            this.Settings.OnError = delegate (string msg, bool warning)
            {
                if (warning)
                { }
                else
                {
                    this.Parent.Parent.LogError(msg);
                }
            };

            // Setup
            this.Initialize();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Traefik settings, kept in Redis
        /// 
        /// </summary>
        internal SettingsClass Settings { get; private set; }

        /// <summary>
        /// 
        /// Forces HTTP only
        /// 
        /// </summary>
        private bool ForceHTTP
        {
            get
            {
                return !this.ENVDomain.HasValue() ||
                        !this.ENVProvider.HasValue() ||
                        !this.ENVEMail.HasValue();
            }
        }

        /// <summary>
        /// 
        /// One-shot to tell if we have been initialized
        /// 
        /// </summary>
        public bool HasInitialized { get; set; }
        #endregion

        #region Enviroment
        public string ENVDomain { get { return this.Parent.Parent["routing_domain"]; } }
        public string ENVEMail { get { return this.Parent.Parent["routing_email"]; } }
        public string ENVProvider { get { return this.Parent.Parent["routing_provider"].IfEmpty("namecheap"); } }
        public string ENVCatchAll { get { return this.Parent.Parent["routing_catchall"].IfEmpty("https://google.com"); } }
        #endregion

        #region Methods
        public void Initialize()
        {
            // Only once
            if (!this.HasInitialized)
            {
                //
                this.Parent.Parent.LogInfo("Traefik: Initializing...");

                // 
                if (this.Settings != null)
                {
                    // Allow self signed certs for backends
                    this.Settings.TraefikPut(ValueTrue, KeyNoVerify);

                    NodeClass c_Node0 = this.Settings.TraefikNode(KeyEntryPoints);

                    // Set the http address
                    NodeClass c_Node1 = c_Node0.Node(ProtocolHTTP);
                    c_Node1.Put(ValuePort80, KeyAddress);
                    // redirect to https
                    NodeClass c_Node2 = c_Node1.Node(KeyRedirect);
                    // Do we support HTTPS?
                    if (this.ForceHTTP)
                    {
                        c_Node2.Put(ProtocolHTTPS, KeyEntryPoint);
                        c_Node2.Put(ValueHTTPRegex, KeyRegex);
                        c_Node2.Put(ValueHTTPReplacement, KeyReplacement);
                        c_Node2.Put(ValueTrue, KeyPermanent);

                        // Set the https address
                        c_Node1 = c_Node0.Node(ProtocolHTTPS);
                        c_Node1.Put(ValuePort443, KeyAddress);
                        // GZip compress
                        c_Node1.Put(ValueTrue, KeyCompress);
                        // Move to the TLS level
                        c_Node2 = c_Node1.Node(KeyTLS);
                        // Enforce TLS1.2
                        c_Node2.Put(ValueTLSVersion, KeyMinVersion);
                        // And set the ciphers
                        c_Node2.NodeIndex(0).Put(ValueCipher1, KeyCipherSuite);
                        c_Node2.NodeIndex(1).Put(ValueCipher2, KeyCipherSuite);

                        // Let's Encrypt
                        c_Node0 = this.Settings.TraefikNode(KeyACME);
                        c_Node0.Put(this.ENVEMail, KeyEMail);
                        c_Node0.Put(ValueStorage, KeyStorage);
                        c_Node0.Put(ValueFalse, KeyOnHostRule);
                        c_Node0.Put(ValueLEURL, KeyCAServer);
                        c_Node0.Put(ValueFalse, KeyACMELoggging);
                        c_Node0.Put(ProtocolHTTPS, KeyEntryPoint);
                        c_Node1 = c_Node0.Node(KeyDNSChallenge);
                        c_Node1.Put(this.ENVProvider, KeyProvider);
                        c_Node1.Put(ValueDelayBC, KeyDelayBC);

                        // Add the datacenter with aliases
                        this.SynchonizeDCDomains(this.ENVDomain);
                    }
                    else
                    {
                        // Remove HTTPS
                        c_Node2.Delete();
                    }

                    // Setup catch all 
                    this.AddSite("x", "", "", true);

                    // Tell world
                    this.Parent.Parent.LogInfo("Traefik: Initialialization completed");

                    // And flag
                    this.HasInitialized = true;
                }
                else
                {
                    // Oops
                    this.Parent.Parent.LogInfo("Traefik: Redis is not available for initialization");
                }
            }
        }
        #endregion

        #region Backup/Restore
        /// <summary>
        /// 
        /// Backup settings to string
        /// 
        /// </summary>
        /// <returns>The string representation</returns>
        public string Backup()
        {
            string sAns = null;

            if (this.Settings != null)
            {
                sAns = this.Settings.ToString();
            }

            return sAns.IfEmpty();
        }

        /// <summary>
        /// 
        /// Restore settings from string
        /// 
        /// </summary>
        /// <param name="value">The string representation</param>
        public void Restore(string value)
        {
            if (this.Settings != null)
            {
                this.Settings.Load(value.ToJObject());
            }
        }
        #endregion

        #region Datacenter domain
        /// <summary>
        /// 
        /// Setup the main domain
        /// 
        /// </summary>
        /// <param name="domain"></param>
        public void SynchonizeDCDomains(string domain)
        {
            this.Parent.Parent.LogInfo("Traefik: Synchronizing domain - {0}".FormatString(domain));

            //
            if (this.Settings != null)
            {
                // Point to base
                NodeClass c_Node0 = this.NodeDCDomain;

                // Add the first as the domain
                int iPos = domain.IndexOf(".");
                if (iPos != -1) domain = domain.Substring(iPos + 1);
                c_Node0.Put(Wildcard + domain, KeyMain);

                this.Parent.Parent.LogInfo("Traefik: Domain is {0}".FormatString(domain));

                this.Parent.Parent.LogInfo("Traefik: Synchronization completed");
            }
            else
            {
                this.Parent.Parent.LogInfo("Traefik: Redis is not available for synchonization");
            }
        }

        /// <summary>
        ///  
        /// The node for the main domain
        ///  
        /// </summary>
        private NodeClass NodeDCDomain
        {
            get { return Settings.TraefikNode(KeyACME).Node(KeyDomains).Node("1"); }
        }

        /// <summary>
        /// 
        /// The main domain
        /// 
        /// </summary>
        public string DCDomain
        {
            get { return this.NodeDCDomain.Get(KeyMain).Replace(Wildcard, ""); }
        }

        private NodeClass NodeSans
        {
            get { return this.NodeDCDomain.Node(KeySans); }
        }

        public List<string> AllDomains
        {
            get
            {
                List<string> c_Ans = new List<string>() { this.DCDomain };

                // Get the base
                NodeClass c_Base = this.NodeDCDomain;
                // Get the Sans
                NodeClass c_Sans = c_Base.Node(KeySans);
                // Loop thru children
                foreach (string sKey in c_Sans.ChildrenKeys)
                {
                    c_Ans.Add(c_Sans.Get(sKey));
                }

                return c_Ans;
            }
        }
        #endregion

        #region Sites
        /// <summary>
        /// 
        /// Removes a site
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <returns>The ID of the site</returns>
        public long RemoveSite(string site)
        {
            long lDOID = 0;

            if (this.Settings != null)
            {
                this.Parent.Parent.LogInfo(MsgSiteRemove.FormatString(site));

                // Get the DigitalOcean ID
                lDOID = this.GetSiteVar(site, "doid").ToLong(0);

                // Base for frontends
                NodeClass c_Node0 = this.Settings.TraefikNode(KeyFrontEnds);

                // This site
                NodeClass c_Node1 = c_Node0.Node(site, true);

                // Base for backends
                c_Node0 = this.Settings.TraefikNode(KeyBackEnds);
                // This sitesRoute
                c_Node1 = c_Node0.Node(site, true);

                this.Parent.Parent.LogInfo(MsgSiteRemoved.FormatString(site));
            }

            return lDOID;
        }

        /// <summary>
        /// 
        /// Ads a site
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <param name="address">The IP address</param>
        /// <param name="pathprefix">Prefix to use for the site</param>
        /// <param name="iscatchall">If the site is a catch all for non-defined calls</param>
        public void AddSite(string site,
                                string address,
                                string pathprefix = null,
                                bool iscatchall = false)
        {
            if (this.Settings != null)
            {
                this.Parent.Parent.LogInfo(MsgSiteSetup.FormatString(site));

                try
                {
                    // Base for frontends
                    NodeClass c_Node0 = this.Settings.TraefikNode(KeyFrontEnds);

                    // This site
                    NodeClass c_Node1 = c_Node0.Node(site);
                    // For security purposes
                    c_Node1.Put(ProtocolHTTPS + (this.ForceHTTP ? ValueComma + ProtocolHTTP : string.Empty), KeyEntryPoints);

                    // Is this a catch- all?
                    if (iscatchall)
                    {
                        // Must have URL
                        if (this.ENVDomain.HasValue())
                        {
                            NodeClass c_Node2 = c_Node1.Node(KeyRoutes).Node(KeyAny);
                            c_Node2.Put("HostRegexp:{catchall:.*}", KeyRule);
                            c_Node1.Put("1", "priority");
                            NodeClass c_Node3 = c_Node1.Node("redirect");
                            c_Node3.Put("^(.*)", "regex");
                            c_Node3.Put(this.ENVDomain, "replacement");
                        }
                    }
                    else
                    {
                        // Point to the backend
                        c_Node1.Put(site, KeyBackEnd);

                        // Handle the prefix
                        pathprefix = pathprefix.IfEmpty();
                        string sSite = site;
                        if (pathprefix.HasValue())
                        {
                            pathprefix = FmtPathPrefix.FormatString(pathprefix);
                            int iIndex = sSite.IndexOf("_");
                            if (iIndex != -1) sSite = sSite.Substring(0, iIndex);
                        }

                        // Routing
                        List<string> c_Routes = new List<string>();
                        c_Routes.Add(sSite + ValueDot + this.DCDomain + pathprefix);
                        NodeClass c_Node2 = c_Node1.Node(KeyRoutes).Node(KeyAny);
                        c_Node2.Put(FmtHost.FormatString(c_Routes.Join(ValueComma)), KeyRule);

                        // Base for backends
                        c_Node0 = this.Settings.TraefikNode(KeyBackEnds);
                        // This sitesRoute
                        c_Node1 = c_Node0.Node(site);
                        // For each address
                        if (address.HasValue())
                        {
                            // Set
                            c_Node2 = c_Node1.Node(KeyServers).Node(KeyServer + "1");
                            // Add
                            c_Node2.Put(FmtHTTP.FormatString(address), KeyURL);
                        }
                    }
                }
                catch (Exception e)
                {
                    this.Parent.Parent.LogException("In AddSite", e);
                }

                this.Parent.Parent.LogInfo(MsgSiteCompleted.FormatString(site));
            }
            else
            {
                this.Parent.Parent.LogInfo(MsgSiteNotSetup.FormatString(site));
            }
        }

        /// <summary>
        /// 
        /// Lists all sites
        /// 
        /// </summary>
        public List<string> Sites
        {
            get
            {
                List<string> c_Ans = new List<string>();

                if (this.Settings != null)
                {
                    c_Ans = this.Settings.TraefikNode(KeyFrontEnds).ChildrenKeys;
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Checks to see if site exists
        /// 
        /// </summary>
        /// <param name="site">The site name</param>
        /// <returns>True if the site exists</returns>
        public bool SiteExists(string site)
        {
            return this.Sites.IndexOf(site) != -1;
        }

        public string GetSiteStatus(string site)
        {
            string sAns = "";

            try
            {
                NodeClass c_Node2 = this.Settings.TraefikNode(KeyFrontEnds).Node(site);
                sAns = c_Node2.Get(KeyStatus);
            }
            catch { }
            return sAns;
        }

        public void SetSiteStatus(string site, string status)
        {
            try
            {
                if (this.SiteExists(site))
                {
                    NodeClass c_Node2 = this.Settings.TraefikNode(KeyFrontEnds).Node(site);
                    c_Node2.Put(status, KeyStatus);
                    c_Node2.Put(DateTime.Now.ToTimeStampSecs(), KeyStatusOn);
                }
            }
            catch { }
        }

        public void SetSiteVar(string site, string key, string value)
        {
            try
            {
                if (this.SiteExists(site))
                {
                    NodeClass c_Node2 = this.Settings.TraefikNode(KeyFrontEnds).Node(site).Node(KeyVar);

                    if (key.IsSameValue("json"))
                    {
                        JObject c_Values = value.ToJObject();
                        if (c_Values != null)
                        {
                            foreach (string sKey in c_Values.Keys())
                            {
                                c_Node2.Put(c_Values.Get(sKey), sKey);
                            }
                        }
                    }
                    else
                    {
                        c_Node2.Put(value, key);
                    }
                }
            }
            catch { }
        }

        public string GetSiteVar(string site, string key)
        {
            string sAns = "";

            try
            {
                NodeClass c_Node2 = this.Settings.TraefikNode(KeyFrontEnds).Node(site).Node(KeyVar);
                sAns = c_Node2.Get(key);
            }
            catch { }
            return sAns;
        }

        public InformationClass SiteInfo(string site)
        {
            NodeClass c_Node1 = this.Settings.TraefikNode(KeyBackEnds).Node(site);
            NodeClass c_Node2 = this.Settings.TraefikNode(KeyFrontEnds).Node(site);

            string sAddr = "";
            string sPort = "";

            List<string> c_Poss = c_Node1.Node(KeyServers).ChildrenKeys;
            while (!sAddr.HasValue() && c_Poss.Count > 0)
            {
                string sKey = c_Poss[0];
                c_Poss.RemoveAt(0);

                string sRaw = c_Node1.Node(KeyServers).Node(sKey).Get(KeyURL);

                if (sRaw.HasValue())
                {
                    int iPos = sRaw.IndexOf("//");
                    if (iPos != -1)
                    {
                        sRaw = sRaw.Substring(iPos + 2);
                    }
                    iPos = sRaw.IndexOf(":");
                    if (iPos == -1)
                    {
                        sAddr = sRaw;
                    }
                    else
                    {
                        sAddr = sRaw.Substring(0, iPos);
                        sPort = sRaw.Substring(iPos + 1);
                    }
                }
            }

            InformationClass c_Ans = new InformationClass(site,
                                                                        c_Node2.Node(KeyRoutes).Node(KeyAny).Get(KeyRule),
                                                                        sAddr,
                                                                        sPort,
                                                                        c_Node2.Node(KeyInfoData),
                                                                        c_Node2.Get(KeyStatus),
                                                                        c_Node2.Get(KeyStatusOn),
                                                                        c_Node2.Node(KeyVar));
            return c_Ans;
        }
        #endregion

        #region SANDomains
        public void AddSANSDomain(string domain)
        {
            if (this.Settings != null)
            {
                // New?
                if (!this.AllDomains.Contains(domain))
                {
                    // Get the count
                    int iCount = this.NodeSans.ChildrenKeys.Count + 1;
                    // Add
                    this.NodeSans.Put(domain, iCount.ToString());
                }
            }
        }

        public void RemoveSANSDomain(string domain)
        {
            if (this.Settings != null)
            {
                // Get the list
                List<string> c_List = this.AllDomains;
                // New?
                if (c_List.Contains(domain))
                {
                    // Base
                    NodeClass c_Base = this.NodeSans;
                    // Remove
                    c_List.Remove(domain);
                    // Reset
                    for (int i = 0; i < c_List.Count; i++)
                    {
                        c_Base.Put(c_List[i], (i + 1).ToString());
                    }
                    // Clear last
                    c_Base.Delete((c_List.Count + 1).ToString());
                }
            }
        }

        public List<string> ACMEDomainsForSite(string site)
        {
            List<string> c_Ans = new List<string>();

            if (this.Settings != null)
            {
                // Point to base
                NodeClass c_Base = this.Settings.TraefikNode(KeyACME).Node(KeyDomains);
                // Get the count
                List<string> c_List = c_Base.ChildrenKeys;
                // Check each
                foreach (string sIndex in c_List)
                {
                    //
                    string sVanity = c_Base.Node(sIndex).Get(KeyVanity);
                    // DC?
                    if (sVanity.IsSameValue(site))
                    {
                        //
                        string sDomain = c_Base.Node(sIndex).Get(KeyMain);
                        if (!site.HasValue() && sDomain.StartsWith(Wildcard))
                        {
                            sDomain = sDomain.Substring(Wildcard.Length);
                        }
                        // Add
                        c_Ans.Add(sDomain);
                    }
                }
            }

            return c_Ans;
        }

        #endregion

        #region Information
        public NodeClass InfoNode
        {
            get { return this.Settings.CoreNode(KeyInfoNode); }
        }

        public JArray MapSites()
        {
            JArray c_Ans = new JArray();

            // Now map sites
            foreach (string sSite in this.Sites)
            {
                InformationClass c_Info = this.SiteInfo(sSite);

                // Get the IP and port
                string sIP = c_Info.Address;

                JObject c_Entry = new JObject();

                c_Entry.Set("name", c_Info.Name);
                c_Entry.Set("ip", sIP);
                c_Entry.Set("port", c_Info.Port);
                c_Entry.Set("status", c_Info.Status);
                c_Entry.Set("statuson", c_Info.StatusOn);

                foreach (string sKey in c_Info.ExtraKeys)
                {
                    c_Entry.Set(sKey.ToLower(), c_Info.GetExtra(sKey));
                }

                c_Entry.Set("dccost", c_Info.GetExtra("docost"));
                c_Entry.Set("slug", c_Info.GetExtra("doslug"));

                // Add
                c_Ans.Add(c_Entry);
            }

            return c_Ans;
        }
        #endregion
    }
}