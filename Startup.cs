using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InzV3.Startup))]
namespace InzV3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
