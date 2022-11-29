using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;
using Open.Nat;
using PluginAPI;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace NWAPI_UPnP
{
    public class Plugin
    {
        [PluginConfig] public Config Config;

        [PluginEntryPoint("NWAPI UPnP Tool", "1.0.0", "Allows automatic port forwarding using UPnP.", "warden161")]
        public void Load()
        {
            NatDiscoverer.TraceSource.Switch.Level = Config.DebugLevel;
            NatDiscoverer.TraceSource.Listeners.Add(new Logger());
            Task.Run(Forward);
        }

        [PluginUnload]
        public void Unload()
            => NatDiscoverer.ReleaseAll();

        public async Task Forward()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cancelation = new CancellationTokenSource(Config.Timeout);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cancelation);

                var mapping = new Mapping(Protocol.Udp, Server.Port, Server.Port, int.MaxValue, Config.MappingName.Replace("%port%", Server.Port.ToString()));
                if (mapping == null)
                {
                    Log.Error("Couldn't port forward: Mapping is null.");
                    return;
                }

                await device.CreatePortMapAsync(mapping);
            } catch (NatDeviceNotFoundException)
            {
                Log.Error("Couldn't port forward: No UPnP-supported devices were found.");
                return;
            } catch (MappingException e)
            {
                switch (e.ErrorCode)
                {
                    case 718:
                        Log.Error("Couldn't port forward: The port is already being forwarded.");
                        return;
                    case 728:
                        Log.Error("Couldn't port forward: The mapping table for your router is already full.");
                        return;
                }
            }
        }

        class Logger : TraceListener
        {
            private void MessageHandler(string message)
            {
                if (message == null)
                    return;

                Log.Info(message);
            }

            public override void Write(string message)
                => MessageHandler(message);
            public override void WriteLine(string message)
                => MessageHandler(message);
        }
    }
}