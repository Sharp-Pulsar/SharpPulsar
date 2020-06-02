using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Newtonsoft.Json;
using PulsarAdmin.Models;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Admin.Api.Models;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.EventSource;
using SharpPulsar.Akka.Function;
using SharpPulsar.Akka.Function.Api;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Protocol.Proto;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageId = SharpPulsar.Impl.MessageId;

namespace Samples
{
    public class Program
    {
        //https://www.splunk.com/en_us/blog/it/effectively-once-semantics-in-apache-pulsar.html
        //https://linuxize.com/post/how-to-save-file-in-vim-quit-editor/
        //https://stackoverflow.com/questions/9612941/how-to-set-java-environment-path-in-ubuntu
        //https://askubuntu.com/questions/175514/how-to-set-java-home-for-java

        //$ pwd
        //$ cd /
        ////bin/pulsar sql-worker run -D "java.vendor"="Oracle Corporation"
        //I think, the substitution of Linux command $(pwd) in Windows is "%cd%".
        public static readonly ConcurrentDictionary<string, Dictionary<string, IActorRef>> Producers = new ConcurrentDictionary<string, Dictionary<string, IActorRef>>();
        public static readonly ConcurrentBag<string> Receipts = new ConcurrentBag<string>();
        public static readonly ConcurrentBag<string> Messages = new ConcurrentBag<string>();
        
        public static readonly Dictionary<string, IActorRef> Consumers = new Dictionary<string, IActorRef>();
        public static readonly Dictionary<string, LastMessageIdResponse> LastMessageId = new Dictionary<string, LastMessageIdResponse>();
        
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome. Enter Pulsar server endpoint");
            var endPoint = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(endPoint))
                 throw new ArgumentException("endpoint cannot be null");
            var uri = new Uri(endPoint);
            if(uri.Scheme.ToLower() != "pulsar")
                throw new ArgumentException("endpoint scheme is invalid");
            Console.WriteLine("Is Broker behind a pulsar proxy(Y/N)");
            var proxy = Console.ReadLine();
            var useProxy = proxy.ToLower() == "y";

