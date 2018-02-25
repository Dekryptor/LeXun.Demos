using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Linq;
using System.IO;
using SuperSocket.SocketBase;
using System.Collections.Concurrent;
using SuperSocket.SocketBase.Config;

namespace ABenNET.SuperSocket.AppServer2Ex //由于项目的命名空间与SuperSocket的命令空间有冲突，故暂时弄了一个别名。
{
    /// <summary>
    /// SuperSocket服务器几种配置方式介绍
    /// 1、通过编码方式实现SuperSocket服务端配置信息。
    /// </summary>
    public partial class AppServerForm : Form
    {
        public delegate void CbDelegate();
        public delegate void CbDelegate<T1>(T1 obj1);
        public delegate void CbDelegate<T1, T2>(T1 obj1, T2 obj2);
        private AppServer tcpServerEngine;
        private Sunisoft.IrisSkin.SkinEngine SkinEngine = new Sunisoft.IrisSkin.SkinEngine();
        public static ConcurrentDictionary<string, AppSession> mOnLineConnections = new ConcurrentDictionary<string, AppSession>();

        public AppServerForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void AppServerForm_Load(object sender, EventArgs e)
        {
            //加载皮肤
            List<string> Skins = Directory.GetFiles(Application.StartupPath + @"\IrisSkin4\Skins\", "*.ssk").ToList();
            SkinEngine.SkinFile = Skins[41];//8 18 23 41 
            SkinEngine.Active = true;
        }

        private void button_StartListen_Click(object sender, EventArgs e)
        {
            try
            {
                #region [=>自定义服务配置]
                IServerConfig serverConfig = new ServerConfig
                {
                    Name = "ABenNET.SuperSocket.AppServer",// "AgileServer",//服务器实例的名称
                    //ServerType = "AgileServer.Socket.TelnetServer, AgileServer.Socket",
                    Ip = "Any",//Any - 所有的IPv4地址 IPv6Any - 所有的IPv6地址
                    Mode = SocketMode.Tcp,//服务器运行的模式, Tcp (默认) 或者 Udp
                    Port = int.Parse(this.textBox_port.Text),//服务器监听的端口
                    SendingQueueSize = 5000,//发送队列最大长度, 默认值为5
                    MaxConnectionNumber = 5000,//可允许连接的最大连接数
                    LogCommand = false,//是否记录命令执行的记录
                    LogBasicSessionActivity = false,//是否记录session的基本活动，如连接和断开
                    LogAllSocketException = false,//是否记录所有Socket异常和错误
                    //Security = "tls",//Empty, Tls, Ssl3. Socket服务器所采用的传输层加密协议
                    MaxRequestLength = 5000,//最大允许的请求长度，默认值为1024
                    TextEncoding = "UTF-8",//文本的默认编码，默认值是 ASCII，（###改成UTF-8,否则的话中文会出现乱码）
                    KeepAliveTime = 60,//网络连接正常情况下的keep alive数据的发送间隔, 默认值为 600, 单位为秒
                    KeepAliveInterval = 60,//Keep alive失败之后, keep alive探测包的发送间隔，默认值为 60, 单位为秒
                    ClearIdleSession = true, // 是否定时清空空闲会话，默认值是 false;（###如果开启定时60秒钟情况闲置的连接，为了保证客户端正常不掉线连接到服务器，故我们需要设置10秒的心跳数据包检查。也就是说清除闲置的时间必须大于心跳数据包的间隔时间，否则就会出现服务端主动踢掉闲置的TCP客户端连接。）
                    ClearIdleSessionInterval = 60,//: 清空空闲会话的时间间隔, 默认值是120, 单位为秒;
                    SyncSend = true,//:是否启用同步发送模式, 默认值: false;
                };
                var rootConfig = new RootConfig()
                {
                    MaxWorkingThreads = 5000,//线程池最大工作线程数量
                    MinWorkingThreads = 10,// 线程池最小工作线程数量;
                    MaxCompletionPortThreads = 5000,//线程池最大完成端口线程数量;
                    MinCompletionPortThreads = 10,// 线程池最小完成端口线程数量;
                    DisablePerformanceDataCollector = true,// 是否禁用性能数据采集;
                    PerformanceDataCollectInterval = 60,// 性能数据采集频率 (单位为秒, 默认值: 60);
                    LogFactory = "ConsoleLogFactory",//默认logFactory的名字
                    Isolation = IsolationMode.AppDomain// 服务器实例隔离级别                
                };
                #endregion
                tcpServerEngine = new AppServer();
                if (tcpServerEngine.Setup(rootConfig: rootConfig, config: serverConfig))
                {
                    if (tcpServerEngine.Start())
                    {
                        tcpServerEngine.NewSessionConnected += tcpServerEngine_NewSessionConnected;
                        tcpServerEngine.NewRequestReceived += tcpServerEngine_NewRequestReceived;
                        tcpServerEngine.SessionClosed += tcpServerEngine_SessionClosed;
                        this.ShowListenStatus();
                        this.ShowMessage("服务启动成功！");
                    }
                    else
                    {
                        this.ShowMessage("服务启动失败！");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        void tcpServerEngine_SessionClosed(AppSession session, global::SuperSocket.SocketBase.CloseReason value)
        {
            this.ShowMessage(session.RemoteEndPoint, "下线");
            AppSession outAppSession;
            mOnLineConnections.TryRemove(session.SessionID, out outAppSession);
            this.ShowConnectionCount(mOnLineConnections.Count);
            //this.ShowConnectionCount(this.tcpServerEngine.SessionCount);
        }

        void tcpServerEngine_NewRequestReceived(AppSession session, global::SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            switch (requestInfo.Key)
            {
                case "echo":
                    this.ShowMessage(session.RemoteEndPoint, requestInfo.Body);
                    break;
                case "heartbeat":
                    this.ShowMessage(session.RemoteEndPoint, requestInfo.Body);
                    string msg = string.Format("push {0}", requestInfo.Body + Environment.NewLine);//一定要加上分隔符
                    byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                    session.Send(new ArraySegment<byte>(bMsg, 0, bMsg.Length));
                    break;
                default:
                    this.ShowMessage(session.RemoteEndPoint, "未知的指令（error unknow command）");
                    break;
            }
        }

        void tcpServerEngine_NewSessionConnected(AppSession session)
        {
            this.ShowMessage(session.RemoteEndPoint, "上线");
            mOnLineConnections.TryAdd(session.SessionID, session);
            this.ShowConnectionCount(mOnLineConnections.Count);
            //this.ShowConnectionCount(this.tcpServerEngine.SessionCount);
        }

        private void ShowListenStatus()
        {
            this.button_StartListen.Enabled = !(this.tcpServerEngine.State == ServerState.Running);
            this.button_Close.Enabled = (this.tcpServerEngine.State == ServerState.Running);
            this.textBox_port.ReadOnly = !(this.tcpServerEngine.State == ServerState.Running);
            this.button_Send.Enabled = (this.tcpServerEngine.State == ServerState.Running);
        }

        private void cboClients_DropDown(object sender, EventArgs e)
        {
            if (tcpServerEngine == null)
            {
                this.cboClients.DataSource = null;
                return;
            }
            //IList<AppSession> list = this.tcpServerEngine.GetAllSessions().ToList();
            IList<AppSession> list = this.tcpServerEngine.GetSessions(s => s.Connected == true).ToList();
            this.cboClients.DisplayMember = "RemoteEndPoint";
            this.cboClients.ValueMember = "SessionID";
            this.cboClients.DataSource = list;
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            try
            {
                AppSession appSession = (AppSession)this.cboClients.SelectedItem;
                if (appSession == null)
                {
                    ShowMessage("没有选中任何在线客户端！");
                    return;
                }
                if (!appSession.Connected)
                {
                    ShowMessage("目标客户端不在线！");
                    return;
                }
                string msg = string.Format("push {0}", this.textBox_msg.Text + Environment.NewLine);//一定要加上分隔符
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                //this.tcpServerEngine.GetSessionByID(appSession.SessionID).Send(new ArraySegment<byte>(bMsg, 0, bMsg.Length));
                appSession.Send(new ArraySegment<byte>(bMsg, 0, bMsg.Length));
            }
            catch (Exception ee)
            {
                ShowMessage(ee.Message);
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            if (tcpServerEngine == null) return;
            this.tcpServerEngine.Stop();
            this.ShowListenStatus();
            this.textBox_port.ReadOnly = false;
            this.textBox_port.SelectAll();
            this.textBox_port.Focus();
            this.tcpServerEngine.Dispose();
            this.tcpServerEngine = null;
            this.button_StartListen.Enabled = true;
        }

        private void ShowMessage(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<string>(this.ShowMessage), msg);
            }
            else
            {
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), msg });
                this.lsvRevicedMsg.Items.Insert(0, item);
            }
        }

        private void ShowMessage(IPEndPoint client, string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowMessage), client, msg);
            }
            else
            {
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), client.ToString(), msg });
                this.lsvRevicedMsg.Items.Insert(0, item);
            }
        }

        private void ShowConnectionCount(int clientCount)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<int>(this.ShowConnectionCount), clientCount);
            }
            else
            {
                this.toolStripLabel_clientCount.Text = "在线数量： " + clientCount.ToString();
            }
        }
    }
}
