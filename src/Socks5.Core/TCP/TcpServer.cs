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
using System.Net.Sockets;

namespace Socks5.Core.TCP;

public class TcpServer
{
    private readonly TcpListener _tcpListener;
    private readonly ManualResetEvent _event = new(false);
    private bool _accepted;

    public TcpServer(IPAddress ip, int port)
    {
        _tcpListener = new TcpListener(ip, port);
    }

    public int PacketSize { get; set; }

    public event EventHandler<ClientEventArgs> OnClientConnected = delegate { };
    public event EventHandler<ClientEventArgs> OnClientDisconnected = delegate { };

    public void Start()
    {
        if (!_accepted)
        {
            _accepted = true;
            _tcpListener.Start(10000);
            new Thread(AcceptConnections).Start();
        }
    }

    public void Stop()
    {
        if (_accepted)
        {
            _accepted = false;
            _tcpListener.Stop();
            _event.Set();
        }
    }

    private void AcceptConnections()
    {
        while (_accepted)
            try
            {
                _event.Reset();
                _tcpListener.BeginAcceptSocket(AcceptClient, _tcpListener);
                _event.WaitOne();
            }
            catch
            {
                //error, most likely server shutdown.
            }
    }

    private void AcceptClient(IAsyncResult res)
    {
        try
        {
            if (res.AsyncState is null)
                return;
            
            var px = (TcpListener)res.AsyncState;
            var socket = px.EndAcceptSocket(res);

            _event.Set();

            var f = new Client(socket, PacketSize);
            //f.onClientDisconnected += onClientDisconnected;
            //f.onDataReceived += onDataReceived;
            //f.onDataSent += onDataSent;
            OnClientConnected(this, new ClientEventArgs(f));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            //server stopped or client errored?
        }
    }
}