using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpPulsar.Deployment.Kubernetes.Extensions
{
    public static class DictsExtensions
    {
        public static IDictionary<string, string> RemoveRN(this IDictionary<string, string> data)
        {         
            return data.ToDictionary(k => k.Key, v => Regex.Replace(v.Value, Environment.NewLine, "\n"));
        }
            
    }
}
