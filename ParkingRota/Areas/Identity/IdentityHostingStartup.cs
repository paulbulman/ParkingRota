using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ParkingRota.Areas.Identity.IdentityHostingStartup))]
namespace ParkingRota.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder.ConfigureServices((context, services) => { });
    }
}