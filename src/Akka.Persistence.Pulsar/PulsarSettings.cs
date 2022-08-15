#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PulsarPersistence.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Security.Cryptography.X509Certificates;
using Akka.Configuration;

namespace Akka.Persistence.Pulsar
{
    public sealed class PulsarSettings
    {
        public PulsarSettings(Config config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config), "Pulsar settings HOCON configuration is missing");
            }
            
            ServiceUrl = config.GetString("service-url", "pulsar://localhost:6650");
            VerifyCertificateAuthority = config.GetBoolean("verify-certificate-authority", true);
            VerifyCertificateName = config.GetBoolean("verify-cerfiticate-name", false);
            OperationTimeOut = TimeSpan.FromMilliseconds(config.GetInt("operation-time-out"));
            AuthClass = config.HasPath("auth-class") ? config.GetString("auth-class") : "";
            AuthParam = config.HasPath("auth-param") ? config.GetString("auth-param") : "";
            TrinoServer = config.GetString("trino-server");
            AdminUrl = config.GetString("admin-url");
            TenantNamespace = config.GetString("pulsar-tenant-namespace");
            TrustedCertificateAuthority = config.HasPath("trusted-certificate-authority-file") 
                ? new X509Certificate2(config.GetString("trusted-certificate-authority-file") )
                : null;
            
            ClientCertificate = config.HasPath("client-certificate-file") 
                ? new X509Certificate2(config.GetString("client-certificate-file") )
                : null;
        }
        public Config Config { get; set; }
        public string ServiceUrl { get; set; }
        public string TenantNamespace { get; set; }
        public string TrinoServer { get; set; }
        public string AuthClass { get; set; }
        public string AuthParam { get; set; }
        public TimeSpan OperationTimeOut { get; set; }
        public string AdminUrl { get; set; }
        public bool VerifyCertificateAuthority { get; set; }
        public bool VerifyCertificateName { get; set; }
        public X509Certificate2 TrustedCertificateAuthority { get; set; }
        public X509Certificate2 ClientCertificate { get; set; }

    }
}