            Console.WriteLine("Enter operation timeout in milliseeconds");
            var opto = Convert.ToInt32(Console.ReadLine());

            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(endPoint)
                .ConnectionsPerBroker(1)
                .UseProxy(useProxy)
                .OperationTimeout(opto)
                .Authentication( new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            var pulsarSystem = new PulsarSystem(clientConfig);
            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    #region producers
                    case "0":
                        Console.WriteLine("[PlainAvroBulkSendProducer] Enter topic: ");
                        var t = Console.ReadLine();
                        PlainAvroBulkSendProducer(pulsarSystem, t);
                        break;
                    case "67":
                        Console.WriteLine("[RegisterSchema] Enter topic: ");
                        var schemaTopic = Console.ReadLine();
                        RegisterSchema(pulsarSystem, schemaTopic);
                        break;
                    case "63":
                        Console.WriteLine("[PlainAvroBulkSendCompressionProducer] Enter topic: ");
                        var t63 = Console.ReadLine();
                        Console.WriteLine("[PlainAvroBulkSendCompressionProducer] Enter Compression Type number ");
                        Console.WriteLine("None:0, Lz4:1, Zlib:2, Zstd:3, Snappy:4");
                        var ct = Convert.ToInt32(Console.ReadLine());
                        PlainAvroBulkSendCompressionProducer(pulsarSystem, t63, ct);
                        break;
                    case "65":
                        Console.WriteLine("[PlainAvroBulkSendBroadcastProducer] Enter topic: ");
                        var t65 = Console.ReadLine();
                        PlainAvroBulkSendBroadcastProducer(pulsarSystem, t65);
                        break;
                    case "1":
                        Console.WriteLine("[PlainAvroProducer] Enter topic: ");
                        var t1 = Console.ReadLine();
                        PlainAvroProducer(pulsarSystem, t1);
                        break;
                    case "51":
                        Console.WriteLine("[PlainAvroCovidProducer] Enter topic: ");
                        var t51 = Console.ReadLine();
                        PlainAvroCovidProducer(pulsarSystem, t51);
                        break;
                    case "2":
                        Console.WriteLine("[PlainByteBulkSendProducer] Enter topic: ");
                        var t2 = Console.ReadLine();
                        PlainByteBulkSendProducer(pulsarSystem, t2);
                        break;
                    case "3":
                        Console.WriteLine("[PlainByteProducer] Enter topic: ");
                        var t3 = Console.ReadLine();
                        PlainByteProducer(pulsarSystem, t3);
                        break;
                    case "4":
                        Console.WriteLine("[EncryptedAvroBulkSendProducer] Enter topic: ");
                        var t4 = Console.ReadLine();
                        EncryptedAvroBulkSendProducer(pulsarSystem, t4);
                        break;
                    case "5":
                        Console.WriteLine("[EncryptedAvroProducer] Enter topic: ");
                        var t5 = Console.ReadLine();
                        EncryptedAvroProducer(pulsarSystem, t5);
                        break;
                    case "6":
                        Console.WriteLine("[EncryptedByteBulkSendProducer] Enter topic: ");
                        var t6 = Console.ReadLine();
                        EncryptedByteBulkSendProducer(pulsarSystem, t6);
                        break;
                    case "7":
                        Console.WriteLine("[EncryptedByteProducer] Enter topic: ");
                        var t7 = Console.ReadLine();
                        EncryptedByteProducer(pulsarSystem, t7);
                        break;

                    #endregion

                    #region consumers
                    case "8":
                        Console.WriteLine("[PlainAvroConsumer] Enter topic: ");
                        var t8 = Console.ReadLine();
                        PlainAvroConsumer(pulsarSystem, t8);
                        break;
                    case "68":
                        Console.WriteLine("[GetLastMessageId] Enter topic: ");
                        var tid = Console.ReadLine();
                        GetLastMessageId(pulsarSystem, tid);
                        break;
                    case "62":
                        Console.WriteLine("[PlainAvroStudentsConsumer] Enter topic: ");
                        var t62 = Console.ReadLine();
                        PlainAvroStudentsConsumer(pulsarSystem, t62);
                        break;
                    case "60":
                        Console.WriteLine("[PlainAvroCovidConsumer] Enter topic: ");
                        var t60 = Console.ReadLine();
                        PlainAvroCovidConsumer(pulsarSystem, t60);
                        break;
                    case "9":
                        Console.WriteLine("[DecryptAvroConsumer] Enter topic: ");
                        var t9 = Console.ReadLine();
                        DecryptAvroConsumer(pulsarSystem, t9);
                        break;
                    case "10":
                        Console.WriteLine("[DecryptAvroConsumerSeek] Enter topic: ");
                        var t10 = Console.ReadLine();
                        DecryptAvroConsumerSeek(pulsarSystem, t10);
                        break;
                    case "11":
                        Console.WriteLine("[DecryptAvroPatternConsumer] Enter topic: ");
                        var t11 = Console.ReadLine();
                        DecryptAvroPatternConsumer(pulsarSystem, t11);
                        break;
                    case "12":
                        Console.WriteLine("[DecryptAvroMultiConsumer] Enter comm delimited topics: ");
                        var topics = Console.ReadLine();
                        var t12 = topics.Split(",");
                        DecryptAvroMultiConsumer(pulsarSystem, t12);
                        break;
                    case "66":
                        Console.WriteLine("[AvroMultiConsumer] Enter comm delimited topics: ");
                        var tops = Console.ReadLine();
                        var t66 = tops.Split(",");
                        AvroMultiConsumer(pulsarSystem, t66);
                        break;
                    case "13":
                        Console.WriteLine("[PlainBytesConsumer] Enter topic: ");
                        var t13 = Console.ReadLine();
                        PlainBytesConsumer(pulsarSystem, t13);
                        break;
                    case "14":
                        Console.WriteLine("[DecryptBytesConsumer] Enter topic: ");
                        var t14 = Console.ReadLine();
                        DecryptBytesConsumer(pulsarSystem, t14);
                        break;
                    case "22":
                        Console.WriteLine("[PlainAvroConsumerSeek] Enter topic: ");
                        var t22 = Console.ReadLine();
                        PlainAvroConsumerSeek(pulsarSystem, t22);
                        break;
                    case "71":
                        Console.WriteLine("[QueueConsumer] Enter topic: ");
                        var q1 = Console.ReadLine();
                        Console.WriteLine("[QueueConsumer] Enter Take: ");
                        var q2 = Convert.ToInt32(Console.ReadLine());
                        QueueConsumer(pulsarSystem, q1, q2);
                        break;
                    #endregion

                    #region EventSource
                    case "72":
                        Console.WriteLine("[GetNumberOfEntries] Enter server: ");
                        var e1 = Console.ReadLine();
                        Console.WriteLine("[GetNumberOfEntries] Enter Topic: ");
                        var e2 = Console.ReadLine();
                        GetNumberOfEntries(pulsarSystem, e1, e2);
                        break;
                    case "73":
                        Console.WriteLine("[ReplayTopic] Enter server: ");
                        var e3 = Console.ReadLine();
                        Console.WriteLine("[ReplayTopic] Enter Topic: ");
                        var e4 = Console.ReadLine();
                        Console.WriteLine("[ReplayTopic] Enter From: ");
                        var e5 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[ReplayTopic] Enter To: ");
                        var e6 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[ReplayTopic] Enter Max: ");
                        var e7 = long.Parse(Console.ReadLine());
                        ReplayTopic(pulsarSystem, e3, e4, e5, e6, e7);
                        break;
                    case "74":
                        Console.WriteLine("[ReplayTaggedTopic] Enter server: ");
                        var e8 = Console.ReadLine();
                        Console.WriteLine("[ReplayTaggedTopic] Enter Topic: ");
                        var e9 = Console.ReadLine();
                        Console.WriteLine("[ReplayTaggedTopic] Enter From: ");
                        var e10 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[ReplayTaggedTopic] Enter To: ");
                        var e11 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[ReplayTaggedTopic] Enter Max: ");
                        var e12 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[ReplayTaggedTopic] Enter Tag key: ");
                        var e13 = Console.ReadLine();
                        Console.WriteLine("[ReplayTaggedTopic] Enter Tag value: ");
                        var e14 = Console.ReadLine();
                        ReplayTaggedTopic(pulsarSystem, e8, e9, e10, e11, e12, e13, e14);
                        break;
                    case "75":
                        Console.WriteLine("[NextPlay] Enter server: ");
                        var e23 = Console.ReadLine();
                        Console.WriteLine("[NextPlay] Enter Topic: ");
                        var e16 = Console.ReadLine();
                        Console.WriteLine("[NextPlay] Enter From: ");
                        var e17 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[NextPlay] Enter To: ");
                        var e18 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[NextPlay] Enter Max: ");
                        var e19 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[NextPlay] Is Tagged(y/n): ");
                        var e199 = Console.ReadLine().ToLower() == "y";
                        NextPlay(pulsarSystem, e23, e16, e17, e18, e19, e199);
                        break;

                    #endregion
                    #region Readers

                    case "15":
                        Console.WriteLine("[DecryptAvroReader] Enter topic: ");
                        var t15 = Console.ReadLine();
                        DecryptAvroReader(pulsarSystem, t15);
                        break;
                    case "16":
                        Console.WriteLine("[DecryptAvroReaderSeek] Enter topic: ");
                        var t16 = Console.ReadLine();
                        DecryptAvroReaderSeek(pulsarSystem, t16);
                        break;
                    case "17":
                        Console.WriteLine("[PlainAvroReader] Enter topic: ");
                        var t17 = Console.ReadLine();
                        PlainAvroReader(pulsarSystem, t17);
                        break;
                    case "18":
                        Console.WriteLine("[PlainBytesReader] Enter topic: ");
                        var t18 = Console.ReadLine();
                        PlainBytesReader(pulsarSystem, t18);
                        break;
                    case "19":
                        Console.WriteLine("[DecryptByteReader] Enter topic: ");
                        var t19 = Console.ReadLine();
                        DecryptByteReader(pulsarSystem, t19);
                        break;
                    case "23":
                        Console.WriteLine("[PlainAvroReaderSeek] Enter topic: ");
                        var t23 = Console.ReadLine();
                        PlainAvroReaderSeek(pulsarSystem, t23);
                        break;

                    #endregion
                    #region Sql

                    case "20":
                        Console.WriteLine("[SqlQuery] Enter destination server: ");
                        var server = Console.ReadLine();
                        Console.WriteLine("[SqlQuery] Enter query statement: ");
                        var query = Console.ReadLine();
                        SqlQuery(pulsarSystem, query, server);
                        break;
                    case "21":
                        Console.WriteLine("[LiveSqlQuery] Enter destination server: ");
                        var lserver = Console.ReadLine();
                        Console.WriteLine("[LiveSqlQuery] Enter query statement: ");
                        var lquery = Console.ReadLine();
                        Console.WriteLine("[LiveSqlQuery] Enter refresh frequency MS: ");
                        var lfreq = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("[LiveSqlQuery] Enter start time epoch: ");
                        var lepoch = string.IsNullOrWhiteSpace(Console.ReadLine())? DateTime.Now.AddHours(-24) :DateTime.Parse(Console.ReadLine());
                        Console.WriteLine("[LiveSqlQuery] Enter topic: ");
                        var ltopic = Console.ReadLine();
                        LiveSqlQuery(pulsarSystem, lquery, lfreq, lepoch, ltopic, lserver);
                        break;
                    #endregion
                    #region Admin
                    case "24":
                        Console.WriteLine("[GetAllSchemas] Enter destination server: ");
                        var adminserver = Console.ReadLine();
                        Console.WriteLine("[GetAllSchemas] Tenant: ");
                        var tn3 = Console.ReadLine();
                        Console.WriteLine("[GetAllSchemas] Namespace: ");
                        var ns3 = Console.ReadLine();
                        Console.WriteLine("[GetAllSchemas] Topic: ");
                        var to1 = Console.ReadLine();
                        GetAllSchemas(pulsarSystem, adminserver, tn3, ns3, to1);
                        break;
                    case "69":
                        Console.WriteLine("[GetPersistentTopicStats] Enter destination server: ");
                        var s = Console.ReadLine();
                        Console.WriteLine("[GetPersistentTopicStats] Tenant: ");
                        var t69 = Console.ReadLine();
                        Console.WriteLine("[GetPersistentTopicStats] Namespace: ");
                        var n69 = Console.ReadLine();
                        Console.WriteLine("[GetPersistentTopicStats] Topic: ");
                        var top69 = Console.ReadLine();
                        Console.WriteLine("[GetPersistentTopicStats] From: ");
                        var fro69 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[GetPersistentTopicStats] To: ");
                        var to69 = long.Parse(Console.ReadLine());
                        Console.WriteLine("[GetPersistentTopicStats] Max: ");
                        var mx69 = long.Parse(Console.ReadLine());
                        GetPersistentTopicStats(pulsarSystem, s, t69, n69, top69, fro69, to69, mx69);
                        break;
                    case "70":
                        Console.WriteLine("[GetManagedLedgerInfoPersistent] Enter destination server: ");
                        var s70 = Console.ReadLine();
                        Console.WriteLine("[GetManagedLedgerInfoPersistent] Tenant: ");
                        var t70 = Console.ReadLine();
                        Console.WriteLine("[GetManagedLedgerInfoPersistent] Namespace: ");
                        var n70 = Console.ReadLine();
                        Console.WriteLine("[GetManagedLedgerInfoPersistent] Topic: ");
                        var to70 = Console.ReadLine();
                        GetManagedLedgerInfoPersistent(pulsarSystem, s70, t70, n70, to70);
                        break;
                    case "58":
                        Console.WriteLine("[RegisterSchema] Enter destination server: ");
                        var ad = Console.ReadLine();
                        Console.WriteLine("[RegisterSchema] Tenant: ");
                        var tn7 = Console.ReadLine();
                        Console.WriteLine("[RegisterSchema] Namespace: ");
                        var ns7 = Console.ReadLine();
                        Console.WriteLine("[RegisterSchema] Topic: ");
                        var to3 = Console.ReadLine();
                        RegisterSchema(pulsarSystem, ad, tn7, ns7, to3);
                        break;
                    case "59":
                        Console.WriteLine("[DeleteTopic] Enter destination server: ");
                        var ad1 = Console.ReadLine();
                        Console.WriteLine("[DeleteTopic] Tenant: ");
                        var tn8 = Console.ReadLine();
                        Console.WriteLine("[DeleteTopic] Namespace: ");
                        var ns8 = Console.ReadLine();
                        Console.WriteLine("[DeleteTopic] Topic: ");
                        var to4 = Console.ReadLine();
                        DeleteTopic(pulsarSystem, ad1, tn8, ns8, to4);
                        break;
                    case "55":
                        Console.WriteLine("[DeleteSchema] Enter destination server: ");
                        var adminserver1 = Console.ReadLine();
                        Console.WriteLine("[DeleteSchema] Tenant: ");
                        var tn4 = Console.ReadLine();
                        Console.WriteLine("[DeleteSchema] Namespace: ");
                        var ns4 = Console.ReadLine();
                        Console.WriteLine("[DeleteSchema] Topic: ");
                        var to2 = Console.ReadLine();
                        DeleteSchema(pulsarSystem, adminserver1, tn4, ns4, to2);
                        break;
                    case "64":
                        Console.WriteLine("[EnableBrokerDeduplication] Enter destination server: ");
                        var adminserver3 = Console.ReadLine();
                        Console.WriteLine("[EnableBrokerDeduplication] Tenant: ");
                        var tn64 = Console.ReadLine();
                        Console.WriteLine("[EnableBrokerDeduplication] Namespace: ");
                        var ns64 = Console.ReadLine();
                        Console.WriteLine("[EnableBrokerDeduplication] enable(y/n): ");
                        var to64 = Console.ReadLine().ToLower() == "y";
                        EnableBrokerDeduplication(pulsarSystem, adminserver3, tn64, ns64, to64);
                        break;
                    case "25":
                        Console.WriteLine("[CreateTenant] Enter destination server: ");
                        var tServer = Console.ReadLine();
                        Console.WriteLine("[CreateTenant] Enter tenant: ");
                        var tTenant = Console.ReadLine();
                        Console.WriteLine("[CreateTenant] Enter coma delimited clusters: ");
                        var tClusters = Console.ReadLine();
                        CreateTenant(pulsarSystem, tServer, tTenant, tClusters);
                        break;
                    case "39":
                        Console.WriteLine("[UpdateTenant] Enter destination server: ");
                        UpdateTenant(pulsarSystem, Console.ReadLine());
                        break;
                    case "26":
                        Console.WriteLine("[GetTenants] Enter destination server: ");
                        GetTenants(pulsarSystem, Console.ReadLine());
                        break;
                    case "27":
                        Console.WriteLine("[CreateNamespace] Enter destination server: ");
                        var nServer = Console.ReadLine();
                        Console.WriteLine("[CreateNamespace] Enter tenant: ");
                        var nTenant = Console.ReadLine();
                        Console.WriteLine("[CreateNamespace] Enter namespace: ");
                        var nNamespace = Console.ReadLine();
                        CreateNamespace(pulsarSystem, nServer, nTenant, nNamespace);
                        break;
                    case "41":
                        Console.WriteLine("[DeleteNamespace] Enter destination server: ");
                        DeleteNamespace(pulsarSystem, Console.ReadLine());
                        break;
                    case "28":
                        Console.WriteLine("[CreateNonPartitionedTopic] Enter destination server: ");
                        CreateNonPartitionedTopic(pulsarSystem, Console.ReadLine());
                        break;
                    case "43":
                        Console.WriteLine("[CreateNonPartitionedPersistentTopic] Enter destination server: ");
                        var ds = Console.ReadLine();
                        Console.WriteLine("[CreateNonPartitionedPersistentTopic] Tenant: ");
                        var tn = Console.ReadLine();
                        Console.WriteLine("[CreateNonPartitionedPersistentTopic] Namespace: ");
                        var ns = Console.ReadLine();
                        Console.WriteLine("[CreateNonPartitionedPersistentTopic] Topic: ");
                        var to = Console.ReadLine();
                        CreateNonPartitionedPersistentTopic(pulsarSystem, ds, tn, ns, to);
                        break;
                    case "56":
                        Console.WriteLine("[SetSchemaCompatibilityStrategy] Enter destination server: ");
                        var ds1 = Console.ReadLine();
                        Console.WriteLine("[SetSchemaCompatibilityStrategy] Tenant: ");
                        var tn5 = Console.ReadLine();
                        Console.WriteLine("[SetSchemaCompatibilityStrategy] Namespace: ");
                        var ns5 = Console.ReadLine();
                        SetSchemaCompatibilityStrategy(pulsarSystem, ds1, tn5, ns5);
                        break;
                    case "57":
                        Console.WriteLine("[GetSchemaCompatibilityStrategy] Enter destination server: ");
                        var ds2 = Console.ReadLine();
                        Console.WriteLine("[GetSchemaCompatibilityStrategy] Tenant: ");
                        var tn6 = Console.ReadLine();
                        Console.WriteLine("[GetSchemaCompatibilityStrategy] Namespace: ");
                        var ns6 = Console.ReadLine();
                        GetSchemaCompatibilityStrategy(pulsarSystem, ds2, tn6, ns6);
                        break;
                    case "29":
                        Console.WriteLine("[CreatePartitionedTopic] Enter destination server: ");
                        var pServer = Console.ReadLine();
                        Console.WriteLine("[CreatePartitionedTopic] Enter tenant: ");
                        var pTenant = Console.ReadLine();
                        Console.WriteLine("[CreatePartitionedTopic] Enter namespace: ");
                        var pNamespace = Console.ReadLine();
                        Console.WriteLine("[CreatePartitionedTopic] Enter topic: ");
                        var pTopic = Console.ReadLine();
                        Console.WriteLine("[CreatePartitionedTopic] Enter partitions: ");
                        var pPart = Convert.ToInt32(Console.ReadLine());
                        CreatePartitionedTopic(pulsarSystem, pServer, pTenant, pNamespace, pTopic, pPart);
                        break;
                    case "42":
                        Console.WriteLine("[GetPartitionedTopicMetadata] Enter destination server: ");
                        GetPartitionedTopicMetadata(pulsarSystem, Console.ReadLine());
                        break;
                    case "44":
                        Console.WriteLine("[GetPartitionedPersistentTopicMetadata] Enter destination server: ");
                        var mServer = Console.ReadLine();
                        Console.WriteLine("[GetPartitionedPersistentTopicMetadata] Enter tenant: ");
                        var mTenant = Console.ReadLine();
                        Console.WriteLine("[GetPartitionedPersistentTopicMetadata] Enter namespace: ");
                        var mNamespace = Console.ReadLine();
                        Console.WriteLine("[GetPartitionedPersistentTopicMetadata] Enter topic: ");
                        var mTopic = Console.ReadLine();
                        GetPartitionedPersistentTopicMetadata(pulsarSystem, mServer, mTenant, mNamespace, mTopic);
                        break;
                    case "30":
                        Console.WriteLine("[GetTopics] Enter destination server: ");
                        GetTopics(pulsarSystem, Console.ReadLine());
                        break;
                    case "31":
                        Console.WriteLine("[GetTopics2] Enter destination server: ");
                        GetTopics2(pulsarSystem, Console.ReadLine());
                        break;
                    case "52":
                        Console.WriteLine("[GetPersistenceList] Enter destination server: ");
                        GetPersistenceList(pulsarSystem, Console.ReadLine());
                        break;
                    case "32":
                        Console.WriteLine("[GetPartitionedTopics] Enter destination server: ");
                        GetPartitionedTopics(pulsarSystem, Console.ReadLine());
                        break;
                    case "33":
                        Console.WriteLine("[SetRetention] Enter destination server: ");
                        var srver = Console.ReadLine();
                        Console.WriteLine("[SetRetention] Tenant: ");
                        var tn1 = Console.ReadLine();
                        Console.WriteLine("[SetRetention] Namespace: ");
                        var ns1 = Console.ReadLine();
                        SetRetention(pulsarSystem, srver, tn1, ns1);
                        break;
                    case "34":
                        Console.WriteLine("[GetRetention] Enter destination server: ");
                        var srver1 = Console.ReadLine();
                        Console.WriteLine("[GetRetention] Tenant: ");
                        var tn2 = Console.ReadLine();
                        Console.WriteLine("[GetRetention] Namespace: ");
                        var ns2 = Console.ReadLine();
                        GetRetention(pulsarSystem, srver1, tn2, ns2);
                        break;
                    case "35":
                        Console.WriteLine("[SetPersistence] Enter destination server: ");
                        SetPersistence(pulsarSystem, Console.ReadLine());
                        break;
                    case "36":
                        Console.WriteLine("[GetPersistence] Enter destination server: ");
                        GetPersistence(pulsarSystem, Console.ReadLine());
                        break;
                    case "37":
                        Console.WriteLine("[GetList] Enter destination server: ");
                        GetList(pulsarSystem, Console.ReadLine());
                        break;
                    case "38":
                        Console.WriteLine("[GetTenantNamespace] Enter destination server: ");
                        GetTenantNamespace(pulsarSystem, Console.ReadLine());
                        break;
                    case "40":
                        Console.WriteLine("[DeleteTenant] Enter destination server: ");
                        DeleteTenant(pulsarSystem, Console.ReadLine());
                        break;
                    #endregion
                    #region Function
                    case "45":
                        Console.WriteLine("[RegisterFunction] Enter destination server: ");
                        RegisterFunction(pulsarSystem, Console.ReadLine());
                        break;
                    case "46":
                        Console.WriteLine("[StartFunction] Enter destination server: ");
                        StartFunction(pulsarSystem, Console.ReadLine());
                        break;
                    case "61":
                        Console.WriteLine("[StopFunction] Enter destination server: ");
                        StopFunction(pulsarSystem, Console.ReadLine());
                        break;
                    case "47":
                        Console.WriteLine("[ListFunctions] Enter destination server: ");
                        ListFunctions(pulsarSystem, Console.ReadLine());
                        break;
                    case "48":
                        Console.WriteLine("[FunctionInfo] Enter destination server: ");
                        FunctionInfo(pulsarSystem, Console.ReadLine());
                        break;
                    case "49":
                        Console.WriteLine("[GetFunctionStatus] Enter destination server: ");
                        GetFunctionStatus(pulsarSystem, Console.ReadLine());
                        break;
                    case "50":
                        Console.WriteLine("[GetFunctionStats] Enter destination server: ");
                        GetFunctionStats(pulsarSystem, Console.ReadLine());
                        break;
                    case "53":
                        Console.WriteLine("[UpdateFunction] Enter destination server: ");
                        UpdateFunction(pulsarSystem, Console.ReadLine());
                        break;
                    case "54":
                        Console.WriteLine("[DeleteFunction] Enter destination server: ");
                        DeleteFunction(pulsarSystem, Console.ReadLine());
                        break;
                    #endregion
                    case "exit":
                        return;
                }
            }

            
        }

        #region Producers
        private static void PlainAvroBulkSendProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Seqquence));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName($"Test-{topic}")
                .Topic(topic)
                .Schema(jsonSchem)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {t.Topic}");
            var sends = new List<Send>();
            for (var i = 1L; i <= 5; i++)
            {
                var student = new Seqquence()
                {
                    SeqName = $"#LockDown Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    SeqAge = 2019 + i,
                    SeqSchool = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["SequenceId"] = i,
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string>
                    {
                        { "Tick", DateTime.Now.Ticks.ToString() },
                        {"Week-Day", "Saturday" }
                    }
                };
                //var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, t.Producer);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void RegisterSchema(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic)
                .Topic(topic)
                .Schema(jsonSchem)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            var schema = system.PulsarProducer(new RegisterSchema(AvroSchema.Of(typeof(Cctv)), "cctv-captures"), t.Producer);
            Console.WriteLine(string.IsNullOrWhiteSpace(schema.ErrorMessage)
                ? $"version: {Encoding.UTF8.GetString(schema.SchemaVersion)}"
                : $"error: {schema.ErrorCode}, message: {schema.ErrorMessage}");
        }
        private static void PlainAvroBulkSendCompressionProducer(PulsarSystem system, string topic, int comp)
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var compression = (ICompressionType)Enum.GetValues(typeof(ICompressionType)).GetValue(comp);
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic)
                .Topic(topic)
                .Schema(jsonSchem)
                .CompressionType(compression)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {t.Topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 5; i++)
            {
                var student = new Students
                {
                    Name = $"#LockDown Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                //var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, t.Producer);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        
        private static void PlainAvroBulkSendBroadcastProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  s =>
            {
                Receipts.Add(s);
            });
            //var compression = (ICompressionType)Enum.GetValues(typeof(ICompressionType)).GetValue(comp);
            
            var topics = topic.Split(",");
            var producers = new List<ProducerConfigurationData>();
            foreach (var t in topics)
            {
                var producerConfig = new ProducerConfigBuilder()
                    .ProducerName(t)
                    .Topic(t)
                    .Schema(jsonSchem)
                    //.CompressionType(compression)
                    .EventListener(producerListener)
                    .ProducerConfigurationData;
                producers.Add(producerConfig);
            }
            var result = system.PulsarProducer(new CreateProducerBroadcastGroup(jsonSchem, producers.ToHashSet(), "TestBroadcast"));
            Console.WriteLine($"Acquired producer for topic: {result.Topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 5; i++)
            {
                var student = new Students
                {
                    Name = $"#Broadcast Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - broadcast-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Broadcast",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, result.Producer);
            Task.Delay(10000).Wait();
            File.AppendAllLines("receipts-broadcast.txt", Receipts);
            Receipts.Clear();
        }
        private static void PlainAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(JournalEntry));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {t.Topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = 2020,
                School = "Akka-Pulsar university"
            };
            var journal = new JournalEntry
            {
                Id = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}",
                PersistenceId = "sampleActor",
                IsDeleted = false,
                Ordering =  0,
                Payload = JsonSerializer.Serialize(student),
                SequenceNr = 0,
                Tags = "root"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var send = new Send(journal, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            //var s = JsonSerializer.Serialize(student);
            system.Send(send, t.Producer);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
            Receipts.Clear();
        }
        private static void PlainAvroCovidProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Covid19Mobile));
            //var jsonSchem = new AutoProduceBytesSchema();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(jsonSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            var rad = new Random();
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var covid = new Covid19Mobile()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Latitude = rad.Next(-10,50),
                Longitude = rad.Next(-5,20),
                Time = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var send = new Send(covid, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            //var s = JsonSerializer.Serialize(student);
            system.Send(send, t.Producer);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }
        private static void PlainByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(byteSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(byteSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 50; i++)
            {
                var student = new Students
                {
                    Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, t.Producer);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void PlainByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  r =>
            {
                Receipts.Add(r);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                .Schema(byteSchem)
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(byteSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var s = JsonSerializer.Serialize(student);
            var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            system.Send(send, t.Producer);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }

        private static void EncryptedAvroBulkSendProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 50; i++)
            {
                var student = new Students
                {
                    Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                //var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, t.Producer);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void EncryptedAvroProducer(PulsarSystem system, string topic)
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            },  s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var send = new Send(student, topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            //var s = JsonSerializer.Serialize(student);
            system.Send(send, t.Producer);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }

        private static void EncryptedByteBulkSendProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(byteSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var sends = new List<Send>();
            for (var i = 0; i < 50; i++)
            {
                var student = new Students
                {
                    Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2019 + i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
                };
                var s = JsonSerializer.Serialize(student);
                sends.Add(new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, topic);
            system.BulkSend(bulk, t.Producer);
            Task.Delay(5000).Wait();
            File.AppendAllLines("receipts-bulk.txt", Receipts);
        }
        private static void EncryptedByteProducer(PulsarSystem system, string topic)
        {
            var byteSchem = BytesSchema.Of();
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, r =>
            {
                Receipts.Add(r);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(topic.Split("/").Last())
                .Topic(topic)
                //presto cannot parse encrypted messages
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .AddEncryptionKey("Crypto")
                .SendTimeout(10000)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = system.PulsarProducer(new CreateProducer(byteSchem, producerConfig));
            
            Console.WriteLine($"Acquired producer for topic: {topic}");
            var student = new Students
            {
                Name = $"Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - presto-ed {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                Age = DateTime.Now.Millisecond,
                School = "Akka-Pulsar university"
            };
            var metadata = new Dictionary<string, object>
            {
                ["Key"] = "Single",
                ["Properties"] = new Dictionary<string, string> { { "Tick", DateTime.Now.Ticks.ToString() } }
            };
            var s = JsonSerializer.Serialize(student);
            var send = new Send(Encoding.UTF8.GetBytes(s), topic, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            system.Send(send, t.Producer);
            Task.Delay(1000).Wait();
            File.AppendAllLines("receipts.txt", Receipts);
        }
        #endregion

        #region Consumers

        private static void PlainAvroConsumer(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));

        }
        private static void QueueConsumer(PulsarSystem system, string topic, int take)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener(null, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)
                .SetConsumptionType(ConsumptionType.Queue)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));
            foreach (var msg in system.Messages<Students>(true, take))
            {
                Console.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions{WriteIndented=true}));
            }
        }
        
        private static void GetLastMessageId(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = new AutoConsumeSchema();//AvroSchema.Of(typeof(JournalEntry));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            var c = system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));
            var messageId = system.PulsarConsumer(new LastMessageId(), c.Consumer);
            Console.WriteLine($"Topic: {messageId.Topic}, Ledger: {messageId.LedgerId}, Entry: {messageId.EntryId}, Partition: {messageId.Partition}, Batch: {messageId.BatchIndex}");
        }
        private static void PlainAvroStudentsConsumer(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));//new AutoConsumeSchema();//
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));

        }
        
        private static void PlainAvroCovidConsumer(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Covid19Mobile>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.DeviceId}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.DeviceId}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = new AutoConsumeSchema(); //AvroSchema.Of(typeof(JournalEntry));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));

        }
        private static void DecryptAvroConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single));
        }
        private static void PlainAvroConsumerSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single, /*new Seek(SeekType.MessageId, "5",1")*/ new Seek(SeekType.Timestamp, DateTimeOffset.Now.AddDays(-15).ToUnixTimeMilliseconds())));
        }
        private static void DecryptAvroConsumerSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            },null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Single, new Seek(SeekType.Timestamp, DateTime.Now.AddHours(-10))));
        }
        private static void DecryptAvroPatternConsumer(PulsarSystem system, string regex)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("pattern-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("pattern-consumer-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .TopicsPattern(new Regex(/*"persistent://public/default/.*"*/ regex))

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Pattern));

        }
        private static void DecryptAvroMultiConsumer(PulsarSystem system, string[] topics)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("topics-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("topics-consumer-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topics)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Multi));

        }
        
        private static void AvroMultiConsumer(PulsarSystem system, string[] topics)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));

            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("topics-consumer")
                .ForceTopicCreation(true)
                .SubscriptionName("topics-consumer-Subscription")
                //.CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topics)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Shared)
                .Schema(jsonSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig, ConsumerType.Multi));

        }
        private static void PlainBytesConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var s = Encoding.UTF8.GetString((byte[])(object)m.Data);
                var students = JsonSerializer.Deserialize<Students>(s);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var byteSchem = BytesSchema.Of();
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(byteSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(byteSchem, consumerConfig, ConsumerType.Single));

        }
        private static void DecryptBytesConsumer(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var s = Encoding.UTF8.GetString((byte[])(object)m.Data);
                var students = JsonSerializer.Deserialize<Students>(s);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, null);
            var byteSchem = BytesSchema.Of();
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Topic(topic)

                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(byteSchem)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(byteSchem, consumerConfig, ConsumerType.Single));

        }

        #endregion

        #region EventSource
        private static void GetNumberOfEntries(PulsarSystem system, string server, string topic)
        {
            var numb = system.EventSource(new GetNumberOfEntries(topic, server));
            Console.WriteLine($"NumberOfEntries: {JsonSerializer.Serialize(numb, new JsonSerializerOptions{WriteIndented = true})}");
        }
        private static void NextPlay(PulsarSystem system, string server, string topic, long fro, long to, long max, bool istagged )
        {
            var numb = system.EventSource(new GetNumberOfEntries(topic, server));
            foreach (var msg in system.EventSource<Students>(new NextPlay(topic, numb.Max.Value, fro, to, istagged)))
            {
                Console.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions{WriteIndented = true}));
            }
        }
        private static void ReplayTopic(PulsarSystem system, string server, string topic, long fro, long to, long max)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = system.EventSource(new GetNumberOfEntries(topic, server));
            var replay = new ReplayTopic(readerConfig, server, fro, to, numb.Max.Value, null, false);
            foreach (var msg in system.EventSource<Students>(replay))
            {
                Console.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        private static void ReplayTaggedTopic(PulsarSystem system, string server, string topic, long fro, long to, long max, string key, string value)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = system.EventSource(new GetNumberOfEntries(topic, server));
            var replay = new ReplayTopic(readerConfig, server, fro, to, numb.Max.Value, new Tag(key, value), true);
            foreach (var msg in system.EventSource<Students>(replay))
            {
                Console.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        #endregion
        private static void PlainAvroReader(PulsarSystem system,  string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-plain-students-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(jsonSchem, readerConfig));
        }
        private static void PlainBytesReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var byteSchem = BytesSchema.Of();
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("byte-plain-students-reader")
                .Schema(byteSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(byteSchem, readerConfig));
        }

        /// <summary>
        /// If you are going to use SQL, presto cannot parse encrypted messages
        /// </summary>
        /// <param name="system"></param>
        /// <param name="producerEventListener"></param>
        /// <param name="consumerEventListener"></param>
        /// <param name="readerListener"></param>
        private static void DecryptAvroReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(jsonSchem, readerConfig));
        }
        private static void DecryptAvroReaderSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(jsonSchem, readerConfig, new Seek(SeekType.Timestamp, DateTime.Now.AddHours(-10))));
        }
        private static void PlainAvroReaderSeek(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("avro-students-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(jsonSchem, readerConfig, new Seek(SeekType.Timestamp, DateTimeOffset.Now.AddDays(-20).ToUnixTimeMilliseconds())));
        }

        private static void DecryptByteReader(PulsarSystem system, string topic)
        {
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });
            var byteSchem = BytesSchema.Of();
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("byte-crypto-students-reader")
                .CryptoKeyReader(new RawFileKeyReader("pulsar_client.pem", "pulsar_client_priv.pem"))
                .Schema(byteSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            system.PulsarReader(new CreateReader(byteSchem, readerConfig));
        }

        private static void SqlQuery(PulsarSystem system, string query, string server)
        {
            var data = system.PulsarSql(new Sql(query, e =>
            {
                Console.WriteLine(e.ToString());
            }, server, Console.WriteLine));
            foreach (var d in data)
            {
                Console.WriteLine(JsonSerializer.Serialize(d, new JsonSerializerOptions{WriteIndented = true}));
            }
        }
        private static void LiveSqlQuery(PulsarSystem system, string query, int frequency, DateTime startAt, string topic , string server)
        {
            var data = system.PulsarSql(new LiveSql(query, frequency, startAt, topic, server, e =>
            {
                Console.WriteLine(e.ToString());
            }, Console.WriteLine));
            foreach (var d in data)
            {
                Console.WriteLine(JsonSerializer.Serialize(d, new JsonSerializerOptions{WriteIndented = true}));
            }
        }
        private static void GetManagedLedgerInfoPersistent(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetManagedLedgerInfoPersistent, new object[] { tenant, ns, topic, true }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, l => Console.WriteLine(l)));
        }
        private static void GetPersistentTopicStats(PulsarSystem system, string server, string tenant, string ns, string topic, long fro, long to, long max)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetInternalStatsPersistent, new object[] { tenant, ns, topic, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    var compute = new ComputeMessageId(data, fro, to, max);
                    var result = compute.GetFrom();
                    Console.WriteLine($"Ledger:{result.Ledger}, Entry:{result.Entry}, Max:{result.Max}, HighestSequence:{result.To}");
                }
                else
                {
                    Console.WriteLine("null");
                }
            }, e => Console.WriteLine(e.ToString()), server, l => Console.WriteLine(l)));
        }
        private static void GetAllSchemas(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetAllSchemas, new object[]{ tenant, ns, topic, false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions {WriteIndented = true});
                Console.WriteLine(data);
            }, e=> Console.WriteLine(e.ToString()), server, l=> Console.WriteLine(l)));
        }
        private static void RegisterSchema(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            var json = AvroSchema.Of(typeof(Covid19Mobile));
            var schema = JsonSerializer.Serialize(json.SchemaInfo);
            system.PulsarAdmin(new Admin(AdminCommands.PostSchema, new object[]{ tenant, ns, topic,
                new PostSchemaPayload("avro", schema, new Dictionary<string, string>{{ "__alwaysAllowNull", "true" }, { "__jsr310ConversionEnabled", "false" } }), false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions {WriteIndented = true});
                Console.WriteLine(data);
            }, e=> Console.WriteLine(e.ToString()), server, l=> Console.WriteLine(l)));
        }
        private static void DeleteTopic(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeletePersistentTopic, new object[]{ tenant, ns, topic, true, false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions {WriteIndented = true});
                Console.WriteLine(data);
            }, e=> Console.WriteLine(e.ToString()), server, l=> Console.WriteLine(l)));
        }
        private static void DeleteSchema(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeleteSchema, new object[]{ tenant, ns, topic, false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions {WriteIndented = true});
                Console.WriteLine(data);
            }, e=> Console.WriteLine(e.ToString()), server, l=> Console.WriteLine(l)));
        }
        private static void CreateTenant(PulsarSystem system, string server, string tenant, string clusters)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { tenant, new TenantInfo { AdminRoles = new List<string> { "Journal", "Query", "Create" }, AllowedClusters = clusters.Split(",") } }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void UpdateTenant(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.UpdateTenant, new object[] { "whitepurple", new TenantInfo{AdminRoles = new List<string>{"Journal", "Query", "Create"}, AllowedClusters = new List<string>{"pulsar"}} }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTenants(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTenants, new object[] { }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNamespace(PulsarSystem system, string server, string tenant, string @namespace)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] {tenant,@namespace }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void DeleteNamespace(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeleteNamespace, new object[] {"whitepurple","akka",false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNonPartitionedTopic(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNonPartitionedTopic, new object[] { "events", "akka", "journal", false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreateNonPartitionedPersistentTopic(PulsarSystem system, string server, string tenant, string ns, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreateNonPartitionedPersistentTopic, new object[] { tenant, ns, topic, false}, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void CreatePartitionedTopic(PulsarSystem system, string server, string tenant, string @namespace, string topic, int partitions)
        {
            system.PulsarAdmin(new Admin(AdminCommands.CreatePartitionedTopic, new object[] { tenant, @namespace, topic, partitions }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedTopicMetadata(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedMetadata, new object[] { "whitepurple", "akka", "iots", false,false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedPersistentTopicMetadata(PulsarSystem system, string server, string tenant, string @namespace, string topic)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedMetadataPersistence, new object[] { tenant, @namespace, topic, false,false }, e =>
            {
                var data = JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTopics(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTopics, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void SetPersistence(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.SetPersistence, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPersistence(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPersistence, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetList(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetList, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTenantNamespace(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTenantNamespaces, new object[] { "whitepurple", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetPartitionedTopics(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetPartitionedTopicList, new object[] { "events", "akka", "ALL"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void SetRetention(PulsarSystem system, string server, string tenant, string ns)
        {
            system.PulsarAdmin(new Admin(AdminCommands.SetRetention, new object[] { tenant, ns,  new RetentionPolicies(-1, -1)}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetRetention(PulsarSystem system, string server, string tenant, string ns)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetRetention, new object[] { tenant, ns}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void DeleteTenant(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.DeleteTenant, new object[] { "whitepurple"}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetTopics2(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetTopics2, new object[] { "events", "akka", "ALL" }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        //cmd 52
        private static void GetPersistenceList(PulsarSystem system, string server)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetListPersistence, new object[] { "public", "default", "ALL" }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        //cmd 56
        private static void SetSchemaCompatibilityStrategy(PulsarSystem system, string server, string tenant, string ns)
        {
            system.PulsarAdmin(new Admin(AdminCommands.SetSchemaCompatibilityStrategy, new object[] { tenant, ns, SchemaCompatibilityStrategy.ALWAYS_INCOMPATIBLE}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        //cmd 64
        private static void EnableBrokerDeduplication(PulsarSystem system, string server, string tenant, string ns, bool enable)
        {
            system.PulsarAdmin(new Admin(AdminCommands.ModifyDeduplication, new object[] { tenant, ns, enable}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetSchemaCompatibilityStrategy(PulsarSystem system, string server, string tenant, string ns)
        {
            system.PulsarAdmin(new Admin(AdminCommands.GetSchemaCompatibilityStrategy, new object[] { tenant, ns}, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void RegisterFunction(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.RegisterFunction, new object[]
            {
                new FunctionConfig
                {
                    Inputs = new []{ "persistent://public/default/covid-19-mobile"},
                    Parallelism = 1,
                    AutoAck = true,
                    ClassName = "Covid19Function",
                    Jar = "Test-Function-0.0.1.jar",
                    Tenant = "public",
                    Namespace= "default",
                    Name = "Covid19-function",
                    Output = "persistent://public/default/covid-19",
                    OutputSchemaType = "AVRO",
                    SubName = "test-function-sub",
                    CleanupSubscription = true,
                    //ProcessingGuarantees = FunctionConfigProcessingGuarantees.EFFECTIVELY_ONCE, 
                    InputSpecs =  new Dictionary<string, ConsumerConfig>{{ "persistent://public/default/covid-19-mobile", new ConsumerConfig
                    {
                        SchemaType = "AVRO"/*, SerdeClassName = "Covid19Mobile"*/
                    } } }
                }, "", Convert.ToBase64String(File.ReadAllBytes(Path.GetFullPath("Test-Function-0.0.1.jar")))

            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        
        private static void UpdateFunction(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.UpdateFunction, new object[]
            {
                new FunctionConfig
                {
                    Inputs = new []{ "persistent://public/default/covid-19-mobile"},
                    Parallelism = 1,
                    AutoAck = true,
                    ClassName = "Covid19Function",
                    Jar = "Test-Function-0.0.1.jar",
                    Tenant = "public",
                    Namespace= "default",
                    Name = "Covid19-function",
                    Output = "persistent://public/default/covid-19",
                    OutputSchemaType = "AVRO",
                    SubName = "test-function-sub",
                    CleanupSubscription = true,
                    ProcessingGuarantees = FunctionConfigProcessingGuarantees.EFFECTIVELY_ONCE,
                    InputSpecs =  new Dictionary<string, ConsumerConfig>{{ "persistent://public/default/covid-19-mobile", new ConsumerConfig
                    {
                        SchemaType = "AUTO"
                    } } }
                },
                 new UpdateOptions{UpdateAuthData = false}, 
                "", Convert.ToBase64String(File.ReadAllBytes(Path.GetFullPath("Test-Function-0.0.1.jar")))

            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void DeleteFunction(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.DeregisterFunction, new object[]
            {
                "public", "default", "Covid19-function"
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void StartFunction(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.StartFunction, new object[]
            {
                "public",
                "default",
                "Covid19-function",
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void StopFunction(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.StopFunction, new object[]
            {
                "public",
                "default",
                "Covid19-function",
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void FunctionInfo(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.GetFunctionInfo, new object[]
            {
                "public",
                "default",
                "Covid19-function",
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetFunctionStatus(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.GetFunctionStatus, new object[]
            {
                "public",
                "default",
                "Covid19-function",
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void GetFunctionStats(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.GetFunctionStats, new object[]
            {
                "public",
                "default",
                "Covid19-function",
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
        private static void ListFunctions(PulsarSystem system, string server)
        {
            system.PulsarFunction(new Function(FunctionCommand.ListFunctions, new object[]
            {
                "public",
                "default"
            }, e =>
            {
                var data = JsonConvert.SerializeObject(e, Formatting.Indented);
                Console.WriteLine(data);
            }, e => Console.WriteLine(e.ToString()), server, Console.WriteLine));
        }
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class Seqquence
    {
        public string SeqName { get; set; }
        public long SeqAge { get; set; }
        public string SeqSchool { get; set; }
    }
    public class Churches
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class Cctv
    {
        public string Location { get; set; }
        public string DeviceId { get; set; }
        public long Timestamp { get; set; }
        public byte[] Image { get; set; }
    }
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public string Payload { get; set; }
        public long Ordering { get; set; }
        public string Tags { get; set; }
    }
}
