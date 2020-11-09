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
/// Install-Package Newtonsoft.Json -Version 12.0.3
/// 

using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;
using Octokit;

using NX.Engine.Hive;
using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// The global environment.  
    /// 
    /// </summary>
    public class EnvironmentClass : SynchronizedStoreClass, ILogger
    {
        #region Constants
        private const string KeyID = "id";

        private const string KeyThreads = "http_threads";
        private const string KeyPort = "http_port";
        private const string KeyTraefikHive = "traefik_hive";
        private const string KeyConfig = "config";
        private const string KeyVerbose = "verbose";

        private const string KeyTier = "tier";

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
        /// Constructor
        /// 
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public EnvironmentClass(string[] args)
            : base(null, "_env")
        {
            // Check to see if secured
            string sCurrentCode = this.Router.SecureCode;

            // Load the arguments
            this.Parse(args);

            // Did we get a secure code?
            string sSecureCode = this[EnvironmentClass.KeySecureCode];
            if (sSecureCode.HasValue())
            {
                // Parse
                ItemClass c_SC = new ItemClass(sSecureCode);

                // Was the original secured?
                if (!sCurrentCode.IsSameValue(RouterClass.UnsecureCode))
                {
                    // Does it match the new one?
                    if (!c_SC.Key.IsSameValue(sCurrentCode))
                    {
                        // Bad
                        sSecureCode = null;
                    }
                }

                // Do we still have a value?
                if (sSecureCode.HasValue())
                {
                    // Parse
                    c_SC = new ItemClass(sSecureCode);

                    // Does it have a new one?
                    if (c_SC.Value.HasValue())
                    {
                        sSecureCode = c_SC.Value;
                    }
                }

                // Save
                if (sSecureCode.HasValue())
                {
                    this[EnvironmentClass.KeySecureCode] = sSecureCode;
                }
                else
                {
                    this.Remove(EnvironmentClass.KeySecureCode);
                }
            }

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

            // -------------------------------------------------------------
            //
            // In this section we setup the defaults
            //

            this[KeyHive] = this[KeyHive].IfEmpty("default");
            this[KeyTier] = this[KeyTier].IfEmpty("latest");
            this[KeyRepoProject] = this[KeyRepoProject].IfEmpty("nxproject");

            this[KeyRootFolder] = this[KeyRootFolder].IfEmpty("".WorkingDirectory()).CombinePath(this[KeyHive]);

            this[KeySharedFolder] = this.GetFolderPath(this.RootFolder, KeySharedFolder, "shared");
            this[KeyDynamicFolder] = this.GetFolderPath(this.SharedFolder, KeyDynamicFolder, "dyn");
            this[KeyDocumentFolder] = this.GetFolderPath(this.SharedFolder, KeyDocumentFolder, "files");

            this["wd"] = this["wd"].IfEmpty("".InContainer() ? "/etc/wd" : "".WorkingDirectory());
            this["nginx_port"] = this["nginx_port"].IfEmpty(this.TraefikHive.HasValue() ? "80" : "$80");
            this["nosql"] = this["nosql"].IfEmpty("mongodb");
            //if (!this.GetAsJArray("field").HasValue()) this.Set("field", "".GetLocalIP() + ":2375");

            if (this.Process.IsSameValue("{proc}") || !this.Process.HasValue()) this.Process = "";
            this.Verbose = !!this.Verbose;

            if(this.TraefikHive.IsSameValue(this[EnvironmentClass.KeyHive]))
            {
                this.Remove("hive_traefik");
                this.Add("hive_traefik", this.TraefikHive);
                this.Add("hive_traefik", "*");
            }

            // Set my own ID
            if (!this.ID.HasValue()) this.ID = "".GUID();

            // -------------------------------------------------------------

            this.LogVerbose("Params: {0}".FormatString(this.SynchObject.ToSimpleString()));

            // Tell user
            this.LogInfo("ID is {0} in hive {1}:{2}".FormatString(this.ID, this[KeyHive], this.Tier));
            this.LogInfo("Root folder is {0}".FormatString(this.RootFolder));
            this.LogInfo("Documents folder is {0}".FormatString(this.DocFolder));

            // To handle the special keys
            this.CallbackBeforeSet = delegate (string key, object value)
            {
                // default
                SetOptions eAns = SetOptions.OK;

                // According to name
                switch (key)
                {
                    case KeyID:
                    case KeyProcess:
                    case KeyMakeGenome:
                    case KeyCodeFolder:
                    case KeyMakeBee:
                    case KeyLoopbackURL:
                    case KeyConfig:

                    case "wd":
                    case "force_queen":

                        eAns = SetOptions.SaveButLocal;
                        break;
                }

                return eAns;
            };

            // And setup the synch (special case as we cannot pass this to base constructor)
            this.SetupSynch(this);

            // Handle very special cases
            this.ChangedCalled += delegate (string key, object value)
            {
                switch (key)
                {
                    case "field":
                        // The hive changed
                        if (this.IHive != null)
                        {
                            this.IHive.Dispose();
                            this.IHive = null;
                        }
                        // And rebuild
                        var junk = this.Hive;
                        break;
                }
            };

            // If we have a field, setup the hive
            if (this["field"].HasValue())
            {
                var junk = this.Hive;
            }
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
            if (this.IMessenger != null)
            {
                this.IMessenger.Dispose();
                this.IMessenger = null;
            }

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
            get { return this[KeyLoopbackURL]; }
            set { this[KeyLoopbackURL] = value; }
        }
        #endregion

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
        /// The messenger
        /// 
        /// </summary>
        private MessengerClass IMessenger { get; set; }
        public MessengerClass Messenger
        {
            get
            {
                if (this.IMessenger == null)
                {
                    this.IMessenger = new MessengerClass(this);
                }
                return this.IMessenger;
            }
        }

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
        /// The current tier
        /// 
        /// </summary>
        public string Tier
        {
            get { return this[KeyTier]; }
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

                return c_Ans;
            }
        }

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

                this.LogInfo("Loaded {0}".FormatString(path));
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
                this.LogInfo("Using {0}".FormatString(module));

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
                if (module.Contains("."))
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
        public void Start()
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
                var a = this.FNS;
                var b = this.Router;
                var c = this.Procs;

                // Create the hive
                var x = this.Hive;

                // Load the built-in
                this.Use("Fn.System");
                this.Use("Proc.Default");
                this.Use("Route.System");
                this.Use("Route.UI");
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
        #endregion
    }
}