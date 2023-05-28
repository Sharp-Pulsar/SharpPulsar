# New Connect

## [text]

```c#
	private static void SetFeatureFlags(FeatureFlags flags)
	{
        flags.SupportsAuthRefresh = true;
        flags.SupportsBrokerEntryMetadata = true;
        flags.SupportsPartialProducer = true;   
	}
```

## [text]

```c#    
    public static ReadOnlySequence<byte> NewConnect(string authMethodName, string authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, string originalAuthData, string originalAuthMethod)
    {
            
        var connect = new CommandConnect
        {
            ClientVersion = libVersion ?? "Pulsar Client", 
            AuthMethodName = authMethodName,
            FeatureFlags = new FeatureFlags()
        };
        if ("ycav1".Equals(authMethodName))
		{
		    // Handle the case of a client that gets updated before the broker and starts sending the string auth method
		    // name. An example would be in broker-to-broker replication. We need to make sure the clients are still
			// passing both the enum and the string until all brokers are upgraded.
			connect.AuthMethod = AuthMethod.AuthMethodYcaV1;
		}

		if (!ReferenceEquals(targetBroker, null))
		{
			// When connecting through a proxy, we need to specify which broker do we want to be proxied through
			connect.ProxyToBrokerUrl = targetBroker;
		}

		if (!ReferenceEquals(authData, null))
		{
			connect.AuthData = ByteString.CopyFromUtf8(authData).ToByteArray();
		}

		if (!ReferenceEquals(originalPrincipal, null))
		{
			connect.OriginalPrincipal = originalPrincipal;
		}

		if (!ReferenceEquals(originalAuthData, null))
		{
			connect.OriginalAuthData = originalAuthData;
		}

		if (!ReferenceEquals(originalAuthMethod, null))
		{
			connect.OriginalAuthMethod = originalAuthMethod;
		}
		connect.ProtocolVersion = protocolVersion;
        SetFeatureFlags(connect.FeatureFlags);
		return Serializer.Serialize(connect.ToBaseCommand());
	}
```

## [text]

```c#
	public ReadOnlySequence<byte> NewTcClientConnectRequest(long tcId, long requestId)
    {
	    var tcClientConnect = new CommandTcClientConnectRequest
        {
            TcId = (ulong)tcId,
            RequestId = (ulong)requestId
        };
        return Serializer.Serialize(tcClientConnect.ToBaseCommand());
    }
```

## [text]

```c#
    public ReadOnlySequence<byte> NewConnect(string authMethodName, AuthData authData, int protocolVersion, string libVersion, string targetBroker, string originalPrincipal, AuthData originalAuthData, string originalAuthMethod)
	{
        var connect = new CommandConnect
        {
            ClientVersion = libVersion,
            AuthMethodName = authMethodName,
            FeatureFlags = new FeatureFlags(),
            ProtocolVersion = protocolVersion
        };

        if (!string.IsNullOrWhiteSpace(targetBroker))
	    {
			// When connecting through a proxy, we need to specify which broker do we want to be proxied through
			connect.ProxyToBrokerUrl = targetBroker;
	    }

	    if (authData != null)
		{
			connect.AuthData = authData.auth_data;
		}

		if (!string.IsNullOrWhiteSpace(originalPrincipal))
		{
			connect.OriginalPrincipal = originalPrincipal;
		}

		if (originalAuthData != null)
		{
			connect.OriginalAuthData = Encoding.UTF8.GetString(originalAuthData.auth_data);
		}

		if (!string.IsNullOrWhiteSpace(originalAuthMethod))
		{
			connect.OriginalAuthMethod = originalAuthMethod;
		}
        SetFeatureFlags(connect.FeatureFlags);
        var ba = connect.ToBaseCommand();
        return Serializer.Serialize(ba);
    }
```