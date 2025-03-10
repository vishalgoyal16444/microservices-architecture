using System; 
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Extensions.Logging; 
using DroneDelivery.Common.Models; 
using DroneDelivery.Common.Services; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
namespace DroneDelivery_after.Controllers 
{ 
    [Route("api/[controller]")] 
    [ApiController] 
    public class DeliveriesController : ControllerBase 
    { 
        private readonly IDeliveryRepository deliveryRepository; 
        private readonly ILogger<DeliveriesController> logger; 
        private readonly TelemetryClient telemetryClient; 
        public DeliveriesController(IDeliveryRepository deliveryRepository, 
                                    ILogger<DeliveriesController> logger, 
                                    TelemetryClient telemetryClient) 
        { 
            this.deliveryRepository = deliveryRepository; 
            this.logger = logger; 
            this.telemetryClient = telemetryClient; 
        } 
        // GET api/deliveries/5 
        [Route("/api/[controller]/{id}", Name = "GetDelivery")] 
        [HttpGet] 
        [ProducesResponseType(typeof(Delivery), 200)] 
        public async Task<IActionResult> Get(string id) 
        { 
            logger.LogInformation("In Get action with id: {Id}", id); 
            var operation = telemetryClient.StartOperation<RequestTelemetry>("GetDelivery"); 
            try 
            { 
                var delivery = await deliveryRepository.GetAsync(id); 
                if (delivery == null) 
                { 
                    logger.LogDebug("Delivery id: {Id} not found", id); 
                    return NotFound(); 
                } 
                return Ok(delivery); 
            } 
            catch (Exception ex) 
            { 
                telemetryClient.TrackException(ex); 
                logger.LogError(ex, "Error occurred in Get action with id: {Id}", id); 
                return StatusCode(500, "Internal server error"); 
            } 
            finally 
            { 
                telemetryClient.StopOperation(operation); 
            } 
        } 
        // GET api/deliveries/5/status 
        [Route("/api/[controller]/{id}/status")] 
        [HttpGet] 
        [ProducesResponseType(typeof(DeliveryStatus), 200)] 
        public async Task<IActionResult> GetStatus(string id) 
        { 
            logger.LogInformation("In GetStatus action with id: {Id}", id); 
            var operation = telemetryClient.StartOperation<RequestTelemetry>("GetDeliveryStatus"); 
            try 
            { 
                var delivery = await deliveryRepository.GetAsync(id); 
                if (delivery == null) 
                { 
                    logger.LogDebug("Delivery id: {Id} not found", id); 
                    return NotFound(); 
                } 
                var status = new DeliveryStatus(DeliveryStage.HeadedToDropoff, new Location(0, 0, 0), DateTime.Now.AddMinutes(10).ToString(), DateTime.Now.AddHours(1).ToString()); 
                return Ok(status); 
            } 
            catch (Exception ex) 
            { 
                telemetryClient.TrackException(ex); 
                logger.LogError(ex, "Error occurred in GetStatus action with id: {Id}", id); 
                return StatusCode(500, "Internal server error"); 
            } 
            finally 
            { 
                telemetryClient.StopOperation(operation); 
            } 
        } 
    } 
}