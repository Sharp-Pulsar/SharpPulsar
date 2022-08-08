﻿
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace SharpPulsar.TestContainer.Container
{
    public sealed class PulsarTestContainer : TestcontainerMessageBroker
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarTestcontainer" /> class.
        /// </summary>
        /// <param name="configuration">The Testcontainers configuration.</param>
        /// <param name="logger">The logger.</param>
        internal PulsarTestContainer(ITestcontainersConfiguration configuration, ILogger logger)
          : base(configuration, logger)
        {

        }

    }
}