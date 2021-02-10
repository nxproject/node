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

using System.Collections.Generic;

using NX.Engine;
using NX.Engine.Hive;
using NX.Engine.NginX;
using NX.Shared;

namespace Proc.NginX
{
    /// <summary>
    /// 
    /// NginX interface
    /// 
    /// </summary>
    public class ManagerClass : BumbleBeeClass
    {
        #region Constructor
        public ManagerClass(EnvironmentClass env)
            : base(env, "nginx")
        {
            this.Parent.LogInfo("NginX");

            // Do we support SSL?
            if (this.Parent["nginx_ssl"].FromDBBoolean())
            {
                this.Certbot = new BumbleBeeClass(this.Parent, "certbot");
                this.Certbot.SetNginxInformation(".well-known", false);
            }

            // Make a starting config
            this.MakeConfig(false);

            // Handle the events
            this.AvailabilityChanged += delegate (bool isavailable)
            {
                // Check
                this.MakeConfig(isavailable && this.IsQueen);
            };

            // Track queen changes
            this.QueenChanged += delegate (bool isqueen)
            {
                // Check
                this.MakeConfig(this.IsAvailable && isqueen);
            };

            // Link for DNA changes
            this.Parent.Hive.Roster.DNAChanged += delegate (string dna, List<string> urls)
            {
                // Valid?
                if (this.ValidDNA(dna))
                {
                    // Update
                    this.UpdateProcess(dna, urls);
                    // And make config
                    this.MakeConfig(this.IsAvailable && this.IsQueen);
                }
            };

            // Get a list of all DNAs
            List<string> c_DNA = this.Parent.Hive.Roster.GetDNAs();
            // Loop thru
            foreach (string sDNA in c_DNA)
            {
                // Valid?
                if (this.ValidDNA(sDNA))
                {
                    // Update
                    this.UpdateProcess(sDNA, this.Parent.Hive.Roster.GetLocationsForDNA(sDNA));
                }
            }

            // Bootstap
            this.CheckForAvailability();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// A map of processors vs. URL list
        /// 
        /// </summary>
        private NamedListClass<List<string>> Map { get; set; } = new NamedListClass<List<string>>();

        /// <summary>
        /// 
        /// Certbot bee
        /// 
        /// </summary>
        private BumbleBeeClass Certbot { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Is this a DNA we want
        /// 
        /// </summary>
        /// <param name="dna">The DNA</param>
        /// <returns>True if we keep track</returns>
        private bool ValidDNA(string dna)
        {
            return dna.StartsWith(HiveClass.ProcessorDNAName);
        }

        /// <summary>
        /// 
        /// Updates the map with a new list
        /// 
        /// </summary>
        /// <param name="dna">The DNA</param>
        /// <param name="urls">The list of URLs</param>
        private void UpdateProcess(string dna, List<string> urls)
        {
            // One we want?
            if (this.ValidDNA(dna))
            {
                // Add
                this.Map[dna] = urls;
            }
        }

        /// <summary>
        /// 
        /// Makes the NginX config
        /// 
        /// </summary>
        /// <param name="cando">True if we can make the config</param>
        private void MakeConfig(bool cando)
        {
            // The path to the file
            string sPath = "/etc/nginx";
            // Assure
            sPath.AssurePath();
            // And the file
            string sFile = sPath.CombinePath("nginx.conf");
            // Where the default fils are kept
            string sSource = "".WorkingDirectory().CombinePath("Hive/External/nginx/defaults");

            // Can we do this?
            if (cando || !sFile.FileExists())
            {
                //
                this.Parent.LogInfo("Creating nginx.conf");

                // Make the new config
                string sConf = this.MakeNginxConfig();
                // Changed?
                if (!sConf.IsExactSameValue(sFile.ReadFile()))
                {
                    // Copy from sources
                    this.Parent.LogInfo("{0} files copied {1} => {2}".FormatString(sSource.CopyDirectoryTree(sPath),
                                            sSource,
                                            sPath));

                    // Write out
                    sFile.WriteFile(sConf);

                    //
                    this.Parent.LogInfo("Nginx.conf has changed - {0}, {1} bytes".FormatString(sFile, sConf.Length));
                    //this.Parent.LogVerbose(sConf);

                    // Recycle
                    //this.Parent.Hive.AssureDNACount("nginx", 0, 0);
                    //this.Parent.Hive.AssureDNACount("nginx", 1);
                    this.Parent.Hive.RecycleDNA("nginx");
                }
                //
                this.Parent.LogVerbose("End of nginx.conf maintenance");
            }
        }

        /// <summary>
        /// 
        /// Makes the NginX.conf contents
        /// 
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        private string MakeNginxConfig()
        {
            //
            string sBody = "";

            // Map the environemnt
            EnvironmentClass c_Env = this.Parent;

            // The worker bee site
            string sWBSite = c_Env["routing_wb"].IfEmpty("workerbees");

            // Are we in debug?
            bool bDebug = c_Env["routing_debug"].ToBoolean();

            // Format
            sBody += "# Generated by MakeNginxConfig for {0}".FormatString(c_Env["hive"]).NginxComment(1);
            sBody += " On {0}".FormatString(System.DateTime.Now).NginxComment(1);
            sBody += " Debug: {0}".FormatString(bDebug).NginxComment(1);

            // Format
            sBody += "Logging".NginxComment(1);

            // Handle debug
            if (bDebug)
            {
                sBody += "access_log /etc/nginx/logs/req.log {0};".FormatString("nxproj_debug").NginxLine(1);
                sBody += "error_log /etc/nginx/logs/error.log debug;".NginxLine(1);
            }
            else
            {
                sBody += "access_log off;".NginxLine(1);
                sBody += "error_log /dev/null crit;".NginxLine(1);
            }

            sBody += "Mapping".NginxComment(1);

            sBody += "map $http_host $this_host {".NginxLine(1);
            sBody += "\"\" $host;".NginxLine(2);
            sBody += "default $http_host;".NginxLine(2);
            sBody += "}".NginxLine(1);

            sBody += "map $http_x_forwarded_host $the_host {".NginxLine(1);
            sBody += "default $http_x_forwarded_host;".NginxLine(2);
            sBody += "\"\" $this_host;".NginxLine(2);
            sBody += "}".NginxLine(1);

            sBody += "map $http_x_forwarded_proto $the_scheme {".NginxLine(1);
            sBody += "\"http\" \"http\";".NginxLine(2);
            sBody += "\"https\" \"https\";".NginxLine(2);
            sBody += "default $scheme; ".NginxLine(2);
            sBody += "}".NginxLine(1);

            sBody += "map $http_x_address $x_address {".NginxLine(1);
            sBody += "default $http_x_address;".NginxLine(2);
            sBody += "\"\" $remote_addr;".NginxLine(2);
            sBody += "}".NginxLine(1);

            // Error handling
            sBody += "Mask errors".NginxComment(1);
            sBody += "".NginxErrors();

            // SSL stuff
            bool bSSL = this.Parent["nginx_ssl"].FromDBBoolean();
            bool bCertbot = false;

            // 
            c_Env.LogVerbose("Creating bumble bee routes");

            //
            ItemsClass c_Bees = new ItemsClass(c_Env.GetAsJArray("routing_bumble"));
            sBody += "Bumble bee sites".NginxComment(1, true);
            // 
            List<string> c_Done = new List<string>();
            // Loop thru
            foreach (ItemClass c_Bee in c_Bees)
            {
                // Get the information
                InformationClass c_Info = this.Parent.NginXInfo[c_Bee.Key, ServicesClass.Types.BumbleBee];

                // Apply the entry
                c_Info.Apply(c_Bee);

                // Assure only once
                if (!c_Done.Contains(c_Info.Name.ToLower()))
                {
                    // Save
                    c_Done.Add(c_Info.Name.ToLower());

                    // Add the DNA
                    sBody += c_Info.NginxUpstream(c_Env.Hive.Roster.GetLocationsForDNA(c_Bee.Key));

                    // Cerbot
                    if (c_Info.Name.IsSameValue("certbot")) bCertbot = true;
                }
            }

            // 
            c_Env.LogVerbose("Creating proc routes");

            //
            ItemsClass c_Procs = new ItemsClass(c_Env.GetAsJArray("routing_proc"));
            sBody += "Proc Sites".NginxComment(1, true);
            // Loop thru
            foreach (ItemClass c_Proc in c_Procs)
            {
                // Get the information
                InformationClass c_Info = this.Parent.NginXInfo[c_Proc.Key, ServicesClass.Types.Proc];
                // Apply the entry
                c_Info.Apply(c_Proc);

                // Add the DNA
                sBody += c_Info.NginxUpstream(c_Env.Hive.Roster.GetLocationsForDNA(HiveClass.ProcessorDNAName + "." + c_Proc.Key));
            }

            // Make the worker bees 
            // Add the DNA
            sBody += sWBSite.NginxUpstream(c_Env.Hive.Roster.GetLocationsForDNA(HiveClass.ProcessorDNAName + "."));

            // 
            sBody += "Locations".NginxComment(1, true);

            // Defualt to the me field
            FieldClass c_Field = this.Parent.Hive.MeField;
            // Get the field to use
            string sField = this.Parent["field_nginx"];
            // Any?
            if (sField.HasValue())
            {
                // Get
                FieldClass c_Poss = this.Parent.Hive.GetField(sField);
                // Valid?
                if (c_Poss != null)
                {
                    // Use it
                    c_Field = c_Poss;
                }
            }

            // 
            sBody += "Server".NginxComment(1, true);

            //
            string sPort = this.Parent["routing_port"].NumOnly();
            bool bOpenServer = true;

            // SSL
            // TBD
            //if (bSSL && bCertbot)
            //{
            //    // 
            //    sBody += "Handle ceertbot".NginxComment(1);
            //    // 
            //    // Open the server
            //    sBody += this.Parent["domain"].IfEmpty(c_Field.URL.RemoveProtocol().RemovePort()).NginxServerStart();
            //    // Normal traffic
            //    sBody += "80".NginxListen();
            //    sBody += ".well-known".NginxLocation("", "certbot".NginxProxyPass());
            //    sBody += "".NginxServerEnd();

            //    // Normal work is done via 443
            //    sPort = "443";
            //    bOpenServer = false;
            //    // SSL open
            //    sBody += this.Parent["domain"].IfEmpty(c_Field.URL.RemoveProtocol().RemovePort()).NginxServerStart(sslCert.NginxListenSSL(sslKey, false, 443));
            //}

            // Open the server            
            if (bOpenServer)
            {
                sBody += this.Parent["domain"].IfEmpty(c_Field.URL.RemoveProtocol().RemovePort()).NginxServerStart();
            }
            // Set the port
            sBody += sPort.NginxListen();

            // Do the bees
            sBody += "Bumble Bees".NginxComment(2, true);
            //
            c_Done = new List<string>();
            // Loop thru
            foreach (ItemClass c_Bee in c_Bees)
            {
                // Get the information
                InformationClass c_Info = this.Parent.NginXInfo[c_Bee.Key, ServicesClass.Types.BumbleBee];

                // Only once
                if (!c_Done.Contains(c_Info.Name.ToLower()))
                {
                    // Flag
                    c_Done.Add(c_Info.Name.ToLower());

                    // Apply the entry
                    c_Info.Apply(c_Bee);

                    // Add the DNA
                    sBody += c_Info.NginxLocation("",
                        c_Info.NginxRemove(),
                        c_Bee.Key.NginxProxyPass());
                }
            }

            // Do the procs
            sBody += "Procs".NginxComment(2, true);
            // Loop thru
            foreach (ItemClass c_Proc in c_Procs)
            {
                // Get the information
                InformationClass c_Info = this.Parent.NginXInfo[c_Proc.Key, ServicesClass.Types.Proc];
                // Apply the entry
                c_Info.Apply(c_Proc);

                // Add the DNA
                sBody += c_Info.NginxLocation("",
                    c_Proc.Key.NginxProxyPass());
            }

            // The worker bees
            sBody += "".NginxLocation("",
                    sWBSite.NginxProxyPass());

            // Close
            sBody += "".NginxServerEnd();

            // Format
            sBody += "# End of generated code.".NginxComment(1);

            // And insert
            string sConfig = this.GetResource("nginx.cfg").FromBytes().ASCIIOnly(true);
            sConfig += sBody;
            sConfig += "}".NginxLine(0);
            sConfig = sConfig.Replace("\r", "").ASCIIOnly(true);

            //
            return sConfig;
        }
        #endregion
    }
}