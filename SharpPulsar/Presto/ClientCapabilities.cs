
namespace SharpPulsar.Presto
{
    public enum ClientCapabilities
    {
		PATH,
		// Whether clients support datetime types with variable precision
		//   timestamp(p) with time zone
		//   timestamp(p) without time zone
		//   time(p) with time zone
		//   time(p) without time zone
		//   interval X(p1) to Y(p2)
		// When this capability is not set, the server returns datetime types with precision = 3
		ParametricDatetime
	}
}
