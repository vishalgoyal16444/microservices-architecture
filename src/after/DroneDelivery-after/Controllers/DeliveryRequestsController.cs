using System.Threading.Tasks; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Extensions.Logging; 
using DroneDelivery.Common.Models; 
using DroneDelivery.Common.Services; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
using System; 
namespace DroneDelivery_after.Controllers 
{ 
    [Route("api/[controller]")] 
    [ApiController] 
    public class DeliveryRequestsController : ControllerBase 
    { 
        private readonly IRequestProcessor requestProcessor; 
        private readonly ILogger<DeliveryRequestsController> logger; 
        private readonly TelemetryClient telemetryClient; 
        public DeliveryRequestsController(IRequestProcessor requestProcessor, 
                                    ILogger<DeliveryRequestsController> logger, 
                                    TelemetryClient telemetryClient) 
        { 
            this.requestProcessor = requestProcessor; 
            this.logger = logger; 
            this.telemetryClient = telemetryClient; 
        } 
        // POST api/deliveries 
        [HttpPost()] 
        [ProducesResponseType(typeof(Delivery), 201)] 
        public async Task<IActionResult> Post([FromBody]Delivery delivery) 
        { 
            logger.LogInformation("In Post action: {Delivery}", delivery); 
            var operation = telemetryClient.StartOperation<RequestTelemetry>("PostDeliveryRequest"); 
            try 
            { 
                var success = await requestProcessor.ProcessDeliveryRequestAsync(delivery); 
                if (success) 
                { 
                    logger.LogInformation("Successfully processed delivery request: {DeliveryId}", delivery.DeliveryId); 
                    return CreatedAtRoute("GetDelivery", new { id = delivery.DeliveryId }, delivery); 
                } 
                else 
                { 
                    logger.LogWarning("Failed to process delivery request: {DeliveryId}", delivery.DeliveryId); 
                    return StatusCode(500, "Failed to process delivery request"); 
                } 
            } 
            catch (Exception ex) 
            { 
                telemetryClient.TrackException(ex); 
                logger.LogError(ex, "Error occurred while processing delivery request: {DeliveryId}", delivery.DeliveryId); 
                return StatusCode(500, "Internal server error"); 
            } 
            finally 
            { 
                telemetryClient.StopOperation(operation); 
            } 
        } 
    } 
}