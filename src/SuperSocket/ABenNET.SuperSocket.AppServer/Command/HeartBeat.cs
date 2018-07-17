using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABenNET.SuperSocket.AppServer2Ex.Command
{
    public class HeartBeat : CommandBase<MyAppSession, StringRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "heartbeat";
            }
        }

        public override void ExecuteCommand(MyAppSession session, StringRequestInfo requestInfo)
        {
            string msg = string.Format("push {0}", requestInfo.Body + Environment.NewLine);//一定要加上分隔符
            byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
            session.Send(new ArraySegment<byte>(bMsg, 0, bMsg.Length));
        }
    }
}
