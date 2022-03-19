using Xunit;
using SharpPulsar.ServiceProvider;
using System.Collections.Generic;
using System;
using SharpPulsar.Builder;
using SharpPulsar.Auth;
using SharpPulsar.Interfaces;
using SharpPulsar.Configuration;
using Moq;
using SharpPulsar.User;

namespace SharpPulsar.Test.ServiceProvider
{
    public class AutoClusterFailoverTest
    {
        [Fact]
        public virtual void TestInitialize()
        {
        }

        [Fact]
        public void TestBuildAutoClusterFailoverInstance()
        {
            var primary = "pulsar://localhost:6650";
            var secondary = "pulsar://localhost:6651";
            long failoverDelay = 30;
            long switchBackDelay = 60;
            long checkInterval = 1_000;

            var provider = AutoClusterFailover.Builder().Primary(primary)
                .Secondary(new List<string> { secondary })
                .FailoverDelay(TimeSpan.FromSeconds(failoverDelay))
                .SwitchBackDelay(TimeSpan.FromSeconds(switchBackDelay))
                .CheckInterval(TimeSpan.FromSeconds(checkInterval));

            var autoClusterFailover = new AutoClusterFailover((AutoClusterFailoverBuilder)provider);
            Assert.Equal(primary, autoClusterFailover.ServiceUrl);
            Assert.Equal(primary, autoClusterFailover.Primary);
            Assert.Equal(secondary, autoClusterFailover.Secondary[0]);
            Assert.Equal(TimeSpan.FromSeconds(failoverDelay), autoClusterFailover.FailoverDelayNs);
            Assert.Equal(TimeSpan.FromSeconds(switchBackDelay), autoClusterFailover.SwitchBackDelayNs);
            Assert.Equal(TimeSpan.FromSeconds(checkInterval), autoClusterFailover.IntervalMs);
            Assert.Null(autoClusterFailover.SecondaryAuthentications);
            Assert.Null(autoClusterFailover.SecondaryTlsTrustCertsFilePaths);

            var primaryTlsTrustCertsFilePath = "primary/path";
            var secondaryTlsTrustCertsFilePath = "primary/path";
            var primaryAuthentication = AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.AuthenticationTls", "tlsCertFile:/path/to/primary-my-role.cert.pem," + "tlsKeyFile:/path/to/primary-my-role.key-pk8.pem");

            var secondaryAuthentication = AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.AuthenticationTls", "tlsCertFile:/path/to/secondary-my-role.cert.pem," + "tlsKeyFile:/path/to/secondary-role.key-pk8.pem");
            var secondaryTlsTrustCertsFilePaths = new Dictionary<string, string>
            {
                [secondary] = secondaryTlsTrustCertsFilePath
            };

            var SecondaryAuthentications = new Dictionary<string, IAuthentication>
            {
                [secondary] = secondaryAuthentication
            };

            var provider1 = AutoClusterFailover
                .Builder()
                .Primary(primary)
                .Secondary(new List<string> { secondary })
                .FailoverDelay(TimeSpan.FromSeconds(failoverDelay))
                .SwitchBackDelay(TimeSpan.FromSeconds(switchBackDelay))
                .CheckInterval(TimeSpan.FromSeconds(checkInterval))
                .SecondaryTlsTrustCertsFilePath(secondaryTlsTrustCertsFilePaths)
                .SecondaryAuthentication(SecondaryAuthentications);

            var autoClusterFailover1 = new AutoClusterFailover((AutoClusterFailoverBuilder)provider1);
            Assert.Equal(secondaryTlsTrustCertsFilePath, autoClusterFailover1.SecondaryTlsTrustCertsFilePaths[secondary]);
            Assert.Equal(secondaryAuthentication, autoClusterFailover1.SecondaryAuthentications[secondary]);
        }
    }
}