using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IEEECheckin.ASPDocs.Startup))]
namespace IEEECheckin.ASPDocs
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
