using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class ZooTemp
    {
        public static IDictionary<string, string> StorageParameters { get; set; } = new Dictionary<string, string>();
        public static string StorageProvisioner { get; set; } = string.Empty;
        public static IDictionary<string, string> DatalogParameters { get; set; } = new Dictionary<string, string>();
        public static string DatalogProvisioner { get; set; } = string.Empty;
    }
}
