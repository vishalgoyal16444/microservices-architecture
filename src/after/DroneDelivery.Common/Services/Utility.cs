namespace DroneDelivery.Common.Services 
{ 
    public class Utility 
    { 
        public static long DoWork(int workUnits) 
        { 
            long count = 0; 
            // Simulate work by doing a basic operation 
            for (int i = 0; i < workUnits; i++) 
            { 
                count += i;  // Simpler operation to simulate workload 
            } 
            return count; 
        } 
    } 
}