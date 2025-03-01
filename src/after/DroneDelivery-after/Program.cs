using Microsoft.AspNetCore.Hosting; 
using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Logging; 
using Microsoft.ApplicationInsights.Extensibility; 
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse; 
namespace DroneDelivery_after 
{ 
    public class Program 
    { 
        public static void Main(string[] args) 
        { 
            CreateWebHostBuilder(args).Build().Run(); 
        } 
        public static IHostBuilder CreateWebHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args) 
                .ConfigureWebHostDefaults(webBuilder => 
                { 
                    webBuilder.UseStartup<Startup>(); 
                }) 
                .ConfigureLogging(logging => 
                { 
                    logging.AddApplicationInsights(); 
                }) 
                .ConfigureServices(services => 
                { 
                    services.AddSingleton<ITelemetryModule, QuickPulseTelemetryModule>(); 
                }); 
    } 
}