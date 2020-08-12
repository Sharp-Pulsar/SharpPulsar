using k8s.Models;
using Newtonsoft.Json;
using SharpPulsar.Deployment.Kubernetes;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpPulsar.Deployment.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");
            var values = new Values();
            var deploy = new DeploymentExecutor();
            var delete = deploy.Delete();
            System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(delete, new JsonSerializerOptions { WriteIndented = true }));
            foreach (var dp in deploy.Run(/*"All"*/))
            {
                try
                {
                    System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(dp, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }
            }
        }
        
    }
}
