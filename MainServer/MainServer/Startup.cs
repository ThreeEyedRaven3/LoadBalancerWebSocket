using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MainServer.Startup))]
namespace MainServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
