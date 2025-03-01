using System; 
using System.Threading.Tasks; 
using Microsoft.Extensions.Logging; 
using DroneDelivery.Common.Models; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
namespace DroneDelivery.Common.Services 
{ 
    public class RequestProcessor : IRequestProcessor 
    { 
        private readonly ILogger<RequestProcessor> logger; 
        private readonly IPackageProcessor packageProcessor; 
        private readonly IDroneScheduler droneScheduler; 
        private readonly IDeliveryRepository deliveryRepository; 
        private readonly TelemetryClient telemetryClient; 
        public RequestProcessor( 
            ILogger<RequestProcessor> logger, 
            IPackageProcessor packageProcessor, 
            IDroneScheduler droneScheduler, 
            IDeliveryRepository deliveryRepository, 
            TelemetryClient telemetryClient) 
        { 
            this.logger = logger; 
            this.packageProcessor = packageProcessor; 
            this.droneScheduler = droneScheduler; 
            this.deliveryRepository = deliveryRepository; 
            this.telemetryClient = telemetryClient; 
        } 
        public async Task<bool> ProcessDeliveryRequestAsync(Delivery deliveryRequest) 
        { 
            logger.LogInformation("Processing delivery request {deliveryId}", deliveryRequest.DeliveryId); 
            var operation = telemetryClient.StartOperation<RequestTelemetry>("ProcessDeliveryRequest"); 
            try 
            { 
                var packageGen = await packageProcessor.CreatePackageAsync(deliveryRequest.PackageInfo).ConfigureAwait(false); 
                if (packageGen != null) 
                { 
                    logger.LogInformation("Generated package {packageId} for delivery {deliveryId}", packageGen.Id, deliveryRequest.DeliveryId); 
                    var droneId = await droneScheduler.GetDroneIdAsync(deliveryRequest).ConfigureAwait(false); 
                    if (droneId != null) 
                    { 
                        logger.LogInformation("Assigned drone {droneId} for delivery {deliveryId}", droneId, deliveryRequest.DeliveryId); 
                        var success = await deliveryRepository.ScheduleDeliveryAsync(deliveryRequest, droneId).ConfigureAwait(false); 
                        if (success) 
                        { 
                            logger.LogInformation("Completed delivery {deliveryId}", deliveryRequest.DeliveryId); 
                            return true; 
                        } 
                        else 
                        { 
                            logger.LogError("Failed delivery for request {deliveryId}", deliveryRequest.DeliveryId); 
                        } 
                    } 
                } 
            } 
            catch (Exception e) 
            { 
                telemetryClient.TrackException(e); 
                logger.LogError(e, "Error processing delivery request {deliveryId}", deliveryRequest.DeliveryId); 
                throw; 
            } 
            finally 
            { 
                telemetryClient.StopOperation(operation); 
            } 
            return false; 
        } 
    } 
}