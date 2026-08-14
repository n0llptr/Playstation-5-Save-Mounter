using System.Net.Sockets;

namespace PS4Saves;

public static class ElfldrClient
{
    public const int DefaultPort = 9021;

    public static void SendElf(string host, int port, byte[] elfData)
    {
        using var tcp = new TcpClient();
        tcp.Connect(host, port);
        using var stream = tcp.GetStream();
        stream.Write(elfData, 0, elfData.Length);
        tcp.Client.Shutdown(SocketShutdown.Send);
    }

    public static void SendElf(string host, byte[] elfData) =>
        SendElf(host, DefaultPort, elfData);
}
