using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Toolset
{
    internal class ToolsetRunner
    {
        private readonly ToolsetConfigMap _toolsetConfigMap;
        private readonly ToolsetService _toolsetService;
        private readonly ToolsetStateful _toolsetStateful;
        public ToolsetRunner(ConfigMap configMap, Service service, StatefulSet statefulSet)
        {
            _toolsetConfigMap = new ToolsetConfigMap(configMap);
            _toolsetService = new ToolsetService(service);
            _toolsetStateful = new ToolsetStateful(statefulSet);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;
            if (Values.Settings.Toolset.Enabled)
            {

                result = _toolsetConfigMap.RunKeyTool(dryRun);
                yield return result;
                result = _toolsetConfigMap.Run(dryRun);
                yield return result;
                result = _toolsetService.Run(dryRun);
                yield return result;

                result = _toolsetStateful.Run(dryRun);
                yield return result;
            }

        }
    }
}
