using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirSoundServer
{
    public class Configuration
    {
        public String IpAddressClient = "";
        public String IPAddressServer = "";
        public int PortClient = 0;
        public int PortServer = 0;
        public String SoundDeviceNameClient = "";
        public String SoundDeviceNameServer = "";
        public int SamplesPerSecondClient = 8000;
        public int BitsPerSampleClient = 16;
        public int ChannelsClient = 1;
        public int SamplesPerSecondServer = 8000;
        public int BitsPerSampleServer = 16;
        public int ChannelsServer = 1;
        public bool IsTimeSyncClient = true;
        public uint JitterBufferCount = 20;

    }
}
