using k8s.Models;
using System.Collections.Generic;
using static SharpPulsar.Deployment.Kubernetes.Probes;

namespace SharpPulsar.Deployment.Kubernetes.Helpers
{
    public class Probe
    {
        public static V1Probe HttpActionLiviness(ComponentProbe probes, string path, int port, string scheme = "HTTP")
        {
            if (!probes.Liveness.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                HttpGet = new V1HTTPGetAction
                {
                    Path = path,
                    Port = port,
                    Scheme = scheme
                },
                InitialDelaySeconds = probes.Liveness.InitialDelaySeconds,
                FailureThreshold = probes.Liveness.FailureThreshold,
                PeriodSeconds = probes.Liveness.PeriodSeconds
            };
        }
        public static V1Probe HttpActionReadiness(ComponentProbe probes, string path, int port, string scheme = "HTTP")
        {
            if (!probes.Readiness.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                HttpGet = new V1HTTPGetAction
                {
                    Path = path,
                    Port = port,
                    Scheme = scheme
                },
                InitialDelaySeconds = probes.Readiness.InitialDelaySeconds,
                FailureThreshold = probes.Readiness.FailureThreshold,
                PeriodSeconds = probes.Readiness.PeriodSeconds
            };
        }
        public static V1Probe HttpActionStartup(ComponentProbe probes, string path, int port, string scheme = "HTTP")
        {
            if (!probes.Startup.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                HttpGet = new V1HTTPGetAction
                {
                    Path = path,
                    Port = port,
                    Scheme = scheme
                },
                InitialDelaySeconds = probes.Startup.InitialDelaySeconds,
                FailureThreshold = probes.Startup.FailureThreshold,
                PeriodSeconds = probes.Startup.PeriodSeconds
            };
        }
        public static V1Probe ExecActionLiviness(ComponentProbe probe, params string[] cmd)
        {
            if (!probe.Liveness.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                Exec = new V1ExecAction
                {
                    Command = new List<string>(cmd)
                },
                InitialDelaySeconds = probe.Liveness.InitialDelaySeconds,
                FailureThreshold = probe.Liveness.FailureThreshold,
                PeriodSeconds = probe.Liveness.PeriodSeconds
            };
        }
        public static V1Probe ExecActionReadiness(ComponentProbe probe, params string[] cmd)
        {
            if (!probe.Liveness.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                Exec = new V1ExecAction
                {
                    Command = new List<string>(cmd)
                },
                InitialDelaySeconds = probe.Readiness.InitialDelaySeconds,
                FailureThreshold = probe.Readiness.FailureThreshold,
                PeriodSeconds = probe.Readiness.PeriodSeconds
            };
        }
        public static V1Probe ExecActionStartup(ComponentProbe probe, params string[] cmd)
        {
            if (!probe.Liveness.Enabled)
                return new V1Probe();
            return new V1Probe
            {
                Exec = new V1ExecAction
                {
                    Command = new List<string>(cmd)
                },
                InitialDelaySeconds = probe.Startup.InitialDelaySeconds,
                FailureThreshold = probe.Startup.FailureThreshold,
                PeriodSeconds = probe.Startup.PeriodSeconds
            };
        }
    }
}
