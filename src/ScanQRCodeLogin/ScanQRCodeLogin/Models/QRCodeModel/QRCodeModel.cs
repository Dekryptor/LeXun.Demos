using ScanQRCodeLogin.Hubs;
using ScanQRCodeLogin.Hubs;
using System;
using System.Text;

namespace ScanQRCodeLogin.Models
{
    /// <summary>
    /// 二维码实体
    /// </summary>
    [Serializable]
    public class QRCodeModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public QRCodeModel()
        {
            UUID = Guid.NewGuid();
            CreateDate = DateTime.Now;
            IsLogin = false;
            //UserName = Password = "";
            TargetUrl = "";
            QRCodeType = QRCodeTypes.Other;
        }

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        //public QRCodeModel(string connectionid)
        //{
        //    UUID = Guid.NewGuid();
        //    CreateDate = DateTime.Now;
        //    IsLogin = false;
        //    UserName = Password = null;
        //    Connectionid = connectionid;
        //    QRCodeType = QRCodeTypes.Other;
        //}

        /// <summary>
        /// 构造函数
        /// </summary>
        public QRCodeModel(QRCodeTypes qRCodeType)
        {
            UUID = Guid.NewGuid();
            CreateDate = DateTime.Now;
            IsLogin = false;
            TargetUrl = "";
            //UserName = Password = "";
            QRCodeType = qRCodeType;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public QRCodeModel(QRCodeTypes qRCodeType, string targetUrl)
        {
            UUID = Guid.NewGuid();
            CreateDate = DateTime.Now;
            IsLogin = false;
            TargetUrl = targetUrl;
            //UserName = Password = "";
            QRCodeType = qRCodeType;
        }

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        //public QRCodeModel(string connectionid, QRCodeTypes qRCodeType)
        //{
        //    UUID = Guid.NewGuid();
        //    CreateDate = DateTime.Now;
        //    IsLogin = false;
        //    UserName = Password = null;
        //    Connectionid = connectionid;
        //    QRCodeType = qRCodeType;
        //}

        ///// <summary>
        ///// Connectionid
        ///// </summary>
        //public string Connectionid { get; set; }

        /// <summary>
        /// 验证码类型
        /// </summary>
        public QRCodeTypes QRCodeType { get; set; }

        /// <summary>
        /// 唯一标识符号
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogin { get; set; }

        /// <summary>
        /// 访问目标地址
        /// </summary>
        public string TargetUrl { get; set; }


        ///// <summary>
        ///// 用户账号
        ///// </summary>
        //public string UserName { get; set; }

        ///// <summary>
        ///// 登录密码
        ///// </summary>
        //public string Password { get; set; }

        /// <summary>
        /// 判断是否有效，有效时间为190秒
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            TimeSpan ts = DateTime.Now - CreateDate;
            return ts.TotalSeconds <= 180; //180秒（3分钟）之内有效
        }



        //public bool _isValid = isGQ();

        //public static bool isGQ()
        //{
        //    TimeSpan ts = DateTime.Now - CreateDate;
        //    return ts.TotalSeconds <= 180; //180秒（3分钟）之内有效
        //}

        ///// <summary>
        ///// 判断是否有效，有效时间为190秒
        ///// </summary>
        ///// <returns></returns>
        //public bool IsValid
        //{
        //    get
        //    {
        //        TimeSpan ts = DateTime.Now - CreateDate;
        //        return ts.TotalSeconds <= 180; //180秒（3分钟）之内有效
        //    }
        //    set {
        //        _isValid = value;
        //    }
        //}

        /// <summary>
        /// 转换为json字符串
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public string ToJson(string connectionId, QRCodeTypes qrCodeType, string targetUrl)
        {
            StringBuilder result = new StringBuilder();
            result.Append("{");
            result.AppendFormat("\"connectionid\":\"{0}\",", connectionId);
            result.AppendFormat("\"uuid\":\"{0}\",", UUID);

            result.AppendFormat("\"targetUrl\":\"{0}\",", targetUrl);

            //result.AppendFormat("\"userName\":\"{0}\",", UserName);
            //result.AppendFormat("\"password\":\"{0}\",", Password);

            result.AppendFormat("\"qrCodeType\":\"{0}\",", qrCodeType);

            result.AppendFormat("\"islogin\":{0},", IsLogin.ToString().ToLower().Equals("false") ? 0 : 1);
            result.AppendFormat("\"isvalid\":{0}", IsValid().ToString().ToLower().Equals("false") ? 0 : 1);

            //result.AppendFormat("\"createdate\":\"{0}\"", CreateDate.ToString(CultureInfo.InvariantCulture).ToLower());
            //result.AppendFormat("\"nowdate\":\"{0}\"", DateTime.Now.ToString(CultureInfo.InvariantCulture).ToLower());
            result.Append("}");
            return result.ToString();

            //var isLogin = IsLogin.ToString().ToLower().Equals("false") ? false  : true;
            //var isvalid = IsValid().ToString().ToLower().Equals("false") ? false : true;
            //var obj = new JsonRes()
            //{
            //    Connectionid = connectionId,
            //    QRCodeType = qrCodeType,
            //    UUID = UUID,
            //    IsLogin = isLogin,
            //    IsValid = isvalid,
            //    UserName = UserName,
            //    Password = Password
            //};
            //var jsonStr = JsonHelper.JsonSerialize.ObjectToJsonByDCJS(obj);
            //return jsonStr;
        }


        //private class JsonRes
        //{
        //    /// <summary>
        //    /// Connectionid
        //    /// </summary>
        //    public string Connectionid { get; set; }

        //    /// <summary>
        //    /// 验证码类型
        //    /// </summary>
        //    public QRCodeTypes QRCodeType { get; set; }

        //    /// <summary>
        //    /// 唯一标识符号
        //    /// </summary>
        //    public Guid UUID { get; set; }

        //    ///// <summary>
        //    ///// 创建时间
        //    ///// </summary>
        //    //public DateTime CreateDate { get; set; }

        //    /// <summary>
        //    /// 是否已登录
        //    /// </summary>
        //    public bool IsLogin { get; set; }


        //    /// <summary>
        //    /// 用户账号
        //    /// </summary>
        //    public string UserName { get; set; }

        //    /// <summary>
        //    /// 登录密码
        //    /// </summary>
        //    public string Password { get; set; }

        //    /// <summary>
        //    /// 是否过期
        //    /// </summary>
        //    public bool IsValid { get; set; }
            
        //}
    }
}