/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Net;
using Socks5.Core.Socks;
using Socks5.Core.TCP;

namespace Socks5.Core.SocksServer;

public class Socks5Server
{
    private readonly TcpServer _server;

    public List<ClientEnd> ClientEnds = new();
    private Thread? _networkStats;

    private bool _started;
    public Stats Stats;

    public Socks5Server(IPAddress ip, int port)
    {
        Timeout = 5000;
        PacketSize = 4096;
        LoadPluginsFromDisk = false;
        Stats = new Stats();
        OutboundIpAddress = IPAddress.Any;
        _server = new TcpServer(ip, port);
        _server.OnClientConnected += Server_onClientConnected;
    }

    public int Timeout { get; set; }
    public int PacketSize { get; set; }
    public bool LoadPluginsFromDisk { get; set; }
    public IPAddress OutboundIpAddress { get; set; }

    public void Start()
    {
        if (_started)
            return;
        
        _server.PacketSize = PacketSize;
        _server.Start();
        _started = true;
        //start thread.
        _networkStats = new Thread(new ThreadStart(delegate
        {
            while (_started)
            {
                Stats.ResetClients(ClientEnds.Count);
                Thread.Sleep(1000);
            }
        }));
        _networkStats.Start();
    }

    public void Stop()
    {
        if (!_started) 
            return;

        _server.Stop();
        foreach (var t in ClientEnds)
        {
            t.Client.Disconnect();
        }

        ClientEnds.Clear();
        _started = false;
    }

    private void Server_onClientConnected(object? sender, ClientEventArgs e)
    {
        Console.WriteLine("Client connected.");

        var clientEnd = new ClientEnd(e.Client);

        e.Client.OnDataReceived += Client_onDataReceived;
        e.Client.OnDataSent += Client_onDataSent;
        clientEnd.OnClientEndDisconnected += ClientEnd_onDisconnected;
        ClientEnds.Add(clientEnd);
        Stats.AddClientEnd();
        clientEnd.Begin(OutboundIpAddress, PacketSize, Timeout);
    }

    private void ClientEnd_onDisconnected(object? sender, SocksClientEventArgs e)
    {
        e.Client.OnClientEndDisconnected -= ClientEnd_onDisconnected;
        e.Client.Client.OnDataReceived -= Client_onDataReceived;
        e.Client.Client.OnDataSent -= Client_onDataSent;
        ClientEnds.Remove(e.Client);
    }

    // All stats data is "Server" bandwidth stats, meaning clientside totals not counted.
    private void Client_onDataSent(object? sender, DataEventArgs e)
    {
        //Technically we are sending data from the remote server to the client, so it's being "received" 
        Stats.AddBytes(e.Count, ByteType.Received);
        Stats.AddPacket(PacketType.Received);
    }

    private void Client_onDataReceived(object? sender, DataEventArgs e)
    {
        //Technically we are receiving data from the client and sending it to the remote server, so it's being "sent" 
        Stats.AddBytes(e.Count, ByteType.Sent);
        Stats.AddPacket(PacketType.Sent);
    }
}