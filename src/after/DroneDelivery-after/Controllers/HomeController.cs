using Microsoft.AspNetCore.Mvc; 
using System; 
using System.Diagnostics; 
using System.Net.Http; 
using System.Threading.Tasks; 
using DroneDelivery.Common.Models; 
using Newtonsoft.Json; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
namespace DroneDelivery_after.Controllers 
{ 
    public class HomeController : Controller 
    { 
        private const int RequestCount = 100; 
        private readonly IHttpClientFactory httpClientFactory; 
        private readonly TelemetryClient telemetryClient; 
        private Delivery payload; 
        public HomeController(IHttpClientFactory httpClientFactory, TelemetryClient telemetryClient) 
        { 
            this.httpClientFactory = httpClientFactory; 
            this.telemetryClient = telemetryClient; 
            this.payload = new Delivery() 
            { 
                DeliveryId = "delivery123", 
                OwnerId = "owner123", 
                PickupLocation = "pickup", 
                DropoffLocation = "dropoff", 
                PickupTime = DateTime.Now.AddDays(3), 
                Deadline = "deadline", 
                Expedited = true, 
                ConfirmationRequired = ConfirmationRequired.None, 
                PackageInfo = new PackageInfo() 
                { 
                    PackageId = "package1234567", 
                    Size = ContainerSize.Small, 
                    Weight = 0, 
                    Tag = "tag" 
                } 
            }; 
        } 
        public IActionResult Index() 
        { 
            return View(); 
        } 
        [HttpPost()] 
        [Route("/[controller]/SendRequests")] 
        public async Task<IActionResult> SendRequests() 
        { 
            var stopWatch = new Stopwatch(); 
            stopWatch.Start(); 
            var httpClient = httpClientFactory.CreateClient(); 
            var urlBuilder = new UriBuilder(this.Request.Scheme, this.Request.Host.Host); 
            httpClient.BaseAddress = urlBuilder.Uri; 
            var tasks = new Task[RequestCount]; 
            for (int i = 0; i < RequestCount; i++) 
            { 
                string jsonString = JsonConvert.SerializeObject(payload); 
                var httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json"); 
                tasks[i] = httpClient.PostAsync("/api/DeliveryRequests", httpContent); 
            } 
            try 
            { 
                await Task.WhenAll(tasks); 
            } 
            catch (Exception ex) 
            { 
                telemetryClient.TrackException(ex); 
                ViewBag.Message = "An error occurred while sending requests."; 
                return View(); 
            } 
            stopWatch.Stop(); 
            telemetryClient.TrackMetric("RequestProcessingTime", stopWatch.ElapsedMilliseconds); 
            ViewBag.Message = $"{RequestCount} messages sent in {stopWatch.Elapsed.Seconds} seconds"; 
            return View(); 
        } 
    } 
}