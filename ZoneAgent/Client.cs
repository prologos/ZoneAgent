using System;
using System.Net.Sockets;
namespace ZoneAgent
{
    class Client
    {
        public Client(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            TcpClient = tcpClient;
            Buffer = buffer;
            UniqID = 0;
        }
        public TcpClient TcpClient { get; private set; }

        public byte[] Buffer { get; private set; }

        public NetworkStream NetworkStream
        {
            get { return TcpClient.GetStream(); }
        }
        public int UniqID { get; set; }
    }
}
