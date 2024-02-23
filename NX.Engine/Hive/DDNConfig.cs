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
/// Install-Package Docker.DotNet -Version 3.125.2
/// 

using System;
using System.Collections.Generic;

using Docker.DotNet.Models;
using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine.Hive
{
    /// <summary>
    /// 
    /// Make a Docker.DotNet CreateContainerParameters
    /// object from a Docker API CreateContainer JSON
    /// object
    /// 
    /// </summary>
    public class DDNConfigClass : IDisposable
    {
        #region Constructor
        public DDNConfigClass(JObject source, bool adjcmds)
        {
            // Convert
            this.DoConvert(source, adjcmds);
        }
        #endregion

        #region Properties
         /// <summary>
        /// 
        /// The Docker.DotNet CreateContainerParameters
        /// 
        /// </summary>
        public CreateContainerParameters Target { get; private set; }
        #endregion

        #region IDisposable
        public void Dispose()
        { }
        #endregion

        #region Methods
        private void DoConvert(JObject source, bool adjcmds)
        {
            // Setup
            this.Target = new CreateContainerParameters();

            // Any?
            if (source.HasValue())
            {
                // Loop thru
                foreach (string sKey in source.Keys())
                {
                    switch (sKey)
                    {
                        case "Shell":
                            this.Target.Shell = source.GetJArray("Shell").ToList();
                            break;

                        case "StopTimeout":
                            this.Target.StopTimeout = source.Get("StopTimeout").ToInteger().MillisecondsAsTimeSpan();
                            break;

                        case "StopSignal":
                            this.Target.StopSignal = source.Get("StopSignal");
                            break;

                        case "Labels":
                            this.DoLabels(source.GetJObject("Labels"));
                            break;

                        case "OnBuild":
                            this.Target.OnBuild = source.GetJArray("OnBuild").ToList();
                            break;

                        case "MacAddress":
                            this.Target.MacAddress = source.Get("MacAddress");
                            break;

                        case "NetworkDisabled":
                            this.Target.NetworkDisabled = source.Get("NetworkDisabled").ToBoolean();
                            break;

                        case "Entrypoint":
                            this.Target.Entrypoint = source.GetJArray("Entrypoint").ToList();
                            break;

                        case "WorkingDir":
                            this.Target.WorkingDir = source.Get("WorkingDir");
                            break;

                        case "Volumes":
                            this.DoVolumes(source.GetJObject("Volumes"));
                            break;

                        case "Image":
                            this.Target.Image = source.Get("Image");
                            break;

                        case "ArgsEscaped":
                            this.Target.ArgsEscaped = source.Get("ArgsEscaped").ToBoolean();
                            break;

                        case "Healthcheck":
                            this.DoHealthcheck(source.GetJObject("Healthcheck"));
                            break;

                        case "Cmd":
                            this.Target.Cmd = source.GetJArray("Cmd").ToList();
                            break;

                        case "Env":
                            this.Target.Env = source.GetJArray("Env").ToList();
                            break;

                        case "StdinOnce":
                            this.Target.StdinOnce = source.Get("StdinOnce").ToBoolean();
                            break;

                        case "OpenStdin":
                            this.Target.OpenStdin = source.Get("OpenStdin").ToBoolean();
                            break;

                        case "Tty":
                            this.Target.Tty = source.Get("Tty").ToBoolean();
                            break;

                        case "ExposedPorts":
                            this.DoExposedPorts(source.GetJObject("ExposedPorts"));
                            break;

                        case "AttachStderr":
                            this.Target.AttachStderr = source.Get("AttachStderr").ToBoolean();
                            break;

                        case "AttachStdin":
                            this.Target.AttachStdin = source.Get("AttachStdin").ToBoolean();
                            break;

                        case "AttachStdout":
                            this.Target.AttachStdout = source.Get("AttachStdout").ToBoolean();
                            break;

                        case "User":
                            this.Target.User = source.Get("User");
                            break;

                        case "Domainname":
                            this.Target.Domainname = source.Get("Domainname");
                            break;

                        case "Hostname":
                            this.Target.Hostname = source.Get("Hostname");
                            break;

                        case "Name":
                            this.Target.Name = source.Get("Name");
                            break;

                        case "HostConfig":
                            this.DoHostConfig(source.GetJObject("HostConfig"));
                            break;

                        case "NetworkingConfig":
                            this.DoNetworkingConfig(source.GetJObject("NetworkingConfig"));
                            break;
                    }
                }
            }

            // Adjusted?
            if (adjcmds)
            {
                // Get the commands
                List<string> c_Cmds = new List<string>(this.Target.Cmd);
                // Loop thru
                for(int i = c_Cmds.Count - 1;i>0;i-=2)
                {
                    // Is the value empty?
                    if(!c_Cmds[i].HasValue())
                    {
                        // Remove pairs
                        c_Cmds.RemoveAt(i);
                        c_Cmds.RemoveAt(i - 1);
                    }
                }
                // And back
                this.Target.Cmd = c_Cmds;
            }

            // Always have a HostConfig
            if (this.Target.HostConfig == null)
            {
                // Make
                this.Target.HostConfig = new HostConfig();
            }

            // Set restart policy
            this.Target.HostConfig.RestartPolicy = new RestartPolicy()
            {
                Name = RestartPolicyKind.UnlessStopped
            };
        }

        private void DoLabels(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                // Assure
                if (this.Target.Labels == null) this.Target.Labels = new Dictionary<string, string>();

                // Setup
                Dictionary<string, string> c_Target = this.Target.Labels as Dictionary<string, string>;

                foreach (string sKey in source.Keys())
                {
                    c_Target[sKey] = source.Get(sKey);
                }
            }
        }

        private void DoVolumes(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                // Assure
                if (this.Target.Volumes == null) this.Target.Volumes = new Dictionary<string, EmptyStruct >();

                // Setup
                Dictionary<string, EmptyStruct> c_Target = this.Target.Volumes as Dictionary<string, EmptyStruct>;

                foreach (string sKey in source.Keys())
                {
                    if (!c_Target.ContainsKey(sKey))
                    {
                        c_Target[sKey] = new EmptyStruct();
                    }
                }
            }
        }

        private void DoHealthcheck(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                // Assure
                if (this.Target.Healthcheck == null) this.Target.Healthcheck = new HealthConfig();

                // Setup
                HealthConfig c_Target = this.Target.Healthcheck;

                foreach (string sKey in source.Keys())
                {
                    switch (sKey)
                    {
                        case "Interval":
                            c_Target.Interval = source.Get("Interval").ToDouble(0).Max(1000000).MillisecondsAsTimeSpan();
                            break;

                        case "Timeout":
                            c_Target.Timeout = source.Get("Timeout").ToDouble(0).Max(1000000).MillisecondsAsTimeSpan();
                            break;

                        case "Retries":
                            c_Target.Retries = source.Get("Retries").ToInteger(0);
                            break;
                    }
                }
            }
        }

        private void DoExposedPorts(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                // Assure
                if (this.Target.ExposedPorts == null) this.Target.ExposedPorts = new Dictionary<string, EmptyStruct>();

                // Setup
                Dictionary<string, EmptyStruct> c_Target = this.Target.ExposedPorts as Dictionary<string, EmptyStruct>;

                foreach (string sKey in source.Keys())
                {
                    if (!c_Target.ContainsKey(sKey))
                    {
                        c_Target[sKey] = new EmptyStruct();
                    }
                }
            }
        }

        private void DoHostConfig(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                //
                if (this.Target.HostConfig == null) this.Target.HostConfig = new HostConfig();
                if (this.Target.ExposedPorts == null) this.Target.ExposedPorts = new Dictionary<string, EmptyStruct>();

                // Setup
                HostConfig c_Target = this.Target.HostConfig;

                foreach (string sKey in source.Keys())
                {
                    switch (sKey)
                    {
                        case "CPURealtimePeriod":
                            c_Target.CPURealtimePeriod = source.Get("CPURealtimePeriod").ToInteger(0);
                            break;

                        case "CpuQuota":
                            c_Target.CPUQuota = source.Get("CpuQuota").ToInteger(0);
                            break;

                        case "CpuPeriod":
                            c_Target.CPUPeriod = source.Get("CpuPeriod").ToInteger(0);
                            break;

                        case "BlkioDeviceWriteIOps":
                            c_Target.BlkioDeviceWriteIOps = this.DoThrotteDevices(source.GetJArray("BlkioDeviceWriteIOps"));
                            break;

                        case "BlkioDeviceReadIOps":
                            c_Target.BlkioDeviceReadIOps = this.DoThrotteDevices(source.GetJArray("BlkioDeviceReadIOps"));
                            break;

                        case "BlkioDeviceWriteBps":
                            c_Target.BlkioDeviceWriteBps = this.DoThrotteDevices(source.GetJArray("BlkioDeviceWriteBps"));
                            break;

                        case "BlkioDeviceReadBps":
                            c_Target.BlkioDeviceReadBps = this.DoThrotteDevices(source.GetJArray("BlkioDeviceReadBps"));
                            break;

                        case "BlkioWeightDevice":
                            c_Target.BlkioWeightDevice = this.DoWeightDevices(source.GetJArray("BlkioWeightDevice"));
                            break;

                        case "BlkioWeight":
                            c_Target.BlkioWeight = (ushort)source.Get("BlkioWeight").ToInteger(0);
                            break;

                        case "CgroupParent":
                            c_Target.CgroupParent = source.Get("CgroupParent");
                            break;

                        case "NanoCPUs":
                            c_Target.NanoCPUs = source.Get("NanoCPUs").ToInteger(0);
                            break;

                        case "Memory":
                            c_Target.Memory = source.Get("Memory").ToInteger(0);
                            break;

                        case "CpuShares":
                            c_Target.CPUShares = source.Get("CpuShares").ToInteger(0);
                            break;

                        case "CPURealtimeRuntime":
                            c_Target.CPURealtimeRuntime = source.Get("CpuRealtimeRuntime").ToInteger(0);
                            break;

                        case "CpusetCpus":
                            c_Target.CpusetCpus = source.Get("CpusetCpus");
                            break;

                        case "CpusetMems":
                            c_Target.CpusetMems = source.Get("CpusetMems");
                            break;

                        case "Devices":
                            c_Target.Devices = this.DoDeviceMappings(source.GetJArray("Devices"));
                            break;

                        case "KernelMemory":
                            c_Target.KernelMemory = source.Get("KernelMemory").ToInteger(0);
                            break;

                        case "MemoryReservation":
                            c_Target.MemoryReservation = source.Get("MemoryReservation").ToInteger(0);
                            break;

                        case "MemorySwap":
                            c_Target.MemorySwap = source.Get("MemorySwap").ToInteger(0);
                            break;

                        case "MemorySwappiness":
                            c_Target.MemorySwappiness = source.Get("MemorySwappiness").ToInteger(0);
                            break;

                        case "OomKillDisable":
                            c_Target.OomKillDisable = source.Get("OomKillDisable").ToBoolean();
                            break;

                        case "Ulimits":
                            c_Target.Ulimits = this.DoUlimits(source.GetJArray("Ulimits"));
                            break;

                        case "CpuCount":
                            c_Target.CPUCount = source.Get("CpuCount").ToInteger(0);
                            break;

                        case "CpuPercent":
                            c_Target.CPUPercent = source.Get("CpuPercent").ToInteger(0);
                            break;

                        case "IOMaximumIOps":
                            c_Target.IOMaximumIOps = (ulong)source.Get("IOMaximumIOps").ToInteger(0);
                            break;

                        case "IOMaximumBandwidth":
                            c_Target.IOMaximumBandwidth = (ulong)source.Get("IOMaximumBandwidth").ToInteger(0);
                            break;

                        case "Mounts":
                            c_Target.Mounts = this.DoMounts(source.GetJArray("Mounts"));
                            break;

                        case "Isolation":
                            c_Target.Isolation = source.Get("Isolation");
                            break;

                        //public ulong[] ConsoleSize { get; set; }                                

                        case "Runtime":
                            c_Target.Runtime = source.Get("Runtime");
                            break;

                        case "Sysctls":
                            c_Target.Sysctls = source.GetJObject("Sysctls").ToDictionary();
                            break;

                        case "Binds":
                            c_Target.Binds = source.GetJArray("Binds").ToList();
                            break;

                        case "ContainerIDFile":
                            c_Target.ContainerIDFile = source.Get("ContainerIDFile");
                            break;

                        case "LogConfig":
                            c_Target.LogConfig = this.DoLogConfig(source.GetJObject("LogConfig"));
                            break;

                        case "NetworkMode":
                            c_Target.NetworkMode = source.Get("NetworkMode");
                            break;

                        case "PortBindings":
                            c_Target.PortBindings = this.DoPortBindings(source.GetJObject("PortBindings"));
                            break;

                        case "RestartPolicy":
                            c_Target.RestartPolicy = this.DoRestartPolicy(source.GetJObject("RestartPolicy"));
                            break;

                        case "AutoRemove":
                            c_Target.AutoRemove = source.Get("AutoRemove").ToBoolean();
                            break;

                        case "VolumeDriver":
                            c_Target.VolumeDriver = source.Get("VolumeDriver");
                            break;

                        case "VolumesFrom":
                            c_Target.VolumesFrom = source.GetJArray("VolumesFrom").ToList();
                            break;

                        case "CapAdd":
                            c_Target.CapAdd = source.GetJArray("CapAdd").ToList();
                            break;

                        case "CapDrop":
                            c_Target.CapDrop = source.GetJArray("CapDrop").ToList();
                            break;

                        case "DNS":
                            c_Target.DNS = source.GetJArray("DNS").ToList();
                            break;

                        case "DNSOptions":
                            c_Target.DNSOptions = source.GetJArray("DNSOptions").ToList();
                            break;

                        case "DNSSearch":
                            c_Target.DNSSearch = source.GetJArray("DNSSearch").ToList();
                            break;

                        case "Init":
                            c_Target.Init = source.Get("Init").ToBoolean();
                            break;

                        case "ExtraHosts":
                            c_Target.ExtraHosts = source.GetJArray("ExtraHosts").ToList();
                            break;

                        case "IpcMode":
                            c_Target.IpcMode = source.Get("IpcMode");
                            break;

                        case "Cgroup":
                            c_Target.Cgroup = source.Get("Cgroup");
                            break;

                        case "Links":
                            c_Target.Links = source.GetJArray("Links").ToList();
                            break;

                        case "OomScoreAdj":
                            c_Target.OomScoreAdj = source.Get("OomScoreAdj").ToInteger(0);
                            break;

                        case "PidMode":
                            c_Target.PidMode = source.Get("PidMode");
                            break;

                        case "Privileged":
                            c_Target.Privileged = source.Get("Privileged").ToBoolean();
                            break;

                        case "PublishAllPorts":
                            c_Target.PublishAllPorts = source.Get("PublishAllPorts").ToBoolean();
                            break;

                        case "ReadonlyRootfs":
                            c_Target.ReadonlyRootfs = source.Get("ReadonlyRootfs").ToBoolean();
                            break;

                        case "SecurityOpt":
                            c_Target.SecurityOpt = source.GetJArray("SecurityOpt").ToList();
                            break;

                        case "StorageOpt":
                            c_Target.StorageOpt = source.GetJObject("StorageOpt").ToDictionary();
                            break;

                        case "Tmpfs":
                            c_Target.Tmpfs = source.GetJObject("Tmpfs").ToDictionary();
                            break;

                        case "UTSMode":
                            c_Target.UTSMode = source.Get("UTSMode");
                            break;

                        case "UsernsMode":
                            c_Target.UsernsMode = source.Get("UsernsMode");
                            break;

                        case "ShmSize":
                            c_Target.ShmSize = source.Get("ShmSize").ToInteger(0);
                            break;

                        case "GroupAdd":
                            c_Target.GroupAdd = source.GetJArray("GroupAdd").ToList();
                            break;
                    }
                }
            }
        }

        private List<ThrottleDevice> DoThrotteDevices(JArray values)
        {
            // Assume none
            List<ThrottleDevice> c_Ans = null;

            // Any?
            if (values.HasValue())
            {
                // Make
                c_Ans = new List<ThrottleDevice>();

                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Do
                    ThrottleDevice c_Dev = this.DoThrottleDevice(values.GetJObject(i));
                    // Any? Add
                    if (c_Dev != null) c_Ans.Add(c_Dev);
                }
            }

            return c_Ans;
        }

        private ThrottleDevice DoThrottleDevice(JObject value)
        {
            // Assume none
            ThrottleDevice c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new ThrottleDevice();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Path":
                            c_Ans.Path = value.Get("Path");
                            break;

                        case "Rate":
                            c_Ans.Rate = (ulong)value.Get("Rate").ToInteger(0);
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private List<WeightDevice> DoWeightDevices(JArray values)
        {
            // Assume none
            List<WeightDevice> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new List<WeightDevice>();

                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Do
                    WeightDevice c_Dev = this.DoWeightDevice(values.GetJObject(i));
                    // Any? Add
                    if (c_Dev != null) c_Ans.Add(c_Dev);
                }
            }

            return c_Ans;
        }

        private WeightDevice DoWeightDevice(JObject value)
        {
            // Assume none
            WeightDevice c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new WeightDevice();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Path":
                            c_Ans.Path = value.Get("Path");
                            break;

                        case "Weight":
                            c_Ans.Weight = (ushort)value.Get("Weight").ToInteger(0);
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private List<DeviceMapping> DoDeviceMappings(JArray values)
        {
            // Assume none
            List<DeviceMapping> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new List<DeviceMapping>();

                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Do
                    DeviceMapping c_Dev = this.DoDeviceMapping(values.GetJObject(i));
                    // Any? Add
                    if (c_Dev != null) c_Ans.Add(c_Dev);
                }
            }

            return c_Ans;
        }

        private DeviceMapping DoDeviceMapping(JObject value)
        {
            // Assume none
            DeviceMapping c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new DeviceMapping();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "PathOnHost":
                            c_Ans.PathOnHost = value.Get("PathOnHost");
                            break;

                        case "PathInContainer":
                            c_Ans.PathInContainer = value.Get("PathInContainer");
                            break;

                        case "CgroupPermissions":
                            c_Ans.CgroupPermissions = value.Get("CgroupPermissions");
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private List<Mount> DoMounts(JArray values)
        {
            // Assume none
            List<Mount> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new List<Mount>();

                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Do
                    Mount c_Dev = this.DoMount(values.GetJObject(i));
                    // Any? Add
                    if (c_Dev != null) c_Ans.Add(c_Dev);
                }
            }

            return c_Ans;
        }

        private Mount DoMount(JObject value)
        {
            // Assume none
            Mount c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new Mount();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Type":
                            c_Ans.Type = value.Get("Type");
                            break;

                        case "Source":
                            c_Ans.Source = value.Get("Source");
                            break;

                        case "Target":
                            c_Ans.Target = value.Get("Target");
                            break;

                        case "ReadOnly":
                            c_Ans.ReadOnly = value.Get("ReadOnly").ToBoolean();
                            break;

                        case "BindOptions":
                            c_Ans.BindOptions = this.DoBindOptions(value.GetJObject("BindOptions"));
                            break;

                        case "VolumeOptions":
                            c_Ans.VolumeOptions = this.DoVolumeOptions(value.GetJObject("VolumeOptions"));
                            break;

                        case "TmpfsOptions":
                            c_Ans.TmpfsOptions = this.DoTmpfsOptions(value.GetJObject("TmpfsOptions"));
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private List<Ulimit> DoUlimits(JArray values)
        {
            // Assume none
            List<Ulimit> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new List<Ulimit>();

                // Loop thru
                for (int i = 0; i < values.Count; i++)
                {
                    // Do
                    Ulimit c_Dev = this.DoUlimit(values.GetJObject(i));
                    // Any? Add
                    if (c_Dev != null) c_Ans.Add(c_Dev);
                }
            }

            return c_Ans;
        }

        private Ulimit DoUlimit(JObject value)
        {
            // Assume none
            Ulimit c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new Ulimit();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Name":
                            c_Ans.Name = value.Get("Name");
                            break;

                        case "Hard":
                            c_Ans.Hard = value.Get("Hard").ToInteger(0);
                            break;

                        case "Soft":
                            c_Ans.Soft = value.Get("Soft").ToInteger(0);
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private LogConfig DoLogConfig(JObject value)
        {
            // Assume none
            LogConfig c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new LogConfig();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Type":
                            c_Ans.Type = value.Get("Type");
                            break;

                        case "Config":
                            c_Ans.Config = value.GetJObject("Config").ToDictionary();
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private IDictionary<string, IList<PortBinding>> DoPortBindings(JObject values)
        {
            // Assume none
            Dictionary<string, IList<PortBinding>> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new Dictionary<string, IList<PortBinding>>();

                // Loop thru
                foreach(string sKey in values.Keys())
                {
                    // Do
                    List<PortBinding> c_PB = this.DoPortBindingsList(values.GetJArray(sKey));
                    // Any? Add
                    if (c_PB != null) c_Ans.Add(sKey, c_PB);
                }
            }

            return c_Ans;
        }

        private List<PortBinding> DoPortBindingsList(JArray values)
        {
            // Assume none
            List<PortBinding> c_Ans = null;

            // Any?
            if (values != null)
            {
                // Make
                c_Ans = new List<PortBinding>();

                // Loop thru
                for(int i=0;i< values.Count;i++)
                {
                    // Do
                    PortBinding c_PB = this.DoPortBinding(values.GetJObject(i));
                    // Any? Add
                    if (c_PB != null) c_Ans.Add(c_PB);
                }
            }

            return c_Ans;
        }

        private PortBinding DoPortBinding(JObject value)
        {
            // Assume none
            PortBinding c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new PortBinding();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "HostIP":
                            c_Ans.HostIP = value.Get("HostIP");
                            break;

                        case "HostPort":
                            c_Ans.HostPort = value.Get("HostPort");
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private RestartPolicy DoRestartPolicy(JObject value)
        {
            // Assume none
            RestartPolicy c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new RestartPolicy();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Name":
                            try
                            {
                                c_Ans.Name =(RestartPolicyKind)Enum.Parse(typeof(RestartPolicyKind), value.Get("Type"), true);
                            }
                            catch { }
                            break;

                        case "MaximumRetryCount":
                            c_Ans.MaximumRetryCount = value.Get("MaximumRetryCount").ToInteger(0);
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private BindOptions DoBindOptions(JObject value)
        {
            // Assume none
            BindOptions c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new BindOptions();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Propagation":
                            c_Ans.Propagation = value.Get("Propagation");
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private VolumeOptions DoVolumeOptions(JObject value)
        {
            // Assume none
            VolumeOptions c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new VolumeOptions();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "NoCopy":
                            c_Ans.NoCopy = value.Get("NoCopy").ToBoolean();
                            break;

                        case "Labels":
                            c_Ans.Labels = value.GetJObject("Labels").ToDictionary();
                            break;

                        case "DriverConfig":
                            c_Ans.DriverConfig = this.DoDriverConfig(value.GetJObject("DriverConfig"));
                            break;


                    }
                }
            }

            return c_Ans;
        }

        private Driver DoDriverConfig(JObject value)
        {
            // Assume none
            Driver c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new Driver();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "Name":
                            c_Ans.Name = value.Get("Name");
                            break;

                        case "Options":
                            c_Ans.Options = value.GetJObject("Options").ToDictionary();
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private TmpfsOptions DoTmpfsOptions(JObject value)
        {
            // Assume none
            TmpfsOptions c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new TmpfsOptions();

                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "SizeBytes":
                            c_Ans.SizeBytes = value.Get("SizeBytes").ToInteger();
                            break;

                        case "Mode":
                            c_Ans.Mode = (uint)value.Get("Mode").ToInteger(0);
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private void DoNetworkingConfig(JObject source)
        {
            // Any?
            if (source.HasValue())
            {
                // Assure
                if (this.Target.NetworkingConfig == null) this.Target.NetworkingConfig = new NetworkingConfig();
                if (this.Target.NetworkingConfig.EndpointsConfig == null) this.Target.NetworkingConfig.EndpointsConfig = new Dictionary<string, EndpointSettings>();

                // Setup
                NamedListClass<EndpointSettings> c_Target = this.Target.NetworkingConfig.EndpointsConfig as NamedListClass<EndpointSettings>;

                foreach (string sKey in source.Keys())
                {
                    // Make
                    EndpointSettings c_EP = this.DoEndpointSettings(source.GetJObject(sKey));
                    // Any?
                    if(c_EP != null)
                    {
                        c_Target[sKey] = c_EP;
                    }
                }
            }
        }

        private EndpointSettings DoEndpointSettings(JObject value)
        {
            // Assume none
            EndpointSettings c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new EndpointSettings();
                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "IPAMConfig":
                            c_Ans.IPAMConfig = this.DoEndpointIPAMConfig(value.GetJObject("IPAMConfig"));
                            break;

                        case "Links":
                            c_Ans.Links = value.GetJArray("Links").ToList();
                            break;

                        case "Aliases":
                            c_Ans.Aliases = value.GetJArray("Aliases").ToList();
                            break;

                        case "NetworkID":
                            c_Ans.NetworkID = value.Get("NetworkID");
                            break;

                        case "EndpointID":
                            c_Ans.EndpointID = value.Get("EndpointID");
                            break;

                        case "Gateway":
                            c_Ans.Gateway = value.Get("Gateway");
                            break;

                        case "IPAddress":
                            c_Ans.IPAddress = value.Get("IPAddress");
                            break;

                        case "IPPrefixLen":
                            c_Ans.IPPrefixLen = value.Get("IPPrefixLen").ToInteger(0);
                            break;

                        case "IPv6Gateway":
                            c_Ans.IPv6Gateway = value.Get("IPv6Gateway");
                            break;

                        case "GlobalIPv6Address":
                            c_Ans.GlobalIPv6Address = value.Get("GlobalIPv6Address");
                            break;

                        case "GlobalIPv6PrefixLen":
                            c_Ans.GlobalIPv6PrefixLen = value.Get("GlobalIPv6PrefixLen").ToInteger(0);
                            break;

                        case "MacAddress":
                            c_Ans.MacAddress = value.Get("MacAddress");
                            break;
                    }
                }
            }

            return c_Ans;
        }

        private EndpointIPAMConfig DoEndpointIPAMConfig(JObject value)
        {
            // Assume none
            EndpointIPAMConfig c_Ans = null;

            // Any?
            if (value.HasValue())
            {
                // Make
                c_Ans = new EndpointIPAMConfig();
                // Loop thru
                foreach (string sKey in value.Keys())
                {
                    switch (sKey)
                    {
                        case "IPv4Address":
                            c_Ans.IPv4Address = value.Get("IPv4Address");
                            break;

                        case "IPv6Address":
                            c_Ans.IPv6Address = value.Get("IPv6Address");
                            break;

                        case "LinkLocalIPs":
                            c_Ans.LinkLocalIPs = value.GetJArray("LinkLocalIPs").ToList();
                            break;
                    }
                }
            }

            return c_Ans;
        }
        #endregion
    }
}