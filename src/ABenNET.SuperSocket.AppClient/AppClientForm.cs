using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Linq;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;

namespace ABenNET.SuperSocket.AppClient
{
    public delegate void CbDelegate();
    public delegate void CbDelegate<T1>(T1 obj1);
    public delegate void CbDelegate<T1, T2>(T1 obj1, T2 obj2);
    public partial class AppClientForm : Form
    {
        //private EasyClient<StringPackageInfo> client = null;

        //存活的服务器
        private Dictionary<Machine, EasyClient<StringPackageInfo>> onlineClients = new Dictionary<Machine, EasyClient<StringPackageInfo>>();

        //掉线的服务器
        private Dictionary<Machine, EasyClient<StringPackageInfo>> dieClients = new Dictionary<Machine, EasyClient<StringPackageInfo>>();

        private List<Machine> machines = new List<Machine>();

        private Sunisoft.IrisSkin.SkinEngine SkinEngine = new Sunisoft.IrisSkin.SkinEngine();
        /// <summary>
        /// 心跳检查定时器
        /// </summary>
        private System.Threading.Timer tmrHeartBeat = null;
        /// <summary>
        /// 断线重连定时器
        /// </summary>
        private System.Threading.Timer tmrReConnection = null;
        private int mHeartBeatInterval = 1000 * 10;
        private int mReConnectionInterval = 1000 * 10;

        public AppClientForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            tmrHeartBeat = new System.Threading.Timer(HeartBeatCallBack, null, mHeartBeatInterval, mHeartBeatInterval);
            tmrReConnection = new System.Threading.Timer(ReConnectionCallBack, null, mReConnectionInterval, mReConnectionInterval);
        }

        private void AppClientForm_Load(object sender, EventArgs e)
        {
            //加载皮肤
            List<string> Skins = Directory.GetFiles(Application.StartupPath + @"\IrisSkin4\Skins\", "*.ssk").ToList();
            SkinEngine.SkinFile = Skins[23];//8 18 23 41 
            SkinEngine.Active = true;
        }

        private void HeartBeatCallBack(object state)
        {
            try
            {
                foreach (var item in onlineClients)
                {
                    var client = item.Value;
                    tmrHeartBeat.Change(Timeout.Infinite, Timeout.Infinite);
                    if (client != null && client.IsConnected)
                    {
                        var sbMessage = new StringBuilder();
                        //sbMessage.AppendFormat(string.Format("heartbeat #{0}#\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff")));
                        sbMessage.AppendFormat(string.Format("heartbeat #{0}#\r\n", "心跳数据包:ok"));
                        var data = Encoding.UTF8.GetBytes(sbMessage.ToString());
                        client.Send(new ArraySegment<byte>(data, 0, data.Length));
                    }
                    else {
                        if (client != null)
                        {
                           
                            if (!dieClients.Keys.Contains(item.Key))
                            {
                                dieClients.Add(item.Key, client);
                            }
                        }
                    }

                }

            }
            finally
            {
                tmrHeartBeat.Change(mHeartBeatInterval, mHeartBeatInterval);
            }
        }

        /// <summary>
        /// 掉线自动重连
        /// </summary>
        /// <param name="state"></param>
        private void ReConnectionCallBack(object state)
        {
            try
            {
                tmrReConnection.Change(Timeout.Infinite, Timeout.Infinite);
                foreach (var item in dieClients)
                {
                    var client = item.Value;
                    if (client != null &&
                        client.IsConnected == false)
                    {
                        btnOpen_Click(null, null);
                    }
                }

            }
            finally
            {
                tmrReConnection.Change(mHeartBeatInterval, mHeartBeatInterval);
            }
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
                this.lsvReceviedMsg.Items.Insert(0, item);
            }
        }

