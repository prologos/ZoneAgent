using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
namespace ZoneAgent
{
    /// <summary>
    /// -----Servers-----
    /// LS:LoginServer
    /// AS:AccountServer
    /// ZS=ZoneServer
    /// BS:BattleServer
    /// ZA:ZoneAgent
    /// Class that will manage the server connection and send and receive packets to and from client and/or servers
    /// </summary>
    class ZoneAgent
    {
        //Declaring objects
        EventDrivenTCPClient LS, AS, ZS, BS; // All Event based TCPClient for severs LS,AS,ZS,BS
        TcpListener ZA; // listener for ZoneAgent
        List<Client> clients;//list to store client information
        Dictionary<int, PlayerInfo> player; // Dictionary to store player information int=client id and PlayerInfo object
        Timer LSReporter;//timer to report LoginServer every 5 seconds
        /// <summary>
        /// Constructor
        /// Will create instance with specified ip and port
        /// Also will create 2 event methods
        /// 1)ConnectionStatusChanged when change in connection status
        /// 2)DataReceived when data is received
        /// </summary>
        public ZoneAgent()
        {
            //For LoginServer
            LS = new EventDrivenTCPClient(Config.LS_IP, Config.LS_PORT);
            LS.ConnectionStatusChanged += LS_ConnectionStatusChanged;
            LS.DataReceived += LS_DataReceived;
            //For AccountServer
            AS = new EventDrivenTCPClient(Config.AS_IP, Config.AS_PORT);
            AS.DataReceived += AS_DataReceived;
            AS.ConnectionStatusChanged += AS_ConnectionStatusChanged;
            //For ZoneServer
            ZS = new EventDrivenTCPClient(Config.ZS_IP, Config.ZS_PORT);
            ZS.DataReceived += ZS_DataReceived;
            ZS.ConnectionStatusChanged += ZS_ConnectionStatusChanged;
            //For BattleServer
            BS = new EventDrivenTCPClient(Config.BS_IP, Config.BS_PORT);
            BS.DataReceived += BS_DataReceived;
            BS.ConnectionStatusChanged += BS_ConnectionStatusChanged;

            clients = new List<Client>();//initializing list of clients
            player = new Dictionary<int, PlayerInfo>();//initializing dictionary of player information
            ZA = new TcpListener(Config.ZA_IP, Config.ZA_PORT);//initializing ZoneAgent Listener
            
            //Timer to send report packet to LoginServer every 5 seconds
            LSReporter = new Timer();
            LSReporter.Interval = 5000;
            LSReporter.Tick += LSReporter_Tick;

            //Connect to servers one by one
            try
            {
                LS.Connect();
                AS.Connect();
                ZS.Connect();
                //BS.Connect();
                Start();
            }
            catch (Exception connect)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "Connect : " + connect.ToString());
            }
        }

        void LSReporter_Tick(object sender, EventArgs e)
        {
            try
            {
                LS.Send(Packet.LSReporter());
            }
            catch (Exception LSReport)
            {
                Logger.Write(Logger.GetLoggerFileName("LoginServer"), "LSRepoter : "+LSReport.ToString());
            }
        }

        /// <summary>
        /// XX_ConnectionStatusChanged will check for connection status and set value true or false in Config variable accrodingly
        /// XX_DataReceived will process received data if required and send packets to client
        /// </summary>
        /// <summary>
        /// LoginServer connection Status Change event method
        /// Will be executed when connections status is changed
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="status">status of TCPClient</param>
        void LS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                LS.Send(Packet.LoginServerConnectPacket());
                Logger.Write("LoginServer.log", "LoginServer Connected");
                Config.isLSConnected = true;
                LSReporter.Enabled = true;
                LSReporter.Start();
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
            {
                if(Config.isLSConnected)
                    Logger.Write("LoginServer.log", "LoginServer Disconnected");
                LSReporter.Enabled = false;
                LSReporter.Stop();
                Config.isLSConnected = false;
            }
        }
        /// <summary>
        /// LoginServer Data Received method
        /// Executed when data received from loginserver
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="data">data received</param>
        void LS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            try
            {
                var packet = Encoding.Default.GetBytes(data.ToString());
                switch (packet.Length)
                {
                    case 40://login
                        var temp = new byte[4];
                        Array.Copy(packet, 4, temp, 0, 4);
                        var clientid = Packet.GetClientId(temp);
                        if (!player.ContainsKey(clientid))
                        {
                            var accountid = Encoding.ASCII.GetString(packet).Substring(10, 15).Trim().TrimEnd('\0');
                            player.Add(clientid, new PlayerInfo(accountid, Packet.GetTime(), false, false));
                            Config.PLAYER_COUNT++;
                        }
                        break;
                    case 48://duplicate login ; request DC to ZA from loginserver
                        var tempByte = new byte[4];
                        Array.Copy(packet, 4, tempByte, 0, 4);
                        var ClientID = Packet.GetClientId(tempByte);
                        if (player.ContainsKey(ClientID))
                        {
                            player.Remove(ClientID);
                            LS.Send(Packet.TrimPacket(packet, 48));
                        }
                        LS.Send(Packet.TrimPacket(packet, 48));
                        //Logger.WriteTransactionLog(Encoding.ASCII.GetString(packet).Substring(11).Trim() + " duplicate login!");
                        // TODO: Send DC to client
                        break;
                    default://if any other packet received
                        Logger.WriteBytes(Logger.GetLoggerFileName("LSNEW"), packet);
                        break;
                }
            }
            catch(Exception LSDataArrival)
            {
                Logger.Write(Logger.GetLoggerFileName("LoginServer"), "LoginServer DataReceived : "+LSDataArrival.ToString());
            }
        }
        /// <summary>
        /// AccountServer connection Status Change event method
        /// Will be executed when connection status to AccountServer is changed
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="status">status of TCPClient</param>
        void AS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                AS.Send(Packet.ZoneConnectPacket());
                Logger.Write("AccountServer.log", "AccountServer Connected");
                Config.isASConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
            {
                if(Config.isASConnected)
                    Logger.Write("AccountServer.log", "AccountServer Disconnected");
                Config.isASConnected = false;
            }
        }
        /// <summary>
        /// AccountServer Data Received methd
        /// Will be executed when data is received from AccountServer
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="data">byte[] data</param>
        void AS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            try
            {
                var packet = Encoding.Default.GetBytes(data.ToString());
                //File.WriteAllBytes("AS_" + Environment.TickCount + "_" + packet.Length, packet);
                var temp = new byte[4];
                Array.Copy(packet, 4, temp, 0, 4);
                int id = Packet.GetClientId(temp);
                if (player.ContainsKey(id))
                {
                    var playerinfo = player[id];
                    switch (packet.Length)
                    {
                        case 952://chacater packet received from AccountServer(.acl file)
                            Write(playerinfo.Client.TcpClient, Packet.AlterAccountServerPacket(packet));
                            break;
                        default://other byte size packet received
                            Write(playerinfo.Client.TcpClient, packet);
                            break;
                    }
                }
            }
            catch (Exception ASDataArrival)
            {
                Logger.Write(Logger.GetLoggerFileName("AccountServer"), "AccountServer DataReceived : "+ASDataArrival.ToString());
            }
        }
        /// <summary>
        /// ZoneServer connection Status Change event method
        /// Executed when status of connection to ZoneServer is changed
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="status">status of TCPClient</param>
        void ZS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                Logger.Write("ZoneServer.log", "ZoneServer Connected");
                ZS.Send(Packet.ZoneConnectPacket());
                Config.isZSConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
            {
                if (Config.isZSConnected)
                    Logger.Write("ZoneServer.log", "ZoneServer Disconnected");
                Config.isZSConnected = false;
            }
        }
        /// <summary>
        /// ZoneServer Data Received methd
        /// Will be executed when data is received from ZoneServer
        /// Will split packet and add each packet in list and then send it to client
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="data">byte[] data</param>
        void ZS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            try
            {
                var packet = Encoding.Default.GetBytes(data.ToString());
                List<byte[]> packetList = new List<byte[]>();
                packetList.Clear();
                Packet.SplitPackets(packet, packet.Length, ref packetList);
                for (int i = 0; i < packetList.Count; i++)
                {
                    var temp = new byte[4];
                    Array.Copy(packetList[i], 4, temp, 0, 4);
                    int id = Packet.GetClientId(temp);
                    if (player.ContainsKey(id))
                    {
                        var playerinfo = player[id];
                        if (packetList[i][10] == 0x08 && packetList[i][11] == 0x11)
                        {
                            Config.PLAYER_COUNT--;
                            playerinfo.Prepared = false;
                            LS.Send(Packet.SendDCToLS(id, playerinfo.Account, Packet.GetTime()));
                            playerinfo.ZoneStatus = false;
                            ZS.Send(Packet.SendDCToASZS(id));
                            player.Remove(id);
                            if (clients.Contains(playerinfo.Client))
                            {
                                lock (clients)
                                {
                                    clients.Remove(playerinfo.Client);
                                }
                            }
                        }
                        Write(playerinfo.Client.TcpClient, packetList[i]);
                    }
                }
            }
            catch (Exception ZSDataArrival)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneServer"), "ZoneServer DataReceived : " + ZSDataArrival.ToString());
            }
        }
        /// <summary>
        /// BatlleServer connection Status Change event method
        /// Executed when status of connection to BattleServer is changed
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="status">status of TCPClient</param>
        void BS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                Logger.Write("BattleServer.log", "BattleServer Connected");
                BS.Send(Packet.ZoneConnectPacket());
                Config.isBSConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
            {
                if (Config.isBSConnected)
                    Logger.Write("BattleServer.log", "BattleServer Disonnected");
                Config.isBSConnected = false;
            }
        }
        /// <summary>
        /// BattleServer Data Received method
        /// Will be executed when data is received from BattleServer
        /// </summary>
        /// <param name="sender">EventDrivenTCPClient object</param>
        /// <param name="data">byte[] data</param>
        void BS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Start listening of ZA and start accepting client request
        /// </summary>
        public void Start()
        {
            ZA.Start();
            ZA.BeginAcceptTcpClient(ClientHandler, null);
            Logger.Write("ZoneAgent.log", "Start => ZoneAgent started listening");
        }
        /// <summary>
        /// Handle incoming clients and start reading stream for the new client
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ClientHandler(IAsyncResult asyncResult)
        {
            try
            {
                TcpClient client = ZA.EndAcceptTcpClient(asyncResult);
                var buffer = new byte[client.ReceiveBufferSize];
                var newClient = new Client(client, buffer);
                lock (clients)
                {
                    clients.Add(newClient);
                }
                NetworkStream networkStream = newClient.NetworkStream;
                networkStream.BeginRead(newClient.Buffer, 0, newClient.Buffer.Length, OnDataRead, newClient);
                ZA.BeginAcceptTcpClient(ClientHandler, null);
            }
            catch (Exception clientHandle)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "ClientHandler : "+clientHandle.ToString());
            }
        }
        /// <summary>
        /// Data received will be received
        /// Further sending to appropriate server (AS,ZS,BS) and keep on reading client stream
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnDataRead(IAsyncResult asyncResult)
        {
            var client = asyncResult.AsyncState as Client;
            try
            {
                if (client == null)
                    return;
                NetworkStream networkStream = client.NetworkStream;
                var newClientEp = (IPEndPoint)client.TcpClient.Client.RemoteEndPoint;
                int read = networkStream.EndRead(asyncResult);
                if (read == 0 || read < 10)
                {
                    lock (clients)
                    {
                        clients.Remove(client);
                        return;
                    }
                }
                var packet = client.Buffer;
                //File.WriteAllBytes(Environment.TickCount+"_"+read, Packet.TrimPacket(packet,read));
                switch (Packet.GetPacketType(packet, read))
                {
                    case Config.INVALID: //For invalid request i.e packet size 0 or <=10
                        lock (clients)
                        {
                            clients.Remove(client);
                            return;
                        }
                    case Config.LOGIN_PACKET: //Login to ZoneAgent
                        var temp = new byte[4];
                        Array.Copy(packet, 4, temp, 0, 4);
                        var clientId = Packet.GetClientId(temp);
                        client.UniqID = clientId;
                        if (player.ContainsKey(clientId))
                        {
                            var playerInfo = player[clientId];
                            playerInfo.Prepared = true;
                            playerInfo.Client = client;
                            LS.Send(Packet.CreateClientStatusPacket(clientId, playerInfo.Account));
                            var character = Packet.CreateGetCharacterPacket(clientId, playerInfo.Account, newClientEp.Address.ToString());
                            AS.Send(character);
                        }
                        //else
                        //{
                        //   Console.WriteLine("Possible attempt of bypass of login server using client ID " + clientId + " and IP " + newClientEp.Address);
                        //Logger.WriteTransactionLog("Possible attempt of bypass of login server using client ID " + clientId + " and IP " + newClientEp.Address);
                        //}
                        break;
                    case Config.AS_PACKET: //Sends packet to AccountServer
                        AS.Send(Packet.AddClientID(packet, client.UniqID, read));
                        break;
                    case Config.ZS_PACKET: //Sends packet to ZoneServer
                        if (read == 33 && player.ContainsKey(client.UniqID))
                        {
                            var playerinfo = player[client.UniqID];
                            if (!playerinfo.ZoneStatus)
                                playerinfo.ZoneStatus = true;
                        }
                        ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.BS_PACKET: //Sends packet to BattleServer
                        BS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.DISCONNECT_PACKET: //Disconnect Packet
                        if (player.ContainsKey(client.UniqID))
                        {
                            Config.PLAYER_COUNT--;
                            var playerinfo = player[client.UniqID];
                            playerinfo.Prepared = false;
                            LS.Send(Packet.SendDCToLS(client.UniqID, playerinfo.Account, Packet.GetTime()));
                            if (playerinfo.ZoneStatus)
                            {//to disconnect from zoneserver
                                playerinfo.ZoneStatus = false;
                                ZS.Send(Packet.AddClientID(packet, client.UniqID, read));
                                ZS.Send(Packet.SendDCToASZS(client.UniqID));
                            }
                            else
                            {//to disconnect from character selection screen
                                AS.Send(Packet.AddClientID(packet, client.UniqID, read));
                                AS.Send(Packet.SendDCToASZS(client.UniqID));
                            }
                            player.Remove(client.UniqID);
                            lock (clients)
                            {
                                clients.Remove(playerinfo.Client);
                            }
                        }
                        Write(client.TcpClient, Packet.AddClientID(packet, client.UniqID, read));
                        break;
                }
                networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, OnDataRead, client);
            }
            catch (Exception onDataRead)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "OnDataRead : " + onDataRead.ToString());
            }
        }
        /// <summary>
        /// Write (send) packet to client
        /// </summary>
        /// <param name="tcpClient">socket of client</param>
        /// <param name="bytes">byte[] data</param>
        private static void Write(TcpClient tcpClient, byte[] bytes)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, tcpClient);
        }
        /// <summary>
        /// Handler for writing packets
        /// </summary>
        /// <param name="result"></param>
        private static void WriteCallback(IAsyncResult result)
        {
            try
            {
                var tcpClient = result.AsyncState as TcpClient;
                if (tcpClient != null)
                {
                    NetworkStream networkStream = tcpClient.GetStream();
                    networkStream.EndWrite(result);
                }
            }
            catch (Exception writeCallBack)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "WriteCallBack : " + writeCallBack.ToString());
            }
        }
    }
}
