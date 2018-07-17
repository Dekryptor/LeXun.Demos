using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ScanQRCodeLogin.Startup))]
namespace ScanQRCodeLogin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
