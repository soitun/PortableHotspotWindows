﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortableHotspotWindows
{
    class HotspotClass
    {
        private ProcessStartInfo ps = null;
        private dynamic netSharingManager = null;
        private dynamic everyConnections = null;
        private bool hasNetSharingManager = false;
        public string Message { get; set; } = "";

        public HotspotClass()
        {
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            ps = new ProcessStartInfo("cmd.exe");
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;
            ps.CreateNoWindow = true;
            ps.FileName = "netsh";

            netSharingManager = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.HNetShare.1"));

            if (netSharingManager == null)
            {
                this.Message = "HNetCfg.HNetShare.1 was not found! \n";
                hasNetSharingManager = true;
            }
            else
            {
                hasNetSharingManager = false;
            }

            if (netSharingManager.SharingInstalled == false)
            {
                this.Message = "Sharing on this platform is not available \n";
                hasNetSharingManager = false;
            }
            else
            {
                hasNetSharingManager = true;
            }

            if (hasNetSharingManager)
            {
                everyConnections = netSharingManager.EnumEveryConnection;

            }

        }

        private bool Execute(ProcessStartInfo ps)
        {
            bool isExecuted = false;
            try
            {
                using (Process p = Process.Start(ps))
                {
                    Message += p.StandardOutput.ReadToEnd() + "\n";
                    p.WaitForExit();
                    isExecuted = true;
                }
            }
            catch (Exception e)
            {
                Message = "";
                Message += e.Message;
                isExecuted = false;
            }
            return isExecuted;
        }

        public void Start()
        {
            ps.Arguments = "wlan start hosted network";
            Execute(ps);
        }
        public void Create(string ssid, string key)
        {
            ps.Arguments = String.Format("wlan set hostednetwork mode=allow ssid={0} key={1}", ssid, key);
            Execute(ps);
        }
        public void Stop()
        {
            ps.Arguments = "wlan stop hosted network";
            Execute(ps);
        }

        public NetworkInfoClass CheckNetwork()
        {
            NetworkInfoClass Info = new NetworkInfoClass();
            ps.Arguments = "wlan show hostednetwork";
            Execute(ps);
            try
            {
                Info.NetworkStatus = (Regex.Match(Message, @"[\n\r].*Status                 : \s*([^\n\r]*)")).Groups[1].Value;
            }
            catch(Exception ex)
            {
                Info.NetworkStatus = null;
            }
            try
            {
                Info.SSID = (Regex.Match(Message, @"[\n\r].*SSID name              : \s*([^\n\r]*)")).Groups[1].Value.Replace("\"", "");
            }
            catch(Exception ex)
            {
                Info.SSID = null;
            }
            try
            {
                Info.NumOfClients = (Regex.Match(Message, @"[\n\r].*Number of clients      : \s*([^\n\r]*)")).Groups[1].Value;
            }
            catch(Exception ex)
            {
                Info.NumOfClients = null;
            }
            try
            {
                Match Clients = Regex.Match(Message, @"([0-9a-fA-F]:?){12}");

                for(int i=1;i<;i++)
                {
                    Info.ConnectedClients.Add(Clients.Captures[i].Value);
                }                
            }
            catch(Exception ex)
            {
                Info.ConnectedClients = null;
            }

            ps.Arguments = "wlan show  hostednetwork setting=security";
            Execute(ps);
            try
            {
                Info.Key = (Regex.Match(Message, @"[\n\r].*User security key      : \s*([^\n\r]*)")).Groups[1].Value;
            }
            catch(Exception ex)
            {
                Info.Key = null;
            }
            return Info;
        }

        public List<String> GetConnections()
        {
            dynamic everyConnection = null;
            dynamic connectionProp = null;
            everyConnections = netSharingManager.EnumEveryConnection;
            List<String> connections = new List<string>();
            foreach (dynamic connection in everyConnections)
            {
                everyConnection = netSharingManager.INetSharingConfigurationForINetConnection(connection);
                connectionProp = netSharingManager.NetConnectionProps(connection);
                connections.Add(connectionProp.Name);
            }

            return connections;
        }

        public void ShareInternet(String pubConnectionName, String priConnectionName, bool isEnabled)
        {
            bool hasCon = false;
            dynamic everyConnection = null;
            dynamic connectionProp = null;
            everyConnections = netSharingManager.EnumEveryConnection;
            foreach (dynamic connection in everyConnections)
            {
                everyConnection = netSharingManager.INetSharingConfigurationForINetConnection(connection);
                connectionProp = netSharingManager.NetConnectionProps(connection);

                if (connectionProp.Name == pubConnectionName)
                {
                    hasCon = true;
                    this.Message += String.Format("Setting ICS Public {0} on connection: {1} \n", isEnabled, pubConnectionName);
                    if (isEnabled)
                    {
                        everyConnection.EnableSharing(0);
                    }
                    else
                    {
                        everyConnection.DisableSharing();
                    }
                }

                if (connectionProp.Name == priConnectionName)
                {
                    hasCon = true;
                    this.Message += String.Format("Setting ICS Private {0} on connection: {1} \n", isEnabled, priConnectionName);
                    if (isEnabled)
                    {
                        everyConnection.EnableSharing(1);
                    }
                    else
                    {
                        everyConnection.DisableSharing();
                    }
                }
            }

            if (!hasCon)
            {
                this.Message += "No connection found!";
            }
        }
    }
}