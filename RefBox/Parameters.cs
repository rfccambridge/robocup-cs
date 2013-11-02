using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RefBox
{
    internal static class Parameters
    {
        public static IPAddress routerIP = IPAddress.Loopback;
        public static List<int> routerPorts;

        static Parameters()
        {
            routerPorts = new List<int>();
            routerPorts.Add(5000);
            routerPorts.Add(5001);
        }
    }
}
