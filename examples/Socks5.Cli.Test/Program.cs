using System.Net;
using Socks5.Core.Plugin;
using Socks5.Core.Socks;
using Socks5.Core.SocksServer;

var socks5Server = new Socks5Server(IPAddress.Any, 4444);
socks5Server.Start();

Console.WriteLine("Server Started!");

int totalClients = -1;
ulong networkReceived = 0;
ulong networkSent = 0;

while (true)
{
    bool changed = false;
    if (socks5Server.Stats.TotalClients != totalClients)
    {
        totalClients = socks5Server.Stats.TotalClients;
        changed = true;
    }
    if (socks5Server.Stats.NetworkReceived != networkReceived)
    {
        networkReceived = socks5Server.Stats.NetworkReceived;
        changed = true;
    }
    if (socks5Server.Stats.NetworkSent != networkSent)
    {
        networkSent = socks5Server.Stats.NetworkSent;
        changed = true;
    }

    if (changed)
    {
        Console.WriteLine(
            "[Total Clients: {0,2}] [Total Recv/Sent: {1,12:N3}{2,12:N3} (MB)] [Speed R|S: {3,12:N3}{4,12:N3} (kB/s)]",
            socks5Server.Stats.TotalClients, 
            socks5Server.Stats.NetworkReceived / 1024f / 1024f, 
            socks5Server.Stats.NetworkSent / 1024f / 1024f,
            socks5Server.Stats.ReceivedBytesPerSecond(), 
            socks5Server.Stats.SentBytesPerSecond()
            );
    }

    Thread.Sleep(1000);
    // Console.Clear();
}
