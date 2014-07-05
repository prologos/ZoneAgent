using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace ZoneAgent
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Prompts user if user wants to close ZoneAgent or not
        /// if yes program exits else not
        /// </summary>
        private void btnclose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close ??", "ZoneAgent", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                ExitZoneAgent();
            }
        }
        /// <summary>
        /// executes when program starts
        /// </summary>
        private void Main_Load(object sender, EventArgs e)
        {
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if(!File.Exists("SvrInfo.ini"))
            {
                MessageBox.Show("SvrInfo.ini file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Write("ZoneAgent.log", "Stop => File not found SvrInfo.ini");
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("msvcp100d.dll"))
            {
                MessageBox.Show("msvcp100d.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Write("ZoneAgent.log", "Stop => File not found msvcp100d.dll");
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("msvcr100d.dll"))
            {
                MessageBox.Show("msvcr100d.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Write("ZoneAgent.log", "Stop => File not found msvcr100d.dll");
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("asdecr.dll"))
            {
                MessageBox.Show("asdecr.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Write("ZoneAgent.log", "Stop => File not found asdecr.dll");
                ExitZoneAgent();
            }
            LoadConfig();
            lblserverid.Text = Config.SERVER_ID.ToString();
            lblagentid.Text = Config.AGENT_ID.ToString();
            lblzoneport.Text = Config.ZA_PORT.ToString();
            new ZoneAgent();
        }

        /// <summary>
        /// loads values from Svrinfo.ini file to variables of Config class
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                StreamReader sr = new StreamReader("SvrInfo.ini");
                string readLine;
                string[] splt;
                while ((readLine = sr.ReadLine()) != null)
                {
                    splt = readLine.Split('=');
                    switch (splt[0])
                    {
                        case "SERVERID":
                            Config.SERVER_ID = Int16.Parse(splt[1]);
                            break;
                        case "AGENTID":
                            Config.AGENT_ID = Int16.Parse(splt[1]);
                            break;
                        case "IP"://ZA IP
                            Config.ZA_IP = IPAddress.Parse(splt[1]);
                            break;
                        case "PORT"://ZA port
                            Config.ZA_PORT = Int16.Parse(splt[1]);
                            break;
                        case "ID0"://AS ID
                            Config.AS_ID = Int16.Parse(splt[1]);
                            break;
                        case "IP0"://AS IP
                            Config.AS_IP = IPAddress.Parse(splt[1]);
                            break;
                        case "PORT0"://AS port
                            Config.AS_PORT = Int16.Parse(splt[1]);
                            break;
                        case "ID1"://ZS ID
                            Config.ZS_ID = Int16.Parse(splt[1]);
                            break;
                        case "IP1"://ZS P
                            Config.ZS_IP = IPAddress.Parse(splt[1]);
                            break;
                        case "PORT1"://ZS port
                            Config.ZS_PORT = Int16.Parse(splt[1]);
                            break;
                        case "ID2"://BS ID
                            Config.BS_ID = Int16.Parse(splt[1]);
                            break;
                        case "IP2"://BS IP
                            Config.BS_IP = IPAddress.Parse(splt[1]);
                            break;
                        case "PORT2"://BS port 
                            Config.BS_PORT = Int16.Parse(splt[1]);
                            break;
                        case "IP3"://LS IP
                            Config.LS_IP = IPAddress.Parse(splt[1]);
                            break;
                        case "PORT3"://LS port
                            Config.LS_PORT = Int16.Parse(splt[1]);
                            break;
                    }
                }
                sr.Close();
            }
            catch (Exception reader)
            {
                Logger.Write(Logger.GetLoggerFileName("ZoneAgent"), "StreamReader : "+reader.ToString());
                ExitZoneAgent();
            }
        }
        
        /// <summary>
        /// Exits ZoneAgent
        /// </summary>
        private void ExitZoneAgent()
        {
            Application.ExitThread();
            Application.Exit();
        }
        
        /// <summary>
        /// refreshes status of Servers connected/disconnected on form every 2 seconds
        /// </summary>
        private void refreshzonestatus_Tick(object sender, EventArgs e)
        {
            Config.CONNECTED_SERVER_COUNT = 0;
            lstzone.Items.Clear();
            if (Config.isASConnected)
                lstzone.Items.Add(Config.AS_IP + ":" + Config.AS_PORT + ": Connected");
            else
                lstzone.Items.Add(Config.AS_IP + ":" + Config.AS_PORT + ": Disconnected");
            if (Config.isZSConnected)
                lstzone.Items.Add(Config.ZS_IP + ":" + Config.ZS_PORT + ": Connected");
            else
                lstzone.Items.Add(Config.ZS_IP + ":" + Config.ZS_PORT + ": Disconnected");
            if (Config.isBSConnected)
                lstzone.Items.Add(Config.BS_IP + ":" + Config.BS_PORT + ": Connected");
            else
                lstzone.Items.Add(Config.BS_IP + ":" + Config.BS_PORT + ": Disconnected");
            if (Config.isLSConnected)
                lbllssockstatus.Text = "Login Server : Connected";
            else
                lbllssockstatus.Text = "Login Server : Disconnected";
            if (Config.isASConnected)
                Config.CONNECTED_SERVER_COUNT++;
            if (Config.isZSConnected)
                Config.CONNECTED_SERVER_COUNT++;
            if (Config.isBSConnected)
                Config.CONNECTED_SERVER_COUNT++;
            lblconnectedzonecount.Text = Config.CONNECTED_SERVER_COUNT.ToString();
        }
        /// <summary>
        /// Executes when form is closed
        /// </summary>
        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.Write("ZoneAgent.log", "Stop => Closed");
        }
    }
}
