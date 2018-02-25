using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABenNET.SuperSocket.AppServer2Ex.Command
{
    public class ECHO : CommandBase<MyAppSession, StringRequestInfo>
    {
        /// <summary>
        /// 自定义echo事件
        /// </summary>
        public static event EventHandler<ECHOEventArgs> ECHOMessageRecevied;

        public override string Name
        {
            get
            {
                return "echo";
            }
        }

        public override void ExecuteCommand(MyAppSession session, StringRequestInfo requestInfo)
        {
            //session.Send(requestInfo.Body);
            if (ECHOMessageRecevied != null)
            {
                ECHOMessageRecevied(session, new ECHOEventArgs(session,requestInfo));
            }
        }
    }

    public class ECHOEventArgs : EventArgs
    {
        public MyAppSession AppSession { get; set; }
        public StringRequestInfo StringRequestInfo { get; set; }

        public ECHOEventArgs(MyAppSession AppSession, StringRequestInfo StringRequestInfo)
        {
            this.AppSession = AppSession;
            this.StringRequestInfo = StringRequestInfo;
        }
    }
}
