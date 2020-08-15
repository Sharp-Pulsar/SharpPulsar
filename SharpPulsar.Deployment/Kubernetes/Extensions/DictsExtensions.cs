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
        public static IDictionary<string, string> AddRange(this IDictionary<string, string> data, IDictionary<string, string> extraData)
        {
            foreach(var d in extraData)
            {
                data.Add(d.Key, Regex.Replace(d.Value, Environment.NewLine, "\n"));
            }
            return data;
        }
    }
}
