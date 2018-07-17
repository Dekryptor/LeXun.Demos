using System;
using System.ComponentModel.DataAnnotations;

namespace ScanQRCodeLogin.Inputs
{
    /// <summary>
    /// 二维码登录认证
    /// </summary>
    [Serializable]
    public class QRCodeVerifyInput
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public QRCodeVerifyInput()
        {
            ConnectionId = Guid.Empty.ToString();
            UUID = Guid.Empty;
            UserName = "";
            //Password = "";
        }

        /// <summary>
        /// 当前回话ID
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ConnectionId { get; set; }

        /// <summary>
        /// 唯一标识符号
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UserName { get; set; }

        ///// <summary>
        ///// 登录密码
        ///// </summary>
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string Password { get; set; }

        ///// <summary>
        ///// 跳转Url
        ///// </summary>
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string returnUrl { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Platform { get; set; }
    }
}