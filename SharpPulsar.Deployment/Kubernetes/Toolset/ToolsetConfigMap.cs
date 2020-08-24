using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Toolset
{
    internal class ToolsetConfigMap
    {
        private readonly ConfigMap _config;
        internal ToolsetConfigMap(ConfigMap config)
        {
            _config = config;
        }
        public RunResult RunKeyTool(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-keytool-configmap", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", "keytool" },
                            })
                .Data(Helpers.Config.KeyTool());
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Toolset.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Toolset.Name },
                            })
                .Data(Values.ConfigMaps.Toolset);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}
