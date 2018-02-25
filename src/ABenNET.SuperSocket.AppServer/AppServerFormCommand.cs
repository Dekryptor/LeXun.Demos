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
using SuperSocket.SocketEngine;
using ABenNET.SuperSocket.AppServer2Ex.Command;

namespace ABenNET.SuperSocket.AppServer2Ex //由于项目的命名空间与SuperSocket的命令空间有冲突，故暂时弄了一个别名。
{
    /// <summary>
    ///  SuperSocket 自定义命令 (Command)类，来满足处理来自客户端的请求的。
    ///  
    /// </summary>
    public partial class AppServerFormCommand : Form
    {
        public delegate void CbDelegate();
        public delegate void CbDelegate<T1>(T1 obj1);
        public delegate void CbDelegate<T1, T2>(T1 obj1, T2 obj2);
        private MyAppServer tcpServerEngine;
        private Sunisoft.IrisSkin.SkinEngine SkinEngine = new Sunisoft.IrisSkin.SkinEngine();
        public static ConcurrentDictionary<string, MyAppSession> mOnLineConnections = new ConcurrentDictionary<string, MyAppSession>();

        public AppServerFormCommand()
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
                //方法一、采用当前应用程序中的【App.config】文件。
                //var bootstrap = BootstrapFactory.CreateBootstrap();

                //=>方法二、采用自定义独立【SuperSocket.config】配置文件
                var bootstrap = BootstrapFactory.CreateBootstrapFromConfigFile("SuperSocket.config");
                if (!bootstrap.Initialize())
                {
                    ShowMessage("Failed to initialize!");
                    return;
                }
                StartResult startResult = bootstrap.Start();
                if (startResult == StartResult.Success)
                {
                    this.ShowMessage("服务启动成功！");
                    tcpServerEngine = bootstrap.AppServers.Cast<MyAppServer>().FirstOrDefault();
                    if (tcpServerEngine != null)
                    {
                        tcpServerEngine.NewSessionConnected += tcpServerEngine_NewSessionConnected;

                        /* 同时你要移除请求处理方法的注册，因为它和命令不能同时被支持：
                         * http://docs.supersocket.net/v1-6/zh-CN/A-Telnet-Example
                         * 如果你的服务器端包含有很多复杂的业务逻辑，这样的switch/case代码将会很长而且非常难看，并且没有遵循OOD的原则。 
                         * 在这种情况下，SuperSocket提供了一个让你在多个独立的类中处理各自不同的请求的命令框架。
                         */
                        //tcpServerEngine.NewRequestReceived += tcpServerEngine_NewRequestReceived;
                        ECHO.ECHOMessageRecevied += ECHO_ECHOMessageRecevied;
                        tcpServerEngine.SessionClosed += tcpServerEngine_SessionClosed;
                        this.ShowListenStatus();
                    }
                    else
                        this.ShowMessage("请检查配置文件中是否又可用的服务信息！");
                }
                else
                    this.ShowMessage("服务启动失败！");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }



        void tcpServerEngine_SessionClosed(MyAppSession session, global::SuperSocket.SocketBase.CloseReason value)
        {
            this.ShowMessage(session.RemoteEndPoint, "下线");
            MyAppSession outAppSession;
            mOnLineConnections.TryRemove(session.SessionID, out outAppSession);
            this.ShowConnectionCount(mOnLineConnections.Count);
            //this.ShowConnectionCount(this.tcpServerEngine.SessionCount);
        }

        void ECHO_ECHOMessageRecevied(object sender, ECHOEventArgs e)
        {
            this.ShowMessage(e.AppSession.RemoteEndPoint, e.StringRequestInfo.Body);
        }

        void tcpServerEngine_NewRequestReceived(MyAppSession session, global::SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            /*
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
             */
        }

        void tcpServerEngine_NewSessionConnected(MyAppSession session)
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
            //IList<MyAppSession> list = this.tcpServerEngine.GetAllSessions().ToList();
            IList<MyAppSession> list = this.tcpServerEngine.GetSessions(s => s.Connected == true).ToList();
            this.cboClients.DisplayMember = "RemoteEndPoint";
            this.cboClients.ValueMember = "SessionID";
            this.cboClients.DataSource = list;
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            try
            {
                MyAppSession appSession = (MyAppSession)this.cboClients.SelectedItem;
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
