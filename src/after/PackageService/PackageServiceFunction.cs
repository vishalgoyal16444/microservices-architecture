using System.Threading.Tasks; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Azure.WebJobs; 
using Microsoft.Azure.WebJobs.Extensions.Http; 
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Logging; 
using DroneDelivery.Common.Services; 
using System; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
namespace PackageService 
{ 
    public static class PackageServiceFunction 
    { 
        private static readonly TelemetryClient telemetryClient = new TelemetryClient(); 
        [FunctionName("PackageServiceFunction")] 
        public static Task<IActionResult> Run( 
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "packages/{id}")] HttpRequest req, 
            string id, ILogger log) 
        { 
            log.LogInformation("C# HTTP trigger function processed a request."); 
            var operation = telemetryClient.StartOperation<RequestTelemetry>("PackageServiceFunction"); 
            try 
            { 
                // Uses common data store e.g., SQL Azure tables 
                log.LogInformation("Starting utility work simulation."); 
                Utility.DoWork(10);  // Reduced work units for efficiency 
                log.LogInformation("Utility work simulation completed successfully."); 
            } 
            catch (Exception ex) 
            { 
                telemetryClient.TrackException(ex); 
                log.LogError($"An error occurred during utility work simulation: {ex.Message}", ex); 
                return Task.FromResult((IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)); 
            } 
            finally 
            { 
                telemetryClient.StopOperation(operation); 
            } 
            return Task.FromResult((IActionResult)new CreatedResult("http://example.com", null)); 
        } 
    } 
}