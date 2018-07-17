using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScanQRCodeLogin.Models
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public class UserModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}