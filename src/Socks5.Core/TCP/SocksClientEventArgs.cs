using Socks5.Core.Socks;

namespace Socks5.Core.TCP;

public class SocksClientEventArgs : EventArgs
{
    public SocksClientEventArgs(ClientEnd client)
    {
        Client = client;
    }

    public ClientEnd Client { get; private set; }
}