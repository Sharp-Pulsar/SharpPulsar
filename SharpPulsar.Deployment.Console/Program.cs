using SharpPulsar.Deployment.Kubernetes;
using System;
using System.IO;
using System.Text.Json;

namespace SharpPulsar.Deployment.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            
            System.Console.WriteLine("Hello World!");
            //var basePath = @"Kubernetes\Certificate\PEMs\";
            /*var certs = new CertificateSecrets
            {
                CertificateAuthority = File.ReadAllText($"{basePath}CA.crt"),
                AzureDnsPassword = "ai1oS2xQMS5lb0FNUU52c0VHQ1B0bEE5R2NaTFFwRTFfdQo=",
                Broker = new CertificateSecrets.ComponentSecret
                {
                    Private = File.ReadAllText($"{basePath}Broker-Private-Key.pem"),
                    Public = File.ReadAllText($"{basePath}Broker-Public-Key.pem")
                },
                Bookie = new CertificateSecrets.ComponentSecret
                {
                    Public = File.ReadAllText($"{ basePath }Bookie-Public-Key.pem"),
                    Private = File.ReadAllText($"{basePath}Bookie-Private-key.pem")
                },
                Proxy = new CertificateSecrets.ComponentSecret
                {
                    Public = File.ReadAllText($"{ basePath }Proxy-Public-Key.pem"),
                    Private = File.ReadAllText($"{basePath}Proxy-Private-Key.pem")
                },
                Recovery = new CertificateSecrets.ComponentSecret
                {
                    Public = File.ReadAllText($"{ basePath }Recovery-Pub-Key.pem"),
                    Private = File.ReadAllText($"{basePath}Recovery-Private-Key.pem")
                },
                Toolset = new CertificateSecrets.ComponentSecret
                {
                    Public = File.ReadAllText($"{ basePath }Toolset-Public.pem"),
                    Private = File.ReadAllText($"{basePath}Toolset-Private.pem")
                },
                Zoo = new CertificateSecrets.ComponentSecret
                {
                    Public = File.ReadAllText($"{ basePath }Zoo-public.pem"),
                    Private = File.ReadAllText($"{basePath}Zoo-Private.pem")
                }
            };*/
            new Values(@namespace: "ingress-nginx");
            //do this only when you are creating a new instance of Values properties. Old value is used
            //Values.ReleaseName = "friday";
            var deploy = new DeploymentExecutor();
            string path = $@"Results\{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                //var delete = deploy.Delete();
                //System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(delete, new JsonSerializerOptions { WriteIndented = true }));
                foreach (var dp in deploy.Run(/*"All"*/))
                {
                    try
                    {
                        if (dp.Success)
                            sw.WriteLine(System.Text.Json.JsonSerializer.Serialize(dp.Response, new JsonSerializerOptions { WriteIndented = true }));
                        else if (dp.HttpOperationException != null)
                            sw.WriteLine(dp.HttpOperationException.Response.Content);
                        else
                            sw.WriteLine(System.Text.Json.JsonSerializer.Serialize(dp.Exception.Data, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }
                }
            }

        }
        
    }
}
