using System.Net.Sockets;
namespace ZoneAgent
{
    //Class to save player information
    class PlayerInfo
    {
        public Client Client { get; set; }
        public string Account { get; set; }
        public string Time { get; set; }
        public bool Prepared { get; set; }
        public bool ZoneStatus { get; set; }
        public PlayerInfo(string account, string time, bool prepared, bool zoneStatus)
        {
            Account = account;
            Time = time;
            Prepared = prepared;
            ZoneStatus = zoneStatus;

        }
    }
}
