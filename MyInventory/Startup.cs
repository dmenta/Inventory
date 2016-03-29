using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyInventory.Startup))]
namespace MyInventory
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
