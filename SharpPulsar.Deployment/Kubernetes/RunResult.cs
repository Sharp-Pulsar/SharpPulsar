using k8s;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes
{
    public sealed class RunResult
    {
        public bool Success { get; set; }
        public RestException Exception { get; set; }
        public HttpOperationException HttpOperationException { get; set; }
        public object Response { get; set; }
    }
}
