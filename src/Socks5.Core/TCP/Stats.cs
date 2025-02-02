﻿/*
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

namespace Socks5.Core.TCP;

public class Stats
{
    public Stats()
    {
    }

    public int TotalClients { get; private set; }
    public int ClientsSinceRun { get; private set; }

    public ulong NetworkReceived { get; private set; }
    public ulong NetworkSent { get; private set; }

    public ulong PacketsSent { get; private set; }
    public ulong PacketsReceived { get; private set; }

    private ulong BytesReceivedPerSecond { get; set; }
    private ulong BytesSentPerSecond { get; set; }
    
    private DateTime _receivedLastRead = DateTime.Now;
    private DateTime _sentLastRead = DateTime.Now;

    public string ReceivedBytesPerSecond()
    {
        var len = BytesReceivedPerSecond / (DateTime.Now - _receivedLastRead).TotalSeconds;
        BytesReceivedPerSecond = 0;
        _receivedLastRead = DateTime.Now;
        return HumanReadable((ulong)len);
    }

    public string SentBytesPerSecond()
    {
        var len = BytesSentPerSecond / (DateTime.Now - _sentLastRead).TotalSeconds;
        BytesSentPerSecond = 0;
        _sentLastRead = DateTime.Now;
        return HumanReadable((ulong)len);
    }

    public string HumanReadable(ulong i)
    {
        // Determine the suffix and readable value
        double readable = i;

        // Divide by 1024 to get fractional value
        readable /= 1024;
        // Return formatted number with suffix
        return readable.ToString("0.###");
    }
    
    public void AddClientEnd()
    {
        TotalClients++;
        ClientsSinceRun++;
    }

    public void ResetClients(int count)
    {
        TotalClients = count;
    }

    public void AddBytes(int bytes, ByteType typ)
    {
        if (typ != ByteType.Sent)
        {
            BytesReceivedPerSecond += (ulong)bytes;
            NetworkReceived += (ulong)bytes;
            return;
        }
        
        BytesSentPerSecond += (ulong)bytes;
        NetworkSent += (ulong)bytes;
    }

    public void AddPacket(PacketType pkt)
    {
        if (pkt != PacketType.Sent)
        {
            PacketsReceived++;
        }
        else
        {
            PacketsSent++;
        }
    }
}