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
using Akka.Actor;
using Akka.Configuration;
using SharpPulsar;

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
            Tenant = config.GetString("pulsar-tenant");
            Namespace = config.GetString("pulsar-namespace");
            LedgerId = config.GetLong("ledger-id");
            EntryId = config.GetLong("entry-id");
            Partition = config.GetInt("partition");
            BatchIndex = config.GetInt("batch-index");
            TrustedCertificateAuthority = config.HasPath("trusted-certificate-authority-file") 
                ? new X509Certificate2(config.GetString("trusted-certificate-authority-file") )
                : null;
            
            ClientCertificate = config.HasPath("client-certificate-file") 
                ? new X509Certificate2(config.GetString("client-certificate-file") )
                : null;
        }
        public Config Config { get; set; }
        public string ServiceUrl { get; set; }
        public string Tenant { get; set; }
        public string Namespace { get; set; }
        public string TrinoServer { get; set; }
        public string AuthClass { get; set; }
        public string AuthParam { get; set; }
        public long LedgerId { get; set; }
        public long EntryId { get; set; }
        public int Partition { get; set; }
        public int BatchIndex { get; set; }
        public TimeSpan OperationTimeOut { get; set; }
        public string AdminUrl { get; set; }
        public bool VerifyCertificateAuthority { get; set; }
        public bool VerifyCertificateName { get; set; }
        public X509Certificate2 TrustedCertificateAuthority { get; set; }
        public X509Certificate2 ClientCertificate { get; set; }

    }
}