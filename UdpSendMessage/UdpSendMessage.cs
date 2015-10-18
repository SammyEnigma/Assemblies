using System.Data.SqlTypes;
using System.Net.Sockets;
using static System.Text.Encoding;

namespace UdpSendMessage
{
    public class UdpSendMessage
    {
        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void SendMessage(SqlString HostName, SqlInt32 Port, SqlString Message, out SqlBoolean Result)
        {
            using (var udpClient = new UdpClient())
            { 
                try
                {
                    udpClient.Connect(HostName.Value, Port.Value);
                    var sendBytes = UTF8.GetBytes(Message.Value);
                    udpClient.Send(sendBytes, sendBytes.Length);
                    udpClient.Close();
                }
                catch
                {
                    Result = false;
                    return;
                }
            Result = true;
}
        }
    }
}
