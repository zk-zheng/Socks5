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
        Console.Write("Total Clients: \t{0}\n" +
                      "Total Recvd: \t{1:0.00##}MB\n" +
                      "Total Sent: \t{2:0.00##}MB\n",
            socks5Server.Stats.TotalClients, ((socks5Server.Stats.NetworkReceived / 1024f) / 1024f), ((socks5Server.Stats.NetworkSent / 1024f) / 1024f));
        Console.Write("Receiving/sec: \t{0}\n" +
                      "Sending/sec: \t{1}\n",
            socks5Server.Stats.ReceivedBytesPerSecond(), socks5Server.Stats.SentBytesPerSecond());
        Console.WriteLine("");
    }

    Thread.Sleep(1000);
    // Console.Clear();
}
