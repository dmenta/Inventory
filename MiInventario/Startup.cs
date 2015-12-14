using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MiInventario.Startup))]
namespace MiInventario
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
