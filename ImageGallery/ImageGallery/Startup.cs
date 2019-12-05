using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ImageGallery.Startup))]
namespace ImageGallery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
