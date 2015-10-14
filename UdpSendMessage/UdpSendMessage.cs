using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net.Sockets;

namespace UdpSendMessage
{
    public class UdpSendMessage
    {
        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void SendMessage(SqlString HostName, SqlInt32 Port, SqlString Message, out SqlBoolean Result)
        {
            UdpClient udpClient = new UdpClient();
            try
            {
                udpClient.Connect(HostName.Value,Port.Value);
                Byte[] sendBytes = Encoding.UTF8.GetBytes(Message.Value);
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
