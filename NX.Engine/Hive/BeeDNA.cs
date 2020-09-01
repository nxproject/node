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

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    public class BeeDNAClass : StoreClass
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="values">The values as a JObject</param>
        public BeeDNAClass(string dna, JObject values)
            : base(values)
        {
            this.DNA = dna;
        }
        #endregion

        #region Properties
        public string Hostname
        {
            get { return this["Hostname"]; }
            set { this["Hostname"] = value; }
        }

        public string Domainname
        {
            get { return this["Domainname"]; }
            set { this["Domainname"] = value; }
        }

        public string User
        {
            get { return this["User"]; }
            set { this["User"] = value; }
        }

        public bool AttachStdin
        {
            get { return this.GetAs<bool>("AttachStdin"); }
            set { this.Set("AttachStdin", value); }
        }

        public bool AttachStdout
        {
            get { return this.GetAs<bool>("AttachStdout"); }
            set { this.Set("AttachStdout", value); }
        }

        public bool AttachStderr
        {
            get { return this.GetAs<bool>("AttachStderr"); }
            set { this.Set("AttachStderr", value); }
        }

        public bool Tty
        {
            get { return this.GetAs<bool>("Tty"); }
            set { this.Set("Tty", value); }
        }

        public bool OpenStdin
        {
            get { return this.GetAs<bool>("OpenStdin"); }
            set { this.Set("OpenStdin", value); }
        }

        public bool StdinOnce
        {
            get { return this.GetAs<bool>("StdinOnce"); }
            set { this.Set("StdinOnce", value); }
        }

        public JArray Env
        {
            get { return this.GetAsJArray("Env"); }
            set { this.Set("Env", value); }
        }

        public JArray Cmd
        {
            get { return this.GetAsJArray("Cmd"); }
            set { this.Set("Cmd", value); }
        }

        public string Entrypoint
        {
            get { return this["Entrypoint"]; }
            set { this["Entrypoint"] = value; }
        }

        public string Image
        {
            get { return this["Image"]; }
            set { this["Image"] = value; }
        }

        public JObject Labels
        {
            get { return this.GetAsJObject("Labels"); }
            set { this.Set("Labels", value); }
        }

        public JObject Volumes
        {
            get { return this.GetAsJObject("Volumes"); }
            set { this.Set("Volumes", value); }
        }

        public string WorkingDir
        {
            get { return this["WorkingDir"]; }
            set { this["WorkingDir"] = value; }
        }

        public bool NetworkDisabled
        {
            get { return this.GetAs<bool>("NetworkDisabled"); }
            set { this.Set("NetworkDisabled", value); }
        }

        public string MacAddress
        {
            get { return this["MacAddress"]; }
            set { this["MacAddress"] = value; }
        }

        public JObject ExposedPorts
        {
            get { return this.GetAsJObject("ExposedPorts"); }
        }

        public string StopSignal
        {
            get { return this["StopSignal"]; }
            set { this["StopSignal"] = value; }
        }

        public int StopTimeout
        {
            get { return this.GetAs<int>("StopTimeout"); }
            set { this.Set("StopTimeout", value); }
        }

        public JObject HostConfig
        {
            get { return this.GetAsJObject("HostConfig"); }
            set { this.Set("HostConfig", value); }
        }

        public JArray Binds
        {
            get { return this.HostConfig.AssureJArray("Binds"); }
            set { this.HostConfig.Set("Binds", value); }
        }

        public JArray Links
        {
            get { return this.HostConfig.AssureJArray("Links"); }
            set { this.HostConfig.Set("Links", value); }
        }

        public int Memory
        {
            get { return this.HostConfig.GetAs<int>("Memory"); }
            set { this.HostConfig.Set("Memory", value); }
        }

        public int MemorySwap
        {
            get { return this.HostConfig.GetAs<int>("MemorySwap"); }
            set { this.HostConfig.Set("MemorySwap", value); }
        }

        public int MemoryReservation
        {
            get { return this.HostConfig.GetAs<int>("MemoryReservation"); }
            set { this.HostConfig.Set("MemoryReservation", value); }
        }

        public int KernelMemory
        {
            get { return this.HostConfig.GetAs<int>("KernelMemory"); }
            set { this.HostConfig.Set("KernelMemory", value); }
        }

        public int NanoCPUs
        {
            get { return this.HostConfig.GetAs<int>("NanoCPUs"); }
            set { this.HostConfig.Set("NanoCPUs", value); }
        }

        public int CpuPercent
        {
            get { return this.HostConfig.GetAs<int>("CpuPercent"); }
            set { this.HostConfig.Set("CpuPercent", value); }
        }

        public int CpuShares
        {
            get { return this.HostConfig.GetAs<int>("CpuShares"); }
            set { this.HostConfig.Set("CpuShares", value); }
        }

        public int CpuPeriod
        {
            get { return this.HostConfig.GetAs<int>("CpuPeriod"); }
            set { this.HostConfig.Set("CpuPeriod", value); }
        }

        public int CpuRealtimePeriod
        {
            get { return this.HostConfig.GetAs<int>("CpuRealtimePeriod"); }
            set { this.HostConfig.Set("CpuRealtimePeriod", value); }
        }

        public int CpuRealtimeRuntime
        {
            get { return this.HostConfig.GetAs<int>("CpuRealtimeRuntime"); }
            set { this.HostConfig.Set("CpuRealtimeRuntime", value); }
        }

        public int CpuQuota
        {
            get { return this.HostConfig.GetAs<int>("CpuQuota"); }
            set { this.HostConfig.Set("CpuQuota", value); }
        }

        public float CpusetCpus
        {
            get { return this.HostConfig.GetAs<float>("CpusetCpus"); }
            set { this.HostConfig.Set("CpusetCpus", value); }
        }

        public float CpusetMems
        {
            get { return this.HostConfig.GetAs<float>("CpusetMems"); }
            set { this.HostConfig.Set("CpusetMems", value); }
        }

        public int MaximumIOps
        {
            get { return this.HostConfig.GetAs<int>("MaximumIOps"); }
            set { this.HostConfig.Set("MaximumIOps", value); }
        }

        public int MaximumIOBps
        {
            get { return this.HostConfig.GetAs<int>("MaximumIOBps"); }
            set { this.HostConfig.Set("MaximumIOBps", value); }
        }

        public int BlkioWeight
        {
            get { return this.HostConfig.GetAs<int>("BlkioWeight"); }
            set { this.HostConfig.Set("BlkioWeight", value); }
        }

        public JArray BlkioWeightDevice
        {
            get { return this.HostConfig.AssureJArray("BlkioWeightDevice"); }
            set { this.HostConfig.Set("BlkioWeightDevice", value); }
        }

        public JArray BlkioDeviceReadBps
        {
            get { return this.HostConfig.AssureJArray("BlkioDeviceReadBps"); }
            set { this.HostConfig.Set("BlkioDeviceReadBps", value); }
        }

        public JArray BlkioDeviceReadIOps
        {
            get { return this.HostConfig.AssureJArray("BlkioDeviceReadIOps"); }
            set { this.HostConfig.Set("BlkioDeviceReadIOps", value); }
        }

        public JArray BlkioDeviceWriteBps
        {
            get { return this.HostConfig.AssureJArray("BlkioDeviceWriteBps"); }
            set { this.HostConfig.Set("BlkioDeviceWriteBps", value); }
        }

        public JArray BlkioDeviceWriteIOps
        {
            get { return this.HostConfig.AssureJArray("BlkioDeviceWriteIOps"); }
            set { this.HostConfig.Set("BlkioDeviceWriteIOps", value); }
        }

        public int MemorySwappiness
        {
            get { return this.HostConfig.GetAs<int>("MemorySwappiness"); }
            set { this.HostConfig.Set("MemorySwappiness", value); }
        }

        public bool OomKillDisable
        {
            get { return this.HostConfig.GetAs<bool>("OomKillDisable"); }
            set { this.HostConfig.Set("OomKillDisable", value); }
        }

        public int OomScoreAdj
        {
            get { return this.HostConfig.GetAs<int>("OomScoreAdj"); }
            set { this.HostConfig.Set("OomScoreAdj", value); }
        }

        public string PidMode
        {
            get { return this.HostConfig.Get("PidMode"); }
            set { this.HostConfig.Set("PidMode", value); }
        }

        public int PidsLimit
        {
            get { return this.HostConfig.GetAs<int>("PidsLimit"); }
            set { this.HostConfig.Set("PidsLimit", value); }
        }

        public JObject PortBindings
        {
            get { return this.HostConfig.AssureJObject("PortBindings"); }
            set { this.HostConfig.Set("PortBindings", value); }
        }

        public bool PublishAllPorts
        {
            get { return this.HostConfig.GetAs<bool>("PublishAllPorts"); }
            set { this.HostConfig.Set("PublishAllPorts", value); }
        }

        public bool Privileged
        {
            get { return this.HostConfig.GetAs<bool>("Privileged"); }
            set { this.HostConfig.Set("Privileged", value); }
        }

        public bool ReadonlyRootfs
        {
            get { return this.HostConfig.GetAs<bool>("ReadonlyRootfs"); }
            set { this.HostConfig.Set("ReadonlyRootfs", value); }
        }

        public JArray Dns
        {
            get { return this.HostConfig.AssureJArray("Dns"); }
            set { this.HostConfig.Set("Dns", value); }
        }

        public JArray DnsOptions
        {
            get { return this.HostConfig.AssureJArray("DnsOptions"); }
            set { this.HostConfig.Set("DnsOptions", value); }
        }

        public JArray DnsSearch
        {
            get { return this.HostConfig.AssureJArray("DnsSearch"); }
            set { this.HostConfig.Set("DnsSearch", value); }
        }

        public JArray VolumesFrom
        {
            get { return this.HostConfig.AssureJArray("VolumesFrom"); }
            set { this.HostConfig.Set("VolumesFrom", value); }
        }

        public JArray CapAdd
        {
            get { return this.HostConfig.AssureJArray("CapAdd"); }
            set { this.HostConfig.Set("CapAdd", value); }
        }

        public JArray CapDrop
        {
            get { return this.HostConfig.AssureJArray("CapDrop"); }
            set { this.HostConfig.Set("CapDrop", value); }
        }

        public JArray GroupAdd
        {
            get { return this.HostConfig.AssureJArray("GroupAdd"); }
            set { this.HostConfig.Set("GroupAdd", value); }
        }

        public JObject RestartPolicy
        {
            get { return this.HostConfig.AssureJObject("RestartPolicy"); }
            set { this.HostConfig.Set("RestartPolicy", value); }
        }

        public int AutoRemove
        {
            get { return this.HostConfig.GetAs<int>("AutoRemove"); }
            set { this.HostConfig.Set("AutoRemove", value); }
        }

        public string NetworkMode
        {
            get { return this.HostConfig.Get("NetworkMode"); }
            set { this.HostConfig.Set("NetworkMode", value); }
        }

        public JArray Devices
        {
            get { return this.HostConfig.AssureJArray("Devices"); }
            set { this.HostConfig.Set("Devices", value); }
        }

        public JArray Ulimits
        {
            get { return this.HostConfig.AssureJArray("Ulimits"); }
            set { this.HostConfig.Set("Ulimits", value); }
        }

        public JObject LogConfig
        {
            get { return this.HostConfig.AssureJObject("LogConfig"); }
            set { this.HostConfig.Set("LogConfig", value); }
        }

        public JArray SecurityOpt
        {
            get { return this.HostConfig.AssureJArray("SecurityOpt"); }
            set { this.HostConfig.Set("SecurityOpt", value); }
        }

        public JObject StorageOpt
        {
            get { return this.HostConfig.AssureJObject("StorageOpt"); }
            set { this.HostConfig.Set("StorageOpt", value); }
        }

        public string CgroupParent
        {
            get { return this.HostConfig.Get("CgroupParent"); }
            set { this.HostConfig.Set("CgroupParent", value); }
        }

        public string VolumeDriver
        {
            get { return this.HostConfig.Get("VolumeDriver"); }
            set { this.HostConfig.Set("VolumeDriver", value); }
        }

        public int ShmSize
        {
            get { return this.HostConfig.GetAs<int>("ShmSize"); }
            set { this.HostConfig.Set("ShmSize", value); }
        }

        public JObject NetworkingConfig
        {
            get { return this.GetAsJObject("NetworkingConfig"); }
            set { this.Set("NetworkingConfig", value); }
        }

        public JObject EndpointsConfig
        {
            get { return this.NetworkingConfig.AssureJObject("EndpointsConfig"); }
            set { this.NetworkingConfig.Set("EndpointsConfig", value); }
        }
        #endregion

        #region Extras
        /// <summary>
        /// 
        /// The name of the genome that created this
        /// 
        /// </summary>
        public string DNA
        {
            get { return this["@DNA"]; }
            set { this["@DNA"] = value; }
        }

        /// <summary>
        /// 
        /// Name to be used for the container.
        /// Must be ?[a-zA-Z0-9][a-zA-Z0-9_.-]+.
        /// 
        /// </summary>
        public string BeeName
        {
            get { return this["ContainerName"]; }
            set { this["ContainerName"] = value; }
        }

        ///
        /// Start of NX defined fields
        /// 

        /// <summary>
        /// 
        /// Container definition that is the source of
        /// this definition
        /// 
        /// </summary>
        public string From
        {
            get { return this["_From"]; }
            set { this["_From"] = value; }
        }

        /// <summary>
        /// 
        /// A JSON array of source:target volume mappings that are translated to the proper
        /// Volumes/Binds entries.
        /// 
        /// Example:
        /// 
        /// /etc/project/mydata:/etc/data
        /// 
        /// </summary>
        public JArray Map
        {
            get { return this.GetAsJArray("@Map"); }
            set { this.Set("@Map", value); }
        }

        /// <summary>
        /// 
        /// A JSON array of source:target mappings that are copied.
        /// The source is relative to the workign directoty
        /// 
        /// Example:
        /// 
        /// Hive/sources:/etc/data
        /// 
        /// </summary>
        public JArray Copy
        {
            get { return this.GetAsJArray("@Copy"); }
            set { this.Set("@Copy", value); }
        }

        /// <summary>
        /// 
        /// A JSON array of ports that are to be translated to public
        /// 
        /// Example:
        /// 
        /// /etc/project/mydata:/etc/data
        /// 
        /// </summary>
        public JArray Ports
        {
            get { return this.GetAsJArray("@Ports"); }
            set { this.Set("@Ports", value); }
        }

        /// <summary>
        /// 
        /// A JSON array of DNAs that are required to be running
        /// 
        /// Example:
        /// 
        /// [ "processor" ]
        /// 
        /// </summary>
        public JArray Requires
        {
            get { return this.GetAsJArray("@Requires"); }
            set { this.Set("@Requires", value); }
        }

        ///
        /// Bee Keeper fields
		///

        /// <summary>
        /// 
        /// Skip recycle process
        /// 
        /// </summary>
        public bool SkipRecycle
        {
            get { return this["@SkipRecycle"].ToBoolean(); }
            set { this["@SkipRecycle"] = value.ToString(); }
        }

        /// <summary>
        /// 
        /// Is the container unique?
        /// 
        /// y - Unique per hive
        /// * - Unique per system
        /// 
        /// </summary>
        public string Unique
        {
            get { return this["@Unique"]; }
            set { this["@Unique"] = value; }
        }

        ///
        /// 
        /// 

        /// <summary>
        ///  
        /// Converts extra information into working values.
        /// 
        /// The 
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        /// <param name="values">The substitution values</param>
        /// <returns>The ready to use JObject</returns>
        public JObject AsParameters(HiveClass hive, FieldClass field, string task, string nextid, JObject xvalues, StoreClass values = null)
        {
            // Make a copy
            JObject c_Ans = this.SynchObject.Clone();
            // Assure a HostConfig
            JObject c_HostConfig = c_Ans.AssureJObject("HostConfig");

            // Copy the environment
            JObject c_Params = xvalues.Clone();
            // Merge the values
            if (values != null) c_Params.Merge(values.SynchObject);
            // Add DNA's
            this.AddDNA(hive, c_Params, "redis");
            this.AddDNA(hive, c_Params, "nginx");
            // And make a new store
            values = new StoreClass(c_Params);
            //
            values["next_id"] = nextid;

            // Format the image
            c_Ans.Set("Image", values.Format(this.Image));

            // Get the command line
            JArray c_Cmds = this.Cmd;
            // Assure, but should not have to
            c_Cmds = c_Cmds.AssureJArray();

            // Format the command
            c_Cmds = values.FormatJArray(c_Cmds, false, true);
            // Pass them 
            c_Ans.Set("Cmd", c_Cmds);

            // Env
            c_Ans.Set("Env", values.FormatJArray(this.Env, false, true));

            //  Add special labels
            JObject c_Labels = c_Ans.AssureJObject("Labels");

            // This is a way to select containers by site, in case
            // multiple sites share a single physical device
            List<string> c_Hives = new List<string>() { hive.Name };
            // See if there are more hives that share this
            JArray c_Shared = hive.Parent.GetAsJArray(hive.LabelHive + "_" + task);
            // Any?
            if (c_Shared.HasValue())
            {
                c_Hives.AddRange(c_Shared.ToList());
            }
            // Make sure we are on list
            if (!c_Hives.Contains(hive.Name)) c_Hives.Add(hive.Name);
            // Loop thru
            foreach (string sHive in c_Hives)
            {
                // Add
                c_Labels.Set(hive.LabelHive + "_" + sHive, "Y");
            }

            // This is so we can relate the container info to the definition
            c_Labels.Set(hive.LabelDNA,  task.IfEmpty(HiveClass.ProcessorDNAName +".").Replace(".", "_"));

            // And to relate it to the instance
            c_Labels.Set(hive.LabelID, nextid);

            // And the field
            c_Labels.Set(hive.LabelField, field.Name);

            // And the command line, for cloning purposes
            c_Labels.Set(hive.LabelCmd, c_Cmds.ToSimpleString());

            // Handle the volume mapping
            if (this.Map.HasValue())
            {
                // Make holding areas
                JArray c_Binds = c_HostConfig.AssureJArray("Binds");
                JObject c_Volumes = c_Ans.AssureJObject("Volumes");

                // Process it
                for (int iLoop = 0; iLoop < this.Map.Count; iLoop++)
                {
                    // Get the string
                    string sDef = values.Format(this.Map.Get(iLoop));
                    //
                    string sSource = null;
                    string sTarget = null;
                    JObject c_VoulmeInfo = new JObject();

                    // Readonly
                    bool bRO = false;
                    if (sDef.StartsWith("!"))
                    {
                        bRO = true;
                        sDef = sDef.Substring(1);
                    }

                    // Break up
                    int iPos = sDef.IndexOf(":");
                    // Valid?
                    if (iPos == -1)
                    {
                        sSource = sDef;
                        sTarget = sDef;
                    }
                    else
                    {
                        sSource = sDef.Substring(0, iPos);
                        sTarget = sDef.Substring(iPos + 1);
                    }

                    // Adjust
                    if (sSource.StartsWith("//"))
                    {
                        // Nothing to add
                        sSource = sSource.Substring(1);
                        sSource.AssurePath();
                    }
                    else
                    {
                        sSource.AssurePath();
                    }

                    // Finalize
                    string sFinal = values.Format(sSource + ":" + sTarget + (bRO ? ":ro" : ""));

                    // Add to bindings
                    c_Binds.Add(sFinal);
                    // And to volumes
                    c_Volumes.Set(sTarget, c_VoulmeInfo);
                }
            }

            // Handle copying
            if (this.Copy.HasValue())
            {
                // Process it
                for (int iLoop = 0; iLoop < this.Copy.Count; iLoop++)
                {
                    // Get the string
                    string sDef = values.Format(this.Copy.Get(iLoop));
                    
                    // Break up
                    int iPos = sDef.IndexOf(":");
                    // Valid?
                    if (iPos != -1)
                    {
                        string sSource = "".WorkingDirectory().CombinePath( sDef.Substring(0, iPos));
                        string sTarget = sDef.Substring(iPos + 1);

                        // Assure target
                        sTarget.AssurePath();
                        // Copy
                        sSource.CopyDirectoryTree(sTarget);
                    }
                }
            }

                // Any ports
                if (this.Ports.HasValue())
            {
                //
                JObject c_Bindings = c_HostConfig.AssureJObject("PortBindings");
                JObject c_Exposed = c_Ans.AssureJObject("ExposedPorts");
                bool bExposeAll = false;

                // Loop
                for (int iLoop = 0; iLoop < this.Ports.Count; iLoop++)
                {
                    // Get the value
                    string sPort = values.Format(this.Ports.Get(iLoop)).Trim();
                    if (sPort.HasValue())
                    {
                        // Dynamic?
                        bool bStatic = sPort.StartsWith("$");
                        // If so clear flag
                        if (bStatic) sPort = sPort.Substring(1);

                        // Flag?
                        if (sPort.IsSameValue("*"))
                        {
                            //
                            bExposeAll = true;
                        }
                        else
                        {
                            //
                            string sProtocol = "/tcp";
                            string sHostPort = sPort;
                            int iPos = sPort.IndexOf("/");
                            if (iPos != -1)
                            {
                                sProtocol = sPort.Substring(iPos);
                                sPort = sPort.Substring(0, iPos);
                            }
                            iPos = sPort.IndexOf(":");
                            if (iPos != -1)
                            {
                                sHostPort = sPort.Substring(iPos + 1);
                                sPort = sPort.Substring(0, iPos);
                            }

                            // Static?
                            if (bStatic)
                            {
                                // Make definition
                                JObject c_Internal = new JObject();
                                c_Internal.Set("HostPort", sHostPort);

                                JArray c_Def = new JArray();
                                c_Def.Add(c_Internal);

                                // Bind it
                                c_Bindings.Add(sPort + sProtocol, c_Def);
                            }

                            // Expose it
                            c_Exposed.Add(sPort + sProtocol, new JObject());
                        }
                    }
                }

                if (c_Exposed.Count > 0)
                {
                    if (c_Bindings.Count == 0 || bExposeAll)
                    {
                        c_HostConfig["PublishAllPorts"] = "true";
                    }
                }
            }

            return c_Ans;
        }

        /// <summary>
        /// 
        /// Adds the IP of the DNA, if any
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="dna"></param>
        private void AddDNA(HiveClass hive, JObject values, string dna)
        {

            // Get the DNA
            List<string> c_IPs = hive.Roster.GetLocationsForDNA(dna);
            // Any?
            if(c_IPs .Count > 0)
            {
                // Use first
                values.Set(dna, c_IPs.First().URLMake().RemoveProtocol());
            }
        }
        #endregion
    }
}