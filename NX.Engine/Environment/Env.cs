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

/// Packet Manager Requirements
/// 
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;
using Octokit;

using NX.Engine.Hive;
using NX.Shared;
using NX.Engine.SocketIO;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// The global environment.  
    /// 
    /// </summary>
    public class EnvironmentClass : StoreClass, ILogger
    {
        #region Constants
        private const string KeyID = "id";

        private const string KeyThreads = "http_threads";
        private const string KeyPort = "http_port";
        private const string KeyTraefikHive = "traefik_hive";
        private const string KeyConfig = "config";
        private const string KeyVerbose = "verbose";

        private const string KeySystemPrefix = "system_prefix";

        private const string KeyRespFormat = "response_format";

        private const string KeyRootFolder = "root_folder";
        private const string KeySettingsFolder = "sett_folder";
        private const string KeyDynamicFolder = "dyn_folder";
        private const string KeyDocumentFolder = "doc_folder";
        private const string KeyUI = "ui";
        private const string KeySharedFolder = "shared_folder";

        private const string KeyGITURL = "git_url";
        private const string KeyGITProduct = "git_product";
        private const string KeyGITToken = "git_token";
        private const string KeyGITRepo = "git_repo";

        // Globals
        public const string KeySecureCode = "secure_code";
        public const string KeyRepoProject = "repo_project";
        public const string KeyHive = "hive";

        // Local use only
        private const string KeyProcess = "proc";
        private const string KeyMakeGenome = "make_genome";
        public const string KeyCodeFolder = "code_folder";
        private const string KeyMakeBee = "make_bee";
        private const string KeyLoopbackURL = "loopback";
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// CLI Constructor
        /// 
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public EnvironmentClass(string[] args)
        {
            // Load the arguments
            this.Parse(args, true);

            // Initialize
            this.Initialize();
        }

        /// <summary>
        /// 
        /// On-the fly constructor
        /// 
        /// </summary>
        /// <param name="args"></param>
        public EnvironmentClass(JObject args)
            : base(null, "_env")
        {
            // Load the arguments
            this.LoadFrom(args);

            // Initialize
            this.Initialize();
        }

        /// <summary>
        /// 
        /// Based constructor
        /// 
        /// </summary>
        /// <param name="env"></param>
        public EnvironmentClass(EnvironmentClass env)
            : this(env.SynchObject.Clone())
        { }

        private void Initialize()
        {
           // Do we have a config?
            if (this.Config.HasValue())
            {
                // Read it
                JObject c_Config = this.Config.ReadFile().ToJObject();
                // Loop thru
                foreach (string sKey in c_Config.Keys())
                {
                    // Already have?  
                    // This allow for the user to override the config
                    // file in the command line
                    if (!this.SynchObject.Contains(sKey))
                    {
                        // Move in
                        this.SynchObject.Set(sKey, c_Config.GetObject(sKey));
                    }
                }
            }

            // Did we get a secure code?
            string sSecureCode = this[EnvironmentClass.KeySecureCode];
            // Do we have to gen?
            if (sSecureCode.IsSameValue("random"))
            {
                sSecureCode = "".GUID();
            }

            if (sSecureCode.IsSameValue("site"))
            {
                sSecureCode = this.ReachableURL.MD5HashString();
            }

            // Set usecured?
            if (sSecureCode.IsSameValue(RouterClass.UnsecureCode))
            {
                sSecureCode = null;
            }

            // Save
            if (sSecureCode.HasValue())
            {
                this[EnvironmentClass.KeySecureCode] = sSecureCode;
                this.LogInfo("Secure code is {0}".FormatString(sSecureCode));
            }
            else
            {
                this.Remove(EnvironmentClass.KeySecureCode);
            }

            // Welcome
            this.LogInfo("NX.Node running at {0}".FormatString("".GetLocalIP()));

            // -------------------------------------------------------------
            //
            // In this section we setup the defaults
            //

            this[KeyHive] = this[KeyHive].IfEmpty("default");
            this[KeyRepoProject] = this[KeyRepoProject].IfEmpty("nxproject");

            this[KeyRootFolder] = this[KeyRootFolder].IfEmpty("".WorkingDirectory()).CombinePath(this[KeyHive]);

            this[KeySharedFolder] = this.GetFolderPath(this.RootFolder, KeySharedFolder, "shared");
            this[KeyDynamicFolder] = this.GetFolderPath(this.SharedFolder, KeyDynamicFolder, "dyn");
            this[KeyDocumentFolder] = this.GetFolderPath(this.SharedFolder, KeyDocumentFolder, "files");

            this["wd"] = this["wd"].IfEmpty("".InContainer() ? "/etc/wd" : "".WorkingDirectory());

            string sPort = this["routing_port"].IfEmpty("80");
            if (!this.TraefikHive.HasValue()) sPort = "$" + sPort;
            this["routing_port"] = sPort;

            this["nosql"] = this["nosql"].IfEmpty("mongodb");

            if (this.Process.IsSameValue("{proc}") || !this.Process.HasValue()) this.Process = "";
            this.Verbose = !!this.Verbose;

            if (this.TraefikHive.IsSameValue(this[EnvironmentClass.KeyHive]))
            {
                this.LogInfo("Hive will host Traefik");

                this.Remove("hive_traefik");
                this.Add("hive_traefik", this.TraefikHive);
                this.Add("hive_traefik", "*");
            }

            // SIO (used elsewhere, so do not change!)
            List<string> c_Channels = new List<string>();
            string sSite = this["site"];
            c_Channels.Add(sSite.MD5HashString());
            sSite += "ws";
            c_Channels.Add(sSite.MD5HashString());
            c_Channels.Add(sSite.MD5HashString().MD5HashString());
            this.SIOChannels = c_Channels;


            // Set my own ID
            if (!this.ID.HasValue()) this.ID = "".GUID();

            // -------------------------------------------------------------

            this.LogVerbose("Params: {0}".FormatString(this.SynchObject.ToSimpleString()));

            // Tell user
            this.LogInfo("ID is {0} in hive {1}".FormatString(this.ID, this[KeyHive]));
            this.LogInfo("Root folder is {0}".FormatString(this.RootFolder));
            this.LogInfo("Documents folder is {0}".FormatString(this.DocFolder));
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
            if (this.HTTP != null)
            {
                this.HTTP.Dispose();
                this.HTTP = null;
            }

            if (this.IRouter != null)
            {
                this.IRouter.Dispose();
                this.IRouter = null;
            }

            if (this.IFNS != null)
            {
                this.IFNS.Dispose();
                this.IFNS = null;
            }

            if (this.IGlobals != null)
            {
                this.IGlobals.Dispose();
                this.IGlobals = null;
            }

            base.Dispose();
        }
        #endregion

        #region Enums
        public enum Profiles
        {
            Fns = 1,
            Routes = 2,
            Procs = 4
        }
        #endregion

        #region Indexer
        /// <summary>
        /// 
        /// setting access
        /// 
        /// </summary>
        /// <param name="key">The key to be used</param>
        /// <returns>The setting value</returns>
        public override string this[string key]
        {
            get => base[key].IfEmpty(Environment.GetEnvironmentVariable("nx_" + key));
            set
            {
                // Did it change?
                if (!this[key].IsSameValue(value))
                {
                    // Store in memory
                    base[key] = value;
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// GUID for the instance
        /// 
        /// </summary>
        public string ID
        {
            get { return this[KeyID]; }
            set { this[KeyID] = value; }
        }

        /// <summary>
        /// 
        /// URL that gets back to me
        /// 
        /// </summary>
        public string LoopbackURL
        {
            get
            {
                string sAns = this[KeyLoopbackURL];

                if (!sAns.HasValue())
                {
                    string sDomain = this.Domain;
                    if (sDomain.HasValue())
                    {
                        sAns = "http";
                        if (this.UsesSSL) sAns += "s";
                        sAns += "://{0}".FormatString(sDomain);

                        this[KeyLoopbackURL] = sAns;
                    }
                }

                return sAns;
            }
            set { this[KeyLoopbackURL] = value; }
        }

        /// <summary>
        /// 
        /// Are we setup for SSL?
        /// 
        /// </summary>
        public bool UsesSSL
        {
            get { return this["certbot_email"].HasValue() && !this.Domain.IsIPV4() && !this.Domain.IsIPV6(); }
        }
        #endregion

        /// <summary>
        /// 
        /// The public URL
        /// 
        /// </summary>
        public string ReachableURL
        {
            get
            {
                // Assume none
                string sAns = null;

                string sDomain = this.Domain;
                if (sDomain.HasValue())
                {
                    sAns = "http";
                    if (this.UsesSSL) sAns += "s";
                    sAns += "://{0}".FormatString(sDomain);
                }

                return sAns.IfEmpty(this.LoopbackURL);
            }
        }

        #region Callbacks
        public Func<bool> ValidatonCallback { get; set; }

        /// <summary>
        /// 
        /// The Kubernetes readiness callback
        /// 
        /// </summary>
        public Func<string> ReadinessCallback { get; set; }
        #endregion

        #region Managers
        /// <summary>
        /// 
        /// The call router
        /// 
        /// </summary>
        private RouterClass IRouter { get; set; }
        public RouterClass Router
        {
            get
            {
                if (this.IRouter == null)
                {
                    this.IRouter = new RouterClass(this);
                    //this.IRouter.Load();
                }
                return this.IRouter;
            }
        }

        /// <summary>
        /// 
        /// The table of processors
        /// 
        /// </summary>
        private ProcsClass IProcs { get; set; }
        public ProcsClass Procs
        {
            get
            {
                if (this.IProcs == null)
                {
                    this.IProcs = new ProcsClass(this);
                    //this.IProcs.Load();
                }
                return this.IProcs;
            }
        }

        /// <summary>
        /// 
        /// The table of functions
        /// 
        /// </summary>
        private FNSClass IFNS { get; set; }
        public FNSClass FNS
        {
            get
            {
                if (this.IFNS == null)
                {
                    this.IFNS = new FNSClass(this);
                    //this.IFNS.Load();
                }
                return this.IFNS;
            }
        }

        /// <summary>
        /// 
        /// The HTTP server
        /// 
        /// </summary>
        public HTTPClass HTTP { get; private set; }

        /// <summary>
        /// 
        /// Site information
        /// 
        /// </summary>
        private SiteInfoClass ISiteInfo { get; set; }
        public SiteInfoClass SiteInfo
        {
            get
            {
                if (this.ISiteInfo == null) this.ISiteInfo = new SiteInfoClass(this);

                return this.ISiteInfo;
            }
        }

        /// <summary>
        /// 
        /// A global place where user objects can be stored
        /// 
        /// </summary>
        private ObjectsClass IGlobals { get; set; }
        public ObjectsClass Globals
        {
            get
            {
                if (this.IGlobals == null) this.IGlobals = new ObjectsClass(this);

                return this.IGlobals;
            }
        }

        /// <summary>
        ///  
        /// The Docker interface
        ///  
        /// </summary>
        private HiveClass IHive { get; set; }
        public HiveClass Hive
        {
            get
            {
                if (this.IHive == null)
                {
                    this.IHive = new HiveClass(this);
                    this.IHive.Initialize();
                }

                return this.IHive;
            }
        }

        /// <summary>
        /// 
        /// Message messenger
        /// 
        /// </summary>
        private NX.Engine.SocketIO.ManagerClass IMessenger { get; set; }
        public NX.Engine.SocketIO.ManagerClass Messenger
        {
            get
            {
                if (this.IMessenger == null)
                {
                    this.IMessenger = new ManagerClass(this);
                }

                return this.IMessenger;
            }
        }

        /// <summary>
        /// 
        /// Resets the hive
        /// 
        /// </summary>
        public void ResetHive()
        {
            this.IHive = null;
        }

        /// <summary>
        /// 
        /// NginX Information
        /// 
        /// </summary>
        private NginX.ServicesClass INginXInfo { get; set; }
        public NginX.ServicesClass NginXInfo
        {
            get
            {
                if (this.INginXInfo == null) this.INginXInfo = new NginX.ServicesClass(this);

                return this.INginXInfo;
            }
        }
        #endregion

        #region Predefined params
        /// <summary>
        /// 
        /// Function to run at startup
        /// 
        /// </summary>
        public string Process
        {
            get { return this[KeyProcess]; }
            set { this[KeyProcess] = value.IfEmpty(); }
        }

        /// <summary>
        /// 
        /// The hive name
        /// 
        /// </summary>
        public string HiveName
        {
            get { return this[KeyHive]; }
        }

        /// <summary>
        /// 
        /// Path to configuration file
        /// 
        /// </summary>
        public string Config
        {
            get { return this[KeyConfig]; }
        }

        /// <summary>
        /// 
        /// DIsplay all messages?
        /// 
        /// </summary>
        public bool Verbose
        {
            get { return this[KeyVerbose].ToBoolean(); }
            set { this[KeyVerbose] = value.ToString(); }
        }

        /// <summary>
        /// 
        ///  The number of threads to run
        ///  Default: 4
        ///  
        /// </summary>
        public int HTTPThreads
        {
            get { return this[KeyThreads].ToInteger(4); }
            set { this[KeyThreads] = value.ToString(); }
        }

        /// <summary>
        /// 
        /// Port to listen on
        /// Default: 8086
        /// 
        /// </summary>
        public int HTTPPort
        {
            get { return this[KeyPort].ToInteger(8086); }
            set { this[KeyPort] = value.ToString(); }
        }

        /// <summary>
        /// 
        /// The hive that holds the traefik system
        /// 
        /// </summary>
        public string TraefikHive
        {
            get { return this[KeyTraefikHive]; }
        }

        /// <summary>
        /// 
        /// Format that any text return should be in
        /// Default: json
        /// 
        /// auto
        /// json
        /// xml
        /// 
        /// </summary>
        public string ResponseFormat
        {
            get { return this[KeyRespFormat].IfEmpty("auto"); }
        }

        /// <summary>
        /// 
        /// The UI system to use
        /// Default: react
        /// 
        /// </summary>
        public string UI
        {
            get { return this[KeyUI]; }
        }

        /// <summary>
        /// 
        /// prefix to use for all system names
        /// 
        /// </summary>
        public string SystemPrefix
        {
            get { return this[KeySystemPrefix].IfEmpty("$$"); }
        }

        /// <summary>
        /// 
        /// Folder where all the other folders branch from settings are kept.
        /// Default: #workingdirectory#
        /// 
        /// </summary>
        public string RootFolder
        {
            get { return this[KeyRootFolder]; }
        }

        /// <summary>
        /// 
        /// Folder to hold the loaded DLLs
        /// Default: #RootFolder#/dyn
        /// 
        /// </summary>
        public string DynamicFolder
        {
            get { return this[KeyDynamicFolder]; }
        }

        /// <summary>
        /// 
        /// Folder to hold all the Key/Values
        /// Default: #RootFolder#/kv
        /// 
        /// </summary>
        public string SharedFolder
        {
            get { return this[KeySharedFolder]; }
        }

        /// <summary>
        /// 
        /// Folder to hold the documents
        /// Default: #SharedFolder#/files
        /// 
        /// </summary>
        public string DocFolder
        {
            get { return this[KeyDocumentFolder]; }
        }

        #region Git items
        /// <summary>
        /// 
        /// The token to be used when accessing the Git repos
        /// 
        /// </summary>
        public string GitToken
        {
            get { return this[KeyGITToken]; }
        }

        /// <summary>
        /// 
        /// The Git repo where the loadable dlls are kept.
        /// These are loaded at start time and only loaded if any
        /// change took place
        /// 
        /// </summary>
        public string GitRepo
        {
            get { return this[KeyGITRepo]; }
        }

        /// <summary>
        /// 
        /// The URL for the Git system.
        /// Only needed if the system is not Git.com
        /// 
        /// </summary>
        public string GitURL
        {
            get { return this[KeyGITURL]; }
        }

        /// <summary>
        /// 
        /// The product id to track any calls
        /// Default: NX.Server
        /// 
        /// </summary>
        public string GitProduct
        {
            get { return this[KeyGITProduct].IfEmpty(this.ObjectName()); }
        }
        #endregion

        /// <summary>
        /// 
        /// Are we making a genome?
        /// 
        /// </summary>
        public bool MakeGenome
        {
            get { return this[EnvironmentClass.KeyMakeGenome].ToBoolean(); }
        }

        /// <summary>
        /// 
        /// Are we making a bee?
        /// 
        /// </summary>
        public bool MakeBee
        {
            get { return this[EnvironmentClass.KeyMakeBee].ToBoolean(); }
        }

        /// <summary>
        /// 
        /// True if we are making things
        /// 
        /// </summary>
        public bool InMakeMode
        {
            get { return !"".InContainer() && (this.MakeGenome || this.MakeBee); }
        }

        /// <summary>
        /// 
        /// Values used when building substitutions
        /// 
        /// </summary>
        public JObject AsParameters
        {
            get
            {
                // Make a copy
                JObject c_Ans = this.SynchObject.Clone();

                // Remove the locals
                c_Ans.Remove(KeyProcess);
                c_Ans.Remove(KeyMakeGenome);
                c_Ans.Remove(KeyCodeFolder);
                c_Ans.Remove(KeyMakeBee);

                // Virtual
                c_Ans.Set("siochannel", this.SIOChannels.Join(","));
                c_Ans.Set("url", this.LoopbackURL);
                c_Ans.Set("publicurl", this.ReachableURL);
                c_Ans.Set("protocol", "http" + (this.UsesSSL ? "s" : "") + "//");

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// Returns a list of fields
        /// 
        /// </summary>
        public List<string> Fields
        {
            get
            {
                List<string> c_Ans = this.GetAsJArray("field").ToList();

                // Do we get from system
                if (this["field_localip"].FromDBBoolean() && "".IsLinux())
                {
                    c_Ans = new List<string>();
                }

                // Any?
                if (c_Ans.Count == 0)
                {
                    c_Ans.Add("".GetLocalIP() + ":2375");
                }

                return c_Ans;
            }
        }

        /// <summary>
        /// 
        /// The domain of the site
        /// 
        /// </summary>
        public string Domain
        {
            get { return this["domain"].IfEmpty(this.Fields[0].RemoveProtocol().RemovePort()); }
            set { this["domain"] = value; }
        }

        /// <summary>
        /// 
        /// The SocketIO channels used 
        /// 
        /// </summary>
        public List<string> SIOChannels { get; set; }
        #endregion

        #region Modules
        /// <summary>
        /// 
        /// Loads a dll at run time
        /// 
        /// </summary>
        /// <param name="path">The path to the dll</param>
        public void LoadModule(string path, Profiles profile = Profiles.Fns | Profiles.Procs | Profiles.Routes)
        {
            // Do we have a name?
            if (path.HasValue())
            {
                // In case just the name
                //if (!path.GetDirectoryFromPath().HasValue()) path = "".WorkingDirectory().CombinePath(path);

                // Load any functions
                if (profile.Contains(Profiles.Fns)) this.FNS.LoadFile(path, this);
                // Load any routes
                if (profile.Contains(Profiles.Routes)) this.Router.LoadFile(path, this);
                // Load any processors
                if (profile.Contains(Profiles.Procs)) this.Procs.LoadFile(path, this);

                this.LogInfo("Loaded module {0}".FormatString(path));
            }
        }

        //public void LoadDLL(string filename)
        //{
        //    try
        //    {
        //        Assembly c_Assm =  Assembly.LoadFile("".WorkingDirectory().CombinePath(filename + ".dll"));

        //        this.LogInfo("LoadDLL: {0} loaded - {1}", filename, c_Assm.FullName);
        //    }
        //    catch (Exception e)
        //    {
        //        this.LogException("At LoaDLL {0}".FormatString(filename), e);
        //    }
        //}

        private List<string> UsedModules { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// Loads a DLL into the routing or function
        /// subsystems.  The module must be stored
        /// in the #workingdirectory#/modules folder
        /// otherwise the Git repository will be used.
        /// In this case, the module should be in the
        /// form of owner/repo/module or /repo/module,
        /// which will use the owner from the git_repo
        /// setting
        /// 
        /// </summary>
        /// <param name="module">The module name</param>
        public void Use(string module, Profiles profile = Profiles.Fns | Profiles.Procs | Profiles.Routes)
        {
            // Only once
            if (!this.UsedModules.Contains(module))
            {
                // Add
                this.UsedModules.Add(module);

                //
                this.LogVerbose("Using {0}".FormatString(module));

                // Remove the .dll if any
                if (module.EndsWith(".dll")) module = module.Substring(0, module.Length - 1);

                // And a possible Git call
                string sGPath = "";
                // Find the delimiter
                int iPos = module.LastIndexOf("/");
                // Any?
                if (iPos != -1)
                {
                    // Parse
                    sGPath = module.Substring(0, iPos);
                    module = module.Substring(iPos + 1);
                }

                // NX module?
                if (module.Contains(".") || !sGPath.HasValue())
                {
                    // Load
                    this.LoadModule(module + ".dll", profile);
                }
                else if (sGPath.HasValue())
                {
                    // Load from Git repository
                    using (GitClass c_Git = new GitClass(this))
                    {
                        // Split the Git path
                        List<string> c_Path = new List<string>(sGPath.Split("/"));
                        // Split the git_repo
                        List<string> c_FromEnv = new List<string>(this.GitRepo.IfEmpty().Split("/"));

                        // Did the caller use a leading /?
                        if (!sGPath.HasValue())
                        {
                            // Use environment
                            c_Path = c_FromEnv;
                        }

                        if (c_Path.Count == 1)
                        {
                            // Use the user name from environment
                            c_Path.Insert(0, c_FromEnv[0]);
                        }
                        else
                        {
                            // Fill any empty
                            if (!c_Path[0].HasValue()) c_Path[0] = c_FromEnv[0];
                            if (!c_Path[1].HasValue()) c_Path[1] = c_FromEnv[1];
                        }

                        // Make the repo name
                        string sRepo = c_Path[0] + "/" + c_Path[1];

                        // Load
                        string sFile = c_Git.FetchFile(sRepo, module);
                        // Add to system
                        this.LoadModule(sFile, profile);
                    }
                }
            }
        }
        #endregion

        #region Loggers
        /// <summary>
        /// 
        /// The master logger,
        /// Writes out a message to the console showing
        /// the timestamp and the message
        /// 
        /// </summary>
        /// <param name="prefix">The text after the timestamp</param>
        /// <param name="msg">The message. The first value is the format string</param>
        public static void ITimestamp(string prefix, string msg)
        {
            string sTS = "".Now().ToString(SysConstantClass.DateFormat) + " ";

            // Write
            Console.WriteLine(sTS + prefix.IfEmpty() + msg);
        }

        /// <summary>
        /// 
        /// Logs an informational message
        /// 
        /// </summary>
        /// <param name="msg">The message. The first value is the format string</param>
        public static void ILogInfo(string msg)
        {
            EnvironmentClass.ITimestamp("", msg);
        }

        /// <summary>
        /// 
        /// Logs an error message
        /// 
        /// </summary>
        /// <param name="msg">The message. The first value is the format string</param>
        public static void ILogError(string msg)
        {
            EnvironmentClass.ITimestamp("".LPad(5, "*") + " ERROR: ", msg);
        }

        /// <summary>
        /// 
        /// Logs an exception
        /// 
        /// </summary>
        /// <param name="e">The exception to be logged</param>
        public static void ILogException(Exception e)
        {
            EnvironmentClass.ILogException(null, e);
        }

        /// <summary>
        /// 
        /// Logs an exception
        /// 
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="e">The exception to be logged</param>
        public static void ILogException(string msg, Exception e)
        {
            // Must have an exception
            if (e != null)
            {
                EnvironmentClass.ILogError("@ " + msg.IfEmpty("An error has occurred") + "\r\n\r\n" + e.GetAllExceptions() + "\r\n\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// Same as ILogInfo but called via env object
        /// 
        /// </summary>
        /// <param name="msg">The message. The first value is the format string</param>
        public void LogInfo(string msg)
        {
            EnvironmentClass.ITimestamp("", msg);
        }

        /// <summary>
        /// 
        /// Same as ILogError but called via env object
        /// 
        /// </summary>
        /// <param name="msg">The message. The first value is the format string</param>
        public void LogError(string msg)
        {
            EnvironmentClass.ILogError(msg);
        }

        /// <summary>
        /// 
        /// Same as ILogException but called via env object
        /// 
        /// </summary>
        /// <param name="e">The exception to be logged</param>
        public void LogException(Exception e)
        {
            EnvironmentClass.ILogException(e);
        }

        /// <summary>
        /// 
        /// Same as ILogException but called via env object
        /// 
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="e">The exception to be logged</param>
        public void LogException(string msg, Exception e)
        {
            EnvironmentClass.ILogException(msg, e);
        }

        /// <summary>
        /// 
        /// Same as ILogInfo but called via env object if in verbose mode
        /// 
        /// </summary>
        /// <param name="msg">The message. The first value is the format string</param>
        public void LogVerbose(string msg)
        {
            if (this.Verbose)
            {
                EnvironmentClass.ILogInfo(msg);
            }
        }
        #endregion

        #region Control
        /// <summary>
        /// 
        /// Starts the system
        /// 
        /// </summary>
        public void Start(bool inithive, params string[] uses)
        {
            // Do we have an HTTP server?
            if (this.HTTP == null)
            {
                // Create it
                this.HTTP = new HTTPClass(this);
                // And start
                this.HTTP.Start();

                // Load repo if any
                if (this.GitRepo.HasValue())
                {
                    using (GitClass c_Git = new GitClass(this))
                    {
                        c_Git.FetchRepo(this.GitRepo);
                    }
                }

                // Load
                var x = this.FNS;
                var y = this.Router;
                var z = this.Procs;

                // Load the built-in
                foreach (string sUse in uses)
                {
                    this.Use(sUse);
                }

                // Do we init the hive?
                if (inithive)
                {
                    HiveClass h = this.Hive;
                }

                // Now others
                JArray c_Uses = this.GetAsJArray("uses");
                // Any?
                if (c_Uses != null)
                {
                    for (int i = 0; i < c_Uses.Count; i++)
                    {
                        this.Use(c_Uses.Get(i));
                    }
                }
            }
        }

        ///// <summary>
        ///// 
        ///// Stops the HTTP server
        ///// 
        ///// </summary>
        //public void Stop()
        //{
        //    if (this.HTTP != null)
        //    {
        //        this.HTTP.Dispose();
        //        this.HTTP = null;
        //    }
        //}

        ///// <summary>
        ///// 
        ///// Recycle the HTTP server
        ///// 
        ///// </summary>
        //public void Recycle()
        //{
        //    this.Stop();
        //    this.StartX();
        //}

        /// <summary>
        /// 
        /// Cretes a debug breakpoint
        /// 
        /// </summary>
        public void Debug()
        {
            if (!"".InContainer() && "".InDebug())
            {
                System.Diagnostics.Debugger.Break();
            }
        }
        #endregion

        #region Settings
        /// <summary>
        /// 
        /// Returns the actual folder path
        /// 
        /// </summary>
        /// <param name="key">The setting to get</param>
        /// <param name="defvalue">Value if empty</param>
        /// <returns></returns>
        public string GetFolderPath(string basefolder, string key, string defvalue)
        {
            // Get the folder
            string sFolder = this[key].IfEmpty(defvalue);
            // By itself
            if (sFolder.StartsWith("@"))
            {
                // Yes, use it
                sFolder = sFolder.Substring(1);
            }
            else
            {
                // Is a subfolder of root
                sFolder = basefolder.CombinePath(sFolder);
            }

            return sFolder;
        }

        /// <summary>
        /// 
        /// Parses an IP address from settings.
        /// The key should be task_defport, for example: redis_6379
        /// 
        /// </summary>
        /// <param name="key">The key to lookup</param>
        /// <returns>The IP address</returns>
        public IPAddressClass GetIPAddress(string key)
        {
            // Parse
            IPAddressClass c_Ans = new IPAddressClass(this[key]);
            // Default port?
            if (c_Ans.UsesDefaultPort)
            {
                // Get the trailing numbers
                c_Ans.Port = key.TrailingNumber().ToInteger(80);
            }

            return c_Ans;
        }
        #endregion

        #region Support
        /// <summary>
        /// 
        /// A way to call a function
        /// 
        /// </summary>
        /// <param name="fn">The function name</param>
        /// <param name="store">The store</param>
        /// <returns></returns>
        public StoreClass FN(string fn, StoreClass store = null)
        {
            using (HTTPCallClass c_Call = new HTTPCallClass(this))
            {
                return c_Call.FN(fn, store);
            }
        }

        /// <summary>
        /// 
        /// A way to call a function
        /// 
        /// </summary>
        /// <param name="fn">The function name</param>
        /// <param name="values">The vparamates as an array</param>
        /// <returns></returns>
        public StoreClass FN(string fn, params string[] values)
        {
            using (HTTPCallClass c_Call = new HTTPCallClass(this))
            {
                return c_Call.FN(fn, values);
            }
        }

        /// <summary>
        /// 
        /// Adds the system prefix to a value
        /// 
        /// </summary>
        /// <param name="value">The value to apply the prefix to</param>
        /// <returns>The prefixed value</returns>
        public string ApplySystemPrefix(string value)
        {
            return this.SystemPrefix + this.Hive.Name + "_" + value;
        }

        /// <summary>
        /// 
        /// Adds a value to an arry field
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddToArray(string key, string value)
        {
            // Get
            JArray c_Wkg = this.GetAsJArray(key);
            // Not there?
            if(!c_Wkg.Contains(key))

            {
                // Add
                c_Wkg.Add(value);
                // Save
                this.Set(key, c_Wkg);
            }
        }
        #endregion
    }
}