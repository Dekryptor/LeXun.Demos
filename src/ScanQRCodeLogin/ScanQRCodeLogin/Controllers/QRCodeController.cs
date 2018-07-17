using Newtonsoft.Json;
using ScanQRCodeLogin.Hubs;
using ScanQRCodeLogin.Inputs;
using ScanQRCodeLogin.Models;
using ScanQRCodeLogin.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScanQRCodeLogin.Controllers
{
    public class QRCodeController : Controller
    {
        /// <summary>
        /// 二维码登录认证
        /// </summary>
        /// <returns>
        /// 0:登录成功；-1:参数错误 -2:ConnectionId、UUID、UserName、Password不允许为空-3:ConnectionId回话id不存在-4:UUID输入错误-5:UUID已过期
        /// -6:本UUID已登录-7:登录账号已停用-8:登录账号已删除-9:登录密码输入错误-10:登录账号不存在
        /// </returns>

        //public async Task<OutputDto> QRCodeVerify([FromBody]QRCodeVerifyInput model)
        //[AllowAnonymous]
        [HttpPost]
        public ActionResult QRCodeVerify(QRCodeVerifyInput model)
        {
            OutputDto result = new OutputDto();
            string jsonStr = string.Empty;

            #region 参数验证

            if (model == null)
            {
                result.Result = -1;
                result.Message = "参数错误";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }
            if (string.IsNullOrEmpty(model.ConnectionId) || model.UUID.Equals(Guid.Empty) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            {
                result.Result = -2;
                result.Message = "ConnectionId、UUID、UserName、Password不允许为空";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }

            #endregion 参数验证

            #region 有效性判断

            //验证ConnectionId合法性
            if (!QRCodeAction.QRCodeLists.ContainsKey(model.ConnectionId))
            {
                result.Result = -3;
                result.Message = "ConnectionId回话id不存在";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }
            //验证UUID有效性
            var findCode = QRCodeAction.QRCodeLists[model.ConnectionId];
            if (!model.UUID.Equals(findCode.UUID))
            {
                result.Result = -4;
                result.Message = "UUID输入错误";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }
            if (!findCode.IsValid())
            {
                result.Result = -5;
                result.Message = "UUID已过期";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }
            //if (!findCode.IsValid)
            //{
            //    result.Result = -5;
            //    result.Message = "UUID已过期";
            //    return result;
            //}
            if (findCode.IsLogin)
            {
                result.Result = -6;
                result.Message = "本UUID已登录";
                jsonStr = JsonConvert.SerializeObject(result);
                return Content(jsonStr);
            }

            #endregion 有效性判断

            //LoginUserNameInput loginParam = new LoginUserNameInput
            //{
            //    UserName = model.UserName,
            //    Password = model.Password,
            //    Platform = model.Platform
            //};
            //LoginOutput loginResult = await new SessionController().LoginUserName(loginParam);
            //UserModel userModel = new UserModel
            //{
            //    UserName = model.UserName,
            //    Password = model.Password
            //};
            LoginOutput loginResult = new LoginOutput() { Result = 1 };
            switch (loginResult.Result)
            {
                case -1:
                    result.Result = -7;
                    result.Message = "登录账号已停用";
                    break;

                case -2:
                    result.Result = -8;
                    result.Message = "登录账号已删除";
                    break;

                case -3:
                    result.Result = -9;
                    result.Message = "登录密码输入错误";
                    break;

                case -4:
                    result.Result = -10;
                    result.Message = "登录账号不存在";
                    break;
            }
            if (loginResult.Result > 0) //登录成功，值为AccId
            {
                result.Result = 0;
                findCode.IsLogin = true; //更改登录状态
                //findCode.UserName = model.UserName;
                //findCode.Password = model.Password;
                UserModel userModel = new UserModel
                {
                    UserName = model.UserName,
                    Password = model.Password
                };

                //1.存入Session
                //oc.CurrentSessionUserInfo = new UserModel
                //{
                //    UserName = model.UserName,
                //    Password = model.Password
                //};

                result.Message = "成功登录";
            }
            jsonStr = JsonConvert.SerializeObject(result);
            return Content(jsonStr);
        }
    }
}