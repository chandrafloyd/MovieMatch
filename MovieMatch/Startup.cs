using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MovieMatch.Startup))]
namespace MovieMatch
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
