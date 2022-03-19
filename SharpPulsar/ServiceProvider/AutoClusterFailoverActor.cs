using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.ServiceName;
using SharpPulsar.User;
using System.Net;
using System.Net.Sockets;
using SharpPulsar.ServiceProvider.Messages;
using SharpPulsar.Builder;
using Akka.Annotations;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.ServiceProvider
{
    [InternalApi]
    public class AutoClusterFailoverActor : ReceiveActor
    {
        private PulsarClient _pulsarClient;
        private string _currentPulsarServiceUrl;
        private readonly string _primary;
        private readonly IList<string> _secondary;
        private readonly FailoverPolicy _failoverPolicy;
        private IAuthentication _primaryAuthentication;
        private readonly IDictionary<string, IAuthentication> _secondaryAuthentications;
        private string _primaryTlsTrustCertsFilePath;
        private readonly IDictionary<string, string> _secondaryTlsTrustCertsFilePaths;
        private string _primaryTlsTrustStorePath;
        private IDictionary<string, string> _secondaryTlsTrustStorePaths;
        private string _primaryTlsTrustStorePassword;
        private IDictionary<string, string> _secondaryTlsTrustStorePasswords;
        private readonly TimeSpan _failoverDelayNs;
        private readonly TimeSpan _switchBackDelayNs;
        private ICancelable _executor;
        private TimeSpan _recoverTimestamp;
        private TimeSpan _failedTimestamp;
        private readonly TimeSpan _intervalMs;
        private int _timeSpan = 30_000;
        private readonly PulsarServiceNameResolver _resolver;
        private ILoggingAdapter _log;

        private AutoClusterFailoverActor(AutoClusterFailoverBuilder builder)
        {
            _log = Context.GetLogger();
            _primary = builder.primary;
            _secondary = builder.secondary;
            _failoverPolicy = builder.failoverPolicy;
            _secondaryAuthentications = builder.SecondaryAuthentications;
            _secondaryTlsTrustCertsFilePaths = builder.SecondaryTlsTrustCertsFilePaths;
            _secondaryTlsTrustStorePaths = builder.SecondaryTlsTrustStorePaths;
            _secondaryTlsTrustStorePasswords = builder.SecondaryTlsTrustStorePasswords;
            _failoverDelayNs = builder.FailoverDelayNs;
            _switchBackDelayNs = builder.SwitchBackDelayNs;
            _currentPulsarServiceUrl = builder.primary;
            _recoverTimestamp = new TimeSpan(-1);
            _failedTimestamp = new TimeSpan(-1);
            _intervalMs = builder.CheckIntervalMs;
            _resolver = new PulsarServiceNameResolver(Context.GetLogger());
            Receive<Initialize>(_=>
            {
                Initialize(_.Client);
            });
            Receive<GetServiceUrl>(_=>
            {
                Sender.Tell(_currentPulsarServiceUrl);
            });
        }
        public static Props Prop(AutoClusterFailoverBuilder builder)
        {
            return Props.Create(() => new AutoClusterFailoverActor(builder));
        }
        
        private void Initialize(PulsarClient client)
        {
            _pulsarClient = client;
            var config = _pulsarClient.Conf;
            if (config != null)
            {
                _primaryAuthentication = config.Authentication;
                _primaryTlsTrustCertsFilePath = config.TlsTrustCertsFilePath;
                _primaryTlsTrustStorePath = config.TlsTrustStorePath;
                _primaryTlsTrustStorePassword = config.TlsTrustStorePassword;
            }
            // start to probe primary cluster active or not
            _executor = Context.System.Scheduler.Advanced.ScheduleRepeatedlyCancelable(_intervalMs, _intervalMs, () =>
            {
                if (_currentPulsarServiceUrl.Equals(_primary))
                {
                    ProbeAndUpdateServiceUrl(_secondary, _secondaryAuthentications, _secondaryTlsTrustCertsFilePaths, _secondaryTlsTrustStorePaths, _secondaryTlsTrustStorePasswords);
                }
                else
                {
                    ProbeAndUpdateServiceUrl(_primary, _primaryAuthentication, _primaryTlsTrustCertsFilePath, _primaryTlsTrustStorePath, _primaryTlsTrustStorePassword);
                    if (!_currentPulsarServiceUrl.Equals(_primary))
                    {
                        ProbeAndCheckSwitchBack(_primary, _primaryAuthentication, _primaryTlsTrustCertsFilePath, _primaryTlsTrustStorePath, _primaryTlsTrustStorePassword);
                    }
                }
            });

        }
        protected override void PostStop()
        {
            _executor.Cancel();
            base.PostStop();
        }

        internal virtual bool ProbeAvailable(string url)
        {
            try
            {
                _resolver.UpdateServiceUrl(url);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = _timeSpan,
                    SendTimeout = _timeSpan
                };
                var uri = _resolver.ResolveHost();
                socket.Connect(uri.Host, uri.Port);
                socket.Close();
                return true;
            }
            catch (Exception e)
            {
                _log.Warning($"Failed to probe available, url: {url} => {e}");
                return false;
            }
        }

        private static long NanosToMillis(long nanos)
        {
            return Math.Max(0L, (long)Math.Round(nanos / 1_000_000.0d, MidpointRounding.AwayFromZero));
        }

        private void UpdateServiceUrl(string target, IAuthentication authentication, string tlsTrustCertsFilePath, string tlsTrustStorePath, string tlsTrustStorePassword)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tlsTrustCertsFilePath))
                {
                    _pulsarClient.UpdateTlsTrustCertsFilePath(tlsTrustCertsFilePath);
                }

                if (authentication != null)
                {
                    _pulsarClient.UpdateAuthentication(authentication);
                }

                if (!string.IsNullOrWhiteSpace(tlsTrustStorePath))
                {
                    _pulsarClient.UpdateTlsTrustStorePathAndPassword(tlsTrustStorePath, tlsTrustStorePassword);
                }

                _pulsarClient.UpdateServiceUrl(target);
                _currentPulsarServiceUrl = target;
            }
            catch (Exception e)
            {
                _log.Error($"Current Pulsar service is {_currentPulsarServiceUrl}, failed to switch back to {target}: {e}");
            }
        }

        private void ProbeAndUpdateServiceUrl(IList<string> targetServiceUrls, IDictionary<string, IAuthentication> authentications, IDictionary<string, string> tlsTrustCertsFilePaths, IDictionary<string, string> tlsTrustStorePaths, IDictionary<string, string> tlsTrustStorePasswords)
        {
            if (ProbeAvailable(_currentPulsarServiceUrl))
            {
                _failedTimestamp = new TimeSpan(-1);
                return;
            }

            var currentTimestamp = TimeSpan.FromTicks(DateTime.Now.Ticks);
            if (_failedTimestamp.TotalSeconds == -1)
            {
                _failedTimestamp = currentTimestamp;
            }
            else if (currentTimestamp - _failedTimestamp >= _failoverDelayNs)
            {
                foreach (var targetServiceUrl in targetServiceUrls)
                {
                    if (ProbeAvailable(targetServiceUrl))
                    {
                        _log.Info($"Current Pulsar service is {_currentPulsarServiceUrl}, it has been down for {NanosToMillis((long)(currentTimestamp - _failedTimestamp).TotalMilliseconds)} ms, switch to the service {targetServiceUrls}. The current service down at {_failedTimestamp}");
                        UpdateServiceUrl(targetServiceUrl, authentications != null ? authentications[targetServiceUrl] : null, tlsTrustCertsFilePaths != null ? tlsTrustCertsFilePaths[targetServiceUrl] : null, tlsTrustStorePaths != null ? tlsTrustStorePaths[targetServiceUrl] : null, tlsTrustStorePasswords != null ? tlsTrustStorePasswords[targetServiceUrl] : null);
                        _failedTimestamp = new TimeSpan(-1);
                        break;
                    }
                    else
                    {
                        _log.Warning($"Current Pulsar service is {_currentPulsarServiceUrl}, it has been down for {NanosToMillis((long)(currentTimestamp - _failedTimestamp).TotalMilliseconds)} ms. Failed to switch to service {targetServiceUrl}, because it is not available, continue to probe next pulsar service.");
                    }
                }
            }
        }

        private void ProbeAndUpdateServiceUrl(string targetServiceUrl, IAuthentication authentication, string tlsTrustCertsFilePath, string tlsTrustStorePath, string tlsTrustStorePassword)
        {
            if (ProbeAvailable(_currentPulsarServiceUrl))
            {
                _failedTimestamp = new TimeSpan(-1);
                return;
            }

            var currentTimestamp = TimeSpan.FromTicks(DateTime.Now.Ticks);
            if (_failedTimestamp.TotalSeconds == -1)
            {
                _failedTimestamp = currentTimestamp;
            }
            else if (currentTimestamp - _failedTimestamp >= _failoverDelayNs)
            {
                if (ProbeAvailable(targetServiceUrl))
                {
                    _log.Info($"Current Pulsar service is {_currentPulsarServiceUrl}, it has been down for {NanosToMillis((long)(currentTimestamp - _failedTimestamp).TotalMilliseconds)} ms, switch to the service {targetServiceUrl}. The current service down at {_failedTimestamp}");
                    UpdateServiceUrl(targetServiceUrl, authentication, tlsTrustCertsFilePath, tlsTrustStorePath, tlsTrustStorePassword);
                    _failedTimestamp = new TimeSpan(-1);
                }
                else
                {
                    _log.Error($"Current Pulsar service is {_currentPulsarServiceUrl}, it has been down for {NanosToMillis((long)(currentTimestamp - _failedTimestamp).TotalMilliseconds)} ms. Failed to switch to service {targetServiceUrl}, because it is not available");
                }
            }
        }

        private void ProbeAndCheckSwitchBack(string target, IAuthentication authentication, string tlsTrustCertsFilePath, string tlsTrustStorePath, string tlsTrustStorePassword)
        {
            var currentTimestamp = TimeSpan.FromTicks(DateTime.Now.Ticks);
            if (!ProbeAvailable(target))
            {
                _recoverTimestamp = new TimeSpan(-1); 
                return;
            }

            if (_recoverTimestamp.TotalMilliseconds == -1)
            {
                _recoverTimestamp = currentTimestamp;
            }
            else if (currentTimestamp - _recoverTimestamp >= _switchBackDelayNs)
            {
                _log.Info($"Current Pulsar service is secondary: {_currentPulsarServiceUrl}, the primary service: {target} has been recover for {NanosToMillis((long)(currentTimestamp - _recoverTimestamp).TotalMilliseconds)} ms, " + "switch back to the primary service");
                UpdateServiceUrl(target, authentication, tlsTrustCertsFilePath, tlsTrustStorePath, tlsTrustStorePassword);
                _recoverTimestamp = new TimeSpan(-1);
            }
        }

    }

}