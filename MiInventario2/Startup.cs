using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MiInventario2.Startup))]
namespace MiInventario2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
