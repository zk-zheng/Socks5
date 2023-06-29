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
using System.Text;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks;

internal class Socks5
{
    public static List<AuthTypes> RequestAuth(ClientEnd client)
    {
        byte[] buff;
        var recv = Receive(client.Client, out buff);

        if ((HeaderTypes)buff[0] != HeaderTypes.Socks5)
        {
            return new List<AuthTypes>();
        }

        var methods = Convert.ToInt32(buff[1]);
        var types = new List<AuthTypes>();
        for (var i = 2; i < methods + 2; i++)
        {
            switch ((AuthTypes)buff[i])
            {
                case AuthTypes.Login:
                    types.Add(AuthTypes.Login);
                    break;
                case AuthTypes.None:
                    types.Add(AuthTypes.None);
                    break;
            }
        }

        return types;
    }

    public static SocksRequest? RequestTunnel(ClientEnd client)
    {
        byte[] data;
        var recv = Receive(client.Client, out data);
        var buff = data;
        if (buff == null || (HeaderTypes)buff[0] != HeaderTypes.Socks5) 
            return null;

        switch ((StreamTypes)buff[1])
        {
            case StreamTypes.Stream:
            {
                var fwd = 4;
                var address = "";
                switch ((AddressType)buff[3])
                {
                    case AddressType.Ip:
                    {
                        for (var i = 4; i < 8; i++)
                            //grab IP.
                            address += Convert.ToInt32(buff[i]) + (i != 7 ? "." : "");
                        fwd += 4;
                    }
                        break;

                    case AddressType.Domain:
                    {
                        var domainlen = Convert.ToInt32(buff[4]);
                        address += Encoding.ASCII.GetString(buff, 5, domainlen);
                        fwd += domainlen + 1;
                    }
                        break;

                    case AddressType.Pv6:
                        //can't handle IPV6 traffic just yet.
                        return null;
                }

                var po = new byte[2];
                Array.Copy(buff, fwd, po, 0, 2);
                var port = BitConverter.ToUInt16(new[] { po[1], po[0] }, 0);
                return new SocksRequest(StreamTypes.Stream, (AddressType)buff[3], address, port);
            }

            default:
                //not supported.
                return null;
        }
    }

    public static int Receive(Client client, out byte[] buffer)
    {
        buffer = new byte[65535];
        return client.Receive(buffer, 0, buffer.Length);
    }
}