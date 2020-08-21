using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Toolset
{
    internal class ToolsetRunner
    {
        private readonly ToolsetConfigMap _toolsetConfigMap;
        public ToolsetRunner(ConfigMap configMap)
        {
            _toolsetConfigMap = new ToolsetConfigMap(configMap);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if (Values.Tls.Enabled && Values.Tls.ZooKeeper.Enabled)
            {
                result = _toolsetConfigMap.Run(dryRun);
                yield return result;
            }
        }
    }
}
