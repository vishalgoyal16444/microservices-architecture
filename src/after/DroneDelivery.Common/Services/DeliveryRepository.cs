using System; 
using System.Threading.Tasks; 
using DroneDelivery.Common.Models; 
using Microsoft.Extensions.Caching.Memory; 
using Microsoft.Extensions.Logging; 
using Microsoft.ApplicationInsights; 
using Microsoft.ApplicationInsights.DataContracts; 
namespace DroneDelivery.Common.Services 
{ 
    public class DeliveryRepository : IDeliveryRepository 
    { 
        private readonly IMemoryCache _cache; 
        private readonly ILogger<DeliveryRepository> _logger; 
        private readonly TelemetryClient _telemetryClient; 
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5); 
        public DeliveryRepository(IMemoryCache cache, ILogger<DeliveryRepository> logger, TelemetryClient telemetryClient) 
        { 
            _cache = cache; 
            _logger = logger; 
            _telemetryClient = telemetryClient; 
        } 
        public async Task<Delivery> GetAsync(string id) 
        { 
            var operation = _telemetryClient.StartOperation<RequestTelemetry>("GetDeliveryAsync"); 
            try 
            { 
                if (_cache.TryGetValue(id, out Delivery cachedDelivery)) 
                { 
                    _logger.LogInformation("Cache hit for delivery id: {Id}", id); 
                    _telemetryClient.TrackEvent("CacheHit", new { DeliveryId = id }); 
                    return cachedDelivery; 
                } 
                _logger.LogInformation("Cache miss for delivery id: {Id}. Fetching from database.", id); 
                _telemetryClient.TrackEvent("CacheMiss", new { DeliveryId = id }); 
                var delivery = await SimulateDatabaseFetch(id); 
                if (delivery != null) 
                { 
                    _cache.Set(id, delivery, CacheExpiration); 
                    _logger.LogInformation("Delivery id: {Id} cached.", id); 
                } 
                return delivery; 
            } 
            catch (Exception ex) 
            { 
                _telemetryClient.TrackException(ex); 
                _logger.LogError(ex, "Error retrieving delivery with id: {Id}", id); 
                throw; 
            } 
            finally 
            { 
                _telemetryClient.StopOperation(operation); 
            } 
        } 
        private Task<Delivery> SimulateDatabaseFetch(string id) 
        { 
            // Simulate a database fetch operation 
            return Task.FromResult(new Delivery 
            { 
                DeliveryId = id, 
                OwnerId = "owner123", 
                PickupLocation = "pickup", 
                DropoffLocation = "dropoff", 
                Deadline = "deadline", 
                Expedited = true, 
                ConfirmationRequired = ConfirmationRequired.None, 
                PickupTime = DateTime.Now.AddDays(3), 
                PackageInfo = new PackageInfo 
                { 
                    PackageId = "package1234567", 
                    Size = ContainerSize.Small, 
                    Weight = 0, 
                    Tag = "tag" 
                } 
            }); 
        } 
        public Task<bool> ScheduleDeliveryAsync(Delivery deliveryRequest, string droneId) 
        { 
            var operation = _telemetryClient.StartOperation<RequestTelemetry>("ScheduleDeliveryAsync"); 
            try 
            { 
                // Access common datastore e.g. SQL Azure 
                Utility.DoWork(50); 
                _logger.LogInformation("Scheduled delivery {DeliveryId} with drone {DroneId}", deliveryRequest.DeliveryId, droneId); 
                _telemetryClient.TrackEvent("DeliveryScheduled", new { DeliveryId = deliveryRequest.DeliveryId, DroneId = droneId }); 
                return Task.FromResult(true); 
            } 
            catch (Exception ex) 
            { 
                _telemetryClient.TrackException(ex); 
                _logger.LogError(ex, "Error scheduling delivery {DeliveryId} with drone {DroneId}", deliveryRequest.DeliveryId, droneId); 
                throw; 
            } 
            finally 
            { 
                _telemetryClient.StopOperation(operation); 
            } 
        } 
    } 
}