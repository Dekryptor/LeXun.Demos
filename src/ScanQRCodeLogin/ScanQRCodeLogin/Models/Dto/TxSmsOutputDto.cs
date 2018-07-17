using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ScanQRCodeLogin.Outputs
{
    /// <summary>
    /// 输出基类
    /// </summary>
    //[ModelBinder(typeof(EmptyStringModelBinder))]
    public class OutputDto
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OutputDto()
        {
            Result = 0; //默认为0，表示初始值或正确
            Message = "";
            ReturnUrl = "";
        }

        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("Result")]
        public int Result { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [JsonProperty("Message")]
        public string Message { get; set; }

        /// <summary>
        /// 跳转Url
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [JsonProperty("ReturnUrl")]
        public string ReturnUrl { get; set; }

    }
}