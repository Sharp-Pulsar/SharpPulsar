using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Jobs
{
    internal class AdmissionPatch
    {
        private readonly Job _job;
        public AdmissionPatch(Job job)
        {
            _job = job;
        }

        public RunResult Run(string dryRun = default)
        {
            _job.Builder()
                .Metadata("ingress-nginx-admission-patch", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "admission-webhook"}
                })
                .Annotation(new Dictionary<string, string>
                {
                    {"helm.sh/hook", "pre-install,pre-upgrade"},
                    {"helm.sh/hook-delete-policy", "before-hook-creation,hook-succeeded"}
                })
                .TempBuilder()
                .Name("ingress-nginx-admission-patch")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "admission-webhook"}
                })
                .SpecBuilder()
                .ServiceAccountName("ingress-nginx-admission")
                .SecurityContext(new V1PodSecurityContext
                {
                    RunAsNonRoot = true,
                    RunAsUser = 2000
                })
                .Containers(new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "patch",
                        Image = "docker.io/jettech/kube-webhook-certgen:v1.2.2",
                        ImagePullPolicy = "IfNotPresent",
                        Args = new List<string>
                        {
                            "patch",
                            "--webhook-name=ingress-nginx-admission",
                            "--namespace=$(POD_NAMESPACE)",
                            "--patch-mutating=false",
                            "--secret-name=ingress-nginx-admission",
                            "--patch-failure-policy=Fail"
                        },
                        Env = new List<V1EnvVar>
                        {
                            new V1EnvVar
                            {
                                Name = "POD_NAMESPACE",
                                ValueFrom = new V1EnvVarSource
                                {
                                    FieldRef = new V1ObjectFieldSelector
                                    {
                                        FieldPath = "metadata.namespace"
                                    }
                                }
                            }
                        }
                    }
                })
                .RestartPolicy("OnFailure");
            return _job.Run(_job.Builder(), "ingress-nginx", dryRun);
        }
    }
}
