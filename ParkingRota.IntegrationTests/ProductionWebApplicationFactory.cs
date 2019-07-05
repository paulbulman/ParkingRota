namespace ParkingRota.IntegrationTests
{
    using Microsoft.AspNetCore.Hosting;

    public class ProductionWebApplicationFactory<TProgram> : DatabaseWebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseSetting("Environment", "Production");
        }
    }
}