using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading;

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
        System.Windows.Forms.Timer LSReporter, PingDisplay;//LSReporter=timer to report LoginServer every 5 seconds,PingDisplay=timer to display ping to each player refresh every 10 seconds
        Ping ping;//to ping ip
        Random randomId;//to generate random client id initially its temporary and will not be used
        Main _Main; // Reference of Main class to access objects
        public static System.Windows.Forms.Timer GMShout; //timer to send GM messages
        Button ShoutManually; //Button created run time
        Thread threadChkClientDeath; //Clients health check tread

        /// <summary>
        /// Constructor
        /// Will create instance with specified ip and port
        /// Also will create 2 event methods
        /// 1)ConnectionStatusChanged when change in connection status
        /// 2)DataReceived when data is received
        /// </summary>
        public ZoneAgent(Main _Main)
        {
            //For LoginServer
            LS = new EventDrivenTCPClient(Config.LS_IP, Config.LS_PORT);
            LS.ConnectionStatusChanged += LS_ConnectionStatusChanged;
            LS.DataReceived += LS_DataReceived;
            //For AccountServer
            AS = new EventDrivenTCPClient(Config.AS_IP, Config.AS_PORT);
            AS.DataReceived += ZS_DataReceived;
            AS.ConnectionStatusChanged += AS_ConnectionStatusChanged;
            //For ZoneServer
            ZS = new EventDrivenTCPClient(Config.ZS_IP, Config.ZS_PORT);
            ZS.DataReceived += ZS_DataReceived;
            ZS.ConnectionStatusChanged += ZS_ConnectionStatusChanged;
            //For BattleServer
            BS = new EventDrivenTCPClient(Config.BS_IP, Config.BS_PORT);
            BS.DataReceived += ZS_DataReceived;
            BS.ConnectionStatusChanged += BS_ConnectionStatusChanged;

            clients = new List<Client>();//initializing list of clients
            player = new Dictionary<int, PlayerInfo>();//initializing dictionary of player information
            //var HostIP = Dns.GetHostEntry(Dns.GetHostName())
            //     .AddressList
            //     .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            ZA = new TcpListener(Config.ZA_IP, Config.ZA_PORT);//initializing ZoneAgent Listener

            randomId = new Random();
            
            //Timer to send report packet to LoginServer every 5 seconds
            LSReporter = new System.Windows.Forms.Timer { Interval = 5000 };
            LSReporter.Tick += LSReporter_Tick;
            //Timer to display ping to each player
            PingDisplay = new System.Windows.Forms.Timer { Interval = 7000 };
            PingDisplay.Tick += PingDisplay_Tick;
            PingDisplay.Enabled = true;
            PingDisplay.Start();

            this._Main = _Main; // Taking refrence of main form

            //timer to send messages to client
            GMShout = new System.Windows.Forms.Timer();
            GMShout.Tick += GMShout_Tick;

            //Button created runtime to send custom messages to client
            ShoutManually = new Button();
            ShoutManually.Size = new System.Drawing.Size(123, 27);
            ShoutManually.Text = "Shout Manually";
            ShoutManually.Location = new System.Drawing.Point(132, 325);
            ShoutManually.Click += ShoutManually_Click;

            _Main.Controls.Add(ShoutManually); //adding control i.e button on form

            //Connect to servers one by one
            try
            {
                LS.Connect();
                AS.Connect();
                ZS.Connect();
                BS.Connect();
                Start();
            }
            catch (Exception connect)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "Connect : " + connect.ToString());
            }
        }

        /// <summary>
        /// Will be executed when ShoutManually button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShoutManually_Click(object sender, EventArgs e)
        {
            SendMessage(_Main.manual_msg.Text);
        }
        /// <summary>
        /// Will display ping to every player refresh time 7 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PingDisplay_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (var client in clients)
                {
                    if (player[client.UniqID].ZoneStatus == Config.ZS_ID || player[client.UniqID].ZoneStatus == Config.BS_ID)
                    {
                        var clientAddress = (IPEndPoint)client.TcpClient.Client.RemoteEndPoint;
                        ping = new Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(clientAddress.Address, client);
                    }

                }
            }
            catch (Exception PingDisp)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "Display Ping : " + PingDisp);
            }
        }
        void  PingCompleted(object sender, PingCompletedEventArgs e)
        {
            Ping p = (Ping)sender;
            var client = (Client)e.UserState;
            if (clients.Contains(client))
            {
                Write(client.TcpClient,
                    e.Reply.Status == IPStatus.Success ?
                    Packet.DisplayPing(client.UniqID, e.Reply.RoundtripTime.ToString() + " ms") :
                    Packet.DisplayPing(client.UniqID, "---"));
            }
            p.PingCompleted -= PingCompleted;
            p.Dispose();
        }

        /// <summary>
        /// Send a GM message to the user connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GMShout_Tick(object sender, EventArgs e)
        {
            try
            {
                SendMessage(Config.GMShout_list[Config.GMShout_count]);
                Config.GMShout_count++;
                if (Config.GMShout_count >= Config.GMShout_list.Length - 1) { Config.GMShout_count = 0; }
                _Main.Show_Next_ShoutMsg(Config.GMShout_list[Config.GMShout_count]);
            }
            catch (Exception gmMessages)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "GM Message : " + gmMessages);
            }
        }
        /// <summary>
        /// will send given message to all clients
        /// </summary>
        /// <param name="message">message</param>
        void SendMessage(string message)
        {
            foreach (var client in clients)
            {
                if (player[client.UniqID].ZoneStatus == Config.ZS_ID || player[client.UniqID].ZoneStatus == Config.BS_ID)
                {
                    Write(client.TcpClient, Packet.PrivateMessage(client.UniqID, message));
                }
            }
        }
        /// <summary>
        /// Will send Report packet to LoginServer at interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            else
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
                var packet = (byte[])Convert.ChangeType(data, typeof(byte[]));
                switch (packet.Length)
                {
                    case 40://login
                        var temp = new byte[4];
                        Array.Copy(packet, 4, temp, 0, 4);
                        var clientid = Packet.GetClientId(temp);
                        if (!player.ContainsKey(clientid))
                        {
                            var accountid = Encoding.ASCII.GetString(packet).Substring(10, 15).Trim().TrimEnd('\0');
                            player.Add(clientid, new PlayerInfo(accountid, Packet.GetTime(), false, -1));
                            Config.PLAYER_COUNT++;
                            if (Config.PLAYER_COUNT > Config.MAX_PLAYER_COUNT) { Config.MAX_PLAYER_COUNT = Config.PLAYER_COUNT; }
                            //player count update
                            _Main.Update_Player_Count();
                            //zonelog update
                            _Main.Update_zonelog("<LC> UID = " + clientid.ToString() + " " + accountid + " Prepared");
                        }
                        break;
                    case 48://duplicate login ; request DC to ZA from loginserver
                        var tempByte = new byte[4];
                        Array.Copy(packet, 4, tempByte, 0, 4);
                        var ClientID = Packet.GetClientId(tempByte);
                        if (player.ContainsKey(ClientID))
                        {
                            foreach (var client in clients)
                            {
                                if (client.UniqID == ClientID)
                                {
                                    client.TcpClient.GetStream().Close();
                                    break;
                                }
                            }
                            LS.Send(Packet.DuplicateUserDCPacket(packet));
                            //zonelog update
                            _Main.Update_zonelog("<LC> UID = " + ClientID.ToString() + " Dropped, Reason = Duplicate login");
                        }
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
            else
            {
                if(Config.isASConnected)
                    Logger.Write("AccountServer.log", "AccountServer Disconnected");
                Config.isASConnected = false;
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
            else
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
                var packet = (byte[])Convert.ChangeType(data, typeof(byte[]));
                //File.WriteAllBytes("OGZS_" + Environment.TickCount + "_" + packet.Length, packet);
                Application.DoEvents();
                var packetList = new List<byte[]>();
                Application.DoEvents();
                packetList.Clear();
                Application.DoEvents();
                Packet.SplitPackets(packet, packet.Length, ref packetList);
                foreach (var t in packetList)
                {
                    var temp = new byte[4];
                    Array.Copy(t, 4, temp, 0, 4);
                    var id = Packet.GetClientId(temp);
                    if (player.ContainsKey(id))
                    {
                        var playerinfo = player[id];
                        //Below condition is for disconneting client or reconnecting client
                        if (playerinfo.ZoneStatus == Config.AS_ID)
                        {
                            if (t.Length == 952)
                            {
                                Write(playerinfo.Client.TcpClient, Packet.AlterAccountServerPacket(t));
                            }
                            else
                            {
                                Write(playerinfo.Client.TcpClient, t);
                            }
                        }
                        else
                        {
                            Write(playerinfo.Client.TcpClient, t);
                            //Set Character Name
                            if (t.Length == 39 && t[10] == 0x06 && t[11] == 0x11)
                            {
                                playerinfo.CharName = Packet.GetCharName(Crypt.Decrypt(t), 12);
                            }
                            //Below condition is to reduce chance of other packets come under same conditions
                            else if (t.Length > 18 && t[10] == 0x00 && t[11] == 0x18 && t[12] == 0x74 && t[13] == 0xCE && t[14] == 0xCA && t[15] == 0xE9 && t[16] == 0x87 && t[17] == 0x7F && t[18] == 0xAB)
                            {
                                var tempPacket = t;
                                Packet.SetStatusValues(tempPacket);
                            }
                        }
                        //Zone changed Packet : ZoneStatus Change
                        if (t.Length == 11 && t[8] == 0x01 && t[9] == 0xE1)
                        {
                            //zonelog update
                            _Main.Update_zonelog(playerinfo.Account + " (" + id + ") user zone changed " + playerinfo.ZoneStatus + " -> " + t[10]);
                            playerinfo.ZoneStatus = t[10];
                        }
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
            else
            {
                if (Config.isBSConnected)
                    Logger.Write("BattleServer.log", "BattleServer Disonnected");
                Config.isBSConnected = false;
            }
        }
        /// <summary>
        /// Start listening of ZA and start accepting client request
        /// </summary>
        public void Start()
        {
            ZA.Start();
            ZA.BeginAcceptTcpClient(ClientHandler, null);
            //Start clients health check tread
            threadChkClientDeath = new Thread(CheckClientDeathThread);
            threadChkClientDeath.IsBackground = true;
            threadChkClientDeath.Start();
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
                var client = ZA.EndAcceptTcpClient(asyncResult);
                var buffer = new byte[client.ReceiveBufferSize];
                var newClient = new Client(client, buffer,randomId.Next());
                lock (clients)
                {
                    clients.Add(newClient);
                }
                var networkStream = newClient.NetworkStream;
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
                var networkStream = client.NetworkStream;
                var newClientEp = (IPEndPoint)client.TcpClient.Client.RemoteEndPoint;
                var read = networkStream.EndRead(asyncResult);
                if (read == 0 || read < 10)
                {
                    client.TcpClient.GetStream().Close();
                    return;
                }
                var packet = client.Buffer;
                PlayerInfo playerInformation=null;
                if (player.ContainsKey(client.UniqID))
                    playerInformation = player[client.UniqID];
                switch (Packet.GetPacketType(packet, read, playerInformation))
                {
                    case Config.INVALID: //For invalid request i.e packet size 0 or <=10
                        client.TcpClient.GetStream().Close();
                        return;
                    case Config.LOGIN_PACKET: //Login to ZoneAgent
                        var temp = new byte[4];
                        Array.Copy(packet, 4, temp, 0, 4);
                        var clientId = Packet.GetClientId(temp);
                        client.UniqID = clientId;
                        if (player.ContainsKey(clientId))
                        {
                            var playerInfo = player[clientId];
                            playerInfo.Prepared = true;
                            playerInfo.ZoneStatus = Config.AS_ID;
                            playerInfo.Client = client;
                            LS.Send(Packet.CreateClientStatusPacket(clientId, playerInfo.Account));
                            //zonelog update
                            _Main.Update_zonelog(playerInfo.Account + "(" + newClientEp.Address.ToString() + ") User Joined");
                            var character = Packet.CreateGetCharacterPacket(clientId, playerInfo.Account, newClientEp.Address.ToString());
                            AS.Send(character);
                        }
                        break;
                    case Config.AS_PACKET: //Sends packet to AccountServer
                        AS.Send(Packet.AddClientID(packet, client.UniqID, read));
                        break;
                    case Config.ZS_PACKET: //Sends packet to ZoneServer
                        ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.BS_PACKET: //Sends packet to BattleServer
                        BS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        break;
                    case Config.DISCONNECT_PACKET: //Disconnect Packet
                        if (player.ContainsKey(client.UniqID))
                        {
                            var playerinfo = player[client.UniqID];
                            if (playerinfo.ZoneStatus == Config.ZS_ID)
                                ZS.Send(Packet.AddClientID(packet, client.UniqID, read));
                            else if (playerinfo.ZoneStatus == Config.BS_ID)
                                BS.Send(Packet.AddClientID(packet, client.UniqID, read));
                        }
                        break;
                    case Config.PAYMENT_PACKET:
                        Write(client.TcpClient, Packet.PrivateMessage(client.UniqID,Config.PayMsg));
                        break;
                    case Config.TELEPORT_PACKET:
                        RestrictingTeleportation(packet, client, read);
                        break;
                    case Config.WHISPERCHAT_PACKET:
                        var Whisper = new byte[98];
                        Array.Copy(packet, 0, Whisper, 0, 98);
                        Whisper = Crypt.Decrypt(Whisper);
                        string Call = Packet.GetCharName(Whisper, 13);
                        if (Config.ANSWERER != "" && Call == Config.ANSWERER)
                        {
                            string Command = Packet.GetCharName(Whisper, 34, 62).Trim();
                            if (Config.FAQ.ContainsKey(Command))
                            {
                                string[] Answer = Config.FAQ[Command].Split('|');
                                foreach (string s in Answer)
                                {
                                    Write(client.TcpClient, Packet.PrivateMessage(client.UniqID, s));
                                }
                            }
                            else
                            {
                                Write(client.TcpClient, Packet.PrivateMessage(client.UniqID, "Invalid command. The command is case sensitive."));
                            }
                        }
                        else
                        {
                            ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        }
                        break;
                }
                if (clients.Contains(client))
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
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, tcpClient);
            }
            catch(Exception Write)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "Write : " + Write.ToString());
            }
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
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "WriteCallBack : " + writeCallBack);
            }
        }
        /// <summary>
        /// Restricting teleportation of players to certain maps
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="client"></param>
        /// <param name="read"></param>
        private void RestrictingTeleportation(byte[] packet, Client client, int read)
        {
            try
            {
                var temp = new byte[2];
                Array.Copy(packet, 16, temp, 0, 2);
                ushort TargetLine = BitConverter.ToUInt16(temp, 0);
                //If Send a malformed packet for ZS crash.
                if (TargetLine > Config.TELEPORT_LIST.Count)
                {
                    Write(client.TcpClient, Packet.PrivateMessage(client.UniqID, "Invalid Request."));
                    return;
                }
                if (_Main.activeRestrict.Checked && _Main.activeRestrict.Enabled)
                {
                    //Activated Restricting teleportation
                    string TargetMapNumber = Config.TELEPORT_LIST[TargetLine];
                    bool isLockedMap = false;
                    bool isAllowUser = false;
                    for (int i = 0; i < Config.MAPLEVEL.Count; i++)
                    {
                        if (Config.MAPLEVEL[i].Contains(TargetMapNumber))
                        {
                            isLockedMap = true;
                            for (int userLevel = i; userLevel < Config.USERLEVEL.Count; userLevel++)
                            {
                                if(Config.USERLEVEL[userLevel].Contains(player[client.UniqID].Account))
                                {
                                    isAllowUser = true;
                                    break;
                                }
                            }
                        }
                        if (isLockedMap)
                            break;
                    }
                    if (isLockedMap)
                    {
                        if (isAllowUser)
                        {
                            ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                        }
                        else
                        {
                            Write(client.TcpClient, Packet.PrivateMessage(client.UniqID, "Your permission is not enough to move the map."));
                        }
                    }
                    else
                    {
                        ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                    }
                }
                else
                {
                    //Deactivated Restricting teleportation
                    ZS.Send(Packet.CheckForMultiplePackets(packet, client.UniqID, read));
                }
            }
            catch { }
        }
        /// <summary>
        /// Thread to Check the disconnect of client
        /// </summary>
        private void CheckClientDeathThread()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(50);
                    Application.DoEvents();
                    for (int i = clients.Count - 1; i >= 0; i--)
                    {
                        //if (clients[i].TcpClient.Client.Poll(10, SelectMode.SelectRead) && (clients[i].TcpClient.Client.Available == 0))
                        if (!clients[i].TcpClient.Connected)
                        {
                            RemoveElements(clients[i].UniqID, i);
                        }
                    }
                }
            }
            catch { }
        }
        private void RemoveElements(int UniqID, int index)
        {
            try
            {
                if (player.ContainsKey(UniqID)) //Remove Disconnected User
                {
                    lock (clients)
                    {
                        clients.Remove(player[UniqID].Client);
                    }
                    //DC Packet to LS
                    LS.Send(Packet.SendDCToLS(UniqID, player[UniqID].Account, Packet.GetTime()));
                    //DC Packet to ZS
                    if (player[UniqID].ZoneStatus == Config.ZS_ID)
                        ZS.Send(Packet.SendDCToASZS(UniqID));
                    else if (player[UniqID].ZoneStatus == Config.BS_ID)
                        BS.Send(Packet.SendDCToASZS(UniqID));
                    //player count update
                    Config.PLAYER_COUNT--;
                    _Main.Update_Player_Count();
                    //zonelog update
                    _Main.Update_zonelog(player[UniqID].Account + " " + UniqID + " User Left");
                    player.Remove(UniqID);
                }
                else //Remove Disconnected Unknown Client
                {
                    lock (clients)
                    {
                        clients.RemoveAt(index);
                    }
                }
            }
            catch { }
        }

        
    }
}
