Demo:
```csharp
 var admin = new SharpPulsar.Admin.Public.Admin("http://localhost:8080/", new HttpClient());
 var metadata = await _admin.GetTransactionMetadataAsync(new TxnID(3, 1));           
            
```