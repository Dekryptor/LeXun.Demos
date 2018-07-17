using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABenNET.SuperSocket.AppClient
{
    /// <summary>
    /// 连接服务器参数信息
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// ip地址
        /// </summary>
        public string ip { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int port { get; set; }
    }
}
