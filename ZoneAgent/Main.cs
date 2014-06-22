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

        private void btnclose_Click(object sender, EventArgs e)
        {
            //Prompts user if user wants to close ZoneAgent or not
            //if yes program exits else not
            if (MessageBox.Show("Are you sure you want to close ??", "ZoneAgent", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                ExitZoneAgent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if(!File.Exists("SvrInfo.ini"))
            {
                MessageBox.Show("SvrInfo.ini file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("msvcp100d.dll"))
            {
                MessageBox.Show("msvcp100d.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("msvcr100d.dll"))
            {
                MessageBox.Show("msvcr100d.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitZoneAgent();
            }
            //Checks if SvrInfo.ini is available or not.If not availabe exits ZoneAgent
            if (!File.Exists("asdecr.dll"))
            {
                MessageBox.Show("asdecr.dll file missing !!!", "ZoneAgent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitZoneAgent();
            }
            LoadConfig();
            lblserverid.Text = Config.SERVER_ID.ToString();
            lblagentid.Text = Config.AGENT_ID.ToString();
            lblzoneport.Text = Config.ZA_PORT.ToString();
            new ZoneAgent();

        }
        //LoadConfig() loads values from svrinfo.ini file to variables of Config class
        private void LoadConfig()
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
                    case "IP0"://AS IP
                        Config.AS_IP = IPAddress.Parse(splt[1]);
                        break;
                    case "PORT0"://AS port
                        Config.AS_PORT = Int16.Parse(splt[1]);
                        break;
                    case "IP1"://ZS P
                        Config.ZS_IP = IPAddress.Parse(splt[1]);
                        break;
                    case "PORT1"://ZS port
                        Config.ZS_PORT = Int16.Parse(splt[1]);
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
        //ExitZoneAgent() Exits program
        private void ExitZoneAgent()
        {
            Process p = Process.GetCurrentProcess();
            p.Kill();
        }
        //refreshzonestatus_Tick refreshes status of Servers connected/disconnected
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
    }
}