        private async void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                machines.Add(new Machine { ip = "127.0.0.1", port = 9000 });
                machines.Add(new Machine { ip = "127.0.0.1", port = 9001 });
                if (onlineClients.Count == 0)
                {
                    foreach (var item in machines)
                    {
                        EasyClient<StringPackageInfo> client = null;
                        try
                        {
                            //初始化并启动客户端引擎（TCP、文本协议

                            client = new EasyClient<StringPackageInfo>()
                            {
                                ReceiveBufferSize = 65535
                            };
                            client.Initialize(new MyTerminatorReceiveFilter());
                            client.Connected += client_Connected;
                            client.NewPackageReceived += Client_NewPackageReceived;
                            client.Closed += client_Closed;
                            client.Error += client_Error;
                            var connected = await client.ConnectAsync(new DnsEndPoint(item.ip, item.port));
                            if (connected && client.IsConnected)
                            {
                                this.BeginInvoke(new CbDelegate(() =>
                                {
                                    this.btnSend.Enabled = true;
                                    this.btnOpen.Enabled = false;
                                    this.btnClose.Enabled = true;
                                    //ShowMessage("连接成功！");
                                    if (!onlineClients.Keys.Contains(item))
                                    {
                                        onlineClients.Add(item, client);
                                    }

                                }));
                            }
                            else
                            {
                                ShowMessage("连接失败！");

                                if (!dieClients.Keys.Contains(item))
                                {
                                    dieClients.Add(item, client);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowMessage(string.Format("连接服务器失败:{0}", ex.Message));
                            if (!dieClients.Keys.Contains(item))
                            {
                                dieClients.Add(item, client);
                            }
                        }
                    }
                }
                else {
                    foreach (var item in dieClients)
                    {
                        EasyClient<StringPackageInfo> client = null;
                        Machine machine = new Machine();
                        try
                        {
                            machine = item.Key;
                            //初始化并启动客户端引擎（TCP、文本协议
                            client = new EasyClient<StringPackageInfo>()
                            {
                                ReceiveBufferSize = 65535
                            };
                            client.Initialize(new MyTerminatorReceiveFilter());
                            client.Connected += client_Connected;
                            client.NewPackageReceived += Client_NewPackageReceived;
                            client.Closed += client_Closed;
                            client.Error += client_Error;
                            var connected = await client.ConnectAsync(new DnsEndPoint(machine.ip, machine.port));
                            if (connected && client.IsConnected)
                            {
                                this.BeginInvoke(new CbDelegate(() =>
                                {
                                    this.btnSend.Enabled = true;
                                    this.btnOpen.Enabled = false;
                                    this.btnClose.Enabled = true;
                                    //ShowMessage("连接成功！");
                                    if (!onlineClients.Keys.Contains(machine))
                                    {
                                        onlineClients.Add(machine, client);
                                    }

                                }));
                            }
                            else
                            {
                                ShowMessage("连接失败！");

                                if (!dieClients.Keys.Contains(machine))
                                {
                                    dieClients.Add(machine, client);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowMessage(string.Format("连接服务器失败:{0}", ex.Message));
                            if (!dieClients.Keys.Contains(machine))
                            {
                                dieClients.Add(machine, client);
                            }
                        }
                    }
                }
                

            }
            catch (Exception ex)
            {
                ShowMessage(string.Format("异常错误:{0}", ex.Message));
            }
        }

        private void Client_NewPackageReceived(object sender, PackageEventArgs<StringPackageInfo> e)
        {
            //e.Package.Key
            ShowMessage(e.Package.Body);
        }

        void client_Error(object sender, global::SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (e.Exception.GetType() == typeof(System.Net.Sockets.SocketException))
            {
                var socketExceptin = e.Exception as System.Net.Sockets.SocketException;
                if (socketExceptin.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    ShowMessage("错误:请先启动AppServer 服务端！");
                }
                else
                    ShowMessage("错误:" + e.Exception.Message);
            }
            else
                ShowMessage("错误:" + e.Exception.Message);
        }

        void client_Closed(object sender, EventArgs e)
        {
            this.BeginInvoke(new CbDelegate(() =>
            {
                this.btnSend.Enabled = false;
                this.btnOpen.Enabled = true;
                ShowMessage("您已经掉线！");
                //this.btnClose_Click(null, null);
            }));
        }

        void client_Connected(object sender, EventArgs e)
        {
            ShowMessage("连接成功！");
        }

        private async void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in onlineClients)
                {
                    var client = item.Value;
                    if (client != null)
                    {
                        await client.Close();
                        client = null;
                    }
                    ShowMessage("连接断开！");
                    this.btnOpen.Enabled = true;
                    this.btnSend.Enabled = false;
                }

            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (var item in onlineClients)
            {
                var client = item.Value;
                if (client != null && client.IsConnected)
                {
                    var sbMessage = new StringBuilder();
                    sbMessage.AppendFormat(string.Format("echo {0}", this.textBox_msg.Text + Environment.NewLine));
                    var data = Encoding.UTF8.GetBytes(sbMessage.ToString());
                    client.Send(new ArraySegment<byte>(data, 0, data.Length));
                    //EchoAsync(client, this.textBox_msg.Text);
                }
            }

        }

        async void EchoAsync(EasyClient<StringPackageInfo> client, string msg)
        {
            await Task.Factory.StartNew(() => 
            {
                msg = string.Format("echo {0}", msg + Environment.NewLine);
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);
                client.Send(new ArraySegment<byte>(bMsg, 0, bMsg.Length));
            });
        }
    }
}
