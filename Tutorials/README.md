This is the sample tutorial for #SharpPulsar
```csharp
   //pulsar client settings builder
   Console.WriteLine("Please enter cmd");
   var cmd = Console.ReadLine();

   var clientConfig = new PulsarClientConfigBuilder()
   .ServiceUrl("pulsar://localhost:6650");

   if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
         clientConfig.EnableTransaction(true);

   //pulsar actor system
   var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

   var pulsarClient = pulsarSystem.NewClient();
   if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
        Transaction(pulsarClient);
    else if (cmd.Equals("exclusive", StringComparison.OrdinalIgnoreCase))
        ExclusiveProduceConsumer(pulsarClient);
    else if (cmd.Equals("exclusive2", StringComparison.OrdinalIgnoreCase))
        ExclusiveProduceNoneConsumer(pulsarClient);
    else if (cmd.Equals("batch", StringComparison.OrdinalIgnoreCase))
        BatchProduceConsumer(pulsarClient);
    else if (cmd.Equals("multi", StringComparison.OrdinalIgnoreCase))
        MultiConsumer(pulsarClient);
    else
        ProduceConsumer(pulsarClient);

   Console.ReadKey();
```