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
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Engine;
using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// Traefik interface
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass
    {
        #region Constants
        private const string ValueLEURL = "https://acme-staging-v02.api.letsencrypt.org/directory";
        //private const string ValueLEURL = "https://acme-v02.api.letsencrypt.org/directory";
        

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

        private const string MsgFound = "Consul found at {0}";
        private const string MsgNotFound = "Consul not found";
        private const string MsgHostSetup = "Setting up host {0} in Traefik";
        private const string MsgHostNotSetup = "Unable to set up host {0} in Traefik";
        private const string MsgHostCompleted = "Setup of host {0} in Traefik completed";
        private const string MsgHostRemove = "Removing host {0} in Traefik";
        private const string MsgHostRemoved = "Removal of host {0} in Traefik completed";
        private const string MsgSiteSetup = "Setting up {0} in Traefik";
        private const string MsgSiteNotSetup = "Unable to set up {0} in Traefik";
        private const string MsgSiteCompleted = "Setup of {0} in Traefik completed";
        private const string MsgSiteRemove = "Removing {0} in Traefik";
        private const string MsgSiteRemoved = "Removal of {0} in Traefik completed";

        private const string AliasDCSite = "";
        //private const string ProviderNamecheap = "namecheap";
        private const string Wildcard = "*.";
        #endregion

        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "redis")
        {
            // Handle redis
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Off?
                if (!isavailable)
                {
                    this.MakeHandler(false, false);
                }
                else
                {
                    // Are we the hive that holds the bumble bee?
                    if (this.Hive.IsSameValue(this.Parent.TraefikHive))
                    {
                        // Make it
                        this.BumbleBee = new BumbleBeeClass(this.Parent, "traefik");

                        // Handle the events
                        this.BumbleBee.AvailabilityChanged += delegate (bool isavailable)
                        {
                            // Check
                            this.MakeHandler(isavailable && this.BumbleBee.IsQueen && this.IsAvailable, true);
                        };

                        // Track queen changes
                        this.BumbleBee.QueenChanged += delegate (bool isqueen)
                        {
                            // Check
                            this.MakeHandler(isqueen && this.IsAvailable, true);
                        };

                        // Bootstap
                        this.BumbleBee.CheckForAvailability();
                    }
                    else
                    {
                        this.MakeHandler(true, false);
                    }
                }
            };

            this.CheckForAvailability();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// Is the buble bee available?
        /// 
        /// </summary>
        public override bool IsAvailable => this.BumbleBee != null && this.BumbleBee.IsAvailable;

        /// <summary>
        /// 
        /// The hive that holds traefik
        /// 
        /// </summary>
        public string Hive { get { return this.Parent.TraefikHive; } }

        /// <summary>
        /// 
        /// The bumble bee.  Found only in one hive
        /// 
        /// </summary>
        public BumbleBeeClass BumbleBee { get; private set; }

        /// <summary>
        /// 
        /// Traefik interface
        /// 
        /// </summary>
        public InterfaceClass Interface { get; private set; }

        /// <summary>
        /// 
        /// Settings shortcut
        /// 
        /// </summary>
        public SettingsClass Settings {  get { return this.Interface.Settings; } }

        /// <summary>
        /// 
        /// Is HTTP trffice allowed?
        /// 
        /// </summary>
        private bool AllowHTTP { get { return false; } }
        #endregion

        #region Methods
        private void MakeHandler(bool isavailable, bool init)
        {
            // Clear
            if (this.Interface != null)
            // Available?l
            {
                this.Interface.Dispose();
                this.Interface = null;
            }

            // Available?
            if (isavailable)
            {
                // Make
                this.Interface = new InterfaceClass(this);

                // Do we init?
                if (init)
                {
                    // Allow self signed certs for backends
                    this.Settings.TraefikPut(ValueTrue, KeyNoVerify);

                    NodeClass c_Node0 = this.Settings.TraefikNode(KeyEntryPoints);

                    // Set the http address
                    NodeClass c_Node1 = c_Node0.Node(ProtocolHTTP);
                    c_Node1.Put(ValuePort80, KeyAddress);
                    //// redirect to https
                    NodeClass c_Node2 = c_Node1.Node(KeyRedirect);
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
                    c_Node0.Put(this.Interface.ENVEMail, KeyEMail);
                    c_Node0.Put(ValueStorage, KeyStorage);
                    c_Node0.Put(ValueFalse, KeyOnHostRule);
                    c_Node0.Put(ValueLEURL, KeyCAServer);
                    c_Node0.Put(ValueFalse, KeyACMELoggging);
                    //c_Node0.Put(ValueTrue, KeyACMELoggging);  // CmdDebug in Task.json
                    c_Node0.Put(ProtocolHTTPS, KeyEntryPoint);
                    c_Node1 = c_Node0.Node(KeyDNSChallenge);
                    c_Node1.Put(this.Interface.ENVProvider, KeyProvider);
                    c_Node1.Put(ValueDelayBC, KeyDelayBC);

                    // Domain
                    c_Node0 = this.Settings.TraefikNode(KeyACME).Node(KeyDomains).Node("1");
                    c_Node0.Put(Wildcard + this.Interface.ENVDomain, KeyMain);
                }
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// 
        /// Creates a command
        /// 
        /// </summary>
        /// <returns>The command</returns>
        public CommandClass New()
        {
            // Make
            CommandClass c_Ans = new CommandClass(this.Interface);

            // Setup
            c_Ans.From = this.Parent.Hive.Name;

            return c_Ans;
        }
        #endregion

        #region Sites
        private NodeClass NodeSiteFrontEnd(string site)
        {
            return this.Settings.TraefikNode(KeyFrontEnds).Node(site);
        }

        private NodeClass NodeSiteBackEnd(string site)
        {
            return this.Settings.TraefikNode(KeyBackEnds).Node(site);
        }

        private List<string> GetCurrentRoutesSite(string site)
        {
            List<string> c_Ans = new List<string>();

            if (this.Settings != null)
            {
                NodeClass c_Node0 = this.Settings.TraefikNode(KeyFrontEnds);
                NodeClass c_Node1 = c_Node0.Node(site, false);
                NodeClass c_Node2 = c_Node1.Node(KeyRoutes);

                List<string> c_Routes = c_Node2.ChildrenKeys;
                foreach (string sRoute in c_Routes)
                {
                    c_Ans.AddRange(c_Node2.Node(sRoute).Get(KeyRule).Substring(5).Split(','));
                }
            }

            return c_Ans;
        }

        public void RemoveSite(string site)
        {
            if (this.Settings != null)
            {
                this.Parent.LogInfo(MsgSiteRemove.FormatString(site));

                // Base for frontends
                NodeClass c_Node0 = this.Settings.TraefikNode(KeyFrontEnds);

                // This site
                NodeClass c_Node1 = c_Node0.Node(site, true);

                // Base for backends
                c_Node0 = this.Settings.TraefikNode(KeyBackEnds);
                // This sitesRoute
                c_Node1 = c_Node0.Node(site, true);

                this.Parent.LogInfo(MsgSiteRemoved.FormatString(site));
            }
        }

        public void AddSite(string site,
                                string address,
                                bool isinternal= false,
                                string pathprefix = null, bool iscatchall = false)
        {
            if (this.Settings != null)
            {
                this.Parent.LogInfo(MsgSiteSetup.FormatString(site));

                try
                {
                    // Base for frontends
                    NodeClass c_Node0 = this.Settings.TraefikNode(KeyFrontEnds);

                    // This site
                    NodeClass c_Node1 = c_Node0.Node(site);
                    // For security purposes
                    c_Node1.Put(ProtocolHTTPS + (this.AllowHTTP ? ValueComma + ProtocolHTTP : string.Empty), KeyEntryPoints);

                    if (iscatchall)
                    {
                        NodeClass c_Node2 = c_Node1.Node(KeyRoutes).Node(KeyAny);
                        c_Node2.Put("HostRegexp:{catchall:.*}", KeyRule);
                        c_Node1.Put("1", "priority");
                        NodeClass c_Node3 = c_Node1.Node("redirect");
                        c_Node3.Put("^(.*)", "regex");
                        c_Node3.Put(this.Interface.ENVCatchAll, "replacement");
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
                        c_Routes.Add(sSite + ValueDot + this.Interface.ENVDomain + pathprefix);
                        //// Do the vanity
                        //string sVanity = this.GetVanity(site);
                        //if (sVanity.HasValue())
                        //{
                        //    c_Routes.Add(sVanity);
                        //}
                        //
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
                            if (isinternal)
                            {
                                c_Node2.Put(FmtHTTPS.FormatString(address), KeyURL);
                            }
                            else
                            {
                                c_Node2.Put(FmtHTTP.FormatString(address), KeyURL);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this.Parent.LogException("In AddSite", e);
                }

                this.Parent.LogInfo(MsgSiteCompleted.FormatString(site));
            }
            else
            {
                this.Parent.LogInfo(MsgSiteNotSetup.FormatString(site));
            }
        }

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

                //string sID = c_Node1.Node(KeyServers).Get(sKey);
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
    }
}