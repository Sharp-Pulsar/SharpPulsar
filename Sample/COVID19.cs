namespace Samples
{
    public class Covid19Mobile
    {
        public string DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Time { get; set; } 
    }
    public class Covid19
    {
        public string DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Time { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Active { get; set; }
        public long Confirmed { get; set; }
        public long Death { get; set; }
        public Flag Flag { get; set; }
    }

    public enum Flag
    {
        Red,// Avoid this area
        Green, //Area Recovered from the virus
        Yellow,// Be cautious in this area
        White // no covid case, caution not to be an index case
    }
}
