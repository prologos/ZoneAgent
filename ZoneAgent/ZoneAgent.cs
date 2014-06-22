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
    /*----Severs----
     * LS:LoginServer
     * AS:AccountServer
     * ZS=ZoneServer
     * BS:BattleServer
     * ZA:ZoneAgent
     */
    //Class that will manage ther Server connection and send and receive packets to and from client and/or servers
    class ZoneAgent
    {
        //Declaring objects
        EventDrivenTCPClient LS, AS, ZS, BS; // All Event based TCPClient
        TcpListener ZA; // listener
        List<Client> clients;//list to store client information
        Dictionary<int, PlayerInfo> player; // Dictionary to store player information int=client id and PlayerInfo object
        Timer LSReporter;
        //Constructor
        public ZoneAgent()
        {
            //initializing objects
            /*
             * Will create instance with specified ip and port
             * also will create 2 event methods
             * 1)ConnectionStatusChanged when change in connection status
             * 2)DataReceived when data is received
             */
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
            LS.Connect();
            AS.Connect();
            ZS.Connect();
            //BS.Connect();
            Start();
        }

        void LSReporter_Tick(object sender, EventArgs e)
        {
            LS.Send(Packet.LSReporter());
        }
        /*
         * XX_ConnectionStatusChanged will check for connection status and set value true or false in Config variable accrodingly
         * XX_DataReceived will process received data if required and send packets to client
         */
        //LoginServer connection Status Change event method
        void LS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                LS.Send(Packet.LoginServerConnectPacket());
                Config.isLSConnected = true;
                LSReporter.Enabled = true;
                LSReporter.Start();
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
            {
                LSReporter.Enabled = false;
                LSReporter.Stop();
                Config.isLSConnected = false;
            }
        }
        //LoginServer Data Received methd
        void LS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            var packet = Encoding.Default.GetBytes(data.ToString());
            switch (packet.Length)
            {
                case 40:
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
                case 48:
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
                default:
                    //Logger.WriteLog("Got unknown packet with length : " + packet.Length);
                    break;
            }
        }
        //AccountServer connection Status Change event method
        void AS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                AS.Send(Packet.ZoneConnectPacket());
                Config.isASConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
                Config.isASConnected = false;
        }
        //AccountServer Data Received methd
        void AS_DataReceived(EventDrivenTCPClient sender, object data)
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
                    case 952:
                        Write(playerinfo.Client.TcpClient, Packet.AlterAccountServerPacket(packet));
                        break;
                    default:
                        Write(playerinfo.Client.TcpClient, packet);
                        break;
                }
            }
        }
        //ZoneServer connection Status Change event method
        void ZS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                ZS.Send(Packet.ZoneConnectPacket());
                Config.isZSConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
                Config.isZSConnected = false;
        }
        //ZoneServer Data Received methd
        void ZS_DataReceived(EventDrivenTCPClient sender, object data)
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
                        if(clients.Contains(playerinfo.Client))
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
        //BatlleServer connection Status Change event method
        void BS_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                BS.Send(Packet.ZoneConnectPacket());
                Config.isBSConnected = true;
                Config.CONNECTED_SERVER_COUNT++;
            }
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost)
                Config.isBSConnected = false;
        }
        //BattleServer Data Received methd
        void BS_DataReceived(EventDrivenTCPClient sender, object data)
        {
            throw new NotImplementedException();
        }
        //Start() will start ZA listener
        public void Start()
        {
            ZA.Start();
            ZA.BeginAcceptTcpClient(ClientHandler, null);
        }
        //Handle incoming clients and start reading stream for the new client
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //Logger.WriteLog("ClientHandler ERROR: " + e.Message);
            }
        }
        //Data received by listener will come in OnDataRead()
        //Further sending to appropriate server (AS,ZS,BS) and keep on reading client stream
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
                    case Config.INVALID:
                        //For invalid request i.e packet size 0 or <=10
                        lock (clients)
                        {
                            clients.Remove(client);
                            return;
                        }
                    case Config.LOGIN_PACKET:
                        //Login to ZoneAgent
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
                    case Config.AS_PACKET:
                        //Sends packet to AccountServer
                        AS.Send(Packet.AddClientID(packet, client.UniqID, read));
                        break;
                    case Config.ZS_PACKET:
                        //Sends packet to ZoneServer
                        if (read == 33 && player.ContainsKey(client.UniqID))
                        {
                            var playerinfo = player[client.UniqID];
                            if (!playerinfo.ZoneStatus)
                                playerinfo.ZoneStatus = true;
                        }
                        ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.BS_PACKET:
                        //Sends packet to BattleServer
                        BS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.DISCONNECT_PACKET:
                        //Disconnect Packet
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
                /*switch (read)
                {
                    case 10:
                        lock (clients)
                        {
                            clients.Remove(client);
                            return;
                        }
                    case 12:
                        //Entering game and disconnection from server
                        if (packet[10] == 0x08 && packet[11] == 0x11)
                        {
                            Config.PLAYER_COUNT--;
                        }
                        ZS.Send(Packet.AddClientID(packet, client.UniqID, 12));
                        break;
                    case 30:
                        //Sending DC to ZS
                        Config.PLAYER_COUNT--;
                        ZS.Send(Packet.AddClientID(packet, client.UniqID, 30));
                        break;
                    case 37:
                        //Checking character by sending packet to AccountServer before entering game
                        AS.Send(Packet.AddClientID(packet, client.UniqID, 37));
                        break;
                    case 33:
                        //Entering game , deleting character , Add Friend
                        //For delete character ,alphabet at 11th index will be 'C'
                        var clientpacket = Crypt.Decrypt(client.Buffer);
                        if (Packet.GetCharName(clientpacket, 11)[0] == 'C')
                        {
                            AS.Send(Packet.AddClientID(Crypt.Encrypt(packet), client.UniqID, 33));
                        }
                        else
                        {
                            ZS.Send(Packet.AddClientID(Crypt.Encrypt(clientpacket), client.UniqID, 33));
                        }
                        break;
                    case 35:
                        //Creating character
                        AS.Send(Packet.AddClientID(packet, client.UniqID, 35));
                        break;
                    case 56:
                        //Login to ZoneAgent
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
                            //Logger.WriteTransactionLog(playerInfo.Account + " has joined!");
                        }
                        //else
                        //{
                         //   Console.WriteLine("Possible attempt of bypass of login server using client ID " + clientId + " and IP " + newClientEp.Address);
                            //Logger.WriteTransactionLog("Possible attempt of bypass of login server using client ID " + clientId + " and IP " + newClientEp.Address);
                        //}
                        break;
                    default:
                        //Rest all packets send to ZoneAgent
                        ZS.Send(Packet.AddClientID(packet, client.UniqID, read));
                        break;
                
                }
                */
                networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, OnDataRead, client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //Logger.WriteLog("OnDataRead error : " + e.Message);
            }
        }
        //Write() will write packet to client
        private static void Write(TcpClient tcpClient, byte[] bytes)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, tcpClient);
        }
        //Handler for writing packets
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //Logger.WriteLog("Write call back exception : " + ex.Message);
            }
        }
    }
}
