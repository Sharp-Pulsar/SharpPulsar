using k8s.Models;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Probe
    {
        public static V1Probe HttpAction(bool enabled, string path, int port, int initialDelay, int periodSeconds, int failureTthreshold)
        {
            if (!enabled)
                return new V1Probe();
            return new V1Probe
            {
                HttpGet = new V1HTTPGetAction
                {
                    Path = path,
                    Port = port
                },
                InitialDelaySeconds = initialDelay,
                FailureThreshold = failureTthreshold,
                PeriodSeconds = periodSeconds
            };
        }
        public static V1Probe ExecAction(bool enabled, string path, int port, int initialDelay, int periodSeconds, int failureTthreshold)
        {
            if (!enabled)
                return new V1Probe();
            return new V1Probe
            {
                HttpGet = new V1HTTPGetAction
                {
                    Path = path,
                    Port = port
                },
                InitialDelaySeconds = initialDelay,
                FailureThreshold = failureTthreshold,
                PeriodSeconds = periodSeconds
            };
        }
    }
}
