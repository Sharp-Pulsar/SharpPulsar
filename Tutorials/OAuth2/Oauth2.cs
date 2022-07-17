using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Common.Enum;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
//https://github.com/fsprojects/pulsar-client-dotnet/blob/d643e7a2518b7ab6bd5a346c1836d944c3b557f8/tests/IntegrationTests/Common.fs
namespace Tutorials.OAuth2
{
    internal class Oauth2
    {
        static string GetConfigFilePath()
        {
            var configFolderName = "Oauth2Files";
            var privateKeyFileName = "credentials_file.json";
            var startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var indexOfConfigDir = startup.IndexOf("examples", StringComparison.Ordinal);
            var examplesFolder = startup.Substring(0, startup.Length - indexOfConfigDir - 3);
            var configFolder = Path.Combine(examplesFolder, configFolderName);
            var ret = Path.Combine(configFolder, privateKeyFileName);
            if (!File.Exists(ret)) throw new FileNotFoundException("can't find credentials file");
            return ret;
        }
//In order to run this example one has to have authentication on broker
//Check configuration files to see how to set up authentication in broker
//In this example Auth0 server is used, look at it's response in Auth0response file 
        internal static async Task RunOauth()
        {
            var fileUri = new Uri(GetConfigFilePath());
            var issuerUrl = new Uri("https://pulsar-sample.us.auth0.com");
            var audience = "https://pulsar-sample.us.auth0.com/api/v2/";

            var serviceUrl = "pulsar://localhost:6650";
            var subscriptionName = "my-subscription";
            var topicName = $"my-topic-%{DateTime.Now.Ticks}";

            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(serviceUrl)                
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));

            //pulsar actor system
            var pulsarSystem = await PulsarSystem.GetInstanceAsync(clientConfig);
            var pulsarClient = pulsarSystem.NewClient();
            
            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<byte[]>()
                .Topic(topicName));

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(topicName)
                .SubscriptionName(subscriptionName)
                .SubscriptionType(SubType.Exclusive));

            var messageId = await producer.SendAsync(Encoding.UTF8.GetBytes($"Sent from C# at '{DateTime.Now}'"));
            Console.WriteLine($"MessageId is: '{messageId}'");

            var message = await consumer.ReceiveAsync();
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(message.Data)}");

            await consumer.AcknowledgeAsync(message.MessageId);
        }
    }
}